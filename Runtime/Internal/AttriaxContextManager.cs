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
            _debugLog(
                "Stored prepared context snapshot.",
                "generation=" + generation
                + ", includesInstallReferrer=" + includesInstallReferrer
                + ", platform=" + preparedContext.InitialSnapshot.Platform
                + ", resolvedTaskStatus=" + DescribeTaskStatus(_resolvedContextTask));
        }

        public void Reset()
        {
            _generation += 1;
            _resolvedContextTask = null;
            _resolvedContextIncludesInstallReferrer = false;
            _snapshot = null;
            _debugLog("Reset context manager state.", "generation=" + _generation);
        }

        public Task<AttriaxContextSnapshot> EnsureResolvedForAppOpenAsync()
        {
            if (_resolvedContextTask != null && _resolvedContextIncludesInstallReferrer)
            {
                _debugLog(
                    "Reusing existing resolved context task for app-open.",
                    "generation=" + _generation
                    + ", status=" + DescribeTaskStatus(_resolvedContextTask));
                return _resolvedContextTask;
            }

            _debugLog(
                "Refreshing context for app-open install-referrer resolution.",
                "generation=" + _generation
                + ", hadTask=" + (_resolvedContextTask != null)
                + ", includesInstallReferrer=" + _resolvedContextIncludesInstallReferrer);
            _resolvedContextIncludesInstallReferrer = true;
            _resolvedContextTask = RefreshResolvedForAppOpenAsync();
            return _resolvedContextTask;
        }

        private async Task<AttriaxContextSnapshot> RefreshResolvedForAppOpenAsync()
        {
            var generation = _generation;
            _debugLog(
                "Preparing refreshed context for app-open.",
                "generation=" + generation);
            var preparedContext = await _refreshProvider.PrepareContextRefreshAsync(true)
                .ConfigureAwait(false);
            _debugLog(
                "Prepared refreshed context for app-open.",
                "generation=" + generation
                + ", platform=" + preparedContext.InitialSnapshot.Platform
                + ", resolvedTaskStatus=" + DescribeTaskStatus(preparedContext.ResolvedSnapshotTask));

            if (generation != _generation)
            {
                _debugLog(
                    "Discarding prepared app-open context because the generation changed.",
                    "preparedGeneration=" + generation + ", currentGeneration=" + _generation);
                return preparedContext.InitialSnapshot;
            }

            _snapshot = preparedContext.InitialSnapshot;
            _debugLog(
                "Awaiting resolved app-open context snapshot.",
                "generation=" + generation);
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
                _debugLog(
                    "Waiting for resolved context snapshot task.",
                    "generation=" + generation
                    + ", taskStatus=" + DescribeTaskStatus(resolvedSnapshotTask));
                var resolvedSnapshot = await resolvedSnapshotTask.ConfigureAwait(false);
                if (generation == _generation)
                {
                    _snapshot = resolvedSnapshot;
                }

                _debugLog(
                    "Resolved context snapshot task completed.",
                    "generation=" + generation
                    + ", currentGeneration=" + _generation
                    + ", platform=" + resolvedSnapshot.Platform);

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

        private static string DescribeTaskStatus(Task? task)
        {
            return task == null ? "null" : task.Status.ToString();
        }
    }
}