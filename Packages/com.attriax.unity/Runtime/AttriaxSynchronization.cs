#nullable enable
using System;

namespace Attriax.Unity
{
    /// <summary>
    /// Convenience wrapper around synchronization state and subscriptions.
    /// </summary>
    public sealed class AttriaxSynchronization
    {
        private readonly Internal.AttriaxRuntime _runtime;

        internal AttriaxSynchronization(Internal.AttriaxRuntime runtime)
        {
            _runtime = runtime;
        }

        /// <summary>
        /// Current synchronization state.
        /// </summary>
        public AttriaxSynchronizationState State
        {
            get { return _runtime.SynchronizationState; }
        }

        /// <summary>
        /// Returns <see langword="true"/> once the queue is fully synchronized.
        /// </summary>
        public bool IsSynchronized
        {
            get { return State == AttriaxSynchronizationState.Synchronized; }
        }

        /// <summary>
        /// Subscribes to synchronization state changes.
        /// </summary>
        public IDisposable Subscribe(Action<AttriaxSynchronizationState> listener)
        {
            return _runtime.SubscribeToSynchronization(listener);
        }
    }
}