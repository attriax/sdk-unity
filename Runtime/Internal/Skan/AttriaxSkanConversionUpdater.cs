#nullable enable
using System;
using System.Threading.Tasks;
using Attriax.Unity;
using Newtonsoft.Json;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Pure computation of a SKAN conversion-value update. Mirrors the Flutter
    /// reference's <c>attriax_skan_conversion_updater.dart</c>: it validates the
    /// requested value, derives the next state, optionally calls the native bridge,
    /// and returns a <see cref="AttriaxSkanConversionUpdate"/> whose
    /// <see cref="AttriaxSkanConversionUpdate.NextState"/> is non-null exactly when
    /// the manager should persist. Persistence stays in the manager.
    /// </summary>
    internal sealed class AttriaxSkanConversionUpdater
    {
        private readonly AttriaxPlatformType _platform;
        private readonly Func<DateTimeOffset> _clock;
        private readonly Func<
            AttriaxPlatformType,
            int,
            AttriaxSkanCoarseValue?,
            bool,
            Task<AttriaxSkanUpdateResult>> _updateConversionValueAsync;

        internal AttriaxSkanConversionUpdater(
            AttriaxPlatformType platform,
            Func<
                AttriaxPlatformType,
                int,
                AttriaxSkanCoarseValue?,
                bool,
                Task<AttriaxSkanUpdateResult>> updateConversionValueAsync,
            Func<DateTimeOffset> clock)
        {
            _platform = platform;
            _updateConversionValueAsync = updateConversionValueAsync;
            _clock = clock;
        }

        private bool SupportsSkan => AttriaxSkanRules.PlatformSupportsSkan(_platform);

        internal async Task<AttriaxSkanConversionUpdate> UpdateAsync(
            AttriaxSkanState? currentState,
            int fineValue,
            AttriaxSkanCoarseValue? coarseValue,
            bool lockWindow,
            bool markFirstLaunchValueRegistered)
        {
            if (!SupportsSkan)
            {
                return new AttriaxSkanConversionUpdate(
                    BuildResult(
                        AttriaxSkanUpdateStatus.NotSupported,
                        "SKAdNetwork updates are only supported on iOS.",
                        null,
                        null,
                        false,
                        null),
                    null);
            }

            var state = currentState ?? new AttriaxSkanState { Enabled = false };

            if (!state.Enabled)
            {
                return new AttriaxSkanConversionUpdate(
                    BuildResult(
                        AttriaxSkanUpdateStatus.Disabled,
                        "SKAdNetwork is disabled for this SDK instance.",
                        state.FineValue,
                        state.CoarseValue,
                        state.LockWindow,
                        state),
                    null);
            }

            if (fineValue < 0 || fineValue > 63)
            {
                return new AttriaxSkanConversionUpdate(
                    BuildResult(
                        AttriaxSkanUpdateStatus.InvalidValue,
                        "fineValue must be between 0 and 63.",
                        state.FineValue,
                        state.CoarseValue,
                        state.LockWindow,
                        state),
                    null);
            }

            var nextFineValue = state.FineValue.HasValue
                ? Math.Max(state.FineValue.Value, fineValue)
                : fineValue;
            var nextCoarseValue = AttriaxSkanRules.MaxCoarseValue(
                state.CoarseValue,
                coarseValue ?? AttriaxSkanRules.DeriveCoarseValue(nextFineValue));
            var nextLockWindow = state.LockWindow || lockWindow;

            var nextState = Clone(state) ?? new AttriaxSkanState();
            nextState.FineValue = nextFineValue;
            nextState.CoarseValue = nextCoarseValue;
            nextState.LockWindow = nextLockWindow;
            // The first-launch latch is a caller intent ("this update registers
            // the install value"), not something to infer from the resolved fine
            // value. A regular event that happens to resolve to fineValue 0 must
            // not flip it.
            nextState.FirstLaunchValueRegistered =
                state.FirstLaunchValueRegistered || markFirstLaunchValueRegistered;
            nextState.LastUpdatedAt = _clock().ToUniversalTime();

            if (state.FineValue == nextState.FineValue &&
                state.CoarseValue == nextState.CoarseValue &&
                state.LockWindow == nextState.LockWindow)
            {
                // The conversion value does not advance, so no native bridge call
                // is needed. Still persist a first-launch latch transition so the
                // install registration is not re-attempted on every launch.
                var shouldPersistLatch =
                    nextState.FirstLaunchValueRegistered != state.FirstLaunchValueRegistered;

                return new AttriaxSkanConversionUpdate(
                    BuildResult(
                        AttriaxSkanUpdateStatus.AlreadyAtOrAboveValue,
                        "The requested conversion value does not advance the stored SKAN state.",
                        state.FineValue,
                        state.CoarseValue,
                        state.LockWindow,
                        shouldPersistLatch ? nextState : state),
                    shouldPersistLatch ? nextState : null);
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
                return new AttriaxSkanConversionUpdate(
                    BuildResult(
                        bridgeResult.Status,
                        bridgeResult.Message,
                        nextState.FineValue,
                        nextState.CoarseValue,
                        nextState.LockWindow,
                        nextState),
                    nextState);
            }

            return new AttriaxSkanConversionUpdate(
                BuildResult(
                    bridgeResult.Status,
                    bridgeResult.Message,
                    bridgeResult.FineValue,
                    bridgeResult.CoarseValue,
                    bridgeResult.LockWindow,
                    state),
                null);
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

    internal sealed class AttriaxSkanConversionUpdate
    {
        internal AttriaxSkanConversionUpdate(AttriaxSkanUpdateResult result, AttriaxSkanState? nextState)
        {
            Result = result;
            NextState = nextState;
        }

        internal AttriaxSkanUpdateResult Result { get; }

        internal AttriaxSkanState? NextState { get; }
    }
}
