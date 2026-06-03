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
        private readonly object _gate = new object();

        private Task<AttriaxContextSnapshot>? _resolvedContextTask;
        private bool _resolvedContextIncludesInstallReferrer;
        private AttriaxContextSnapshot? _snapshot;
        private int _generation;

        public AttriaxContextManager(
            IAttriaxContextRefreshProvider refreshProvider)
        {
            _refreshProvider = refreshProvider;
        }

        public AttriaxContextSnapshot Snapshot =>
            GetSnapshotOrThrow();

        public void SetPreparedContext(
            AttriaxPreparedContextRefresh preparedContext,
            bool includesInstallReferrer)
        {
            lock (_gate)
            {
                var generation = _generation;
                _snapshot = preparedContext.InitialSnapshot;
                _resolvedContextTask = ResolveContextSnapshotSafelyAsync(
                    preparedContext.ResolvedSnapshotTask,
                    generation);
                _resolvedContextIncludesInstallReferrer = includesInstallReferrer;
            }
        }

        public void Reset()
        {
            lock (_gate)
            {
                _generation += 1;
                _resolvedContextTask = null;
                _resolvedContextIncludesInstallReferrer = false;
                _snapshot = null;
            }
        }

        public Task<AttriaxContextSnapshot> EnsureResolvedForAppOpenAsync()
        {
            lock (_gate)
            {
                if (_resolvedContextTask != null && _resolvedContextIncludesInstallReferrer)
                {
                    return _resolvedContextTask;
                }

                _resolvedContextIncludesInstallReferrer = true;
                _resolvedContextTask = RefreshResolvedForAppOpenAsync();
                return _resolvedContextTask;
            }
        }

        private async Task<AttriaxContextSnapshot> RefreshResolvedForAppOpenAsync()
        {
            int generation;
            lock (_gate)
            {
                generation = _generation;
            }

            var preparedContext = await _refreshProvider.PrepareContextRefreshAsync(true)
                .ConfigureAwait(false);

            lock (_gate)
            {
                if (generation != _generation)
                {
                    return preparedContext.InitialSnapshot;
                }

                _snapshot = preparedContext.InitialSnapshot;
            }

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
                lock (_gate)
                {
                    if (generation == _generation)
                    {
                        _snapshot = resolvedSnapshot;
                    }
                }

                return resolvedSnapshot;
            }
            catch (Exception exception)
            {
                lock (_gate)
                {
                    if (generation != _generation || _snapshot == null)
                    {
                        throw;
                    }

                    return _snapshot;
                }
            }
        }

        private AttriaxContextSnapshot GetSnapshotOrThrow()
        {
            lock (_gate)
            {
                return _snapshot ?? throw new InvalidOperationException("Attriax context has not been initialized.");
            }
        }
    }
}