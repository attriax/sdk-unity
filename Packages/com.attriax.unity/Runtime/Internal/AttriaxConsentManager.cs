#nullable enable
using System;
using System.Globalization;
using System.Threading.Tasks;
using Attriax.Unity.Generated.Model;

namespace Attriax.Unity.Internal
{
    internal readonly struct AttriaxConsentIdentity
    {
        public AttriaxConsentIdentity(string consentId)
        {
            ConsentId = consentId;
        }

        public string ConsentId { get; }
    }

    internal enum AttriaxTrackingSignal
    {
        Analytics,
        AdEvents,
        Attribution,
        Session,
        DeepLink,
        UninstallTracking,
    }

    internal enum AttriaxTrackingIdentityMode
    {
        Identified,
        Anonymous,
        Withheld,
    }

    internal readonly struct AttriaxTrackingDecision
    {
        public AttriaxTrackingDecision(
            bool capture,
            AttriaxTrackingIdentityMode identityMode,
            bool deferNetwork)
        {
            Capture = capture;
            IdentityMode = identityMode;
            DeferNetwork = deferNetwork;
        }

        public bool Capture { get; }

        public AttriaxTrackingIdentityMode IdentityMode { get; }

        public bool DeferNetwork { get; }

        public bool AttachDeviceIdentity => IdentityMode == AttriaxTrackingIdentityMode.Identified;

        public bool SendNetworkDirectly => Capture && !DeferNetwork;
    }

    internal interface IAttriaxGdprConsentGateway
    {
        Task<SdkGdprConsentStatusDto> CheckGdprConsentAsync(SdkV1GdprConsentCheckDto request);

        Task<SdkGdprConsentStatusDto> UpsertGdprConsentAsync(SdkV1GdprConsentWriteDto request);
    }

    internal interface IAttriaxConsentReadView
    {
        AttriaxGdprConsentState State { get; }

        AttriaxGdprConsentValues? Values { get; }

        bool AnonymousTrackingEnabled { get; }

        bool IsWaitingForConsent { get; }

        bool ShouldDeferNetworkDispatch { get; }

        bool AllowsAnalyticsTracking { get; }

        bool AllowsAttributionTracking { get; }

        bool AllowsAdEventsTracking { get; }

        bool CanCaptureAnalytics { get; }

        bool CanCaptureAttribution { get; }

        bool CanCaptureAdEvents { get; }

        bool CanCaptureUninstallTracking { get; }

        AttriaxTrackingDecision TrackingDecisionFor(AttriaxTrackingSignal signal);
    }

    internal sealed class AttriaxConsentManager : IAttriaxConsentReadView
    {
        private readonly IAttriaxConsentStore _store;
        private readonly string _projectToken;
        private readonly bool _gdprEnabled;
        private bool _anonymousTrackingEnabled;
        private readonly Func<AttriaxConsentIdentity> _ensureConsentIdentity;
        private readonly Func<string?> _resolveTimezone;
        private readonly IAttriaxGdprConsentGateway _gateway;
        private readonly Action? _onStateChanged;
        private readonly Action<string, string?> _debugLog;

        private AttriaxGdprConsentState _state = AttriaxGdprConsentState.Unknown;
        private AttriaxGdprConsentValues? _values;
        private string? _countryCode;
        private string? _regionSource;
        private DateTime? _checkedAt;
        private bool _pendingSync;
        private bool _didRestore;
        private int _consentGeneration;
        private Task<bool>? _needsConsentTask;
        private Task? _pendingSyncTask;

        public AttriaxConsentManager(
            IAttriaxConsentStore store,
            string projectToken,
            bool gdprEnabled,
            bool anonymousTracking,
            Func<AttriaxConsentIdentity> ensureConsentIdentity,
            Func<string?> resolveTimezone,
            IAttriaxGdprConsentGateway gateway,
            Action? onStateChanged,
            Action<string, string?> debugLog)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _projectToken = projectToken;
            _gdprEnabled = gdprEnabled;
            _anonymousTrackingEnabled = anonymousTracking;
            _ensureConsentIdentity = ensureConsentIdentity;
            _resolveTimezone = resolveTimezone;
            _gateway = gateway;
            _onStateChanged = onStateChanged;
            _debugLog = debugLog;
        }

