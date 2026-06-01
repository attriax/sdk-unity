#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Attriax.Unity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxSkanManager
    {
        private const string AttriaxRetentionEventName = "_attriax_retention";
        private const int SkanFineValueBitCount = 6;
        private const int SkanWindow1MaxDay = 2;
        private const int SkanWindow2MaxDay = 7;
        private const int SkanWindow3MaxDay = 35;
        private const long MicrosPerUnit = 1000000L;

        private readonly AttriaxSkanConfig _config;
        private readonly AttriaxPlatformType _platform;
        private readonly Func<DateTimeOffset> _clock;
        private readonly Func<string?> _readStateJson;
        private readonly Action<string?> _writeStateJson;
        private readonly Action<string, string?> _debugLog;
        private readonly Func<long, string, DateTimeOffset, Task<long?>>? _convertRevenueToUsdMicrosAsync;
        private readonly Func<
            AttriaxPlatformType,
            int,
            AttriaxSkanCoarseValue?,
            bool,
            Task<AttriaxSkanUpdateResult>> _updateConversionValueAsync;

        private AttriaxSkanState? _state;

        private bool SupportsSkan => _platform == AttriaxPlatformType.IOS;

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
            _convertRevenueToUsdMicrosAsync = convertRevenueToUsdMicrosAsync;
            _updateConversionValueAsync = updateConversionValueAsync
                ?? AttriaxNativeBridge.UpdateSkanConversionValueAsync;
        }

        internal AttriaxSkanState? State => SupportsSkan ? Clone(_state) : null;

        internal async Task InitializeAsync(bool isFirstLaunch)
        {
            if (!SupportsSkan)
            {
                await ResetAsync().ConfigureAwait(false);
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

            await UpdateConversionValueAsync(0, null, false).ConfigureAwait(false);
            await EvaluateRetentionMilestonesAsync().ConfigureAwait(false);
        }

        internal Task ResetAsync()
        {
            _state = null;
            _writeStateJson(null);
            return Task.CompletedTask;
        }

        internal async Task ApplyAppOpenResultAsync(AttriaxAppOpenResult? result)
        {
            if (!SupportsSkan)
            {
                await ResetAsync().ConfigureAwait(false);
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
                await UpdateConversionValueAsync(0, null, false).ConfigureAwait(false);
            }

            await EvaluateRetentionMilestonesAsync().ConfigureAwait(false);
        }

        internal async Task<AttriaxSkanUpdateResult> UpdateConversionValueAsync(
            int fineValue,
            AttriaxSkanCoarseValue? coarseValue,
            bool lockWindow)
        {
            if (!SupportsSkan)
            {
                return BuildResult(
                    AttriaxSkanUpdateStatus.NotSupported,
                    "SKAdNetwork updates are only supported on iOS.",
                    null,
                    null,
                    false,
                    null);
            }

            var currentState = EnsureState();

            if (!currentState.Enabled)
            {
                return BuildResult(
                    AttriaxSkanUpdateStatus.Disabled,
                    "SKAdNetwork is disabled for this SDK instance.",
                    currentState.FineValue,
                    currentState.CoarseValue,
                    currentState.LockWindow,
                    currentState);
            }

            if (fineValue < 0 || fineValue > 63)
            {
                return BuildResult(
                    AttriaxSkanUpdateStatus.InvalidValue,
                    "fineValue must be between 0 and 63.",
                    currentState.FineValue,
                    currentState.CoarseValue,
                    currentState.LockWindow,
                    currentState);
            }

            var nextFineValue = currentState.FineValue.HasValue
                ? Math.Max(currentState.FineValue.Value, fineValue)
                : fineValue;
            var nextCoarseValue = MaxCoarseValue(
                currentState.CoarseValue,
                coarseValue ?? DeriveCoarseValue(nextFineValue));
            var nextLockWindow = currentState.LockWindow || lockWindow;

            var nextState = Clone(currentState) ?? new AttriaxSkanState();
            nextState.FineValue = nextFineValue;
            nextState.CoarseValue = nextCoarseValue;
            nextState.LockWindow = nextLockWindow;
            nextState.FirstLaunchValueRegistered =
                currentState.FirstLaunchValueRegistered || nextFineValue == 0;
            nextState.LastUpdatedAt = _clock().ToUniversalTime();

            if (currentState.FineValue == nextState.FineValue &&
                currentState.CoarseValue == nextState.CoarseValue &&
                currentState.LockWindow == nextState.LockWindow)
            {
                return BuildResult(
                    AttriaxSkanUpdateStatus.AlreadyAtOrAboveValue,
                    "The requested conversion value does not advance the stored SKAN state.",
                    currentState.FineValue,
                    currentState.CoarseValue,
                    currentState.LockWindow,
                    currentState);
            }

            var bridgeResult = await _updateConversionValueAsync(
                    _platform,
                    nextFineValue,
                    nextCoarseValue,
                    nextLockWindow)
                .ConfigureAwait(false);

            if (bridgeResult.Status == AttriaxSkanUpdateStatus.Updated ||
                bridgeResult.Status == AttriaxSkanUpdateStatus.Skipped)
            {
                _state = nextState;
                await PersistStateAsync().ConfigureAwait(false);

                return BuildResult(
                    bridgeResult.Status,
                    bridgeResult.Message,
                    nextState.FineValue,
                    nextState.CoarseValue,
                    nextState.LockWindow,
                    nextState);
            }

            return BuildResult(
                bridgeResult.Status,
                bridgeResult.Message,
                bridgeResult.FineValue,
                bridgeResult.CoarseValue,
                bridgeResult.LockWindow,
                currentState);
        }

        internal Task<AttriaxSkanUpdateResult?> HandleTrackedEventAsync(
            string eventName,
            IDictionary<string, object>? eventData)
        {
            if (!SupportsSkan)
            {
                return Task.FromResult<AttriaxSkanUpdateResult?>(null);
            }

            return ApplyEventCandidatesAsync(eventName, eventData);
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
            if (groups == null || groups.Count == 0)
            {
                return null;
            }

            var nextFineValue = currentState.FineValue ?? 0;
            var nextCoarseValue = currentState.CoarseValue;
            var nextLockWindow = currentState.LockWindow;
            var matchedAnyGroup = false;

            foreach (var group in groups)
            {
                if (!IsValidBitRange(group.StartBit, group.BitCount))
                {
                    continue;
                }

                var match = MatchedWindow1Event(group, eventName, eventData);
                if (match == null)
                {
                    continue;
                }

                matchedAnyGroup = true;
                var currentSegmentValue = ExtractBitRangeValue(nextFineValue, group.StartBit, group.BitCount);
                var nextSegmentValue = Math.Max(currentSegmentValue, match.Rank);
                nextFineValue = ReplaceBitRangeValue(
                    nextFineValue,
                    group.StartBit,
                    group.BitCount,
                    nextSegmentValue);
                nextCoarseValue = MaxCoarseValue(nextCoarseValue, match.Event.CoarseValue);
                nextLockWindow = nextLockWindow || match.Event.LockWindow;
            }

            if (!matchedAnyGroup)
            {
                return null;
            }

            return await UpdateConversionValueAsync(nextFineValue, nextCoarseValue, nextLockWindow)
                .ConfigureAwait(false);
        }

        private async Task<AttriaxSkanUpdateResult?> ApplyCoarseWindowEventsAsync(
            AttriaxSkanState currentState,
            string eventName,
            IDictionary<string, object> eventData,
            IList<AttriaxSkanCoarseWindowEvent>? events)
        {
            if (events == null || events.Count == 0)
            {
                return null;
            }

            AttriaxSkanCoarseValue? nextCoarseValue = currentState.CoarseValue;
            var nextLockWindow = currentState.LockWindow;
            var matchedAnyEvent = false;

            foreach (var skanEvent in events)
            {
                if (!string.Equals(skanEvent.EventName, eventName, StringComparison.Ordinal) ||
                    !MatchesConditions(skanEvent.Conditions, eventData))
                {
                    continue;
                }

                matchedAnyEvent = true;
                nextCoarseValue = MaxCoarseValue(nextCoarseValue, skanEvent.CoarseValue);
                nextLockWindow = nextLockWindow || skanEvent.LockWindow;
            }

            if (!matchedAnyEvent)
            {
                return null;
            }

            return await UpdateConversionValueAsync(currentState.FineValue ?? 0, nextCoarseValue, nextLockWindow)
                .ConfigureAwait(false);
        }

        private SkanActiveWindow? ActiveWindowForState(AttriaxSkanState state)
        {
            if (!state.InstallAnchorAt.HasValue)
            {
                return SkanActiveWindow.Window1;
            }

            var currentDay = RetentionDay(state.InstallAnchorAt.Value, _clock().ToUniversalTime());
            return ActiveWindowForDay(currentDay);
        }

        private static SkanActiveWindow? ActiveWindowForDay(int day)
        {
            if (day < 0)
            {
                return null;
            }

            if (day <= SkanWindow1MaxDay)
            {
                return SkanActiveWindow.Window1;
            }

            if (day <= SkanWindow2MaxDay)
            {
                return SkanActiveWindow.Window2;
            }

            if (day <= SkanWindow3MaxDay)
            {
                return SkanActiveWindow.Window3;
            }

            return null;
        }

        private static SkanWindow1Match? MatchedWindow1Event(
            AttriaxSkanWindow1Group group,
            string eventName,
            IDictionary<string, object> eventData)
        {
            if (group.Events == null || group.Events.Count == 0)
            {
                return null;
            }

            SkanWindow1Match? match = null;
            for (var index = 0; index < group.Events.Count; index += 1)
            {
                var skanEvent = group.Events[index];
                if (!string.Equals(skanEvent.EventName, eventName, StringComparison.Ordinal) ||
                    !MatchesConditions(skanEvent.Conditions, eventData))
                {
                    continue;
                }

                match = new SkanWindow1Match(index + 1, skanEvent);
            }

            return match;
        }

        private async Task<IDictionary<string, object>> AugmentLocalEventDataAsync(
            string eventName,
            IDictionary<string, object> eventData)
        {
            if (string.Equals(eventName, "purchase", StringComparison.Ordinal))
            {
                return await AugmentPurchaseEventDataAsync(eventData).ConfigureAwait(false);
            }

            if (string.Equals(eventName, "ad_show", StringComparison.Ordinal))
            {
                return await AugmentAdShowEventDataAsync(eventData).ConfigureAwait(false);
            }

            return eventData;
        }

        private async Task<IDictionary<string, object>> AugmentPurchaseEventDataAsync(
            IDictionary<string, object> eventData)
        {
            var currentState = EnsureState();
            var usdMicros = await ResolvePurchaseUsdMicrosAsync(eventData).ConfigureAwait(false) ?? 0L;
            currentState.PurchaseRevenueUsdMicros += usdMicros;
            currentState.PurchaseCount += 1;
            _state = currentState;
            await PersistStateAsync().ConfigureAwait(false);

            var payload = new Dictionary<string, object>(eventData)
            {
                ["revenue"] = currentState.PurchaseRevenueUsdMicros / (double)MicrosPerUnit,
                ["count"] = currentState.PurchaseCount,
            };
            return payload;
        }

        private async Task<IDictionary<string, object>> AugmentAdShowEventDataAsync(
            IDictionary<string, object> eventData)
        {
            var currentState = EnsureState();
            currentState.AdShowCount += 1;
            _state = currentState;
            await PersistStateAsync().ConfigureAwait(false);

            var payload = new Dictionary<string, object>(eventData)
            {
                ["shown"] = currentState.AdShowCount,
                ["count"] = currentState.AdShowCount,
            };
            return payload;
        }

        private async Task<long?> ResolvePurchaseUsdMicrosAsync(IDictionary<string, object> eventData)
        {
            if (!eventData.TryGetValue("revenue", out var rawRevenue))
            {
                return null;
            }

            var revenue = CoerceNumber(rawRevenue);
            if (!revenue.HasValue)
            {
                return null;
            }

            var revenueInMicros = ReadBoolean(eventData, "revenueInMicros") ??
                ReadBoolean(eventData, "revenue_in_micros") ??
                false;
            var amountMicros = ToMicros(revenue.Value, revenueInMicros);
            var currency = ReadString(eventData, "currency")?.ToUpperInvariant() ?? "USD";
            if (string.Equals(currency, "USD", StringComparison.Ordinal))
            {
                return amountMicros;
            }

            if (_convertRevenueToUsdMicrosAsync == null)
            {
                _debugLog("Skipping non-USD purchase revenue for SKAN because no USD conversion gateway is available.", null);
                return null;
            }

            try
            {
                return await _convertRevenueToUsdMicrosAsync(amountMicros, currency, _clock().ToUniversalTime())
                    .ConfigureAwait(false);
            }
            catch (Exception error)
            {
                _debugLog("Failed to convert purchase revenue to USD for SKAN.", error.Message);
                return 1L;
            }
        }

        private static bool MatchesConditions(
            IList<AttriaxSkanCondition>? conditions,
            IDictionary<string, object> eventData)
        {
            if (conditions == null || conditions.Count == 0)
            {
                return true;
            }

            foreach (var condition in conditions)
            {
                var hasValue = eventData.TryGetValue(condition.ParamKey, out var actualValue);
                if (!ConditionMatches(condition, actualValue, hasValue))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ConditionMatches(
            AttriaxSkanCondition condition,
            object? actualValue,
            bool hasValue)
        {
            switch (condition.Operator)
            {
                case AttriaxSkanRuleOperator.Exists:
                    return hasValue && actualValue != null;
                case AttriaxSkanRuleOperator.Eq:
                    return hasValue && ValuesEqual(actualValue, condition.Value);
                case AttriaxSkanRuleOperator.NotEq:
                    return hasValue && !ValuesEqual(actualValue, condition.Value);
                case AttriaxSkanRuleOperator.Gt:
                case AttriaxSkanRuleOperator.Gte:
                case AttriaxSkanRuleOperator.Lt:
                case AttriaxSkanRuleOperator.Lte:
                {
                    var actualNumber = CoerceNumber(actualValue);
                    var expectedNumber = CoerceNumber(condition.Value);
                    if (!hasValue || !actualNumber.HasValue || !expectedNumber.HasValue)
                    {
                        return false;
                    }

                    return condition.Operator switch
                    {
                        AttriaxSkanRuleOperator.Gt => actualNumber.Value > expectedNumber.Value,
                        AttriaxSkanRuleOperator.Gte => actualNumber.Value >= expectedNumber.Value,
                        AttriaxSkanRuleOperator.Lt => actualNumber.Value < expectedNumber.Value,
                        AttriaxSkanRuleOperator.Lte => actualNumber.Value <= expectedNumber.Value,
                        _ => false,
                    };
                }
                case AttriaxSkanRuleOperator.Contains:
                {
                    if (!hasValue || actualValue == null || condition.Value == null)
                    {
                        return false;
                    }

                    var actualString = UnwrapJToken(actualValue) as string;
                    var expectedString = UnwrapJToken(condition.Value) as string;
                    if (actualString != null && expectedString != null)
                    {
                        return actualString.IndexOf(expectedString, StringComparison.OrdinalIgnoreCase) >= 0;
                    }

                    if (actualValue is JArray array)
                    {
                        return array.Any(item => ValuesEqual(item, condition.Value));
                    }

                    if (actualValue is IEnumerable enumerable && !(actualValue is string))
                    {
                        foreach (var item in enumerable)
                        {
                            if (ValuesEqual(item, condition.Value))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
                default:
                    return false;
            }
        }

        private static bool ValuesEqual(object? left, object? right)
        {
            left = UnwrapJToken(left);
            right = UnwrapJToken(right);

            var leftNumber = CoerceNumber(left);
            var rightNumber = CoerceNumber(right);
            if (leftNumber.HasValue && rightNumber.HasValue)
            {
                return Math.Abs(leftNumber.Value - rightNumber.Value) < double.Epsilon;
            }

            if (left is bool leftBool && right is bool rightBool)
            {
                return leftBool == rightBool;
            }

            return string.Equals(
                Convert.ToString(left, CultureInfo.InvariantCulture),
                Convert.ToString(right, CultureInfo.InvariantCulture),
                StringComparison.Ordinal);
        }

        private static double? CoerceNumber(object? value)
        {
            value = UnwrapJToken(value);
            if (value == null)
            {
                return null;
            }

            switch (value)
            {
                case byte number:
                    return number;
                case sbyte number:
                    return number;
                case short number:
                    return number;
                case ushort number:
                    return number;
                case int number:
                    return number;
                case uint number:
                    return number;
                case long number:
                    return number;
                case ulong number:
                    return number;
                case float number:
                    return number;
                case double number:
                    return number;
                case decimal number:
                    return (double)number;
                case string text when double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed):
                    return parsed;
                default:
                    return null;
            }
        }

        private static long ToMicros(double value, bool alreadyMicros)
        {
            var scaled = alreadyMicros ? value : value * MicrosPerUnit;
            return Convert.ToInt64(Math.Round(scaled, MidpointRounding.AwayFromZero));
        }

        private static string? ReadString(IDictionary<string, object> values, string key)
        {
            return values.TryGetValue(key, out var value)
                ? Convert.ToString(UnwrapJToken(value), CultureInfo.InvariantCulture)?.Trim()
                : null;
        }

        private static bool? ReadBoolean(IDictionary<string, object> values, string key)
        {
            if (!values.TryGetValue(key, out var rawValue))
            {
                return null;
            }

            rawValue = UnwrapJToken(rawValue);
            switch (rawValue)
            {
                case bool value:
                    return value;
                case string text when bool.TryParse(text.Trim(), out var parsed):
                    return parsed;
                case string text when text.Trim() == "1":
                    return true;
                case string text when text.Trim() == "0":
                    return false;
                default:
                    var number = CoerceNumber(rawValue);
                    return number.HasValue ? Math.Abs(number.Value) > double.Epsilon : null;
            }
        }

        private static bool IsValidBitRange(int startBit, int bitCount)
        {
            return startBit >= 0 &&
                bitCount > 0 &&
                startBit + bitCount <= SkanFineValueBitCount;
        }

        private static int ExtractBitRangeValue(int fineValue, int startBit, int bitCount)
        {
            var mask = (1 << bitCount) - 1;
            return (fineValue >> startBit) & mask;
        }

        private static int ReplaceBitRangeValue(int fineValue, int startBit, int bitCount, int value)
        {
            var maxValue = (1 << bitCount) - 1;
            var clampedValue = value < 0 ? 0 : Math.Min(value, maxValue);
            var mask = maxValue << startBit;
            return (fineValue & ~mask) | ((clampedValue << startBit) & mask);
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

            var actualDay = RetentionDay(installAnchorAt.Value, _clock().ToUniversalTime());
            var activeWindow = ActiveWindowForDay(actualDay);
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

                var milestoneWindow = ActiveWindowForDay(day);
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

                var milestoneWindow = ActiveWindowForDay(day);
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

                var number = CoerceNumber(condition.Value);
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

        private static int RetentionDay(DateTimeOffset installAnchorAt, DateTimeOffset now)
        {
            var normalizedInstallDay = new DateTimeOffset(
                installAnchorAt.UtcDateTime.Year,
                installAnchorAt.UtcDateTime.Month,
                installAnchorAt.UtcDateTime.Day,
                0,
                0,
                0,
                TimeSpan.Zero);
            var normalizedCurrentDay = new DateTimeOffset(
                now.UtcDateTime.Year,
                now.UtcDateTime.Month,
                now.UtcDateTime.Day,
                0,
                0,
                0,
                TimeSpan.Zero);
            var difference = (normalizedCurrentDay - normalizedInstallDay).Days;
            return difference < 0 ? 0 : difference;
        }

        private static AttriaxSkanCoarseValue DeriveCoarseValue(int fineValue)
        {
            if (fineValue >= 40)
            {
                return AttriaxSkanCoarseValue.High;
            }

            if (fineValue >= 20)
            {
                return AttriaxSkanCoarseValue.Medium;
            }

            return AttriaxSkanCoarseValue.Low;
        }

        private static AttriaxSkanCoarseValue? MaxCoarseValue(
            AttriaxSkanCoarseValue? current,
            AttriaxSkanCoarseValue? next)
        {
            if (!current.HasValue)
            {
                return next;
            }

            if (!next.HasValue)
            {
                return current;
            }

            return current.Value >= next.Value ? current : next;
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

        private static object? UnwrapJToken(object? value)
        {
            return value switch
            {
                JValue jValue => jValue.Value,
                _ => value,
            };
        }

        private static AttriaxSkanUpdateResult BuildResult(
            AttriaxSkanUpdateStatus status,
            string? message,
            int? fineValue,
            AttriaxSkanCoarseValue? coarseValue,
            bool lockWindow,
            AttriaxSkanState? state)
        {
            return new AttriaxSkanUpdateResult
            {
                Status = status,
                Message = message,
                FineValue = fineValue,
                CoarseValue = coarseValue,
                LockWindow = lockWindow,
                State = Clone(state),
            };
        }

        private enum SkanActiveWindow
        {
            Window1,
            Window2,
            Window3,
        }

        private sealed class SkanWindow1Match
        {
            internal SkanWindow1Match(int rank, AttriaxSkanEvent skanEvent)
            {
                Rank = rank;
                Event = skanEvent;
            }

            internal int Rank { get; }

            internal AttriaxSkanEvent Event { get; }
        }
    }
}
