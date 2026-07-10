#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Attriax.Unity;
using Newtonsoft.Json.Linq;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Pure helper functions backing the SKAN manager. These mirror the Flutter
    /// reference's <c>attriax_skan_rules.dart</c>: platform gating, coarse-value
    /// derivation, bit-range encoding, condition matching, and value coercion.
    /// They hold no state and never persist.
    /// </summary>
    internal static class AttriaxSkanRules
    {
        internal const int SkanFineValueBitCount = 6;
        internal const int SkanWindow1MaxDay = 2;
        internal const int SkanWindow2MaxDay = 7;
        internal const int SkanWindow3MaxDay = 35;
        internal const long MicrosPerUnit = 1000000L;

        /// <summary>
        /// Whether SKAdNetwork is supported on <paramref name="platform"/>. SKAN is
        /// iOS-only; every other platform is excluded. The manager and the conversion
        /// updater both gate on this single predicate so the exclusion can never drift.
        /// </summary>
        internal static bool PlatformSupportsSkan(AttriaxPlatformType platform)
        {
            return platform == AttriaxPlatformType.IOS;
        }

        internal static SkanActiveWindow? ActiveWindowForDay(int day)
        {
            if (day < 0)
            {
                return null;
            }

            if (day <= SkanWindow1MaxDay)
            {
                return SkanActiveWindow.Window1;
            }

            if (day <= SkanWindow2MaxDay)
            {
                return SkanActiveWindow.Window2;
            }

            if (day <= SkanWindow3MaxDay)
            {
                return SkanActiveWindow.Window3;
            }

            return null;
        }

        internal static int RetentionDay(DateTimeOffset installAnchorAt, DateTimeOffset now)
        {
            var normalizedInstallDay = new DateTimeOffset(
                installAnchorAt.UtcDateTime.Year,
                installAnchorAt.UtcDateTime.Month,
                installAnchorAt.UtcDateTime.Day,
                0,
                0,
                0,
                TimeSpan.Zero);
            var normalizedCurrentDay = new DateTimeOffset(
                now.UtcDateTime.Year,
                now.UtcDateTime.Month,
                now.UtcDateTime.Day,
                0,
                0,
                0,
                TimeSpan.Zero);
            var difference = (normalizedCurrentDay - normalizedInstallDay).Days;
            return difference < 0 ? 0 : difference;
        }

        internal static AttriaxSkanCoarseValue DeriveCoarseValue(int fineValue)
        {
            if (fineValue >= 40)
            {
                return AttriaxSkanCoarseValue.High;
            }

            if (fineValue >= 20)
            {
                return AttriaxSkanCoarseValue.Medium;
            }

            return AttriaxSkanCoarseValue.Low;
        }

        internal static AttriaxSkanCoarseValue? MaxCoarseValue(
            AttriaxSkanCoarseValue? current,
            AttriaxSkanCoarseValue? next)
        {
            if (!current.HasValue)
            {
                return next;
            }

            if (!next.HasValue)
            {
                return current;
            }

            return current.Value >= next.Value ? current : next;
        }

        internal static bool IsValidBitRange(int startBit, int bitCount)
        {
            return startBit >= 0 &&
                bitCount > 0 &&
                startBit + bitCount <= SkanFineValueBitCount;
        }

        internal static int ExtractBitRangeValue(int fineValue, int startBit, int bitCount)
        {
            var mask = (1 << bitCount) - 1;
            return (fineValue >> startBit) & mask;
        }

        internal static int ReplaceBitRangeValue(int fineValue, int startBit, int bitCount, int value)
        {
            var maxValue = (1 << bitCount) - 1;
            var clampedValue = value < 0 ? 0 : Math.Min(value, maxValue);
            var mask = maxValue << startBit;
            return (fineValue & ~mask) | ((clampedValue << startBit) & mask);
        }

        internal static bool MatchesConditions(
            IList<AttriaxSkanCondition>? conditions,
            IDictionary<string, object> eventData)
        {
            if (conditions == null || conditions.Count == 0)
            {
                return true;
            }

            foreach (var condition in conditions)
            {
                var hasValue = eventData.TryGetValue(condition.ParamKey, out var actualValue);
                if (!ConditionMatches(condition, actualValue, hasValue))
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool ConditionMatches(
            AttriaxSkanCondition condition,
            object? actualValue,
            bool hasValue)
        {
            switch (condition.Operator)
            {
                case AttriaxSkanRuleOperator.Exists:
                    return hasValue && actualValue != null;
                case AttriaxSkanRuleOperator.Eq:
                    return hasValue && ValuesEqual(actualValue, condition.Value);
                case AttriaxSkanRuleOperator.NotEq:
                    return hasValue && !ValuesEqual(actualValue, condition.Value);
                case AttriaxSkanRuleOperator.Gt:
                case AttriaxSkanRuleOperator.Gte:
                case AttriaxSkanRuleOperator.Lt:
                case AttriaxSkanRuleOperator.Lte:
                {
                    var actualNumber = CoerceNumber(actualValue);
                    var expectedNumber = CoerceNumber(condition.Value);
                    if (!hasValue || !actualNumber.HasValue || !expectedNumber.HasValue)
                    {
                        return false;
                    }

                    return condition.Operator switch
                    {
                        AttriaxSkanRuleOperator.Gt => actualNumber.Value > expectedNumber.Value,
                        AttriaxSkanRuleOperator.Gte => actualNumber.Value >= expectedNumber.Value,
                        AttriaxSkanRuleOperator.Lt => actualNumber.Value < expectedNumber.Value,
                        AttriaxSkanRuleOperator.Lte => actualNumber.Value <= expectedNumber.Value,
                        _ => false,
                    };
                }
                case AttriaxSkanRuleOperator.Contains:
                {
                    if (!hasValue || actualValue == null || condition.Value == null)
                    {
                        return false;
                    }

                    var actualString = UnwrapJToken(actualValue) as string;
                    var expectedString = UnwrapJToken(condition.Value) as string;
                    if (actualString != null && expectedString != null)
                    {
                        return actualString.IndexOf(expectedString, StringComparison.OrdinalIgnoreCase) >= 0;
                    }

                    if (actualValue is JArray array)
                    {
                        return array.Any(item => ValuesEqual(item, condition.Value));
                    }

                    if (actualValue is IEnumerable enumerable && !(actualValue is string))
                    {
                        foreach (var item in enumerable)
                        {
                            if (ValuesEqual(item, condition.Value))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
                default:
                    return false;
            }
        }

        internal static bool ValuesEqual(object? left, object? right)
        {
            left = UnwrapJToken(left);
            right = UnwrapJToken(right);

            var leftNumber = CoerceNumber(left);
            var rightNumber = CoerceNumber(right);
            if (leftNumber.HasValue && rightNumber.HasValue)
            {
                return Math.Abs(leftNumber.Value - rightNumber.Value) < double.Epsilon;
            }

            if (left is bool leftBool && right is bool rightBool)
            {
                return leftBool == rightBool;
            }

            return string.Equals(
                Convert.ToString(left, CultureInfo.InvariantCulture),
                Convert.ToString(right, CultureInfo.InvariantCulture),
                StringComparison.Ordinal);
        }

        internal static double? CoerceNumber(object? value)
        {
            value = UnwrapJToken(value);
            if (value == null)
            {
                return null;
            }

            switch (value)
            {
                case byte number:
                    return number;
                case sbyte number:
                    return number;
                case short number:
                    return number;
                case ushort number:
                    return number;
                case int number:
                    return number;
                case uint number:
                    return number;
                case long number:
                    return number;
                case ulong number:
                    return number;
                case float number:
                    return number;
                case double number:
                    return number;
                case decimal number:
                    return (double)number;
                case string text when double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed):
                    return parsed;
                default:
                    return null;
            }
        }

        internal static long ToMicros(double value, bool alreadyMicros)
        {
            var scaled = alreadyMicros ? value : value * MicrosPerUnit;
            return Convert.ToInt64(Math.Round(scaled, MidpointRounding.AwayFromZero));
        }

        internal static string? ReadString(IDictionary<string, object> values, string key)
        {
            return values.TryGetValue(key, out var value)
                ? Convert.ToString(UnwrapJToken(value), CultureInfo.InvariantCulture)?.Trim()
                : null;
        }

        internal static bool? ReadBoolean(IDictionary<string, object> values, string key)
        {
            if (!values.TryGetValue(key, out var rawValue))
            {
                return null;
            }

            rawValue = UnwrapJToken(rawValue);
            switch (rawValue)
            {
                case bool value:
                    return value;
                case string text when bool.TryParse(text.Trim(), out var parsed):
                    return parsed;
                case string text when text.Trim() == "1":
                    return true;
                case string text when text.Trim() == "0":
                    return false;
                default:
                    var number = CoerceNumber(rawValue);
                    return number.HasValue ? Math.Abs(number.Value) > double.Epsilon : null;
            }
        }

        internal static object? UnwrapJToken(object? value)
        {
            return value switch
            {
                JValue jValue => jValue.Value,
                _ => value,
            };
        }
    }

    internal enum SkanActiveWindow
    {
        Window1,
        Window2,
        Window3,
    }
}