        public AttriaxGdprConsentState State => _state;

        public AttriaxGdprConsentValues? Values => CloneValues(_values);

        public bool AnonymousTrackingEnabled => _anonymousTrackingEnabled;

        public bool IsWaitingForConsent =>
            _state == AttriaxGdprConsentState.Pending ||
            _state == AttriaxGdprConsentState.Unknown;

        public bool ShouldDeferNetworkDispatch => Policy.ShouldDeferNetworkDispatch;

        /// <summary>
        /// Whether runtime-scoped data may be persisted under the current consent.
        /// </summary>
        public bool AllowsRuntimePersistence => Policy.AllowsRuntimePersistence;

        // Immutable snapshot of the current consent gate. Rebuilt per access (a
        // cheap readonly struct) so it always reflects the latest stored state.
        private AttriaxConsentPolicy Policy =>
            new AttriaxConsentPolicy(_gdprEnabled, _state, _values, _anonymousTrackingEnabled);

        public bool AllowsAnalyticsTracking => AllowsCategory(values => values.Analytics);

        public bool AllowsAttributionTracking => AllowsCategory(values => values.Attribution);

        public bool AllowsAdEventsTracking => AllowsCategory(values => values.AdEvents);

        public bool CanCaptureAnalytics => TrackingDecisionFor(AttriaxTrackingSignal.Analytics).Capture;

        public bool CanCaptureAttribution => TrackingDecisionFor(AttriaxTrackingSignal.Attribution).Capture;

        public bool CanCaptureAdEvents => TrackingDecisionFor(AttriaxTrackingSignal.AdEvents).Capture;

        public bool CanCaptureUninstallTracking => TrackingDecisionFor(AttriaxTrackingSignal.UninstallTracking).Capture;

        public AttriaxTrackingDecision TrackingDecisionFor(AttriaxTrackingSignal signal)
        {
            return Policy.TrackingDecisionFor(signal);
        }

        public void Init()
        {
            Restore();
        }

        public void ClearMemory()
        {
            _state = AttriaxGdprConsentState.Unknown;
            _values = null;
            _countryCode = null;
            _regionSource = null;
            _checkedAt = null;
            _pendingSync = false;
            _didRestore = false;
            _consentGeneration++;
            _needsConsentTask = null;
            _pendingSyncTask = null;
        }

        public Task FlushPendingSyncAsync()
        {
            Restore();
            return FlushPendingSyncInternalAsync();
        }

        public Task<bool> NeedsConsentAsync(bool localOnly = false)
        {
            Restore();

            var canUseCachedState =
                (_state == AttriaxGdprConsentState.Granted ||
                    _state == AttriaxGdprConsentState.NotRequired) &&
                (localOnly || !ShouldRefreshRemoteDecision());
            if (canUseCachedState)
            {
                if (!localOnly)
                {
                    _ = FlushPendingSyncInternalAsync();
                }

                return Task.FromResult(IsWaitingForConsent);
            }

            if (_needsConsentTask != null)
            {
                return _needsConsentTask;
            }

            var resolutionTask = ResolveNeedsConsentAsync(localOnly);
            _needsConsentTask = resolutionTask;
            ObserveTaskCompletion(
                resolutionTask,
                () =>
                {
                    if (ReferenceEquals(_needsConsentTask, resolutionTask))
                    {
                        _needsConsentTask = null;
                    }
                });
            return resolutionTask;
        }

        public void SetConsent(bool analytics, bool attribution, bool adEvents)
        {
            ApplyState(
                AttriaxGdprConsentState.Granted,
                new AttriaxGdprConsentValues
                {
                    Analytics = analytics,
                    Attribution = attribution,
                    AdEvents = adEvents,
                },
                DateTime.UtcNow,
                _countryCode,
                "manual",
                true);
            _consentGeneration++;
            _ = PersistAndFlushAsync();
        }

