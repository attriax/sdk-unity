#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Attriax.Unity.Generated.Client;
using GeneratedApiException = Attriax.Unity.Generated.Client.ApiException;
using GeneratedAttributionType = Attriax.Unity.Generated.Model.AttributionType;
using GeneratedConfiguration = Attriax.Unity.Generated.Client.Configuration;
using GeneratedConnectionException = Attriax.Unity.Generated.Client.ConnectionException;
using GeneratedDeepLinkResolutionStatus = Attriax.Unity.Generated.Model.DeepLinkResolutionStatus;
using GeneratedDynamicLinkRecordDto = Attriax.Unity.Generated.Model.SdkDynamicLinkRecordDto;
using GeneratedInstallReferrerResultDto = Attriax.Unity.Generated.Model.SdkInstallReferrerResultDto;
using GeneratedInstallState = Attriax.Unity.Generated.Model.SdkInstallState;
using GeneratedJsonDeepLinkDto = Attriax.Unity.Generated.Model.SdkJsonDeepLinkDto;
using GeneratedPlatform = Attriax.Unity.Generated.Model.Platform;
using GeneratedRevenueReceiptValidateStatus = Attriax.Unity.Generated.Model.SdkRevenueReceiptValidateResponseDto.StatusEnum;
using GeneratedRouteUrlOpenMode = Attriax.Unity.Generated.Model.RouteUrlOpenMode;
using GeneratedSdkApi = Attriax.Unity.Generated.Api.SdkApi;
using GeneratedUnexpectedResponseException = Attriax.Unity.Generated.Client.UnexpectedResponseException;
using SdkBatchItemKind = Attriax.Unity.Generated.Model.SdkBatchItemKind;
using SdkCrashDto = Attriax.Unity.Generated.Model.SdkCrashDto;
using SdkCreateDynamicLinkDto = Attriax.Unity.Generated.Model.SdkCreateDynamicLinkDto;
using SdkCreateDynamicLinkResponseDto = Attriax.Unity.Generated.Model.SdkCreateDynamicLinkResponseDto;
using SdkEventDto = Attriax.Unity.Generated.Model.SdkEventDto;
using SdkNotificationDto = Attriax.Unity.Generated.Model.SdkNotificationDto;
using SdkGdprConsentStatusDto = Attriax.Unity.Generated.Model.SdkGdprConsentStatusDto;
using SdkRegisterUninstallTokenDto = Attriax.Unity.Generated.Model.SdkRegisterUninstallTokenDto;
using SdkRevenueReceiptValidateResponseDto = Attriax.Unity.Generated.Model.SdkRevenueReceiptValidateResponseDto;
using SdkSessionDto = Attriax.Unity.Generated.Model.SdkSessionDto;
using SdkUserDto = Attriax.Unity.Generated.Model.SdkUserDto;
using SdkV1BatchDto = Attriax.Unity.Generated.Model.SdkV1BatchDto;
using SdkV1BatchItemDto = Attriax.Unity.Generated.Model.SdkV1BatchItemDto;
using SdkV1ConfigDto = Attriax.Unity.Generated.Model.SdkV1ConfigDto;
using SdkV1DeepLinkResolveDto = Attriax.Unity.Generated.Model.SdkV1DeepLinkResolveDto;
using SdkV1DeepLinkResolveResponseDto = Attriax.Unity.Generated.Model.SdkV1DeepLinkResolveResponseDto;
using SdkV1GdprConsentCheckDto = Attriax.Unity.Generated.Model.SdkV1GdprConsentCheckDto;
using SdkV1GdprDataEraseDto = Attriax.Unity.Generated.Model.SdkV1GdprDataEraseDto;
using SdkV1GdprConsentWriteDto = Attriax.Unity.Generated.Model.SdkV1GdprConsentWriteDto;
using SdkV1OpenDto = Attriax.Unity.Generated.Model.SdkV1OpenDto;
using SdkV1OpenResponseDto = Attriax.Unity.Generated.Model.SdkV1OpenResponseDto;
using SdkV1RevenueReceiptValidateDto = Attriax.Unity.Generated.Model.SdkV1RevenueReceiptValidateDto;
using SdkSkanRuntimeConfigurationDto = Attriax.Unity.Generated.Model.SdkV1SkanRuntimeConfigurationDto;
using SdkV1UnityEditorValidateDto = Attriax.Unity.Generated.Model.SdkV1UnityEditorValidateDto;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxGeneratedGateway : IDisposable, IAttriaxGdprConsentGateway
    {
        private static readonly JsonSerializerSettings BatchSerializerSettings = new JsonSerializerSettings
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    OverrideSpecifiedNames = false,
                },
            },
        };

        // Stable fallback used only if the runtime ever hands the gateway an empty
        // User-Agent. It must never be the OpenAPI generator default — see below.
        private const string FallbackUserAgent = "attriax-unity-sdk (Unity unknown; unity_unknown)";

        private readonly GeneratedSdkApi _sdkApi;

        public AttriaxGeneratedGateway(string apiBaseUrl, int requestTimeoutMs, string? userAgent = null)
        {
            // Resolve the User-Agent up front so EVERY request this single, long-lived
            // gateway emits — including the very first bootstrap/config call — carries the
            // SAME Attriax UA. This is load-bearing for stable anonymous identity: the
            // backend hashes anonymous users on appId+ip+userAgent+dailySalt, so any UA
            // drift across a run mints multiple anonymous users for one player (parity with
            // sdk-js, which relies on one stable fetch client and never varies the UA).
            UserAgent = string.IsNullOrWhiteSpace(userAgent) ? FallbackUserAgent : userAgent;

            var configuration = new GeneratedConfiguration
            {
                BasePath = BuildGeneratedApiBaseUrl(apiBaseUrl),
                Timeout = TimeSpan.FromMilliseconds(requestTimeoutMs),
            };

            // ALWAYS override the OpenAPI-generated default User-Agent. The generated default
            // ("OpenAPI-Generator/<v>/csharp", URL-encoded) is classified as a bot by the
            // backend's `isbot` user-agent check, which flagged every Unity Editor/native
            // request as BOT traffic. It must never reach the wire — not even on the first
            // request — so we unconditionally replace it (no "only when non-empty" guard,
            // which previously let the generator default leak when no UA was supplied). A
            // descriptive, non-bot Attriax UA keeps native runs human-classified (parity
            // with Flutter/native, which never advertise the generator UA). WebGL builds do
            // not set this header at all (the browser UA is used), so this only affects
            // Editor + native players.
            configuration.UserAgent = UserAgent;

            _sdkApi = new GeneratedSdkApi(configuration);
        }

        /// <summary>
        /// The exact User-Agent every outbound request from this gateway carries (on
        /// Editor + native players). Surfaced so the gated outbound-payload trace can print
        /// it, making any UA drift visible in a single Editor run.
        /// </summary>
        public string UserAgent { get; }

        public void Dispose()
        {
            _sdkApi.Dispose();
        }

        public Task<AttriaxAppOpenResult> SendOpenAsync(SdkV1OpenDto request)
        {
            var requestOptions = BuildRequestOptions(request);
            return ExecuteMappedAsync(
                () => _sdkApi.AsynchronousClient.PostAsync<JObject>(
                    "/api/sdk/v1/open",
                    requestOptions,
                    _sdkApi.Configuration),
                response => MapAppOpenResultEnvelope(response.Data));
        }

        public Task SendTrackEventAsync(SdkEventDto request)
        {
            return ExecuteCommandAsync(() => _sdkApi.SdkControllerRecordEventV1Async(request));
        }

        public Task SendTrackSessionAsync(SdkSessionDto request)
        {
            return ExecuteCommandAsync(() => _sdkApi.SdkControllerRecordSessionV1Async(request));
        }

        public Task SendSetUserAsync(SdkUserDto request)
        {
            return ExecuteCommandAsync(() => _sdkApi.SdkControllerSetUserV1Async(request));
        }

        public Task SendTrackCrashAsync(AttriaxCrashRequest request)
        {
            return ExecuteCommandAsync(() => _sdkApi.SdkControllerRecordCrashV1Async(MapCrashRequest(request)));
        }

        public Task SendTrackNotificationAsync(SdkNotificationDto request)
        {
            return ExecuteCommandAsync(() => _sdkApi.SdkControllerRecordNotificationV1Async(request));
        }

        public async Task SendBatchAsync(
            IReadOnlyList<AttriaxQueuedRequest> entries,
            int maxItemCount,
            int maxBodyBytes)
        {
            var request = BuildBatchRequest(entries);
            if (!FitsBatch(entries, maxItemCount, maxBodyBytes, request))
            {
                throw new AttriaxApiError(
                    "Attriax batch request exceeds the supported payload size.",
                    413,
                    false,
                    true);
            }

                await ExecuteCommandAsync(
                    () => _sdkApi.SdkControllerBatchV1Async(request))
                .ConfigureAwait(false);
        }

        public bool FitsBatch(
            IReadOnlyList<AttriaxQueuedRequest> entries,
            int maxItemCount,
            int maxBodyBytes)
        {
            var request = BuildBatchRequest(entries);
            return FitsBatch(entries, maxItemCount, maxBodyBytes, request);
        }

        public Task<AttriaxDeepLinkResolutionResultInternal> SendDeepLinkResolutionAsync(SdkV1DeepLinkResolveDto request)
        {
            return ExecuteMappedAsync(
                () => _sdkApi.SdkControllerResolveDeepLinkV1Async(request),
                envelope => MapDeepLinkResolutionResult(envelope.data));
        }

        public Task<AttriaxCreateDynamicLinkResult> SendCreateDynamicLinkAsync(SdkCreateDynamicLinkDto request)
        {
            return ExecuteMappedAsync(
                () => _sdkApi.SdkControllerCreateDynamicLinkV1Async(request),
                envelope => MapDynamicLinkCreateResult(envelope.data));
        }

        public Task SendRegisterUninstallTokenAsync(SdkRegisterUninstallTokenDto request)
        {
            return ExecuteCommandAsync(() => _sdkApi.SdkControllerRegisterUninstallTokenV1Async(request));
        }

        public Task<SdkGdprConsentStatusDto> CheckGdprConsentAsync(SdkV1GdprConsentCheckDto request)
        {
            return ExecuteMappedAsync(
                () => _sdkApi.SdkControllerCheckGdprConsentV1Async(request),
                envelope => envelope.data);
        }

        public Task<AttriaxSdkRuntimeConfig> FetchSdkRuntimeConfigAsync(SdkV1ConfigDto request)
        {
            var requestOptions = BuildRequestOptions(request);
            return ExecuteMappedAsync(
                () => _sdkApi.AsynchronousClient.PostAsync<JObject>(
                    "/api/sdk/v1/config",
                    requestOptions,
                    _sdkApi.Configuration),
                response => MapRuntimeConfigEnvelope(response.Data));
        }

        public Task<SdkGdprConsentStatusDto> UpsertGdprConsentAsync(SdkV1GdprConsentWriteDto request)
        {
            return ExecuteMappedAsync(
                () => _sdkApi.SdkControllerUpsertGdprConsentV1Async(request),
                envelope => envelope.data);
        }

        public Task EraseGdprDataAsync(SdkV1GdprDataEraseDto request)
        {
            return ExecuteCommandAsync(() => _sdkApi.SdkControllerEraseGdprDataV1Async(request));
        }

        public Task<AttriaxRevenueReceiptValidationResult> SendValidateReceiptAsync(
            SdkV1RevenueReceiptValidateDto request)
        {
            return ExecuteMappedAsync(
                () => _sdkApi.SdkControllerValidateReceiptV1Async(request),
                envelope => MapRevenueReceiptValidationResult(envelope.data));
        }

        public Task SendValidateUnityEditorAsync(SdkV1UnityEditorValidateDto request)
        {
            return ExecuteCommandAsync(() => _sdkApi.SdkControllerValidateUnityEditorV1Async(request));
        }

        public Task<long?> ConvertRevenueToUsdMicrosAsync(
            string projectToken,
            long amountMicros,
            string currency,
            DateTimeOffset clientOccurredAt)
        {
            var requestOptions = new RequestOptions
            {
                Data = new Dictionary<string, object>
                {
                    ["projectToken"] = projectToken,
                    ["currency"] = currency,
                    ["amountMicros"] = amountMicros.ToString(CultureInfo.InvariantCulture),
                    ["clientOccurredAt"] = clientOccurredAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                },
            };

            return ExecuteMappedAsync(
                () => _sdkApi.AsynchronousClient.PostAsync<JObject>(
                    "/api/sdk/v1/revenue/convert-to-usd",
                    requestOptions,
                    _sdkApi.Configuration),
                response => MapRevenueUsdMicros(response.Data));
        }

        private static string BuildGeneratedApiBaseUrl(string apiBaseUrl)
        {
            if (apiBaseUrl.EndsWith("/api/sdk", StringComparison.OrdinalIgnoreCase))
            {
                return apiBaseUrl.Substring(0, apiBaseUrl.Length - "/api/sdk".Length);
            }

            if (apiBaseUrl.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                return apiBaseUrl.Substring(0, apiBaseUrl.Length - "/api".Length);
            }

            return apiBaseUrl;
        }

        private static bool FitsBatch(
            IReadOnlyList<AttriaxQueuedRequest> entries,
            int maxItemCount,
            int maxBodyBytes,
            SdkV1BatchDto request)
        {
            var serializedRequest = JsonConvert.SerializeObject(request, BatchSerializerSettings);
            var requestBytes = Encoding.UTF8.GetByteCount(serializedRequest);
            return entries.Count <= maxItemCount && requestBytes <= maxBodyBytes;
        }

        private static RequestOptions BuildRequestOptions(object request)
        {
            return new RequestOptions
            {
                Data = request,
            };
        }

        private static SdkV1BatchDto BuildBatchRequest(IReadOnlyList<AttriaxQueuedRequest> entries)
        {
            if (entries.Count == 0)
            {
                throw new ArgumentException("Batch requests are required.", nameof(entries));
            }

            var sharedIdentity = RequireBatchIdentity(entries[0]);
            var items = new List<SdkV1BatchItemDto>(entries.Count);
            for (var i = 0; i < entries.Count; i += 1)
            {
                var entry = entries[i];
                if (!CanShareBatchIdentity(sharedIdentity, entry))
                {
                    throw new AttriaxApiError(
                        "Attriax batch entries must share the same project token and device identity.",
                        400,
                        false,
                        true);
                }

                items.Add(new SdkV1BatchItemDto(
                    body: RequireBatchBody(entry),
                    kind: MapBatchKind(entry.Kind)));
            }

            return new SdkV1BatchDto(
                projectToken: sharedIdentity.ProjectToken,
                deviceId: sharedIdentity.DeviceId,
                deviceIdSource: sharedIdentity.DeviceIdSource,
                items: items,
                requestId: BuildBatchRequestId(entries[0].Id));
        }

        private static SdkBatchItemKind MapBatchKind(AttriaxQueuedRequestKind kind)
        {
            switch (kind)
            {
                case AttriaxQueuedRequestKind.Event:
                    return SdkBatchItemKind.Event;
                case AttriaxQueuedRequestKind.Session:
                    return SdkBatchItemKind.Session;
                case AttriaxQueuedRequestKind.User:
                    return SdkBatchItemKind.User;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported batch request kind.");
            }
        }

        private static Dictionary<string, Object> RequireBatchBody(AttriaxQueuedRequest entry)
        {
            switch (entry.Kind)
            {
                case AttriaxQueuedRequestKind.Event:
                    return StripSharedBatchIdentity(SerializeBatchBody(entry.RequireEventRequest()));
                case AttriaxQueuedRequestKind.Session:
                    return StripSharedBatchIdentity(SerializeBatchBody(entry.RequireSessionRequest()));
                case AttriaxQueuedRequestKind.User:
                    return StripSharedBatchIdentity(SerializeBatchBody(entry.RequireUserRequest()));
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry.Kind, "Unsupported batch request kind.");
            }
        }

        private static BatchIdentity RequireBatchIdentity(AttriaxQueuedRequest entry)
        {
            switch (entry.Kind)
            {
                case AttriaxQueuedRequestKind.Event:
                {
                    var request = entry.RequireEventRequest();
                    return new BatchIdentity(request.projectToken, request.deviceId, request.deviceIdSource);
                }
                case AttriaxQueuedRequestKind.Session:
                {
                    var request = entry.RequireSessionRequest();
                    return new BatchIdentity(request.projectToken, request.deviceId, request.deviceIdSource);
                }
                case AttriaxQueuedRequestKind.User:
                {
                    var request = entry.RequireUserRequest();
                    return new BatchIdentity(request.projectToken, request.deviceId, request.deviceIdSource);
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry.Kind, "Unsupported batch request kind.");
            }
        }

        private static bool CanShareBatchIdentity(BatchIdentity identity, AttriaxQueuedRequest entry)
        {
            var otherIdentity = RequireBatchIdentity(entry);
            return string.Equals(identity.ProjectToken, otherIdentity.ProjectToken, StringComparison.Ordinal)
                && string.Equals(identity.DeviceId, otherIdentity.DeviceId, StringComparison.Ordinal)
                && string.Equals(identity.DeviceIdSource, otherIdentity.DeviceIdSource, StringComparison.Ordinal);
        }

        private static Dictionary<string, Object> StripSharedBatchIdentity(Dictionary<string, Object> body)
        {
            body.Remove("projectToken");
            body.Remove("deviceId");
            body.Remove("deviceIdSource");
            return body;
        }

        private static string BuildBatchRequestId(string entryId)
        {
            return string.Format(CultureInfo.InvariantCulture, "batch_{0}", entryId);
        }

        private static Dictionary<string, Object> SerializeBatchBody(object request)
        {
            return JObject.FromObject(request, JsonSerializer.Create(BatchSerializerSettings))
                    .ToObject<Dictionary<string, Object>>(JsonSerializer.Create(BatchSerializerSettings))
                ?? new Dictionary<string, Object>();
        }

        private sealed class BatchIdentity
        {
            public BatchIdentity(string projectToken, string deviceId, string deviceIdSource)
            {
                ProjectToken = projectToken;
                DeviceId = deviceId;
                DeviceIdSource = deviceIdSource;
            }

            public string ProjectToken { get; }

            public string DeviceId { get; }

            public string DeviceIdSource { get; }
        }

        private static async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> execute)
        {
            var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            AttriaxLifecycleDispatcher.PostToMainThread(async () =>
            {
                try
                {
                    TResult result = await execute().ConfigureAwait(false);
                    tcs.TrySetResult(result);
                }
                catch (GeneratedApiException exception)
                {
                    tcs.TrySetException(NormalizeGeneratedApiException(exception));
                }
                catch (GeneratedConnectionException exception)
                {
                    tcs.TrySetException(NormalizeGeneratedConnectionException(exception));
                }
                catch (GeneratedUnexpectedResponseException exception)
                {
                    tcs.TrySetException(NormalizeGeneratedUnexpectedResponseException(exception));
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            });
            return await tcs.Task.ConfigureAwait(false);
        }

        private static async Task ExecuteCommandAsync<TResult>(Func<Task<TResult>> execute)
        {
            await ExecuteAsync(execute).ConfigureAwait(false);
        }

        private static async Task<TMappedResult> ExecuteMappedAsync<TEnvelope, TMappedResult>(
            Func<Task<TEnvelope>> execute,
            Func<TEnvelope, TMappedResult> map)
        {
            var envelope = await ExecuteAsync(execute).ConfigureAwait(false);
            return map(envelope);
        }

        private static AttriaxApiError NormalizeGeneratedApiException(GeneratedApiException exception)
        {
            if (exception.ErrorCode > 0)
            {
                return BuildApiFailure(
                    exception.ErrorCode,
                    SerializeGeneratedErrorContent(exception.ErrorContent),
                    exception.Headers);
            }

            return new AttriaxApiError(
                string.IsNullOrWhiteSpace(exception.Message) ? "Attriax API request failed." : exception.Message,
                null,
                false,
                true,
                exception);
        }

        private static AttriaxApiError NormalizeGeneratedConnectionException(GeneratedConnectionException exception)
        {
            return new AttriaxApiError(
                BuildConnectionFailureMessage(exception.Error),
                null,
                true,
                false,
                exception);
        }

        private static string BuildConnectionFailureMessage(string? transportError)
        {
            var normalized = string.IsNullOrWhiteSpace(transportError)
                ? null
                : transportError.Trim();

            if (!string.IsNullOrWhiteSpace(normalized))
            {
                return normalized + " Attriax did not receive an HTTP response. This usually means the API is unreachable or blocked. On WebGL builds, also check HTTPS and browser preflight/CORS handling.";
            }

            return "Attriax request failed before the server returned a response. This usually means the API is unreachable or blocked. On WebGL builds, also check HTTPS and browser preflight/CORS handling.";
        }

        private static AttriaxApiError NormalizeGeneratedUnexpectedResponseException(
            GeneratedUnexpectedResponseException exception)
        {
            return new AttriaxApiError(
                "Attriax API returned an unexpected response.",
                exception.ErrorCode > 0 ? (int?)exception.ErrorCode : null,
                false,
                true,
                exception);
        }

        private static string SerializeGeneratedErrorContent(object errorContent)
        {
            if (errorContent == null)
            {
                return string.Empty;
            }

            if (errorContent is string text)
            {
                return text;
            }

            if (errorContent is JToken token)
            {
                return token.ToString(Formatting.None);
            }

            try
            {
                return JsonConvert.SerializeObject(errorContent);
            }
            catch (JsonException)
            {
                return errorContent.ToString();
            }
        }

        private static AttriaxApiError BuildApiFailure(
            long statusCode,
            string payloadText,
            Multimap<string, string>? headers)
        {
            // Retry rate-limiting (429), all 5xx, plus the two 4xx codes that are
            // explicitly transient: 408 (Request Timeout) and 425 (Too Early).
            // Every other 4xx is a client error and is dropped.
            var retriable = statusCode == 408 ||
                statusCode == 425 ||
                statusCode == 429 ||
                statusCode >= 500;
            var message = string.Format(CultureInfo.InvariantCulture, "Attriax API request failed with status {0}.", statusCode);

            if (!string.IsNullOrWhiteSpace(payloadText))
            {
                try
                {
                    var parsed = JObject.Parse(payloadText);
                    var parsedMessage = parsed.Value<string>("message");
                    if (!string.IsNullOrWhiteSpace(parsedMessage))
                    {
                        message = parsedMessage;
                    }
                }
                catch (JsonException)
                {
                    message = string.Format(CultureInfo.InvariantCulture, "Attriax API returned an unexpected response ({0}).", statusCode);
                }
            }

            return new AttriaxApiError(
                message,
                (int)statusCode,
                retriable,
                !retriable,
                retryAfterAt: TryParseRetryAfterAt(headers));
        }

        private static DateTimeOffset? TryParseRetryAfterAt(Multimap<string, string>? headers)
        {
            if (headers == null)
            {
                return null;
            }

            IList<string>? values = null;
            foreach (var pair in headers)
            {
                if (string.Equals(pair.Key, "Retry-After", StringComparison.OrdinalIgnoreCase))
                {
                    values = pair.Value;
                    break;
                }
            }

            if (values == null || values.Count == 0)
            {
                return null;
            }

            var rawValue = values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return null;
            }

            var trimmed = rawValue.Trim();
            if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var retryAfterSeconds))
            {
                return retryAfterSeconds < 0
                    ? (DateTimeOffset?)null
                    : DateTimeOffset.UtcNow.AddSeconds(retryAfterSeconds);
            }

            if (DateTimeOffset.TryParseExact(
                    trimmed,
                    "r",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out var retryAfterAt))
            {
                return retryAfterAt.ToUniversalTime();
            }

            if (DateTimeOffset.TryParse(
                    trimmed,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out retryAfterAt))
            {
                return retryAfterAt.ToUniversalTime();
            }

            return null;
        }

        private static AttriaxDeepLinkResolutionResultInternal MapDeepLinkResolutionResult(SdkV1DeepLinkResolveResponseDto response)
        {
            return new AttriaxDeepLinkResolutionResultInternal
            {
                Matched = response.matched,
                Status = MapDeepLinkResolutionStatus(response.status),
                IsFirstLaunch = response.isFirstLaunch,
                Reason = response.reason,
                DeepLink = MapDeepLink(response.deepLink),
                BrowserAction = MapBrowserAction(response.browserAction),
                RequestVersion = response.requestVersion,
                AcceptedAt = ToDateTimeOffset(response.acceptedAt),
                ConsumedAt = ToDateTimeOffset(response.consumedAt),
            };
        }

        private static AttriaxResolvedUrlAction? MapBrowserAction(Generated.Model.SdkBrowserActionDto browserAction)
        {
            if (browserAction == null || string.IsNullOrWhiteSpace(browserAction.url))
            {
                return null;
            }

            return new AttriaxResolvedUrlAction
            {
                Url = browserAction.url,
                OpenMode = MapResolvedUrlOpenMode(browserAction.openMode),
            };
        }

        internal static AttriaxSdkRuntimeConfig MapRuntimeConfigEnvelope(JObject? envelope)
        {
            var data = RequireEnvelopeData(envelope);
            return new AttriaxSdkRuntimeConfig(
                clipboardAttributionEnabled: data.Value<bool?>("clipboardAttributionEnabled") ?? false);
        }

        internal static AttriaxAppOpenResult MapAppOpenResultEnvelope(JObject? envelope)
        {
            var data = RequireEnvelopeData(envelope);
            var originalInstallReferrer = MapInstallReferrerDetails(data["originalInstallReferrer"])
                ?? MapInstallReferrerDetails(data["installReferrer"]);

            return new AttriaxAppOpenResult
            {
                UserId = data.Value<string>("userId") ?? string.Empty,
                IsNewUser = data.Value<bool?>("isNewUser") ?? false,
                IsFirstLaunch = data.Value<bool?>("isFirstLaunch") ?? false,
                InstallState = MapInstallState(data["installState"]),
                RequestVersion = data.Value<string>("requestVersion"),
                AcceptedAt = ToOptionalDateTimeOffset(data["acceptedAt"]),
                DeepLink = MapDeepLink(data["deepLink"]),
                DeepLinkClickedAt = ToOptionalDateTimeOffset(data["deepLinkClickedAt"]),
                DeepLinkConsumedAt = ToOptionalDateTimeOffset(data["deepLinkConsumedAt"]),
                OriginalInstallReferrer = originalInstallReferrer,
                ReinstallReferrer = MapInstallReferrerDetails(data["reinstallReferrer"]),
                Skan = MapSkanRuntimeConfiguration(data["skan"]),
            };
        }

        private static JObject RequireEnvelopeData(JObject? envelope)
        {
            var data = envelope?["data"] as JObject;
            if (data != null)
            {
                return data;
            }

            throw new AttriaxApiError(
                "Attriax API returned an unexpected response.",
                null,
                false,
                true);
        }

        private static AttriaxAppOpenResult MapAppOpenResult(SdkV1OpenResponseDto response)
        {
            var originalInstallReferrer = MapInstallReferrerDetails(response.originalInstallReferrer)
                ?? MapInstallReferrerDetails(response.installReferrer);

            return new AttriaxAppOpenResult
            {
                UserId = response.userId,
                IsNewUser = response.isNewUser,
                IsFirstLaunch = response.isFirstLaunch,
                InstallState = MapInstallState(response.installState),
                RequestVersion = response.requestVersion,
                AcceptedAt = ToDateTimeOffset(response.acceptedAt),
                DeepLink = MapDeepLink(response.deepLink),
                DeepLinkClickedAt = response.deepLinkClickedAt.HasValue
                    ? ToDateTimeOffset(response.deepLinkClickedAt.Value)
                    : null,
                DeepLinkConsumedAt = response.deepLinkConsumedAt.HasValue
                    ? ToDateTimeOffset(response.deepLinkConsumedAt.Value)
                    : null,
                OriginalInstallReferrer = originalInstallReferrer,
                ReinstallReferrer = MapInstallReferrerDetails(response.reinstallReferrer),
                Skan = MapSkanRuntimeConfiguration(response.skan),
            };
        }

        private static AttriaxSkanRuntimeConfiguration? MapSkanRuntimeConfiguration(
            SdkSkanRuntimeConfigurationDto response)
        {
            if (response == null)
            {
                return null;
            }

            return new AttriaxSkanRuntimeConfiguration
            {
                Enabled = response.enabled,
                Schema = MapSkanSchema(response.schema),
            };
        }

        private static AttriaxSkanRuntimeConfiguration? MapSkanRuntimeConfiguration(JToken? response)
        {
            var payload = response as JObject;
            if (payload == null)
            {
                return null;
            }

            return new AttriaxSkanRuntimeConfiguration
            {
                Enabled = payload.Value<bool?>("enabled") ?? false,
                Schema = MapSkanSchema(payload["schema"]),
            };
        }

        private static AttriaxSkanSchema? MapSkanSchema(
            Generated.Model.SdkV1SkanSchemaDto response)
        {
            if (response == null)
            {
                return null;
            }

            return new AttriaxSkanSchema
            {
                Version = decimal.ToInt32(response.version),
                UpdatedAt = ToOptionalDateTimeOffset(response.updatedAt),
                Window1 = MapSkanWindow1(response.window1) ?? new AttriaxSkanWindow1(),
                Window2 = MapSkanCoarseWindow(response.window2) ?? new AttriaxSkanCoarseWindow(),
                Window3 = MapSkanCoarseWindow(response.window3) ?? new AttriaxSkanCoarseWindow(),
            };
        }

        private static AttriaxSkanSchema? MapSkanSchema(JToken? response)
        {
            var payload = response as JObject;
            if (payload == null)
            {
                return null;
            }

            return new AttriaxSkanSchema
            {
                Version = payload.Value<int?>("version") ?? 0,
                UpdatedAt = ToOptionalDateTimeOffset(payload["updatedAt"]),
                Window1 = MapSkanWindow1(payload["window1"]) ?? new AttriaxSkanWindow1(),
                Window2 = MapSkanCoarseWindow(payload["window2"]) ?? new AttriaxSkanCoarseWindow(),
                Window3 = MapSkanCoarseWindow(payload["window3"]) ?? new AttriaxSkanCoarseWindow(),
            };
        }

        private static AttriaxSkanWindow1? MapSkanWindow1(Generated.Model.SdkV1SkanWindow1Dto response)
        {
            if (response == null)
            {
                return null;
            }

            return new AttriaxSkanWindow1
            {
                Groups = response.groups != null
                    ? response.groups.Select(MapSkanWindow1Group).ToList()
                    : new List<AttriaxSkanWindow1Group>(),
            };
        }

        private static AttriaxSkanWindow1? MapSkanWindow1(JToken? response)
        {
            var payload = response as JObject;
            if (payload == null)
            {
                return null;
            }

            return new AttriaxSkanWindow1
            {
                Groups = (payload["groups"] as JArray)?.OfType<JObject>().Select(MapSkanWindow1Group).ToList()
                    ?? new List<AttriaxSkanWindow1Group>(),
            };
        }

        private static AttriaxSkanWindow1Group MapSkanWindow1Group(Generated.Model.SdkV1SkanWindow1GroupDto response)
        {
            return new AttriaxSkanWindow1Group
            {
                Id = response?.id ?? string.Empty,
                DisplayName = ToOptionalString(response?.displayName),
                StartBit = response != null ? decimal.ToInt32(response.startBit) : 0,
                BitCount = response != null ? decimal.ToInt32(response.bitCount) : 0,
                Events = response?.events != null
                    ? response.events.Select(MapSkanEvent).ToList()
                    : new List<AttriaxSkanEvent>(),
            };
        }

        private static AttriaxSkanWindow1Group MapSkanWindow1Group(JObject? response)
        {
            return new AttriaxSkanWindow1Group
            {
                Id = response?.Value<string>("id") ?? string.Empty,
                DisplayName = ToOptionalString(response?["displayName"]),
                StartBit = response?.Value<int?>("startBit") ?? 0,
                BitCount = response?.Value<int?>("bitCount") ?? 0,
                Events = (response?["events"] as JArray)?.OfType<JObject>().Select(MapSkanEvent).ToList()
                    ?? new List<AttriaxSkanEvent>(),
            };
        }

        private static AttriaxSkanEvent MapSkanEvent(Generated.Model.SdkV1SkanEventDto response)
        {
            return new AttriaxSkanEvent
            {
                Id = response?.id ?? string.Empty,
                EventName = response?.eventName ?? string.Empty,
                DisplayName = ToOptionalString(response?.displayName),
                CoarseValue = ParseSkanCoarseValue(response?.coarseValue),
                LockWindow = response?.lockWindow ?? false,
                Conditions = response?.conditions != null
                    ? response.conditions.Select(MapSkanCondition).ToList()
                    : new List<AttriaxSkanCondition>(),
            };
        }

        private static AttriaxSkanEvent MapSkanEvent(JObject? response)
        {
            return new AttriaxSkanEvent
            {
                Id = response?.Value<string>("id") ?? string.Empty,
                EventName = response?.Value<string>("eventName") ?? string.Empty,
                DisplayName = ToOptionalString(response?["displayName"]),
                CoarseValue = ParseSkanCoarseValue(response?["coarseValue"]),
                LockWindow = response?.Value<bool?>("lockWindow") ?? false,
                Conditions = (response?["conditions"] as JArray)?.OfType<JObject>().Select(MapSkanCondition).ToList()
                    ?? new List<AttriaxSkanCondition>(),
            };
        }

        private static AttriaxSkanCoarseWindow? MapSkanCoarseWindow(Generated.Model.SdkV1SkanCoarseWindowDto response)
        {
            if (response == null)
            {
                return null;
            }

            return new AttriaxSkanCoarseWindow
            {
                Events = response.events != null
                    ? response.events.Select(MapSkanCoarseWindowEvent).ToList()
                    : new List<AttriaxSkanCoarseWindowEvent>(),
            };
        }

        private static AttriaxSkanCoarseWindow? MapSkanCoarseWindow(JToken? response)
        {
            var payload = response as JObject;
            if (payload == null)
            {
                return null;
            }

            return new AttriaxSkanCoarseWindow
            {
                Events = (payload["events"] as JArray)?.OfType<JObject>().Select(MapSkanCoarseWindowEvent).ToList()
                    ?? new List<AttriaxSkanCoarseWindowEvent>(),
            };
        }

        private static AttriaxSkanCoarseWindowEvent MapSkanCoarseWindowEvent(
            Generated.Model.SdkV1SkanCoarseWindowEventDto response)
        {
            return new AttriaxSkanCoarseWindowEvent
            {
                Id = response?.id ?? string.Empty,
                EventName = response?.eventName ?? string.Empty,
                DisplayName = ToOptionalString(response?.displayName),
                LockWindow = response?.lockWindow ?? false,
                Conditions = response?.conditions != null
                    ? response.conditions.Select(MapSkanCondition).ToList()
                    : new List<AttriaxSkanCondition>(),
                CoarseValue = ParseSkanCoarseValue(response?.coarseValue) ?? AttriaxSkanCoarseValue.Low,
            };
        }

        private static AttriaxSkanCoarseWindowEvent MapSkanCoarseWindowEvent(JObject? response)
        {
            return new AttriaxSkanCoarseWindowEvent
            {
                Id = response?.Value<string>("id") ?? string.Empty,
                EventName = response?.Value<string>("eventName") ?? string.Empty,
                DisplayName = ToOptionalString(response?["displayName"]),
                LockWindow = response?.Value<bool?>("lockWindow") ?? false,
                Conditions = (response?["conditions"] as JArray)?.OfType<JObject>().Select(MapSkanCondition).ToList()
                    ?? new List<AttriaxSkanCondition>(),
                CoarseValue = ParseSkanCoarseValue(response?["coarseValue"]) ?? AttriaxSkanCoarseValue.Low,
            };
        }

        private static AttriaxSkanCondition MapSkanCondition(Generated.Model.SdkV1SkanConditionDto response)
        {
            return new AttriaxSkanCondition
            {
                Id = response?.id ?? string.Empty,
                ParamKey = response?.paramKey ?? string.Empty,
                Operator = ParseSkanRuleOperator(response?.VarOperator.ToString()),
                Value = NormalizeArbitraryValue(response?.value),
            };
        }

        private static AttriaxSkanCondition MapSkanCondition(JObject? response)
        {
            return new AttriaxSkanCondition
            {
                Id = response?.Value<string>("id") ?? string.Empty,
                ParamKey = response?.Value<string>("paramKey") ?? string.Empty,
                Operator = ParseSkanRuleOperator(response?["operator"]?.ToString()),
                Value = NormalizeArbitraryValue(response?["value"]),
            };
        }

        private static AttriaxSkanRuleOperator ParseSkanRuleOperator(string? value)
        {
            switch ((value ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "exists":
                    return AttriaxSkanRuleOperator.Exists;
                case "noteq":
                case "not_eq":
                case "not-eq":
                    return AttriaxSkanRuleOperator.NotEq;
                case "gt":
                    return AttriaxSkanRuleOperator.Gt;
                case "gte":
                    return AttriaxSkanRuleOperator.Gte;
                case "lt":
                    return AttriaxSkanRuleOperator.Lt;
                case "lte":
                    return AttriaxSkanRuleOperator.Lte;
                case "contains":
                    return AttriaxSkanRuleOperator.Contains;
                case "eq":
                default:
                    return AttriaxSkanRuleOperator.Eq;
            }
        }

        private static AttriaxSkanCoarseValue? ParseSkanCoarseValue(string? value)
        {
            switch ((value ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "low":
                    return AttriaxSkanCoarseValue.Low;
                case "medium":
                    return AttriaxSkanCoarseValue.Medium;
                case "high":
                    return AttriaxSkanCoarseValue.High;
                default:
                    return null;
            }
        }

        private static AttriaxSkanCoarseValue? ParseSkanCoarseValue(Generated.Model.SdkV1SkanCoarseValue? value)
        {
            return value.HasValue ? ParseSkanCoarseValue(value.Value.ToString()) : null;
        }

        private static AttriaxSkanCoarseValue? ParseSkanCoarseValue(JToken? value)
        {
            if (value == null || value.Type == JTokenType.Null)
            {
                return null;
            }

            return ParseSkanCoarseValue(value.ToString());
        }

        private static string? ToOptionalString(object? value)
        {
            return NormalizeArbitraryValue(value)?.ToString();
        }

        private static DateTimeOffset? ToOptionalDateTimeOffset(object? value)
        {
            var normalized = NormalizeArbitraryValue(value);
            if (normalized == null)
            {
                return null;
            }

            if (normalized is DateTimeOffset dateTimeOffset)
            {
                return dateTimeOffset;
            }

            if (normalized is DateTime dateTime)
            {
                return ToDateTimeOffset(dateTime);
            }

            if (DateTimeOffset.TryParse(normalized.ToString(), out var parsed))
            {
                return parsed.ToUniversalTime();
            }

            return null;
        }

        private static object? NormalizeArbitraryValue(object? value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is JToken token)
            {
                return NormalizeArbitraryValue(token.ToObject<object>());
            }

            if (value is string || value is bool || value is byte || value is sbyte ||
                value is short || value is ushort || value is int || value is uint ||
                value is long || value is ulong || value is float || value is double ||
                value is decimal)
            {
                return value;
            }

            if (value is Enum)
            {
                return value.ToString();
            }

            if (value is DateTimeOffset dateTimeOffset)
            {
                return dateTimeOffset;
            }

            if (value is DateTime dateTime)
            {
                return ToDateTimeOffset(dateTime);
            }

            if (value is IDictionary dictionary)
            {
                var output = new Dictionary<string, object>();
                foreach (DictionaryEntry entry in dictionary)
                {
                    output[entry.Key.ToString()] = NormalizeArbitraryValue(entry.Value);
                }

                return output;
            }

            if (value is IEnumerable enumerable && value is not string)
            {
                var array = new List<object>();
                foreach (var item in enumerable)
                {
                    array.Add(NormalizeArbitraryValue(item));
                }

                return array;
            }

            return value.ToString();
        }

        private static AttriaxCreateDynamicLinkResult MapDynamicLinkCreateResult(SdkCreateDynamicLinkResponseDto response)
        {
            return new AttriaxCreateDynamicLinkResult
            {
                RequestVersion = response.requestVersion,
                AcceptedAt = ToDateTimeOffset(response.acceptedAt),
                Link = MapDynamicLinkRecord(response.link),
            };
        }

        private static AttriaxRevenueReceiptValidationResult MapRevenueReceiptValidationResult(
            SdkRevenueReceiptValidateResponseDto response)
        {
            return new AttriaxRevenueReceiptValidationResult
            {
                ValidationId = response.validationId,
                Status = MapRevenueReceiptValidationStatus(response.status),
                RequestVersion = response.requestVersion,
                AcceptedAt = ToDateTimeOffset(response.acceptedAt),
                Provider = response.provider,
                Environment = response.environment,
                TransactionId = response.transactionId,
                OriginalTransactionId = response.originalTransactionId,
                ProductId = response.productId,
                FailureReason = response.failureReason,
                ExpiresAt = response.expiresAt.HasValue ? ToDateTimeOffset(response.expiresAt.Value) : null,
                ProviderResult = response.providerResult != null
                    ? AttriaxObjectNormalizer.NormalizeObjectMap(response.providerResult)
                    : null,
                PublicReceipt = AttriaxObjectNormalizer.NormalizeObjectMap(response.publicReceipt) ?? new Dictionary<string, object>(),
            };
        }

        private static long? MapRevenueUsdMicros(JObject? envelope)
        {
            var data = envelope?["data"] as JObject;
            var amountUsdMicros = data?["amountUsdMicros"];
            if (amountUsdMicros == null || amountUsdMicros.Type == JTokenType.Null)
            {
                return null;
            }

            if (amountUsdMicros.Type == JTokenType.Integer)
            {
                return amountUsdMicros.Value<long>();
            }

            return long.TryParse(
                amountUsdMicros.ToString(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var parsed)
                ? parsed
                : null;
        }

        private static SdkCrashDto MapCrashRequest(AttriaxCrashRequest request)
        {
            return new SdkCrashDto(
                appBuildNumber: request.AppBuildNumber,
                appPackageName: request.AppPackageName,
                projectToken: request.ProjectToken,
                appVersion: request.AppVersion,
                clientOccurredAt: request.ClientOccurredAt.UtcDateTime,
                deviceId: request.DeviceId,
                deviceIdSource: request.DeviceIdSource,
                exceptionType: request.ExceptionType,
                isFatal: request.IsFatal,
                isFirstLaunch: request.IsFirstLaunch,
                locale: request.Locale,
                message: request.Message,
                metadata: AttriaxObjectNormalizer.NormalizeObjectMap(request.Metadata),
                platform: MapGeneratedPlatform(request.Platform),
                reason: request.Reason,
                sdkApiVersion: request.SdkApiVersion,
                sdkPackageVersion: request.SdkPackageVersion,
                sessionId: request.SessionId,
                sessionRelativeTimeMs: request.SessionRelativeTimeMs ?? default,
                source: request.Source,
                stackTrace: request.StackTrace);
        }

        private static AttriaxDeepLink? MapDeepLink(GeneratedJsonDeepLinkDto deepLink)
        {
            if (deepLink == null)
            {
                return null;
            }

            return new AttriaxDeepLink
            {
                Path = deepLink.path,
                Data = AttriaxObjectNormalizer.NormalizeObjectMap(deepLink.data),
                Uri = TryCreateAbsoluteUri(deepLink.uri),
                Utm = MapUtmParameters(deepLink.utm),
            };
        }

        private static AttriaxDeepLink? MapDeepLink(JToken? deepLink)
        {
            var payload = deepLink as JObject;
            if (payload == null)
            {
                return null;
            }

            return new AttriaxDeepLink
            {
                Path = payload.Value<string>("path"),
                Data = ToNormalizedObjectMap(payload["data"]),
                Uri = TryCreateAbsoluteUri(payload.Value<string>("uri")),
                Utm = MapUtmParameters(payload["utm"]),
            };
        }

        private static AttriaxUtmParameters? MapUtmParameters(
            global::Attriax.Unity.Generated.Model.SdkUtmPayloadDto utm)
        {
            if (utm == null)
            {
                return null;
            }

            return new AttriaxUtmParameters
            {
                Source = utm.source,
                Medium = utm.medium,
                Campaign = utm.campaign,
                Term = utm.term,
                Content = utm.content,
            };
        }

        private static AttriaxUtmParameters? MapUtmParameters(JToken? utm)
        {
            var payload = utm as JObject;
            if (payload == null)
            {
                return null;
            }

            return new AttriaxUtmParameters
            {
                Source = payload.Value<string>("source"),
                Medium = payload.Value<string>("medium"),
                Campaign = payload.Value<string>("campaign"),
                Term = payload.Value<string>("term"),
                Content = payload.Value<string>("content"),
            };
        }

        private static Uri? TryCreateAbsoluteUri(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                return null;
            }

            return Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri)
                ? parsedUri
                : null;
        }

        private static AttriaxInstallReferrerDetails? MapInstallReferrerDetails(GeneratedInstallReferrerResultDto installReferrer)
        {
            if (installReferrer == null)
            {
                return null;
            }

            return new AttriaxInstallReferrerDetails
            {
                RawPlatformInstallReferrer = installReferrer.rawPlatformInstallReferrer,
                Source = installReferrer.source,
                Medium = installReferrer.medium,
                Campaign = installReferrer.campaign,
                Term = installReferrer.term,
                Content = installReferrer.content,
                AdNetwork = installReferrer.adNetwork,
                AdClickId = installReferrer.adClickId,
                AttributionType = MapAttributionType(installReferrer.attributionType),
                DeepLinkUri = string.IsNullOrWhiteSpace(installReferrer.deepLinkUri)
                    ? installReferrer.deepLinkUrl
                    : installReferrer.deepLinkUri,
                DeepLinkData = AttriaxObjectNormalizer.NormalizeObjectMap(installReferrer.deepLinkData),
                RegisteredAt = ToOptionalDateTimeOffset(installReferrer.registeredAt),
                InstallBeginTimestampSeconds = ToNullableLong(installReferrer.installBeginTimestampSeconds),
                ReferrerClickTimestampSeconds = ToNullableLong(installReferrer.referrerClickTimestampSeconds),
                GooglePlayInstantParam = installReferrer.googlePlayInstantParam,
                Precision = (double)installReferrer.precision,
            };
        }

        private static AttriaxInstallReferrerDetails? MapInstallReferrerDetails(JToken? installReferrer)
        {
            var payload = installReferrer as JObject;
            if (payload == null)
            {
                return null;
            }

            return new AttriaxInstallReferrerDetails
            {
                RawPlatformInstallReferrer = payload.Value<string>("rawPlatformInstallReferrer"),
                Source = payload.Value<string>("source"),
                Medium = payload.Value<string>("medium"),
                Campaign = payload.Value<string>("campaign"),
                Term = payload.Value<string>("term"),
                Content = payload.Value<string>("content"),
                AdNetwork = payload.Value<string>("adNetwork"),
                AdClickId = payload.Value<string>("adClickId"),
                AttributionType = MapAttributionType(payload["attributionType"]),
                DeepLinkUri = string.IsNullOrWhiteSpace(payload.Value<string>("deepLinkUri"))
                    ? payload.Value<string>("deepLinkUrl")
                    : payload.Value<string>("deepLinkUri"),
                DeepLinkData = ToNormalizedObjectMap(payload["deepLinkData"]),
                RegisteredAt = ToOptionalDateTimeOffset(payload["registeredAt"]),
                InstallBeginTimestampSeconds = ToNullableLong(payload["installBeginTimestampSeconds"]),
                ReferrerClickTimestampSeconds = ToNullableLong(payload["referrerClickTimestampSeconds"]),
                GooglePlayInstantParam = payload.Value<bool?>("googlePlayInstantParam") ?? false,
                Precision = payload.Value<double?>("precision") ?? 0d,
            };
        }

        private static AttriaxInstallState MapInstallState(GeneratedInstallState installState)
        {
            switch (installState)
            {
                case GeneratedInstallState.NewInstall:
                    return AttriaxInstallState.NewInstall;
                case GeneratedInstallState.Reinstall:
                    return AttriaxInstallState.Reinstall;
                case GeneratedInstallState.AppDataClear:
                    return AttriaxInstallState.AppDataClear;
                case GeneratedInstallState.Existing:
                default:
                    return AttriaxInstallState.Existing;
            }
        }

        private static AttriaxInstallState MapInstallState(JToken? installState)
        {
            switch ((installState?.ToString() ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "new_install":
                    return AttriaxInstallState.NewInstall;
                case "reinstall":
                    return AttriaxInstallState.Reinstall;
                case "app_data_clear":
                    return AttriaxInstallState.AppDataClear;
                case "existing":
                default:
                    return AttriaxInstallState.Existing;
            }
        }

        private static AttriaxDynamicLinkRecord MapDynamicLinkRecord(GeneratedDynamicLinkRecordDto link)
        {
            return new AttriaxDynamicLinkRecord
            {
                Id = link.id,
                Path = link.path,
                ShortUrl = link.shortUrl,
                Name = link.name,
                DestinationUrl = link.destinationUrl,
                Group = link.group,
                Prefix = link.prefix,
                Data = AttriaxObjectNormalizer.NormalizeObjectMap(link.data),
                PreviewTitle = link.previewTitle,
                PreviewDescription = link.previewDescription,
                IOSRedirect = link.iosRedirect,
                AndroidRedirect = link.androidRedirect,
                UtmSource = link.utmSource,
                UtmMedium = link.utmMedium,
                UtmCampaign = link.utmCampaign,
                UtmTerm = link.utmTerm,
                UtmContent = link.utmContent,
                CreatedAt = ToDateTimeOffset(link.createdAt),
            };
        }

        private static AttriaxDeepLinkResolutionStatus MapDeepLinkResolutionStatus(GeneratedDeepLinkResolutionStatus status)
        {
            switch (status)
            {
                case GeneratedDeepLinkResolutionStatus.Matched:
                    return AttriaxDeepLinkResolutionStatus.Matched;
                case GeneratedDeepLinkResolutionStatus.Unmatched:
                    return AttriaxDeepLinkResolutionStatus.Unmatched;
                default:
                    return AttriaxDeepLinkResolutionStatus.Invalid;
            }
        }

        private static AttributionType MapAttributionType(GeneratedAttributionType attributionType)
        {
            switch (attributionType)
            {
                case GeneratedAttributionType.Referrer:
                    return AttributionType.Referrer;
                case GeneratedAttributionType.Fingerprint:
                    return AttributionType.Fingerprint;
                case GeneratedAttributionType.External:
                    return AttributionType.External;
                default:
                    return AttributionType.Organic;
            }
        }

        private static AttributionType MapAttributionType(JToken? attributionType)
        {
            switch ((attributionType?.ToString() ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "referrer":
                    return AttributionType.Referrer;
                case "fingerprint":
                    return AttributionType.Fingerprint;
                case "external":
                    return AttributionType.External;
                case "organic":
                default:
                    return AttributionType.Organic;
            }
        }

        private static GeneratedPlatform MapGeneratedPlatform(string platform)
        {
            switch (platform)
            {
                case "android":
                    return GeneratedPlatform.Android;
                case "ios":
                    return GeneratedPlatform.Ios;
                case "unity_editor":
                    return GeneratedPlatform.UnityEditor;
                case "windows":
                    return GeneratedPlatform.Windows;
                case "macos":
                    return GeneratedPlatform.Macos;
                case "linux":
                    return GeneratedPlatform.Linux;
                case "web":
                    return GeneratedPlatform.Web;
                case "unknown":
                default:
                    return GeneratedPlatform.Unknown;
            }
        }

        private static AttriaxRevenueReceiptValidationStatus MapRevenueReceiptValidationStatus(
            GeneratedRevenueReceiptValidateStatus status)
        {
            switch (status)
            {
                case GeneratedRevenueReceiptValidateStatus.Verified:
                    return AttriaxRevenueReceiptValidationStatus.Verified;
                case GeneratedRevenueReceiptValidateStatus.Pending:
                    return AttriaxRevenueReceiptValidationStatus.Pending;
                case GeneratedRevenueReceiptValidateStatus.Unconfigured:
                    return AttriaxRevenueReceiptValidationStatus.Unconfigured;
                case GeneratedRevenueReceiptValidateStatus.ProviderError:
                    return AttriaxRevenueReceiptValidationStatus.ProviderError;
                case GeneratedRevenueReceiptValidateStatus.Passthrough:
                    return AttriaxRevenueReceiptValidationStatus.Passthrough;
                case GeneratedRevenueReceiptValidateStatus.Rejected:
                default:
                    return AttriaxRevenueReceiptValidationStatus.Rejected;
            }
        }

        private static AttriaxResolvedUrlOpenMode MapResolvedUrlOpenMode(
            GeneratedRouteUrlOpenMode openMode)
        {
            switch (openMode)
            {
                case GeneratedRouteUrlOpenMode.InApp:
                    return AttriaxResolvedUrlOpenMode.InApp;
                case GeneratedRouteUrlOpenMode.External:
                    return AttriaxResolvedUrlOpenMode.External;
                default:
                    return AttriaxResolvedUrlOpenMode.Unknown;
            }
        }

        private static DateTimeOffset ToDateTimeOffset(DateTime value)
        {
            var normalized = value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value;
            return new DateTimeOffset(normalized.ToUniversalTime());
        }

        private static DateTimeOffset? ToOptionalDateTimeOffset(DateTime value)
        {
            return value == default ? null : ToDateTimeOffset(value);
        }

        private static DateTimeOffset? ToOptionalDateTimeOffset(JToken? value)
        {
            return ToOptionalDateTimeOffset((object?)value);
        }

        private static long? ToNullableLong(decimal value)
        {
            return value == default ? null : decimal.ToInt64(value);
        }

        private static long? ToNullableLong(JToken? value)
        {
            if (value == null || value.Type == JTokenType.Null)
            {
                return null;
            }

            if (value.Type == JTokenType.Integer)
            {
                return value.Value<long>();
            }

            if (value.Type == JTokenType.Float)
            {
                return decimal.ToInt64(value.Value<decimal>());
            }

            return long.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;
        }

        private static Dictionary<string, object>? ToNormalizedObjectMap(JToken? token)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                return null;
            }

            var raw = token.ToObject<Dictionary<string, object>>();
            return AttriaxObjectNormalizer.NormalizeObjectMap(raw);
        }

    }

    internal sealed class AttriaxDeepLinkResolutionResultInternal
    {
        public bool Matched;
        public AttriaxDeepLinkResolutionStatus Status;
        public bool IsFirstLaunch;
        public string? Reason;
        public AttriaxDeepLink? DeepLink;
        public AttriaxResolvedUrlAction? BrowserAction;
        public string? RequestVersion;
        public DateTimeOffset? AcceptedAt;
        public DateTimeOffset? ConsumedAt;
    }
}