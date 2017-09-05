/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

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
using System.Collections.Generic;
using TWCore.Collections;
// ReSharper disable CheckNamespace

namespace TWCore
{
    /// <summary>
    /// Extensions for Dictionary classes
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Gets a KeyValue object from a KeyValuePair
        /// </summary>
        /// <param name="item">Item to convert</param>
        /// <returns>KeyValue item instance</returns>
        public static KeyValue<TK, TV> GetKeyValue<TK, TV>(this KeyValuePair<TK, TV> item) 
            => new KeyValue<TK, TV>(item.Key, item.Value);
        /// <summary>
        /// Gets a KeyValueCollection from an IDictionary
        /// </summary>
        /// <param name="dictionary">Dictionary to convert</param>
        /// <returns>KeyValueCollection instance</returns>
        public static KeyValueCollection<TK, TV> GetKeyValueCollection<TK, TV>(this IDictionary<TK, TV> dictionary)
            => new KeyValueCollection<TK, TV>(dictionary);
        /// <summary>
        /// Gets a value from the dictionary, adding and returning a new instance if it is missing.
        /// </summary>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns>The new or existing value.</returns>
        public static TValue GetOrAddValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
        {
            if (dict.TryGetValue(key, out var value))
                return value;
            value = new TValue();
            dict.Add(key, value);
            return value;
        }
        /// <summary>
        /// Gets a value from the dictionary, adding and returning a new instance if it is missing.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="creator">Used to create a new value if necessary</param>
        /// <returns>The new or existing value.</returns>
        public static TValue GetOrAddValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> creator)
        {
            if (dict.TryGetValue(key, out var value))
                return value;
            value = creator();
            dict.Add(key, value);
            return value;
        }
        /// <summary>
        /// Gets a value from the dictionary, returning a default value if it is missing.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns>The value, or a default value.</returns>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            // specification for IDictionary<> requires that the returned value be the default if it fails
            dict.TryGetValue(key, out var value);
            return value;
        }
        /// <summary>
        /// Gets a value from the dictionary, returning the specified default value if it is missing.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value, or a default value.</returns>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
            => dict.TryGetValue(key, out var value) ? value : defaultValue;
        /// <summary>
        /// Gets a value from the dictionary, returning the generated default value if it is missing.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultCreator">The default value generator.</param>
        /// <returns>The value, or a default value.</returns>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> defaultCreator)
            => dict.TryGetValue(key, out var value) ? value : defaultCreator();
    }
}
