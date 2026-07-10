#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Attriax.Unity;
using Newtonsoft.Json;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Thin orchestrator for SKAdNetwork conversion handling. It owns the in-memory
    /// state, persistence, the operation lock, retention-milestone evaluation, and
    /// window dispatch, delegating the pure computation to <see cref="AttriaxSkanRules"/>,
    /// <see cref="AttriaxSkanEventResolution"/>, <see cref="AttriaxSkanConversionUpdater"/>
    /// and <see cref="AttriaxSkanEventAugmenter"/>. This mirrors the Flutter reference's
    /// <c>attriax_skan_manager.dart</c> decomposition.
    /// </summary>
    internal sealed class AttriaxSkanManager
    {
        private const string AttriaxRetentionEventName = "_attriax_retention";

        private readonly AttriaxSkanConfig _config;
        private readonly AttriaxPlatformType _platform;
        private readonly Func<DateTimeOffset> _clock;
        private readonly Func<string?> _readStateJson;
        private readonly Action<string?> _writeStateJson;
        private readonly Action<string, string?> _debugLog;
        private readonly AttriaxSkanConversionUpdater _conversionUpdater;
        private readonly AttriaxSkanEventAugmenter _eventAugmenter;

        private AttriaxSkanState? _state;

        // Serializes all state-mutating operations. State is read, mutated, and
        // persisted across await boundaries (FX conversion, native bridge), so
        // concurrent tracked events would otherwise clobber each other's counter
        // increments. Public entry points acquire this; internal helpers call the
        // *Unlocked variants directly to avoid re-entrant deadlock.
        private readonly SemaphoreSlim _operationLock = new SemaphoreSlim(1, 1);

        private bool SupportsSkan => AttriaxSkanRules.PlatformSupportsSkan(_platform);

        internal AttriaxSkanManager(
            AttriaxSkanConfig config,
            AttriaxPlatformType platform,
            Func<DateTimeOffset> clock,
            Func<string?> readStateJson,
            Action<string?> writeStateJson,
            Action<string, string?> debugLog,
            Func<long, string, DateTimeOffset, Task<long?>>? convertRevenueToUsdMicrosAsync = null,
            Func<
                AttriaxPlatformType,
                int,
                AttriaxSkanCoarseValue?,
                bool,
                Task<AttriaxSkanUpdateResult>>? updateConversionValueAsync = null)
        {
            _config = config ?? new AttriaxSkanConfig();
            _platform = platform;
            _clock = clock;
            _readStateJson = readStateJson;
            _writeStateJson = writeStateJson;
            _debugLog = debugLog;
            _conversionUpdater = new AttriaxSkanConversionUpdater(
                platform,
                updateConversionValueAsync ?? AttriaxNativeBridge.UpdateSkanConversionValueAsync,
                clock);
            _eventAugmenter = new AttriaxSkanEventAugmenter(
                clock,
                convertRevenueToUsdMicrosAsync,
                debugLog);
        }

        internal AttriaxSkanState? State => SupportsSkan ? Clone(_state) : null;

        internal async Task InitializeAsync(bool isFirstLaunch)
        {
            await _operationLock.WaitAsync().ConfigureAwait(false);
            try
            {
                await InitializeUnlockedAsync(isFirstLaunch).ConfigureAwait(false);
            }
            finally
            {
                _operationLock.Release();
            }
        }

        private async Task InitializeUnlockedAsync(bool isFirstLaunch)
        {
            if (!SupportsSkan)
            {
                await ResetUnlockedAsync().ConfigureAwait(false);
                return;
            }

            var restoredState = ReadPersistedState();
            _state = restoredState ?? new AttriaxSkanState
            {
                Enabled = _config.Enabled,
            };

            _state.Enabled = _config.Enabled;
            if (_state.InstallAnchorAt == null && isFirstLaunch)
            {
                _state.InstallAnchorAt = _clock().ToUniversalTime();
            }

            if (_state.SchemaVersion == null && _state.Schema != null)
            {
                _state.SchemaVersion = _state.Schema.Version;
            }

            _state.CompletedRetentionDays ??= new List<int>();
            await PersistStateAsync().ConfigureAwait(false);

            if (!_config.Enabled)
            {
                return;
            }

            if (!_config.RegisterFirstLaunchValue || !isFirstLaunch)
            {
                await EvaluateRetentionMilestonesAsync().ConfigureAwait(false);
                return;
            }

            var currentState = EnsureState();
            if (currentState.FirstLaunchValueRegistered)
            {
                await EvaluateRetentionMilestonesAsync().ConfigureAwait(false);
                return;
            }

            await UpdateConversionValueUnlockedAsync(0, null, false, markFirstLaunchValueRegistered: true)
                .ConfigureAwait(false);
            await EvaluateRetentionMilestonesAsync().ConfigureAwait(false);
        }

        internal async Task ResetAsync()
        {
            await _operationLock.WaitAsync().ConfigureAwait(false);
            try
            {
                await ResetUnlockedAsync().ConfigureAwait(false);
            }
            finally
            {
                _operationLock.Release();
            }
        }

        private Task ResetUnlockedAsync()
        {
            _state = null;
            _writeStateJson(null);
            return Task.CompletedTask;
        }

        internal async Task ApplyAppOpenResultAsync(AttriaxAppOpenResult? result)
        {
            await _operationLock.WaitAsync().ConfigureAwait(false);
            try
            {
                await ApplyAppOpenResultUnlockedAsync(result).ConfigureAwait(false);
            }
            finally
            {
                _operationLock.Release();
            }
        }

        private async Task ApplyAppOpenResultUnlockedAsync(AttriaxAppOpenResult? result)
        {
            if (!SupportsSkan)
            {
                await ResetUnlockedAsync().ConfigureAwait(false);
                return;
            }

            var currentState = EnsureState();
            var runtimeConfiguration = result?.Skan;
            var installState = result?.InstallState ?? AttriaxInstallState.Existing;
            var nextSchema = runtimeConfiguration?.Schema ?? currentState.Schema;
            var nextSchemaVersion = nextSchema?.Version ?? currentState.SchemaVersion;
            var schemaVersionChanged = nextSchemaVersion.HasValue && nextSchemaVersion != currentState.SchemaVersion;

            var nextState = Clone(currentState) ?? new AttriaxSkanState();
            nextState.Enabled = runtimeConfiguration?.Enabled ?? currentState.Enabled;
            nextState.SchemaVersion = nextSchemaVersion;
            nextState.Schema = Clone(nextSchema);
            nextState.CompletedRetentionDays = schemaVersionChanged
                ? new List<int>()
                : new List<int>(currentState.CompletedRetentionDays ?? Array.Empty<int>());

            if (installState != AttriaxInstallState.Existing)
            {
                var installAnchorAt = result?.AcceptedAt?.ToUniversalTime() ?? _clock().ToUniversalTime();
                nextState.FineValue = null;
                nextState.CoarseValue = null;
                nextState.LockWindow = false;
                nextState.FirstLaunchValueRegistered = false;
                nextState.LastUpdatedAt = installAnchorAt;
                nextState.InstallAnchorAt = installAnchorAt;
                nextState.CompletedRetentionDays = new List<int>();
                nextState.PurchaseRevenueUsdMicros = 0;
                nextState.PurchaseCount = 0;
                nextState.AdShowCount = 0;
            }
            else if (nextState.InstallAnchorAt == null && result?.AcceptedAt != null)
            {
                nextState.InstallAnchorAt = result.AcceptedAt.Value.ToUniversalTime();
            }

            _state = nextState;
            await PersistStateAsync().ConfigureAwait(false);

            if (installState != AttriaxInstallState.Existing &&
                _config.RegisterFirstLaunchValue &&
                nextState.Enabled &&
                !nextState.FirstLaunchValueRegistered)
            {
                await UpdateConversionValueUnlockedAsync(0, null, false, markFirstLaunchValueRegistered: true)
                    .ConfigureAwait(false);
            }

            await EvaluateRetentionMilestonesAsync().ConfigureAwait(false);
        }

        internal async Task<AttriaxSkanUpdateResult> UpdateConversionValueAsync(
            int fineValue,
            AttriaxSkanCoarseValue? coarseValue,
            bool lockWindow)
        {
            await _operationLock.WaitAsync().ConfigureAwait(false);
            try
            {
                return await UpdateConversionValueUnlockedAsync(fineValue, coarseValue, lockWindow, false)
                    .ConfigureAwait(false);
            }
            finally
            {
                _operationLock.Release();
            }
        }

        private async Task<AttriaxSkanUpdateResult> UpdateConversionValueUnlockedAsync(
            int fineValue,
            AttriaxSkanCoarseValue? coarseValue,
            bool lockWindow,
            bool markFirstLaunchValueRegistered)
        {
            var update = await _conversionUpdater.UpdateAsync(
                    SupportsSkan ? EnsureState() : null,
                    fineValue,
                    coarseValue,
                    lockWindow,
                    markFirstLaunchValueRegistered)
                .ConfigureAwait(false);

            if (update.NextState != null)
            {
                _state = update.NextState;
                await PersistStateAsync().ConfigureAwait(false);
            }

            return update.Result;
        }

        internal async Task<AttriaxSkanUpdateResult?> HandleTrackedEventAsync(
            string eventName,
            IDictionary<string, object>? eventData)
        {
            if (!SupportsSkan)
            {
                return null;
            }

            await _operationLock.WaitAsync().ConfigureAwait(false);
            try
            {
                return await ApplyEventCandidatesAsync(eventName, eventData).ConfigureAwait(false);
            }
            finally
            {
                _operationLock.Release();
            }
        }

        private AttriaxSkanState EnsureState()
        {
            if (_state == null)
            {
                _state = new AttriaxSkanState
                {
                    Enabled = _config.Enabled,
                    CompletedRetentionDays = new List<int>(),
                };
            }

            _state.CompletedRetentionDays ??= new List<int>();
            return _state;
        }

        private async Task<AttriaxSkanUpdateResult?> ApplyEventCandidatesAsync(
            string eventName,
            IDictionary<string, object>? eventData)
        {
            var normalizedEventName = string.IsNullOrWhiteSpace(eventName)
                ? null
                : eventName.Trim();
            if (string.IsNullOrWhiteSpace(normalizedEventName))
            {
                return null;
            }

            var currentState = EnsureState();
            var schema = currentState.Schema;
            if (!currentState.Enabled || schema == null)
            {
                return null;
            }

            var payload = await AugmentLocalEventDataAsync(
                    normalizedEventName!,
                    eventData ?? new Dictionary<string, object>())
                .ConfigureAwait(false);
            currentState = EnsureState();
            var activeWindow = ActiveWindowForState(currentState);
            if (!activeWindow.HasValue)
            {
                return null;
            }

            return activeWindow.Value switch
            {
                SkanActiveWindow.Window1 => await ApplyWindow1GroupsAsync(
                        currentState,
                        normalizedEventName!,
                        payload,
                        schema.Window1?.Groups)
                    .ConfigureAwait(false),
                SkanActiveWindow.Window2 => await ApplyCoarseWindowEventsAsync(
                        currentState,
                        normalizedEventName!,
                        payload,
                        schema.Window2?.Events)
                    .ConfigureAwait(false),
                SkanActiveWindow.Window3 => await ApplyCoarseWindowEventsAsync(
                        currentState,
                        normalizedEventName!,
                        payload,
                        schema.Window3?.Events)
                    .ConfigureAwait(false),
                _ => null,
            };
        }

        private async Task<AttriaxSkanUpdateResult?> ApplyWindow1GroupsAsync(
            AttriaxSkanState currentState,
            string eventName,
            IDictionary<string, object> eventData,
            IList<AttriaxSkanWindow1Group>? groups)
        {
            var update = AttriaxSkanEventResolution.ResolveWindow1SkanUpdate(
                currentState,
                eventName,
                eventData,
                groups);
            if (update == null)
            {
                return null;
            }

            return await UpdateConversionValueUnlockedAsync(
                    update.FineValue,
                    update.CoarseValue,
                    update.LockWindow,
                    false)
                .ConfigureAwait(false);
        }

        private async Task<AttriaxSkanUpdateResult?> ApplyCoarseWindowEventsAsync(
            AttriaxSkanState currentState,
            string eventName,
            IDictionary<string, object> eventData,
            IList<AttriaxSkanCoarseWindowEvent>? events)
        {
            var update = AttriaxSkanEventResolution.ResolveCoarseWindowSkanUpdate(
                currentState,
                eventName,
                eventData,
                events);
            if (update == null)
            {
                return null;
            }

            return await UpdateConversionValueUnlockedAsync(
                    update.FineValue,
                    update.CoarseValue,
                    update.LockWindow,
                    false)
                .ConfigureAwait(false);
        }

        private SkanActiveWindow? ActiveWindowForState(AttriaxSkanState state)
        {
            if (!state.InstallAnchorAt.HasValue)
            {
                return SkanActiveWindow.Window1;
            }

            var currentDay = AttriaxSkanRules.RetentionDay(state.InstallAnchorAt.Value, _clock().ToUniversalTime());
            return AttriaxSkanRules.ActiveWindowForDay(currentDay);
        }

        private async Task<IDictionary<string, object>> AugmentLocalEventDataAsync(
            string eventName,
            IDictionary<string, object> eventData)
        {
            var augmentation = await _eventAugmenter.AugmentAsync(
                    eventName,
                    eventData,
                    EnsureState())
                .ConfigureAwait(false);

            if (augmentation.StateChanged)
            {
                _state = augmentation.State;
                await PersistStateAsync().ConfigureAwait(false);
            }

            return augmentation.EventData;
        }

        private async Task<AttriaxSkanUpdateResult?> EvaluateRetentionMilestonesAsync()
        {
            var currentState = EnsureState();
            var schema = currentState.Schema;
            var installAnchorAt = currentState.InstallAnchorAt;

            if (!currentState.Enabled || schema == null || !installAnchorAt.HasValue)
            {
                return null;
            }

            var configuredDays = ConfiguredRetentionDays(schema)
                .OrderBy(day => day)
                .ToList();
            if (configuredDays.Count == 0)
            {
                return null;
            }

            var actualDay = AttriaxSkanRules.RetentionDay(installAnchorAt.Value, _clock().ToUniversalTime());
            var activeWindow = AttriaxSkanRules.ActiveWindowForDay(actualDay);
            var completedDays = new HashSet<int>(currentState.CompletedRetentionDays ?? Array.Empty<int>());
            var stateChanged = false;

            if (!activeWindow.HasValue)
            {
                foreach (var day in configuredDays.Where(day => day <= actualDay && !completedDays.Contains(day)))
                {
                    completedDays.Add(day);
                    stateChanged = true;
                }

                if (stateChanged)
                {
                    currentState.CompletedRetentionDays = completedDays.OrderBy(day => day).ToList();
                    _state = currentState;
                    await PersistStateAsync().ConfigureAwait(false);
                }

                return null;
            }

            foreach (var day in configuredDays)
            {
                if (day > actualDay || completedDays.Contains(day))
                {
                    continue;
                }

                var milestoneWindow = AttriaxSkanRules.ActiveWindowForDay(day);
                if (!milestoneWindow.HasValue || milestoneWindow.Value < activeWindow.Value)
                {
                    completedDays.Add(day);
                    stateChanged = true;
                }
            }

            AttriaxSkanUpdateResult? latestResult = null;
            foreach (var day in configuredDays)
            {
                if (day > actualDay || completedDays.Contains(day))
                {
                    continue;
                }

                var milestoneWindow = AttriaxSkanRules.ActiveWindowForDay(day);
                if (milestoneWindow != activeWindow)
                {
                    continue;
                }

                latestResult = await ApplyEventCandidatesAsync(
                        AttriaxRetentionEventName,
                        new Dictionary<string, object>
                        {
                            ["day"] = day,
                            ["actualDay"] = actualDay,
                        })
                    .ConfigureAwait(false);
                completedDays.Add(day);
                stateChanged = true;
            }

            if (stateChanged)
            {
                currentState.CompletedRetentionDays = completedDays.OrderBy(day => day).ToList();
                _state = currentState;
                await PersistStateAsync().ConfigureAwait(false);
            }

            return latestResult;
        }

        private static HashSet<int> ConfiguredRetentionDays(AttriaxSkanSchema schema)
        {
            var days = new HashSet<int>();

            if (schema.Window1?.Groups != null)
            {
                foreach (var group in schema.Window1.Groups)
                {
                    if (group.Events == null)
                    {
                        continue;
                    }

                    foreach (var skanEvent in group.Events)
                    {
                        AddRetentionDaysFromConditions(days, skanEvent.EventName, skanEvent.Conditions);
                    }
                }
            }

            if (schema.Window2?.Events != null)
            {
                foreach (var skanEvent in schema.Window2.Events)
                {
                    AddRetentionDaysFromConditions(days, skanEvent.EventName, skanEvent.Conditions);
                }
            }

            if (schema.Window3?.Events != null)
            {
                foreach (var skanEvent in schema.Window3.Events)
                {
                    AddRetentionDaysFromConditions(days, skanEvent.EventName, skanEvent.Conditions);
                }
            }

            return days;
        }

        private static void AddRetentionDaysFromConditions(
            ISet<int> days,
            string? eventName,
            IList<AttriaxSkanCondition>? conditions)
        {
            if (!string.Equals(eventName, AttriaxRetentionEventName, StringComparison.Ordinal) ||
                conditions == null)
            {
                return;
            }

            foreach (var condition in conditions)
            {
                if (!string.Equals(condition.ParamKey, "day", StringComparison.Ordinal) ||
                    condition.Operator != AttriaxSkanRuleOperator.Eq)
                {
                    continue;
                }

                var number = AttriaxSkanRules.CoerceNumber(condition.Value);
                if (!number.HasValue)
                {
                    continue;
                }

                var day = (int)number.Value;
                if (day >= 0 && Math.Abs(number.Value - day) < double.Epsilon)
                {
                    days.Add(day);
                }
            }
        }

        private AttriaxSkanState? ReadPersistedState()
        {
            var serialized = _readStateJson();
            if (string.IsNullOrWhiteSpace(serialized))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<AttriaxSkanState>(serialized!);
            }
            catch (Exception error)
            {
                _debugLog("Failed to restore persisted SKAN state.", error.Message);
                return null;
            }
        }

        private Task PersistStateAsync()
        {
            _writeStateJson(_state == null ? null : JsonConvert.SerializeObject(_state));
            _debugLog(
                "Updated local SKAN state:",
                _state == null ? null : JsonConvert.SerializeObject(_state));
            return Task.CompletedTask;
        }

        private static T? Clone<T>(T? value)
            where T : class
        {
            if (value == null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value));
        }
    }
}
