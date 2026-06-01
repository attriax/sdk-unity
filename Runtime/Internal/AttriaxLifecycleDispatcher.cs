#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxLifecycleDispatcher : MonoBehaviour
    {
        private static AttriaxLifecycleDispatcher? _instance;
        private static readonly object MainThreadBindingGate = new object();
        private static SynchronizationContext? _mainThreadContext;
        private static int _mainThreadId = -1;

        public static event Action<float>? Tick;
        public static event Action<string>? DeepLinkActivated;
        public static event Action<string, string>? SceneChanged;
        public static event Action<bool>? ApplicationPaused;
        public static event Action<bool>? ApplicationFocusChanged;
        public static event Action<string, string, LogType>? UnhandledExceptionLogged;
        public static event Action? Quitting;

        public static AttriaxLifecycleDispatcher EnsureCreated()
        {
            BindToCurrentThread();

            if (_instance != null)
            {
                return _instance;
            }

            var existing = FindFirstObjectByType<AttriaxLifecycleDispatcher>();
            if (existing != null)
            {
                _instance = existing;
                return _instance;
            }

            var gameObject = new GameObject("AttriaxLifecycleDispatcher");
            DontDestroyOnLoad(gameObject);
            _instance = gameObject.AddComponent<AttriaxLifecycleDispatcher>();
            return _instance;
        }

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

        public static string InitialAbsoluteUrl
        {
            get { return Application.absoluteURL; }
        }

        private static bool IsMainThread
        {
            get { return Volatile.Read(ref _mainThreadId) == Thread.CurrentThread.ManagedThreadId; }
        }

        private void OnEnable()
        {
            BindToCurrentThread();
            Application.deepLinkActivated += HandleDeepLinkActivated;
            Application.logMessageReceivedThreaded += HandleLogMessageReceived;
            Application.quitting += HandleApplicationQuit;
            SceneManager.activeSceneChanged += HandleSceneChanged;
        }

        private void OnDisable()
        {
            Application.deepLinkActivated -= HandleDeepLinkActivated;
            Application.logMessageReceivedThreaded -= HandleLogMessageReceived;
            Application.quitting -= HandleApplicationQuit;
            SceneManager.activeSceneChanged -= HandleSceneChanged;
        }

        private void Update()
        {
            BindToCurrentThread();
            Tick?.Invoke(Time.unscaledDeltaTime);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            ApplicationPaused?.Invoke(pauseStatus);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            ApplicationFocusChanged?.Invoke(hasFocus);
        }

        private static void HandleDeepLinkActivated(string url)
        {
            DeepLinkActivated?.Invoke(url);
        }

        private static void HandleApplicationQuit()
        {
            Quitting?.Invoke();
        }

        private static void HandleLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Exception)
            {
                return;
            }

            UnhandledExceptionLogged?.Invoke(condition, stackTrace, type);
        }

        private static void HandleSceneChanged(Scene previousScene, Scene nextScene)
        {
            SceneChanged?.Invoke(previousScene.name, nextScene.name);
        }
    }

    internal enum AttriaxPlayerPrefsPersistenceMode
    {
        ConsentOnly,
        FullRuntime,
    }

    internal static class AttriaxPlayerPrefs
    {
        private static readonly object PersistenceGate = new object();
        private static readonly Dictionary<string, AttriaxPlayerPrefsPersistenceMode> RuntimeKeyModes =
            new Dictionary<string, AttriaxPlayerPrefsPersistenceMode>(StringComparer.Ordinal);
        private static readonly Dictionary<string, object> MemoryValues =
            new Dictionary<string, object>(StringComparer.Ordinal);

        public static void SetRuntimePersistenceMode(
            IEnumerable<string> runtimeKeys,
            AttriaxPlayerPrefsPersistenceMode mode)
        {
            var keys = NormalizeKeys(runtimeKeys);
            lock (PersistenceGate)
            {
                foreach (var key in keys)
                {
                    RuntimeKeyModes[key] = mode;
                }
            }

            if (mode == AttriaxPlayerPrefsPersistenceMode.FullRuntime)
            {
                foreach (var key in keys)
                {
                    SyncMemoryValueToPersistentStorage(key);
                }

                Save();
                return;
            }

            foreach (var key in keys)
            {
                RemovePersistedValue(key);
            }

            Save();
        }

        public static void ForgetRuntimeKeys(IEnumerable<string> runtimeKeys)
        {
            var keys = NormalizeKeys(runtimeKeys);
            lock (PersistenceGate)
            {
                foreach (var key in keys)
                {
                    RuntimeKeyModes.Remove(key);
                    MemoryValues.Remove(key);
                }
            }
        }

        public static bool HasKey(string key)
        {
            lock (PersistenceGate)
            {
                if (MemoryValues.ContainsKey(key))
                {
                    return true;
                }

                if (IsRuntimeKeyLockedToMemoryOnly(key))
                {
                    return false;
                }
            }

            return AttriaxLifecycleDispatcher.InvokeOnMainThread(() => PlayerPrefs.HasKey(key));
        }

        public static string GetString(string key, string defaultValue)
        {
            lock (PersistenceGate)
            {
                if (MemoryValues.TryGetValue(key, out var memoryValue) && memoryValue is string cachedValue)
                {
                    return cachedValue;
                }

                if (IsRuntimeKeyLockedToMemoryOnly(key))
                {
                    return defaultValue;
                }
            }

            var value = AttriaxLifecycleDispatcher.InvokeOnMainThread(() => PlayerPrefs.GetString(key, defaultValue));
            CachePersistentValueIfTracked(key, value, isPresent: HasPersistentKey(key));
            return value;
        }

        public static int GetInt(string key, int defaultValue)
        {
            lock (PersistenceGate)
            {
                if (MemoryValues.TryGetValue(key, out var memoryValue) && memoryValue is int cachedValue)
                {
                    return cachedValue;
                }

                if (IsRuntimeKeyLockedToMemoryOnly(key))
                {
                    return defaultValue;
                }
            }

            var value = AttriaxLifecycleDispatcher.InvokeOnMainThread(() => PlayerPrefs.GetInt(key, defaultValue));
            CachePersistentValueIfTracked(key, value, isPresent: HasPersistentKey(key));
            return value;
        }

        public static void SetString(string key, string value)
        {
            var shouldPersist = TrackWriteAndCheckPersistence(key, value);
            if (!shouldPersist)
            {
                RemovePersistedValue(key);
                return;
            }

            AttriaxLifecycleDispatcher.InvokeOnMainThread(() => PlayerPrefs.SetString(key, value));
        }

        public static void SetInt(string key, int value)
        {
            var shouldPersist = TrackWriteAndCheckPersistence(key, value);
            if (!shouldPersist)
            {
                RemovePersistedValue(key);
                return;
            }

            AttriaxLifecycleDispatcher.InvokeOnMainThread(() => PlayerPrefs.SetInt(key, value));
        }

        public static void DeleteKey(string key)
        {
            lock (PersistenceGate)
            {
                MemoryValues.Remove(key);
            }

            AttriaxLifecycleDispatcher.InvokeOnMainThread(() => PlayerPrefs.DeleteKey(key));
        }

        public static void Save()
        {
            AttriaxLifecycleDispatcher.InvokeOnMainThread(PlayerPrefs.Save);
        }

        private static bool TrackWriteAndCheckPersistence(string key, object value)
        {
            lock (PersistenceGate)
            {
                if (RuntimeKeyModes.ContainsKey(key))
                {
                    MemoryValues[key] = value;
                    return RuntimeKeyModes[key] == AttriaxPlayerPrefsPersistenceMode.FullRuntime;
                }
            }

            return true;
        }

        private static void CachePersistentValueIfTracked(string key, object value, bool isPresent)
        {
            if (!isPresent)
            {
                return;
            }

            lock (PersistenceGate)
            {
                if (RuntimeKeyModes.ContainsKey(key))
                {
                    MemoryValues[key] = value;
                }
            }
        }

        private static bool IsRuntimeKeyLockedToMemoryOnly(string key)
        {
            return RuntimeKeyModes.TryGetValue(key, out var mode) &&
                mode == AttriaxPlayerPrefsPersistenceMode.ConsentOnly;
        }

        private static bool HasPersistentKey(string key)
        {
            return AttriaxLifecycleDispatcher.InvokeOnMainThread(() => PlayerPrefs.HasKey(key));
        }

        private static void SyncMemoryValueToPersistentStorage(string key)
        {
            object memoryValue;
            lock (PersistenceGate)
            {
                if (!MemoryValues.TryGetValue(key, out memoryValue))
                {
                    return;
                }
            }

            switch (memoryValue)
            {
                case string stringValue:
                    AttriaxLifecycleDispatcher.InvokeOnMainThread(() => PlayerPrefs.SetString(key, stringValue));
                    break;
                case int intValue:
                    AttriaxLifecycleDispatcher.InvokeOnMainThread(() => PlayerPrefs.SetInt(key, intValue));
                    break;
            }
        }

        private static void RemovePersistedValue(string key)
        {
            AttriaxLifecycleDispatcher.InvokeOnMainThread(() => PlayerPrefs.DeleteKey(key));
        }

        private static IReadOnlyList<string> NormalizeKeys(IEnumerable<string> runtimeKeys)
        {
            var keys = new List<string>();
            foreach (var key in runtimeKeys)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                keys.Add(key);
            }

            return keys;
        }
    }
}