        public void SetNotRequired()
        {
            ApplyState(
                AttriaxGdprConsentState.NotRequired,
                null,
                DateTime.UtcNow,
                _countryCode,
                "manual",
                true);
            _consentGeneration++;
            _ = PersistAndFlushAsync();
        }

        public void Reset()
        {
            ApplyState(
                AttriaxGdprConsentState.Unknown,
                null,
                DateTime.UtcNow,
                null,
                null,
                true);
            _consentGeneration++;
            _ = PersistAndFlushAsync();
        }

        public void SetAnonymousTrackingEnabled(bool enabled)
        {
            if (_anonymousTrackingEnabled == enabled)
            {
                return;
            }

            _anonymousTrackingEnabled = enabled;
            _onStateChanged?.Invoke();
        }

        private bool AllowsCategory(Func<AttriaxGdprConsentValues, bool> selector) =>
            Policy.AllowsCategory(selector);

        private bool ShouldRefreshRemoteDecision()
        {
            return string.Equals(_regionSource, "local_only_timezone", StringComparison.Ordinal)
                || string.Equals(_regionSource, "local_only_timezone_unresolved", StringComparison.Ordinal)
                || string.Equals(_regionSource, "auto_timezone", StringComparison.Ordinal)
                || string.Equals(_regionSource, "local_timezone_fallback", StringComparison.Ordinal);
        }

        private async Task<bool> ResolveNeedsConsentAsync(bool localOnly)
        {
            if (_pendingSync && _state == AttriaxGdprConsentState.Unknown)
            {
                await FlushPendingSyncInternalAsync().ConfigureAwait(false);
                if (_pendingSync && _state == AttriaxGdprConsentState.Unknown)
                {
                    return true;
                }
            }

            if (!localOnly)
            {
                try
                {
                    var identity = _ensureConsentIdentity();
                    // Capture the generation before the network await: a SetConsent
                    // that lands during the check must not be downgraded by the
                    // (now stale) check echo.
                    var gen = _consentGeneration;
                    var status = await _gateway.CheckGdprConsentAsync(
                            AttriaxGeneratedRequestFactory.BuildGdprConsentCheckRequest(
                                _projectToken,
                                identity.ConsentId))
                        .ConfigureAwait(false);
                    if (_consentGeneration == gen)
                    {
                        ApplyRemoteStatus(status, false);
                    }
                    else
                    {
                        // A newer local SetConsent landed during the check; ensure its
                        // intent is upserted rather than dropped (its now-stale echo is
                        // discarded).
                        _ = FlushPendingSyncInternalAsync();
                    }
                    return IsWaitingForConsent;
                }
                catch (Exception error)
                {
                    _debugLog(
                        "Failed to check GDPR consent with Attriax. Falling back to local timezone detection.",
                        error.Message);
                }
            }

            var localState = AttriaxGdprRegion.ResolveStateForTimezone(_resolveTimezone());
            if (localState.HasValue)
            {
                ApplyState(
                    localState.Value,
                    null,
                    DateTime.UtcNow,
                    null,
                    localOnly ? "local_only_timezone" : "local_timezone_fallback",
                    false);
                PersistCurrentState();

                return IsWaitingForConsent;
            }

            ApplyState(
                AttriaxGdprConsentState.Unknown,
                null,
                DateTime.UtcNow,
                null,
                localOnly ? "local_only_timezone_unresolved" : "local_timezone_fallback",
                false);
            PersistCurrentState();
            return true;
        }

        private void Restore()
        {
            if (_didRestore)
            {
                return;
            }

            var stored = _store.ReadConsentState();
            if (stored == null)
            {
                _didRestore = true;
                return;
            }

            _state = StateFromStorage(stored.State);
            _values = CloneValues(stored.Values);
            _countryCode = NormalizeString(stored.CountryCode)?.ToUpperInvariant();
            _regionSource = NormalizeString(stored.RegionSource);
            _checkedAt = NormalizeDate(stored.CheckedAt);
            _pendingSync = stored.PendingSync;

            _didRestore = true;
        }

