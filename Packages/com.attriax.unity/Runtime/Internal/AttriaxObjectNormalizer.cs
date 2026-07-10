#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Attriax.Unity.Internal
{
    internal static class AttriaxObjectNormalizer
    {
        public static Dictionary<string, object>? NormalizeObjectMap(IDictionary<string, object>? input)
        {
            if (input == null || input.Count == 0)
            {
                return null;
            }

            var output = new Dictionary<string, object>(input.Count);
            foreach (var pair in input)
            {
                output[pair.Key] = NormalizeObjectValue(pair.Value);
            }

            return output;
        }

        public static Dictionary<string, object>? NormalizeObjectMap(IDictionary<string, string>? input)
        {
            if (input == null || input.Count == 0)
            {
                return null;
            }

            var output = new Dictionary<string, object>(input.Count);
            foreach (var pair in input)
            {
                output[pair.Key] = pair.Value;
            }

            return output;
        }

        private static object NormalizeObjectValue(object? value)
        {
            if (value == null)
            {
                return null!;
            }

            if (value is JValue jsonValue)
            {
                return NormalizeObjectValue(jsonValue.Value);
            }

            if (value is JObject jsonObject)
            {
                var output = new Dictionary<string, object>(jsonObject.Count);
                foreach (var property in jsonObject.Properties())
                {
                    output[property.Name] = NormalizeObjectValue(property.Value);
                }

                return output;
            }

            if (value is JArray jsonArray)
            {
                var array = new List<object>(jsonArray.Count);
                foreach (var item in jsonArray)
                {
                    array.Add(NormalizeObjectValue(item));
                }

                return array;
            }

            if (value is JToken token)
            {
                return token.ToString();
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
                return dateTimeOffset.ToString("o", CultureInfo.InvariantCulture);
            }

            if (value is DateTime dateTime)
            {
                return dateTime.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
            }

            if (value is Guid guid)
            {
                return guid.ToString("D", CultureInfo.InvariantCulture);
            }

            if (value is Uri uri)
            {
                return uri.ToString();
            }

            if (value is IDictionary dictionary)
            {
                var output = new Dictionary<string, object>();
                foreach (DictionaryEntry entry in dictionary)
                {
                    output[entry.Key.ToString()] = NormalizeObjectValue(entry.Value);
                }

                return output;
            }

            if (value is IEnumerable enumerable && value is not string)
            {
                var array = new List<object>();
                foreach (var item in enumerable)
                {
                    array.Add(NormalizeObjectValue(item));
                }

                return array;
            }

            return value.ToString();
        }
    }
}