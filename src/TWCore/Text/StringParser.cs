/*
Copyright 2015-2018 Daniel Adrian Redondo Suarez

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
 */

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TWCore.Text
{
    /// <summary>
    /// Parse a string to another type.
    /// </summary>
    /// <typeparam name="T">Result type of the parser</typeparam>
    public static class StringParser<T>
    {
        /// <summary>
        /// Parse a string to another type.
        /// </summary>
        /// <param name="value">String to parse</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="dataFormat">Data format</param>
        /// <param name="provider">Number Provider</param>
        /// <returns>Type instance as result of the string parser</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Parse(string value, T defaultValue, string dataFormat = null, IFormatProvider provider = null)
            => (T)StringParser.Parse(value, typeof(T), defaultValue, dataFormat, provider);
    }

    /// <summary>
    /// Parse a string to another type.
    /// </summary>
    public static class StringParser
    {
        private static readonly ConcurrentDictionary<string, object> Cache = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// Parse a string to another type.
        /// </summary>
        /// <param name="value">String to parse</param>
        /// <param name="returnType">Return type of the parser</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="dataFormat">Data format</param>
        /// <param name="provider">Number Provider</param>
        /// <returns>Type instance as result of the string parser</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Parse(string value, Type returnType, object defaultValue, string dataFormat = null, IFormatProvider provider = null)
        {
            provider = provider ?? CultureInfo.CurrentCulture;
            dataFormat = dataFormat ?? string.Empty;
            if (string.IsNullOrEmpty(value)) return defaultValue;
            var cacheKey = value + "|" + returnType.FullName + "|" + provider?.GetFormat(returnType) + "|" + dataFormat;
            return Cache.GetOrAdd(cacheKey, key =>
            {
                var type = returnType.GetUnderlyingType();
                var resp = defaultValue;

                if (type == typeof(string))
                {
                    resp = value;
                }
                else if (type == typeof(int))
                {
                    if (int.TryParse(value, NumberStyles.Integer, provider, out var o))
                        resp = o;
                }
                else if (type == typeof(byte))
                {
                    if (byte.TryParse(value, NumberStyles.Integer, provider, out var o))
                        resp = o;
                }
                else if (type == typeof(double))
                {
                    if (double.TryParse(value, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent | NumberStyles.Integer | NumberStyles.Float, provider, out var o))
                        resp = o;
                }
                else if (type == typeof(decimal))
                {
                    if (decimal.TryParse(value, NumberStyles.Number, provider, out var o))
                        resp = o;
                }
                else if (type == typeof(float))
                {
                    if (float.TryParse(value, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent | NumberStyles.Integer | NumberStyles.Float, provider, out var o))
                        resp = o;
                }
                else if (type == typeof(long))
                {
                    if (long.TryParse(value, NumberStyles.Integer, provider, out var o))
                        resp = o;
                }
                else if (type == typeof(short))
                {
                    if (short.TryParse(value, NumberStyles.Integer, provider, out var o))
                        resp = o;
                }
                else if (type == typeof(bool))
                {
                    if (bool.TryParse(value, out var o))
                        resp = o;
                }
                else if (type == typeof(uint))
                {
                    if (uint.TryParse(value, NumberStyles.Integer, provider, out var o))
                        resp = o;
                }
                else if (type == typeof(sbyte))
                {
                    if (sbyte.TryParse(value, NumberStyles.Integer, provider, out var o))
                        resp = o;
                }
                else if (type == typeof(ulong))
                {
                    if (ulong.TryParse(value, NumberStyles.Integer, provider, out var o))
                        resp = o;
                }
                else if (type == typeof(ushort))
                {
                    if (ushort.TryParse(value, NumberStyles.Integer, provider, out var o))
                        resp = o;
                }
                else if (type == typeof(DateTime))
                {
                    if (DateTime.TryParseExact(value, dataFormat, provider, DateTimeStyles.None, out var o))
                        resp = o;
                    else if (DateTime.TryParse(value, out o))
                        resp = o;
                }
                else if (type == typeof(TimeSpan))
                {
                    if (TimeSpan.TryParseExact(value, dataFormat, provider, out var o))
                        resp = o;
                    else if (TimeSpan.TryParse(value, out o))
                        resp = o;
                }
                else if (type == typeof(Guid))
                {
                    if (Guid.TryParse(value, out var guid))
                        resp = guid;
                }
                else if (type.GetTypeInfo().IsEnum)
                {
                    if (value == null) return resp;
                    var objValue = Enum.Parse(type, value);
                    resp = objValue;
                }
                return resp;
            });
        }
    }
}
