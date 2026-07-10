#nullable enable
using System;
using System.Collections.Generic;
using GeneratedUninstallTokenProvider = Attriax.Unity.Generated.Model.AppUserUninstallTokenProvider;
using SdkRegisterUninstallTokenDto = Attriax.Unity.Generated.Model.SdkRegisterUninstallTokenDto;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Builds and enqueues uninstall-tracking (push) token registrations. Mirrors
    /// the Flutter reference's <c>attriax_uninstall_token_registrar.dart</c>: the
    /// consent gate and request construction for FCM/APNs tokens live here so they
    /// can be read and tested without the full runtime. The runtime keeps the
    /// public entrypoints and the initialized/enabled lifecycle checks.
    /// </summary>
    internal sealed class AttriaxUninstallTokenRegistrar
    {
        private readonly string _projectToken;
        private readonly IAttriaxConsentReadView _consent;
        private readonly Func<string?> _getDeviceId;
        private readonly Func<string> _requireDeviceIdSource;
        private readonly Func<AttriaxPlatformType> _getPlatform;
        private readonly Action<SdkRegisterUninstallTokenDto> _enqueue;

        internal AttriaxUninstallTokenRegistrar(
            string projectToken,
            IAttriaxConsentReadView consent,
            Func<string?> getDeviceId,
            Func<string> requireDeviceIdSource,
            Func<AttriaxPlatformType> getPlatform,
            Action<SdkRegisterUninstallTokenDto> enqueue)
        {
            _projectToken = projectToken;
            _consent = consent;
            _getDeviceId = getDeviceId;
            _requireDeviceIdSource = requireDeviceIdSource;
            _getPlatform = getPlatform;
            _enqueue = enqueue;
        }

        internal void Register(
            GeneratedUninstallTokenProvider provider,
            string? token,
            IDictionary<string, object>? metadata)
        {
            if (!_consent.CanCaptureUninstallTracking)
            {
                return;
            }

            var request = AttriaxGeneratedRequestFactory.BuildRegisterUninstallTokenRequest(
                _projectToken,
                _getDeviceId(),
                _requireDeviceIdSource(),
                _getPlatform(),
                provider,
                string.IsNullOrWhiteSpace(token) ? null : token.Trim(),
                metadata);

            _enqueue(request);
        }
    }
}
