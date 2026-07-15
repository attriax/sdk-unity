#nullable enable
using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using UnityEngine;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Static Unity main-thread marshaling helpers. The native engine owns lifecycle,
    /// persistence, and session tracking now, so this type carries no MonoBehaviour /
    /// PlayerPrefs surface — only the thread-marshaling primitives the engine platforms
    /// use to re-raise engine callbacks (which fan in on background threads) onto the
    /// Unity main thread.
    /// </summary>
    internal static class AttriaxLifecycleDispatcher
    {
        private static readonly object MainThreadBindingGate = new object();
        private static SynchronizationContext? _mainThreadContext;
        private static int _mainThreadId = -1;

        /// <summary>
        /// Binds Unity's real main thread at player startup, BEFORE any engine work can
        /// run. Without this the binding is whoever-calls-first: the engine platforms
        /// create/init the native engine on a background worker, so the worker would
        /// claim the main-thread slot with a null <see cref="SynchronizationContext"/>,
        /// and every later <see cref="PostToMainThread"/> would silently drop its action
        /// (engine sync-state + deep-link events never reach the SDK's public surface).
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BindMainThreadOnStartup() => BindToCurrentThread();

        /// <summary>
        /// Binds the current thread as the Unity main thread (capturing its
        /// <see cref="SynchronizationContext"/>) the first time it is called on that
        /// thread; every marshaling entry point calls this so binding happens lazily on
        /// the first main-thread interaction.
        /// <para>
        /// A thread with NO <see cref="SynchronizationContext"/> can never be Unity's
        /// main thread (the player installs UnitySynchronizationContext there), so such a
        /// caller is refused the binding — otherwise a background engine thread that
        /// happens to marshal first would capture the slot with a null context and wedge
        /// all main-thread delivery.
        /// </para>
        /// </summary>
        public static void BindToCurrentThread()
        {
            var currentThreadId = Thread.CurrentThread.ManagedThreadId;
            var currentContext = SynchronizationContext.Current;
            if (Volatile.Read(ref _mainThreadId) == currentThreadId)
            {
                if (_mainThreadContext == null && currentContext != null)
                {
                    lock (MainThreadBindingGate)
                    {
                        if (_mainThreadContext == null && Volatile.Read(ref _mainThreadId) == currentThreadId)
                        {
                            _mainThreadContext = currentContext;
                        }
                    }
                }

                return;
            }

            // A context-less thread is a background thread, never Unity's main thread —
            // refuse the binding rather than capture the slot with a null context.
            if (currentContext == null)
            {
                return;
            }

            lock (MainThreadBindingGate)
            {
                if (Volatile.Read(ref _mainThreadId) == -1)
                {
                    _mainThreadContext = currentContext;
                    Volatile.Write(ref _mainThreadId, currentThreadId);
                    return;
                }

                if (_mainThreadContext == null &&
                    Volatile.Read(ref _mainThreadId) == currentThreadId &&
                    currentContext != null)
                {
                    _mainThreadContext = currentContext;
                }
            }
        }

        public static void InvokeOnMainThread(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            BindToCurrentThread();
            if (IsMainThread)
            {
                action();
                return;
            }

            var context = _mainThreadContext;
            if (context == null)
            {
                throw new InvalidOperationException(
                    "Attriax could not access the Unity main thread. Initialize the runtime on the Unity thread before using background continuations.");
            }

            Exception? capturedException = null;
            using var completed = new ManualResetEventSlim(false);
            context.Post(
                _ =>
                {
                    try
                    {
                        BindToCurrentThread();
                        action();
                    }
                    catch (Exception exception)
                    {
                        capturedException = exception;
                    }
                    finally
                    {
                        completed.Set();
                    }
                },
                null);
            completed.Wait();

            if (capturedException != null)
            {
                ExceptionDispatchInfo.Capture(capturedException).Throw();
            }
        }

        public static T InvokeOnMainThread<T>(Func<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            BindToCurrentThread();
            if (IsMainThread)
            {
                return action();
            }

            var context = _mainThreadContext;
            if (context == null)
            {
                throw new InvalidOperationException(
                    "Attriax could not access the Unity main thread. Initialize the runtime on the Unity thread before using background continuations.");
            }

            T result = default!;
            Exception? capturedException = null;
            using var completed = new ManualResetEventSlim(false);
            context.Post(
                _ =>
                {
                    try
                    {
                        BindToCurrentThread();
                        result = action();
                    }
                    catch (Exception exception)
                    {
                        capturedException = exception;
                    }
                    finally
                    {
                        completed.Set();
                    }
                },
                null);
            completed.Wait();

            if (capturedException != null)
            {
                ExceptionDispatchInfo.Capture(capturedException).Throw();
            }

            return result;
        }

        // Fire-and-forget main-thread post for non-blocking work (e.g. debug logging).
        // Unlike InvokeOnMainThread it NEVER waits on a ManualResetEventSlim, so a
        // background thread holding a lock can post and return immediately without
        // risking a deadlock against the main thread contending for that same lock.
        // During teardown (no bound context) it no-ops safely instead of throwing,
        // because a logger must never crash or block its caller.
        public static void PostToMainThread(Action action)
        {
            if (action == null)
            {
                return;
            }

            BindToCurrentThread();
            if (IsMainThread)
            {
                action();
                return;
            }

            var context = _mainThreadContext;
            if (context == null)
            {
                return;
            }

            context.Post(_ =>
            {
                BindToCurrentThread();
                action();
            }, null);
        }

        private static bool IsMainThread
        {
            get { return Volatile.Read(ref _mainThreadId) == Thread.CurrentThread.ManagedThreadId; }
        }
    }
}
