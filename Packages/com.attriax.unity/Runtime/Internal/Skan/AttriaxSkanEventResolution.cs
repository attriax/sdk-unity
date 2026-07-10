#nullable enable
using System;
using System.Collections.Generic;
using Attriax.Unity;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Pure resolution of a tracked event into a conversion-value update. Mirrors
    /// the Flutter reference's <c>attriax_skan_event_resolution.dart</c>. These
    /// functions never mutate or persist state; they return the resolved
    /// fine/coarse/lock triple (or null when nothing matched) and leave the actual
    /// conversion update to the manager.
    /// </summary>
    internal static class AttriaxSkanEventResolution
    {
        internal static SkanResolvedUpdate? ResolveWindow1SkanUpdate(
            AttriaxSkanState currentState,
            string eventName,
            IDictionary<string, object> eventData,
            IList<AttriaxSkanWindow1Group>? groups)
        {
            if (groups == null || groups.Count == 0)
            {
                return null;
            }

            var nextFineValue = currentState.FineValue ?? 0;
            var nextCoarseValue = currentState.CoarseValue;
            var nextLockWindow = currentState.LockWindow;
            var matchedAnyGroup = false;

            foreach (var group in groups)
            {
                if (!AttriaxSkanRules.IsValidBitRange(group.StartBit, group.BitCount))
                {
                    continue;
                }

                var match = MatchedWindow1Event(group, eventName, eventData);
                if (match == null)
                {
                    continue;
                }

                matchedAnyGroup = true;
                var currentSegmentValue = AttriaxSkanRules.ExtractBitRangeValue(nextFineValue, group.StartBit, group.BitCount);
                var nextSegmentValue = Math.Max(currentSegmentValue, match.Rank);
                nextFineValue = AttriaxSkanRules.ReplaceBitRangeValue(
                    nextFineValue,
                    group.StartBit,
                    group.BitCount,
                    nextSegmentValue);
                nextCoarseValue = AttriaxSkanRules.MaxCoarseValue(nextCoarseValue, match.Event.CoarseValue);
                nextLockWindow = nextLockWindow || match.Event.LockWindow;
            }

            if (!matchedAnyGroup)
            {
                return null;
            }

            return new SkanResolvedUpdate(nextFineValue, nextCoarseValue, nextLockWindow);
        }

        internal static SkanResolvedUpdate? ResolveCoarseWindowSkanUpdate(
            AttriaxSkanState currentState,
            string eventName,
            IDictionary<string, object> eventData,
            IList<AttriaxSkanCoarseWindowEvent>? events)
        {
            if (events == null || events.Count == 0)
            {
                return null;
            }

            AttriaxSkanCoarseValue? nextCoarseValue = currentState.CoarseValue;
            var nextLockWindow = currentState.LockWindow;
            var matchedAnyEvent = false;

            foreach (var skanEvent in events)
            {
                if (!string.Equals(skanEvent.EventName, eventName, StringComparison.Ordinal) ||
                    !AttriaxSkanRules.MatchesConditions(skanEvent.Conditions, eventData))
                {
                    continue;
                }

                matchedAnyEvent = true;
                nextCoarseValue = AttriaxSkanRules.MaxCoarseValue(nextCoarseValue, skanEvent.CoarseValue);
                nextLockWindow = nextLockWindow || skanEvent.LockWindow;
            }

            if (!matchedAnyEvent)
            {
                return null;
            }

            return new SkanResolvedUpdate(currentState.FineValue ?? 0, nextCoarseValue, nextLockWindow);
        }

        private static SkanWindow1Match? MatchedWindow1Event(
            AttriaxSkanWindow1Group group,
            string eventName,
            IDictionary<string, object> eventData)
        {
            if (group.Events == null || group.Events.Count == 0)
            {
                return null;
            }

            SkanWindow1Match? match = null;
            for (var index = 0; index < group.Events.Count; index += 1)
            {
                var skanEvent = group.Events[index];
                if (!string.Equals(skanEvent.EventName, eventName, StringComparison.Ordinal) ||
                    !AttriaxSkanRules.MatchesConditions(skanEvent.Conditions, eventData))
                {
                    continue;
                }

                match = new SkanWindow1Match(index + 1, skanEvent);
            }

            return match;
        }
    }

    internal sealed class SkanResolvedUpdate
    {
        internal SkanResolvedUpdate(int fineValue, AttriaxSkanCoarseValue? coarseValue, bool lockWindow)
        {
            FineValue = fineValue;
            CoarseValue = coarseValue;
            LockWindow = lockWindow;
        }

        internal int FineValue { get; }

        internal AttriaxSkanCoarseValue? CoarseValue { get; }

        internal bool LockWindow { get; }
    }

    internal sealed class SkanWindow1Match
    {
        internal SkanWindow1Match(int rank, AttriaxSkanEvent skanEvent)
        {
            Rank = rank;
            Event = skanEvent;
        }

        internal int Rank { get; }

        internal AttriaxSkanEvent Event { get; }
    }
}
