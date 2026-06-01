#nullable enable
using System;
using System.Collections.Generic;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxSanitizedUserPropertyUpdate
    {
        public Dictionary<string, object>? Properties { get; set; }

        public List<string>? ClearPropertyKeys { get; set; }

        public bool IsEmpty =>
            (Properties == null || Properties.Count == 0) &&
            (ClearPropertyKeys == null || ClearPropertyKeys.Count == 0);
    }

    internal static class AttriaxUserPropertySanitizer
    {
        private const int MaxUserPropertyKeys = 30;
        private const int MaxUserPropertyValueLength = 256;

        public static AttriaxSanitizedUserPropertyUpdate SanitizeUpdate(IDictionary<string, object?> input)
        {
            var properties = new Dictionary<string, object>();
            var clearPropertyKeys = new List<string>();

            foreach (var pair in input)
            {
                var normalizedKey = pair.Key?.Trim();
                if (string.IsNullOrWhiteSpace(normalizedKey))
                {
                    continue;
                }

                if (pair.Value == null)
                {
                    properties.Remove(normalizedKey);
                    if (!clearPropertyKeys.Contains(normalizedKey))
                    {
                        clearPropertyKeys.Add(normalizedKey);
                    }

                    continue;
                }

                if (!TryNormalizePropertyValue(pair.Value, out var normalizedValue))
                {
                    continue;
                }

                clearPropertyKeys.Remove(normalizedKey);
                if (!properties.ContainsKey(normalizedKey) && properties.Count >= MaxUserPropertyKeys)
                {
                    continue;
                }

                properties[normalizedKey] = normalizedValue;
            }

            return new AttriaxSanitizedUserPropertyUpdate
            {
                Properties = properties.Count > 0 ? properties : null,
                ClearPropertyKeys = clearPropertyKeys.Count > 0 ? clearPropertyKeys : null,
            };
        }

        public static AttriaxSetUserOptions SanitizeSetUserOptions(AttriaxSetUserOptions options)
        {
            return new AttriaxSetUserOptions
            {
                ExternalUserName = options.ExternalUserName,
                Properties = SanitizeProperties(options.Properties),
                ClearPropertyKeys = NormalizePropertyKeys(options.ClearPropertyKeys),
                ClearAllProperties = options.ClearAllProperties,
            };
        }

        public static Dictionary<string, object>? SanitizeProperties(IDictionary<string, object>? input)
        {
            if (input == null || input.Count == 0)
            {
                return null;
            }

            var properties = new Dictionary<string, object>();
            foreach (var pair in input)
            {
                var normalizedKey = pair.Key?.Trim();
                if (string.IsNullOrWhiteSpace(normalizedKey))
                {
                    continue;
                }

                if (!TryNormalizePropertyValue(pair.Value, out var normalizedValue))
                {
                    continue;
                }

                if (!properties.ContainsKey(normalizedKey) && properties.Count >= MaxUserPropertyKeys)
                {
                    continue;
                }

                properties[normalizedKey] = normalizedValue;
            }

            return properties.Count > 0 ? properties : null;
        }

        public static List<string>? NormalizePropertyKeys(IEnumerable<string>? propertyNames)
        {
            if (propertyNames == null)
            {
                return null;
            }

            var normalizedPropertyNames = new List<string>();
            foreach (var propertyName in propertyNames)
            {
                var normalizedPropertyName = propertyName?.Trim();
                if (string.IsNullOrWhiteSpace(normalizedPropertyName) ||
                    normalizedPropertyNames.Contains(normalizedPropertyName))
                {
                    continue;
                }

                normalizedPropertyNames.Add(normalizedPropertyName);
            }

            return normalizedPropertyNames.Count > 0 ? normalizedPropertyNames : null;
        }

        private static bool TryNormalizePropertyValue(object? value, out object normalizedValue)
        {
            switch (value)
            {
                case string stringValue:
                    normalizedValue = stringValue.Length <= MaxUserPropertyValueLength
                        ? stringValue
                        : stringValue.Substring(0, MaxUserPropertyValueLength);
                    return true;
                case bool boolValue:
                    normalizedValue = boolValue;
                    return true;
                case byte byteValue:
                    normalizedValue = byteValue;
                    return true;
                case sbyte sbyteValue:
                    normalizedValue = sbyteValue;
                    return true;
                case short shortValue:
                    normalizedValue = shortValue;
                    return true;
                case ushort ushortValue:
                    normalizedValue = ushortValue;
                    return true;
                case int intValue:
                    normalizedValue = intValue;
                    return true;
                case uint uintValue:
                    normalizedValue = uintValue;
                    return true;
                case long longValue:
                    normalizedValue = longValue;
                    return true;
                case ulong ulongValue:
                    normalizedValue = ulongValue;
                    return true;
                case float floatValue when !float.IsNaN(floatValue) && !float.IsInfinity(floatValue):
                    normalizedValue = floatValue;
                    return true;
                case double doubleValue when !double.IsNaN(doubleValue) && !double.IsInfinity(doubleValue):
                    normalizedValue = doubleValue;
                    return true;
                case decimal decimalValue:
                    normalizedValue = decimalValue;
                    return true;
                default:
                    normalizedValue = null!;
                    return false;
            }
        }
    }
}