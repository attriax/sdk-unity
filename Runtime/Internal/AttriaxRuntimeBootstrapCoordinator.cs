#nullable enable
using System;
using System.Threading.Tasks;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxRuntimeBootstrapCoordinator
    {
        private readonly AttriaxRuntime _runtime;

        public AttriaxRuntimeBootstrapCoordinator(AttriaxRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public async Task RunAsync(AttriaxInitOptions options)
        {
            _runtime.BootstrapInitializeState(options);
            await _runtime.BootstrapPrepareContextAndSessionAsync(options).ConfigureAwait(false);

            if (!_runtime.Enabled)
            {
                _runtime.BootstrapCompleteDisabledState();
                return;
            }

            _runtime.BootstrapCompleteEnabledState(options);
        }
    }
}
