#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Emits an automatic <c>page_view</c> for the scene the app launched into and for
    /// every subsequent active-scene change, when
    /// <see cref="AttriaxConfig.AutomaticSceneTracking"/> is enabled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Scene tracking is a <b>wrapper concern</b>: the shared KMP engine has no notion
    /// of a Unity scene, so no native binding can supply it. The managed C# engine used
    /// to own this hook; it was removed together with that engine in the native re-wrap
    /// (<c>refactor(unity)!: delete the managed C# engine</c>), which left
    /// <see cref="AttriaxConfig.AutomaticSceneTracking"/> as dead config on every
    /// platform. This type restores the behavior on top of the native engine, driving
    /// the same <c>recordPageView</c> command any manual call uses.
    /// </para>
    /// <para>
    /// The emitted payloads reproduce the pre-rewrap contract: <c>source</c> is always
    /// <c>automatic_scene</c>; the launch page view carries the scene <em>path</em> as
    /// <c>pageClass</c> (the only point where the full asset path is known) while a
    /// navigation carries the scene <em>name</em>; <c>pageTitle</c> is always the scene
    /// name.
    /// </para>
    /// <para>
    /// <b>Binding vs. emitting.</b> These are deliberately separate. The tracker binds
    /// at construction — capturing the launch scene and subscribing to
    /// <see cref="SceneManager.activeSceneChanged"/> — but the engine cannot accept a
    /// page view until <see cref="Start"/> is called after initialization completes.
    /// Scenes observed in between are buffered and replayed in order once the engine is
    /// up. Binding late instead would lose the launch page view (initialization finishes
    /// on a background worker, so by the time it marshals back the app may already have
    /// navigated) and would miss any navigation that raced initialization.
    /// </para>
    /// <para>
    /// <see cref="SceneManager"/> is main-thread-only, so every hop marshals through
    /// <see cref="AttriaxLifecycleDispatcher.PostToMainThread"/>, which runs inline when
    /// the caller is already on the main thread (the normal case for construction) and
    /// never blocks otherwise — see the DebugLog main-thread deadlock lesson. All state
    /// is therefore touched on the main thread only. The engine owns enabled/consent
    /// gating, so this type deliberately does not re-check those flags.
    /// </para>
    /// </remarks>
    internal sealed class AttriaxSceneTracker : IDisposable
    {
        /// <summary>The <c>source</c> stamped on every automatically emitted page view.</summary>
        internal const string SceneSource = "automatic_scene";

        private readonly IAttriaxEngine _engine;

        /// <summary>Scenes observed before the engine was ready, replayed in order by <see cref="Start"/>.</summary>
        private readonly List<PendingPageView> _pending = new List<PendingPageView>();

        private bool _live;
        private bool _disposed;
        private bool _subscribed;

        /// <summary>
        /// The last scene observed, used as <c>previousPageName</c> on the next
        /// navigation. Unity's <c>activeSceneChanged</c> reports the outgoing scene, but a
        /// single-mode load has already unloaded it by then, so the handler is routinely
        /// handed an invalid <see cref="Scene"/> with an empty name. Remembering the last
        /// observed name is the only reliable source for this field.
        /// </summary>
        private string? _lastPageName;

        public AttriaxSceneTracker(IAttriaxEngine engine)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            AttriaxLifecycleDispatcher.PostToMainThread(Bind);
        }

        /// <summary>
        /// Allows the tracker to emit, replaying everything observed while the engine was
        /// still initializing. Called once initialization completes; safe from any thread.
        /// </summary>
        public void Start()
        {
            AttriaxLifecycleDispatcher.PostToMainThread(GoLive);
        }

        /// <summary>
        /// Captures the launch scene and starts observing changes. Runs on the main
        /// thread; nothing is emitted until <see cref="Start"/>.
        /// </summary>
        private void Bind()
        {
            if (_disposed || _subscribed)
            {
                return;
            }

            SceneManager.activeSceneChanged += HandleActiveSceneChanged;
            _subscribed = true;

            // The scene the app launched into never raises activeSceneChanged, so it is
            // captured here or it would be missed entirely.
            var launchScene = SceneManager.GetActiveScene();
            if (!string.IsNullOrWhiteSpace(launchScene.name))
            {
                Observe(
                    pageName: launchScene.name,
                    pageClass: launchScene.path,
                    previousPageName: null);
            }
        }

        private void GoLive()
        {
            if (_disposed || _live)
            {
                return;
            }

            _live = true;
            foreach (var pending in _pending)
            {
                Record(pending);
            }

            _pending.Clear();
        }

        private void HandleActiveSceneChanged(Scene previousScene, Scene nextScene)
        {
            if (_disposed || string.IsNullOrWhiteSpace(nextScene.name))
            {
                return;
            }

            // Prefer what Unity reports; fall back to the last scene we observed when the
            // outgoing scene has already been unloaded (the common single-mode case).
            Observe(
                pageName: nextScene.name,
                pageClass: nextScene.name,
                previousPageName: string.IsNullOrWhiteSpace(previousScene.name)
                    ? _lastPageName
                    : previousScene.name);
        }

        private void Observe(string pageName, string? pageClass, string? previousPageName)
        {
            _lastPageName = pageName;
            var pageView = new PendingPageView(pageName, pageClass, previousPageName);

            if (!_live)
            {
                _pending.Add(pageView);
                return;
            }

            Record(pageView);
        }

        private void Record(PendingPageView pageView)
        {
            try
            {
                FireAndForget(_engine.RecordPageViewAsync(
                    pageView.PageName,
                    new AttriaxPageViewOptions
                    {
                        PageClass = pageView.PageClass,
                        PageTitle = pageView.PageName,
                        PreviousPageName = pageView.PreviousPageName,
                        Source = SceneSource,
                    }));
            }
            catch (Exception error)
            {
                // Automatic tracking must never break the app's scene loading.
                Debug.LogWarning(
                    "[Attriax] Automatic scene page-view tracking failed for scene '"
                    + pageView.PageName + "': " + error.Message);
            }
        }

        private static async void FireAndForget(Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "[Attriax] Automatic scene page-view tracking failed: " + exception.Message);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _pending.Clear();
            if (!_subscribed)
            {
                return;
            }

            _subscribed = false;
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
        }

        private readonly struct PendingPageView
        {
            public PendingPageView(string pageName, string? pageClass, string? previousPageName)
            {
                PageName = pageName;
                PageClass = pageClass;
                PreviousPageName = previousPageName;
            }

            public string PageName { get; }

            public string? PageClass { get; }

            public string? PreviousPageName { get; }
        }
    }
}
