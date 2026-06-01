#nullable enable
using System;
using System.Threading.Tasks;

namespace Attriax.Unity.Internal
{
    internal readonly struct AttriaxPreparedContextRefresh
    {
        public AttriaxPreparedContextRefresh(
            AttriaxContextSnapshot initialSnapshot,
            Task<AttriaxContextSnapshot> resolvedSnapshotTask)
        {
            InitialSnapshot = initialSnapshot;
            ResolvedSnapshotTask = resolvedSnapshotTask;
        }

        public AttriaxContextSnapshot InitialSnapshot { get; }

        public Task<AttriaxContextSnapshot> ResolvedSnapshotTask { get; }
    }

    internal sealed class AttriaxContextManager
    {
        private readonly IAttriaxContextRefreshProvider _refreshProvider;
        private readonly Action<string, string?> _debugLog;

        private Task<AttriaxContextSnapshot>? _resolvedContextTask;
        private bool _resolvedContextIncludesInstallReferrer;
        private AttriaxContextSnapshot? _snapshot;
        private int _generation;

        public AttriaxContextManager(
            IAttriaxContextRefreshProvider refreshProvider,
            Action<string, string?> debugLog)
        {
            _refreshProvider = refreshProvider;
            _debugLog = debugLog;
        }

        public AttriaxContextSnapshot Snapshot =>
            _snapshot ?? throw new InvalidOperationException("Attriax context has not been initialized.");

        public void SetPreparedContext(
            AttriaxPreparedContextRefresh preparedContext,
            bool includesInstallReferrer)
        {
            var generation = _generation;
            _snapshot = preparedContext.InitialSnapshot;
            _resolvedContextTask = ResolveContextSnapshotSafelyAsync(
                preparedContext.ResolvedSnapshotTask,
                generation);
            _resolvedContextIncludesInstallReferrer = includesInstallReferrer;
        }

        public void Reset()
        {
            _generation += 1;
            _resolvedContextTask = null;
            _resolvedContextIncludesInstallReferrer = false;
            _snapshot = null;
        }

        public Task<AttriaxContextSnapshot> EnsureResolvedForAppOpenAsync()
        {
            if (_resolvedContextTask != null && _resolvedContextIncludesInstallReferrer)
            {
                return _resolvedContextTask;
            }

            _resolvedContextIncludesInstallReferrer = true;
            _resolvedContextTask = RefreshResolvedForAppOpenAsync();
            return _resolvedContextTask;
        }

        private async Task<AttriaxContextSnapshot> RefreshResolvedForAppOpenAsync()
        {
            var generation = _generation;
            var preparedContext = await _refreshProvider.PrepareContextRefreshAsync(true)
                .ConfigureAwait(false);

            if (generation != _generation)
            {
                return preparedContext.InitialSnapshot;
            }

            _snapshot = preparedContext.InitialSnapshot;
            return await ResolveContextSnapshotSafelyAsync(
                    preparedContext.ResolvedSnapshotTask,
                    generation)
                .ConfigureAwait(false);
        }

        private async Task<AttriaxContextSnapshot> ResolveContextSnapshotSafelyAsync(
            Task<AttriaxContextSnapshot> resolvedSnapshotTask,
            int generation)
        {
            try
            {
                var resolvedSnapshot = await resolvedSnapshotTask.ConfigureAwait(false);
                if (generation == _generation)
                {
                    _snapshot = resolvedSnapshot;
                }

                return resolvedSnapshot;
            }
            catch (Exception exception)
            {
                if (generation != _generation || _snapshot == null)
                {
                    throw;
                }

                _debugLog(
                    "Failed to resolve the background install-referrer context.",
                    exception.Message);
                return Snapshot;
            }
        }
    }
}