        private void PersistCurrentState()
        {
            if (!_pendingSync && _state == AttriaxGdprConsentState.Unknown)
            {
                _store.ClearConsentState();
                return;
            }

            _store.WriteConsentState(new AttriaxStoredConsentState
            {
                State = StateToStorage(_state),
                Values = CloneValues(_values),
                CheckedAt = _checkedAt?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                CountryCode = _countryCode,
                RegionSource = _regionSource,
                PendingSync = _pendingSync,
            });
        }

        private async Task PersistAndFlushAsync()
        {
            PersistCurrentState();
            await FlushPendingSyncInternalAsync().ConfigureAwait(false);
        }

        private Task FlushPendingSyncInternalAsync()
        {
            if (!_pendingSync)
            {
                return Task.CompletedTask;
            }

            if (_pendingSyncTask != null)
            {
                return _pendingSyncTask;
            }

            var syncTask = SyncPendingStateAsync();
            _pendingSyncTask = syncTask;
            ObserveTaskCompletion(
                syncTask,
                () =>
                {
                    if (ReferenceEquals(_pendingSyncTask, syncTask))
                    {
                        _pendingSyncTask = null;
                    }
                });
            return syncTask;
        }

        private async Task SyncPendingStateAsync()
        {
            // Convergence loop. SetConsent #2 can coalesce into the in-flight task
            // created by SetConsent #1 (see FlushPendingSyncInternalAsync). The
            // generation guard ensures that when an upsert returns we never let its
            // (now stale) echo downgrade values set by a newer local SetConsent that
            // landed during the await: we capture the generation BEFORE the await,
            // and if it advanced we discard the echo and re-sync the current intent.
            while (true)
            {
                var gen = _consentGeneration;
                try
                {
                    var identity = _ensureConsentIdentity();
                    var status = await _gateway.UpsertGdprConsentAsync(
                            AttriaxGeneratedRequestFactory.BuildGdprConsentWriteRequest(
                                _projectToken,
                                identity.ConsentId,
                                _checkedAt ?? DateTime.UtcNow,
                                _countryCode,
                                _regionSource,
                                StateToGenerated(_state),
                                _values))
                        .ConfigureAwait(false);

                    if (_consentGeneration != gen)
                    {
                        // A newer local SetConsent landed while we were syncing. The
                        // echo we just received reflects the OLD intent — applying it
                        // would clobber the newer values. Discard it and re-sync the
                        // now-current state until the generation is stable.
                        continue;
                    }

                    ApplyRemoteStatus(status, false);
                    return;
                }
                catch (Exception error)
                {
                    _debugLog(
                        "Failed to sync GDPR consent state to Attriax. The SDK will retry later.",
                        error.Message);
                    _pendingSync = true;
                    PersistCurrentState();
                    return;
                }
            }
        }

        private void ApplyRemoteStatus(SdkGdprConsentStatusDto status, bool pendingSync)
        {
            var mappedState = StateFromGenerated(status.state);
            var mappedValues = NormalizeGeneratedValues(status.values);
            if (mappedState == AttriaxGdprConsentState.Granted && mappedValues == null)
            {
                mappedState = AttriaxGdprConsentState.Pending;
            }

            ApplyState(
                mappedState,
                mappedValues,
                status.checkedAt == default ? DateTime.UtcNow : status.checkedAt,
                NormalizeString(status.countryCode),
                NormalizeString(status.regionSource),
                pendingSync);
            PersistCurrentState();
        }

