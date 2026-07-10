#nullable enable
using System;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Mirrors <c>AttriaxRuntimeSettingsState</c> in
    /// <c>sdk-flutter/attriax/lib/src/internal/attriax_runtime_settings_state.dart</c>.
    /// In-memory layer that fronts <see cref="AttriaxRuntimeSettingsStore"/> and
    /// keeps <see cref="AttriaxRuntimeState"/> in sync on writes.
    /// </summary>
    internal sealed class AttriaxRuntimeSettingsState
    {
        private readonly AttriaxRuntimeSettingsStore _store;
        private readonly AttriaxRuntimeState _runtimeState;

        public AttriaxRuntimeSettingsState(
            AttriaxRuntimeSettingsStore store,
            AttriaxRuntimeState runtimeState)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _runtimeState = runtimeState ?? throw new ArgumentNullException(nameof(runtimeState));
        }

        public bool IsEnabled => _runtimeState.IsEnabled;

        public bool AreEventsEnabled => _runtimeState.AreEventsEnabled;

        /// <summary>
        /// Seeds the in-memory toggles from persistent storage.
        /// </summary>
        public void RestoreFromStore(bool defaultEnabled = true, bool defaultEventsEnabled = true)
        {
            _runtimeState.IsEnabled = _store.ReadEnabled(defaultEnabled);
            _runtimeState.AreEventsEnabled = _store.ReadEventsEnabled(defaultEventsEnabled);
        }

        /// <summary>
        /// Writes the enabled flag through to memory and persistent storage.
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            _runtimeState.IsEnabled = enabled;
            _store.WriteEnabled(enabled);
        }

        /// <summary>
        /// Writes the events-enabled flag through to memory and persistent storage.
        /// </summary>
        public void SetEventsEnabled(bool enabled)
        {
            _runtimeState.AreEventsEnabled = enabled;
            _store.WriteEventsEnabled(enabled);
        }

        /// <summary>
        /// Reads the persisted "has-launched" flag.
        /// </summary>
        public bool ReadHasLaunched(bool defaultValue = false)
        {
            return _store.ReadHasLaunched(defaultValue);
        }

        /// <summary>
        /// Persists the "has-launched" flag.
        /// </summary>
        public void WriteHasLaunched(bool value)
        {
            _store.WriteHasLaunched(value);
        }

        /// <summary>
        /// Direct access to the underlying store for callers that still need
        /// device-id read/write or full clear semantics.
        /// </summary>
        public AttriaxRuntimeSettingsStore Store => _store;
    }
}
