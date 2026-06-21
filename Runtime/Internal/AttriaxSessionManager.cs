#nullable enable
using System;
using System.Collections.Generic;
using SdkSessionLifecycleKind = Attriax.Unity.Generated.Model.SdkSessionLifecycleKind;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxSessionManager
    {
        private readonly bool _sessionTrackingEnabled;
        private readonly Func<bool> _canTrackSessions;
        private readonly AttriaxRuntimeState _runtimeState;
        private readonly AttriaxContextManager _contextManager;
        private readonly int _firstLaunchHeartbeatIntervalMs;
        private readonly int _sessionHeartbeatIntervalMs;
        private readonly IAttriaxSessionStore _sessionStore;
        private readonly IAttriaxSessionLifecycleQueue _sessionLifecycleQueue;
        private readonly Action<string, string?> _debugLog;

        private AttriaxSessionSnapshot? _currentSession;
        private AttriaxSessionSnapshot? _pendingRecoveredSessionEnd;
        private bool _isBackgrounded;
        private float _sessionHeartbeatAccumulatorSeconds;

        public AttriaxSessionManager(
            bool sessionTrackingEnabled,
            Func<bool> canTrackSessions,
            AttriaxRuntimeState runtimeState,
            AttriaxContextManager contextManager,
            int firstLaunchHeartbeatIntervalMs,
            int sessionHeartbeatIntervalMs,
            IAttriaxSessionStore sessionStore,
            IAttriaxSessionLifecycleQueue sessionLifecycleQueue,
            Action<string, string?> debugLog)
        {
            _sessionTrackingEnabled = sessionTrackingEnabled;
            _canTrackSessions = canTrackSessions;
            _runtimeState = runtimeState;
            _contextManager = contextManager;
            _firstLaunchHeartbeatIntervalMs = firstLaunchHeartbeatIntervalMs;
            _sessionHeartbeatIntervalMs = sessionHeartbeatIntervalMs;
            _sessionStore = sessionStore ?? throw new ArgumentNullException(nameof(sessionStore));
            _sessionLifecycleQueue = sessionLifecycleQueue;
            _debugLog = debugLog;
        }

        public AttriaxSessionSnapshot? CurrentSession => _currentSession;

        public AttriaxContextSnapshot ContextSnapshot => _contextManager.Snapshot;

        public bool IsBackgrounded => _isBackgrounded;

        public void Initialize(DateTimeOffset occurredAt)
        {
            _pendingRecoveredSessionEnd = null;

            if (_sessionTrackingEnabled && _canTrackSessions())
            {
                _currentSession = RestoreOrStartSession(occurredAt).CurrentSession;
            }
            else
            {
                _currentSession = null;
                PersistSessionSnapshot(null);
            }

            _isBackgrounded = false;
            _sessionHeartbeatAccumulatorSeconds = 0f;
        }

        public void HandleSdkDisabled()
        {
            _sessionHeartbeatAccumulatorSeconds = 0f;
        }

        public void HandleSdkEnabled(DateTimeOffset occurredAt)
        {
            _isBackgrounded = false;
            if (_sessionTrackingEnabled && _canTrackSessions())
            {
                ResumeOrStartSession(occurredAt);
            }
            else
            {
                _currentSession = null;
                _pendingRecoveredSessionEnd = null;
                PersistSessionSnapshot(null);
            }

            _sessionHeartbeatAccumulatorSeconds = 0f;
        }

        public void Reset()
        {
            _currentSession = null;
            _pendingRecoveredSessionEnd = null;
            _isBackgrounded = false;
            _sessionHeartbeatAccumulatorSeconds = 0f;
            PersistSessionSnapshot(null);
        }

        public void SyncCurrentSessionContext()
        {
            if (_currentSession == null || string.IsNullOrWhiteSpace(_runtimeState.DeviceId))
            {
                return;
            }

            var context = _contextManager.Snapshot;
            if (string.Equals(_currentSession.DeviceId, _runtimeState.DeviceId, StringComparison.Ordinal))
            {
                return;
            }

            _currentSession.DeviceId = _runtimeState.DeviceId;
            _currentSession.Locale = context.Device.Language ?? _currentSession.Locale;
            _currentSession.AppVersion = context.App.Version ?? _currentSession.AppVersion;
            _currentSession.AppBuildNumber = context.App.BuildNumber ?? _currentSession.AppBuildNumber;
            _currentSession.AppPackageName = context.App.PackageName ?? _currentSession.AppPackageName;
            PersistSessionSnapshot(_currentSession);
        }

        public void HandlePause(DateTimeOffset occurredAt)
        {
            if (!_sessionTrackingEnabled || !_canTrackSessions())
            {
                return;
            }

            HandleBackgrounded(occurredAt);
        }

        public void HandleFocus(DateTimeOffset occurredAt)
        {
            if (!_sessionTrackingEnabled || !_canTrackSessions())
            {
                return;
            }

            HandleForegrounded(occurredAt);
        }

        public void HandleQuitting(DateTimeOffset occurredAt)
        {
            if (!_sessionTrackingEnabled || !_canTrackSessions())
            {
                return;
            }

            HandleEnding(occurredAt);
        }

        public void HandleTick(float deltaSeconds, DateTimeOffset occurredAt)
        {
            if (!_sessionTrackingEnabled || !_canTrackSessions() || !_runtimeState.IsEnabled || _isBackgrounded || _currentSession == null)
            {
                return;
            }

            _sessionHeartbeatAccumulatorSeconds += deltaSeconds;
            var heartbeatIntervalSeconds = Math.Max(_currentSession.HeartbeatIntervalMs, 1000) / 1000f;
            if (_sessionHeartbeatAccumulatorSeconds >= heartbeatIntervalSeconds)
            {
                _sessionHeartbeatAccumulatorSeconds = 0f;
                HandleHeartbeat(occurredAt);
            }
        }

        public void HandleSuccessfulForegroundFlush(string sessionId, DateTimeOffset occurredAt)
        {
            if (!_sessionTrackingEnabled || !_canTrackSessions() || !_runtimeState.IsEnabled || _isBackgrounded || _currentSession == null)
            {
                return;
            }

            if (!string.Equals(_currentSession.Id, sessionId, StringComparison.Ordinal))
            {
                return;
            }

            RecordExistingSessionActivity(occurredAt);
            _sessionHeartbeatAccumulatorSeconds = 0f;
        }

        public AttriaxSessionRestoreResult PrepareTrackedActivity(DateTimeOffset occurredAt)
        {
            var sessionResult = ResumeOrStartSession(occurredAt);
            FlushPendingRecoveredSessionEnd();
            if (sessionResult.StartedNewSession)
            {
                _sessionLifecycleQueue.QueueSessionLifecycle(
                    SdkSessionLifecycleKind.Start,
                    sessionResult.CurrentSession,
                    sessionResult.CurrentSession.StartedAt,
                    null);
            }

            _sessionHeartbeatAccumulatorSeconds = 0f;
            return sessionResult;
        }

        private void HandleBackgrounded(DateTimeOffset occurredAt)
        {
            if (!_sessionTrackingEnabled || !_canTrackSessions() || !_runtimeState.IsEnabled || _isBackgrounded)
            {
                return;
            }

            _isBackgrounded = true;
            _sessionHeartbeatAccumulatorSeconds = 0f;

            var currentSession = RecordExistingSessionActivity(occurredAt);
            if (currentSession == null)
            {
                return;
            }

            FlushPendingRecoveredSessionEnd();
            _sessionLifecycleQueue.QueueSessionLifecycle(SdkSessionLifecycleKind.Pause, currentSession, occurredAt, null);
        }

        private void HandleForegrounded(DateTimeOffset occurredAt)
        {
            var wasBackgrounded = _isBackgrounded;
            _isBackgrounded = false;

            if (!_sessionTrackingEnabled || !_canTrackSessions() || !_runtimeState.IsEnabled)
            {
                return;
            }

            var sessionResult = ResumeOrStartSession(occurredAt);
            _sessionHeartbeatAccumulatorSeconds = 0f;
            if (!wasBackgrounded)
            {
                return;
            }

            FlushPendingRecoveredSessionEnd();
            _sessionLifecycleQueue.QueueSessionLifecycle(
                sessionResult.StartedNewSession ? SdkSessionLifecycleKind.Start : SdkSessionLifecycleKind.Resume,
                sessionResult.CurrentSession,
                sessionResult.StartedNewSession ? sessionResult.CurrentSession.StartedAt : occurredAt,
                null);
        }

        private void HandleHeartbeat(DateTimeOffset occurredAt)
        {
            if (!_sessionTrackingEnabled || !_canTrackSessions() || !_runtimeState.IsEnabled || _isBackgrounded)
            {
                return;
            }

            var sessionResult = ResumeOrStartSession(occurredAt);
            FlushPendingRecoveredSessionEnd();
            _sessionLifecycleQueue.QueueSessionLifecycle(
                sessionResult.StartedNewSession ? SdkSessionLifecycleKind.Start : SdkSessionLifecycleKind.Heartbeat,
                sessionResult.CurrentSession,
                sessionResult.StartedNewSession ? sessionResult.CurrentSession.StartedAt : occurredAt,
                null);
        }

        private void HandleEnding(DateTimeOffset occurredAt)
        {
            if (!_sessionTrackingEnabled || !_canTrackSessions() || !_runtimeState.IsEnabled)
            {
                return;
            }

            _isBackgrounded = true;
            _sessionHeartbeatAccumulatorSeconds = 0f;

            var endedSession = EndCurrentSession(occurredAt);
            if (endedSession == null)
            {
                return;
            }

            FlushPendingRecoveredSessionEnd();
            _sessionLifecycleQueue.QueueSessionLifecycle(SdkSessionLifecycleKind.End, endedSession, occurredAt, null);
        }

        private void FlushPendingRecoveredSessionEnd()
        {
            if (!_sessionTrackingEnabled || !_runtimeState.IsEnabled || _pendingRecoveredSessionEnd == null)
            {
                return;
            }

            var recoveredSession = _pendingRecoveredSessionEnd;
            _pendingRecoveredSessionEnd = null;
            _sessionLifecycleQueue.QueueSessionLifecycle(
                SdkSessionLifecycleKind.End,
                recoveredSession,
                InferSessionEndAt(recoveredSession),
                new Dictionary<string, object>
                {
                    ["recovered"] = true,
                });
        }

        private AttriaxSessionRestoreResult RestoreOrStartSession(DateTimeOffset occurredAt)
        {
            return ActivateSession(ReadPersistedSessionSnapshot(), occurredAt);
        }

        private AttriaxSessionRestoreResult ResumeOrStartSession(DateTimeOffset occurredAt)
        {
            return ActivateSession(_currentSession ?? ReadPersistedSessionSnapshot(), occurredAt);
        }

        private AttriaxSessionRestoreResult ActivateSession(
            AttriaxSessionSnapshot? existingSession,
            DateTimeOffset occurredAt)
        {
            if (existingSession != null && ShouldContinueSession(existingSession, occurredAt))
            {
                if (occurredAt > existingSession.LastActivityAt)
                {
                    existingSession.LastActivityAt = occurredAt;
                }

                _currentSession = existingSession;
                PersistSessionSnapshot(existingSession);
                return new AttriaxSessionRestoreResult(existingSession, false, null);
            }

            var currentSession = BuildNewSession(occurredAt);
            _currentSession = currentSession;
            PersistSessionSnapshot(currentSession);
            if (existingSession != null)
            {
                _pendingRecoveredSessionEnd = existingSession;
            }

            return new AttriaxSessionRestoreResult(currentSession, true, existingSession);
        }

        private AttriaxSessionSnapshot BuildNewSession(DateTimeOffset occurredAt)
        {
            var context = _contextManager.Snapshot;
            return new AttriaxSessionSnapshot
            {
                Id = Guid.NewGuid().ToString("N"),
                DeviceId = _runtimeState.DeviceId,
                Platform = context.Platform,
                Locale = context.Device.Language,
                IsFirstLaunch = _runtimeState.IsFirstLaunch,
                StartedAt = occurredAt,
                LastActivityAt = occurredAt,
                HeartbeatIntervalMs = _runtimeState.IsFirstLaunch
                    ? _firstLaunchHeartbeatIntervalMs
                    : _sessionHeartbeatIntervalMs,
                AppVersion = context.App.Version,
                AppBuildNumber = context.App.BuildNumber,
                AppPackageName = context.App.PackageName,
                SdkPackageVersion = context.Sdk.PackageVersion,
            };
        }

        private bool ShouldContinueSession(AttriaxSessionSnapshot session, DateTimeOffset occurredAt)
        {
            return AttriaxSessionContinuationPolicy.ShouldContinue(
                session,
                _runtimeState.DeviceId,
                _contextManager.Snapshot,
                occurredAt);
        }

        private AttriaxSessionSnapshot? RecordExistingSessionActivity(DateTimeOffset occurredAt)
        {
            if (_currentSession == null)
            {
                return null;
            }

            if (occurredAt > _currentSession.LastActivityAt)
            {
                _currentSession.LastActivityAt = occurredAt;
                PersistSessionSnapshot(_currentSession);
            }

            return _currentSession;
        }

        private AttriaxSessionSnapshot? EndCurrentSession(DateTimeOffset occurredAt)
        {
            var currentSession = RecordExistingSessionActivity(occurredAt);
            _currentSession = null;
            PersistSessionSnapshot(null);
            return currentSession;
        }

        private DateTimeOffset InferSessionEndAt(AttriaxSessionSnapshot session)
        {
            return session.LastActivityAt.AddMilliseconds(
                AttriaxSessionContinuationPolicy.ResolveContinuationWindowMs(session.HeartbeatIntervalMs));
        }

        private AttriaxSessionSnapshot? ReadPersistedSessionSnapshot()
        {
            return _sessionStore.ReadSessionSnapshot();
        }

        private void PersistSessionSnapshot(AttriaxSessionSnapshot? session)
        {
            _sessionStore.WriteSessionSnapshot(session);
        }
    }
}