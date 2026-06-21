#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Mirrors <c>AttriaxCrashReportingCoordinator</c> in
    /// <c>sdk-flutter/attriax/lib/src/internal/attriax_crash_reporting_coordinator.dart</c>.
    /// Owns the Unity unhandled-exception subscription and forwards captured
    /// frames to <see cref="AttriaxTrackingManager"/> for crash recording.
    /// </summary>
    /// <remarks>
    /// The Flutter coordinator additionally persists fatal crashes for replay
    /// on the next launch and consumes pending native crash reports. Unity
    /// does not yet have those capabilities, so this coordinator only handles
    /// the live <c>Application.logMessageReceived</c> path.
    /// </remarks>
    internal sealed class AttriaxCrashReportingCoordinator
    {
        private readonly AttriaxConfig _config;
        private readonly Func<bool> _isRuntimeActive;
        private readonly AttriaxTrackingManager _trackingManager;
        private readonly Action<Task, string> _observeBackgroundTask;

        private bool _attached;

        public AttriaxCrashReportingCoordinator(
            AttriaxConfig config,
            Func<bool> isRuntimeActive,
            AttriaxTrackingManager trackingManager,
            Action<Task, string> observeBackgroundTask)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _isRuntimeActive = isRuntimeActive ?? throw new ArgumentNullException(nameof(isRuntimeActive));
            _trackingManager = trackingManager ?? throw new ArgumentNullException(nameof(trackingManager));
            _observeBackgroundTask = observeBackgroundTask
                ?? throw new ArgumentNullException(nameof(observeBackgroundTask));
        }

        /// <summary>
        /// Subscribes to <see cref="AttriaxLifecycleDispatcher.UnhandledExceptionLogged"/>.
        /// Idempotent.
        /// </summary>
        public void Activate()
        {
            if (_attached)
            {
                return;
            }

            AttriaxLifecycleDispatcher.UnhandledExceptionLogged += HandleUnhandledExceptionLogged;
            _attached = true;
        }

        /// <summary>
        /// Unsubscribes from <see cref="AttriaxLifecycleDispatcher.UnhandledExceptionLogged"/>.
        /// Idempotent.
        /// </summary>
        public void Deactivate()
        {
            if (!_attached)
            {
                return;
            }

            AttriaxLifecycleDispatcher.UnhandledExceptionLogged -= HandleUnhandledExceptionLogged;
            _attached = false;
        }

        private void HandleUnhandledExceptionLogged(string condition, string stackTrace, LogType logType)
        {
            if (!_isRuntimeActive() ||
                !_config.AutomaticCrashReportingEnabled ||
                logType != LogType.Exception)
            {
                return;
            }

            try
            {
                var metadata = new Dictionary<string, object>
                {
                    ["logType"] = logType.ToString(),
                };

                _observeBackgroundTask(
                    _trackingManager.RecordCrashAsync(
                        ExtractExceptionType(condition),
                        string.IsNullOrWhiteSpace(condition) ? "Unhandled Unity exception" : condition,
                        string.IsNullOrWhiteSpace(stackTrace)
                            ? (string.IsNullOrWhiteSpace(condition) ? "Unhandled Unity exception" : condition)
                            : stackTrace,
                        new AttriaxRecordErrorOptions
                        {
                            Source = "unity_log_exception",
                            IsFatal = false,
                            Reason = "Unhandled Unity exception",
                            Metadata = metadata,
                        }),
                    "Automatic Unity crash reporting failed.");
            }
            catch (Exception error)
            {
                UnityEngine.Debug.LogError(
                    "[Attriax] Automatic crash-reporting handler threw an unexpected error: " + error.Message);
            }
        }

        internal static string ExtractExceptionType(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition))
            {
                return "Exception";
            }

            var separatorIndex = condition.IndexOf(':');
            if (separatorIndex > 0)
            {
                return condition.Substring(0, separatorIndex).Trim();
            }

            return "Exception";
        }
    }
}