        private void ApplyState(
            AttriaxGdprConsentState state,
            AttriaxGdprConsentValues? values,
            DateTime checkedAt,
            string? countryCode,
            string? regionSource,
            bool pendingSync)
        {
            var nextValues = CloneValues(values);
            var nextCountryCode = NormalizeString(countryCode)?.ToUpperInvariant();
            var nextRegionSource = NormalizeString(regionSource);
            var nextCheckedAt = checkedAt == default ? DateTime.UtcNow : checkedAt;
            var changed =
                state != _state ||
                !AreValuesEqual(nextValues, _values) ||
                !string.Equals(nextCountryCode, _countryCode, StringComparison.Ordinal) ||
                !string.Equals(nextRegionSource, _regionSource, StringComparison.Ordinal) ||
                nextCheckedAt != _checkedAt ||
                pendingSync != _pendingSync;

            _state = state;
            _values = nextValues;
            _countryCode = nextCountryCode;
            _regionSource = nextRegionSource;
            _checkedAt = nextCheckedAt;
            _pendingSync = pendingSync;
            _didRestore = true;

            if (changed)
            {
                _onStateChanged?.Invoke();
            }
        }

        private static void ObserveTaskCompletion(Task task, Action onFinally)
        {
            task.ContinueWith(
                _ => onFinally(),
                TaskContinuationOptions.ExecuteSynchronously);
        }

        private static AttriaxGdprConsentValues? CloneValues(AttriaxGdprConsentValues? values)
        {
            if (values == null)
            {
                return null;
            }

            return new AttriaxGdprConsentValues
            {
                Analytics = values.Analytics,
                Attribution = values.Attribution,
                AdEvents = values.AdEvents,
            };
        }

        private static AttriaxGdprConsentValues? NormalizeGeneratedValues(SdkGdprConsentValuesDto? values)
        {
            if (values == null)
            {
                return null;
            }

            return new AttriaxGdprConsentValues
            {
                Analytics = values.analytics,
                Attribution = values.attribution,
                AdEvents = values.adEvents,
            };
        }

        private static bool AreValuesEqual(AttriaxGdprConsentValues? left, AttriaxGdprConsentValues? right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return left.Analytics == right.Analytics
                && left.Attribution == right.Attribution
                && left.AdEvents == right.AdEvents;
        }

        private static string? NormalizeString(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static DateTime? NormalizeDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (DateTime.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var parsed))
            {
                return parsed.Kind == DateTimeKind.Utc
                    ? parsed
                    : parsed.ToUniversalTime();
            }

            return null;
        }

        private static AttriaxGdprConsentState StateFromStorage(string? state)
        {
            switch (state)
            {
                case "not_required":
                    return AttriaxGdprConsentState.NotRequired;
                case "pending":
                    return AttriaxGdprConsentState.Pending;
                case "granted":
                    return AttriaxGdprConsentState.Granted;
                default:
                    return AttriaxGdprConsentState.Unknown;
            }
        }

        private static string StateToStorage(AttriaxGdprConsentState state)
        {
            switch (state)
            {
                case AttriaxGdprConsentState.NotRequired:
                    return "not_required";
                case AttriaxGdprConsentState.Pending:
                    return "pending";
                case AttriaxGdprConsentState.Granted:
                    return "granted";
                default:
                    return "unknown";
            }
        }

        private static AppUserGdprConsentState StateToGenerated(AttriaxGdprConsentState state)
        {
            switch (state)
            {
                case AttriaxGdprConsentState.NotRequired:
                    return AppUserGdprConsentState.NotRequired;
                case AttriaxGdprConsentState.Pending:
                    return AppUserGdprConsentState.Pending;
                case AttriaxGdprConsentState.Granted:
                    return AppUserGdprConsentState.Granted;
                default:
                    return AppUserGdprConsentState.Unknown;
            }
        }

        private static AttriaxGdprConsentState StateFromGenerated(AppUserGdprConsentState state)
        {
            switch (state)
            {
                case AppUserGdprConsentState.NotRequired:
                    return AttriaxGdprConsentState.NotRequired;
                case AppUserGdprConsentState.Pending:
                    return AttriaxGdprConsentState.Pending;
                case AppUserGdprConsentState.Granted:
                    return AttriaxGdprConsentState.Granted;
                default:
                    return AttriaxGdprConsentState.Unknown;
            }
        }
    }
}