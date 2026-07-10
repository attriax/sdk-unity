#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Attriax.Unity;
using Newtonsoft.Json;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Pure augmentation of locally-tracked event data with SKAN-derived counters.
    /// Mirrors the Flutter reference's <c>attriax_skan_event_augmenter.dart</c>: for
    /// purchase/ad-show events it folds the running revenue/count into the returned
    /// state and event payload. It never persists; the manager applies the returned
    /// state when <see cref="AttriaxSkanEventAugmentation.StateChanged"/> is true.
    /// </summary>
    internal sealed class AttriaxSkanEventAugmenter
    {
        private readonly Func<DateTimeOffset> _clock;
        private readonly Func<long, string, DateTimeOffset, Task<long?>>? _convertRevenueToUsdMicrosAsync;
        private readonly Action<string, string?> _debugLog;

        internal AttriaxSkanEventAugmenter(
            Func<DateTimeOffset> clock,
            Func<long, string, DateTimeOffset, Task<long?>>? convertRevenueToUsdMicrosAsync,
            Action<string, string?> debugLog)
        {
            _clock = clock;
            _convertRevenueToUsdMicrosAsync = convertRevenueToUsdMicrosAsync;
            _debugLog = debugLog;
        }

        internal async Task<AttriaxSkanEventAugmentation> AugmentAsync(
            string eventName,
            IDictionary<string, object> eventData,
            AttriaxSkanState state)
        {
            if (string.Equals(eventName, "purchase", StringComparison.Ordinal))
            {
                return await AugmentPurchaseEventDataAsync(state, eventData).ConfigureAwait(false);
            }

            if (string.Equals(eventName, "ad_show", StringComparison.Ordinal))
            {
                return AugmentAdShowEventData(state, eventData);
            }

            return new AttriaxSkanEventAugmentation(state, eventData, false);
        }

        private async Task<AttriaxSkanEventAugmentation> AugmentPurchaseEventDataAsync(
            AttriaxSkanState currentState,
            IDictionary<string, object> eventData)
        {
            var usdMicros = await ResolvePurchaseUsdMicrosAsync(eventData).ConfigureAwait(false) ?? 0L;
            var nextState = Clone(currentState) ?? new AttriaxSkanState();
            nextState.PurchaseRevenueUsdMicros += usdMicros;
            nextState.PurchaseCount += 1;

            var payload = new Dictionary<string, object>(eventData)
            {
                ["revenue"] = nextState.PurchaseRevenueUsdMicros / (double)AttriaxSkanRules.MicrosPerUnit,
                ["count"] = nextState.PurchaseCount,
            };

            return new AttriaxSkanEventAugmentation(nextState, payload, true);
        }

        private AttriaxSkanEventAugmentation AugmentAdShowEventData(
            AttriaxSkanState currentState,
            IDictionary<string, object> eventData)
        {
            var nextState = Clone(currentState) ?? new AttriaxSkanState();
            nextState.AdShowCount += 1;

            var payload = new Dictionary<string, object>(eventData)
            {
                ["shown"] = nextState.AdShowCount,
                ["count"] = nextState.AdShowCount,
            };

            return new AttriaxSkanEventAugmentation(nextState, payload, true);
        }

        private async Task<long?> ResolvePurchaseUsdMicrosAsync(IDictionary<string, object> eventData)
        {
            if (!eventData.TryGetValue("revenue", out var rawRevenue))
            {
                return null;
            }

            var revenue = AttriaxSkanRules.CoerceNumber(rawRevenue);
            if (!revenue.HasValue)
            {
                return null;
            }

            var revenueInMicros = AttriaxSkanRules.ReadBoolean(eventData, "revenueInMicros") ??
                AttriaxSkanRules.ReadBoolean(eventData, "revenue_in_micros") ??
                false;
            var amountMicros = AttriaxSkanRules.ToMicros(revenue.Value, revenueInMicros);
            var currency = AttriaxSkanRules.ReadString(eventData, "currency")?.ToUpperInvariant() ?? "USD";
            if (string.Equals(currency, "USD", StringComparison.Ordinal))
            {
                return amountMicros;
            }

            if (_convertRevenueToUsdMicrosAsync == null)
            {
                _debugLog("Skipping non-USD purchase revenue for SKAN because no USD conversion gateway is available.", null);
                return null;
            }

            try
            {
                return await _convertRevenueToUsdMicrosAsync(amountMicros, currency, _clock().ToUniversalTime())
                    .ConfigureAwait(false);
            }
            catch (Exception error)
            {
                _debugLog("Failed to convert purchase revenue to USD for SKAN.", error.Message);
                // Optimistically count failed conversions as $1 USD so SKAN value
                // updates are not missed when a transient FX lookup fails. The
                // converter contract is in micros, so $1 is one full unit of
                // micros, not a single micro.
                return AttriaxSkanRules.MicrosPerUnit;
            }
        }

        private static T? Clone<T>(T? value)
            where T : class
        {
            if (value == null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value));
        }
    }

    internal sealed class AttriaxSkanEventAugmentation
    {
        internal AttriaxSkanEventAugmentation(
            AttriaxSkanState state,
            IDictionary<string, object> eventData,
            bool stateChanged)
        {
            State = state;
            EventData = eventData;
            StateChanged = stateChanged;
        }

        internal AttriaxSkanState State { get; }

        internal IDictionary<string, object> EventData { get; }

        internal bool StateChanged { get; }
    }
}
