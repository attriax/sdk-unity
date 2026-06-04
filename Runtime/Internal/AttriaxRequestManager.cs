#nullable enable
using System;
using System.Threading.Tasks;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Thin indirection layer that mirrors Flutter's request manager shape.
    /// Unity still binds this to <see cref="AttriaxRequestQueue"/> until the
    /// synchronizer extraction is complete.
    /// </summary>
    internal sealed class AttriaxRequestManager
    {
        private AttriaxRequestQueue? _queue;

        public bool IsBound => _queue != null;

        public void BindQueue(AttriaxRequestQueue queue)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        }

        public Task<object> Enqueue(AttriaxQueuedRequest request)
        {
            var queue = _queue;
            if (queue == null)
            {
                return Task.FromException<object>(
                    new InvalidOperationException(
                        "Attriax request manager is not bound to a queue or synchronizer."));
            }

            return queue.Enqueue(request);
        }

        public void Clear(Exception error)
        {
            var queue = _queue;
            if (queue == null)
            {
                return;
            }

            queue.Clear(error);
        }

        public void RejectAll(Exception error)
        {
            var queue = _queue;
            if (queue == null)
            {
                return;
            }

            queue.RejectAll(error);
        }
    }
}
