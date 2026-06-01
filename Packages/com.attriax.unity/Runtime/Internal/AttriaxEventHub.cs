#nullable enable
using System;
using System.Collections.Generic;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxEventHub
    {
        private readonly List<Action<AttriaxRawDeepLinkEvent>> _rawDeepLinkListeners = new List<Action<AttriaxRawDeepLinkEvent>>();
        private readonly List<Action<AttriaxDeepLinkEvent>> _deepLinkListeners = new List<Action<AttriaxDeepLinkEvent>>();
        private readonly List<Action<AttriaxSynchronizationState>> _synchronizationListeners = new List<Action<AttriaxSynchronizationState>>();

        public AttriaxSynchronizationState SynchronizationState { get; private set; } = AttriaxSynchronizationState.Initializing;

        public IDisposable SubscribeToRawDeepLinks(Action<AttriaxRawDeepLinkEvent> listener)
        {
            _rawDeepLinkListeners.Add(listener);
            return new AttriaxSubscription(() => _rawDeepLinkListeners.Remove(listener));
        }

        public IDisposable SubscribeToDeepLinks(Action<AttriaxDeepLinkEvent> listener)
        {
            _deepLinkListeners.Add(listener);
            return new AttriaxSubscription(() => _deepLinkListeners.Remove(listener));
        }

        public IDisposable SubscribeToSynchronization(Action<AttriaxSynchronizationState> listener)
        {
            _synchronizationListeners.Add(listener);
            return new AttriaxSubscription(() => _synchronizationListeners.Remove(listener));
        }

        public void EmitRawDeepLinkEvent(AttriaxRawDeepLinkEvent deepLinkEvent)
        {
            var listeners = _rawDeepLinkListeners.ToArray();
            AttriaxLifecycleDispatcher.InvokeOnMainThread(() =>
            {
                for (var index = 0; index < listeners.Length; index += 1)
                {
                    listeners[index](deepLinkEvent);
                }
            });
        }

        public void EmitDeepLinkEvent(AttriaxDeepLinkEvent deepLinkEvent)
        {
            var listeners = _deepLinkListeners.ToArray();
            AttriaxLifecycleDispatcher.InvokeOnMainThread(() =>
            {
                for (var index = 0; index < listeners.Length; index += 1)
                {
                    listeners[index](deepLinkEvent);
                }
            });
        }

        public void SetSynchronizationState(AttriaxSynchronizationState state)
        {
            var listeners = _synchronizationListeners.ToArray();
            AttriaxLifecycleDispatcher.InvokeOnMainThread(() =>
            {
                SynchronizationState = state;
                for (var index = 0; index < listeners.Length; index += 1)
                {
                    listeners[index](state);
                }
            });
        }

        public void Reset()
        {
            SynchronizationState = AttriaxSynchronizationState.Initializing;
        }

        public void Clear()
        {
            _rawDeepLinkListeners.Clear();
            _deepLinkListeners.Clear();
            _synchronizationListeners.Clear();
        }
    }
}