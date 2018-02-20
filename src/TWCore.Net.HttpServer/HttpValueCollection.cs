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
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TWCore.Collections;
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable MemberCanBeProtected.Global

namespace TWCore.Net.HttpServer
{
    /// <inheritdoc />
    /// <summary>
    /// Http value collection
    /// </summary>
    public class HttpValueCollection : Collection<KeyValue>
    {
        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Http value collection
        /// </summary>
        /// <param name="query">String with the encoded values</param>
        /// <param name="urlencoded">Indicate if the string is url encoded</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HttpValueCollection(string query = null, bool urlencoded = true)
        {
            if (!string.IsNullOrEmpty(query))
                FillFromString(query, urlencoded);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets the value for a key
        /// </summary>
        /// <param name="key">Item key</param>
        /// <returns>Item value</returns>
        public string this[string key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var item = this.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
                return item?.Value;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Add a Key/Value pair to the end of the collection
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="value">Item value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(string key, string value) => Add(new KeyValue(key, value));
        /// <summary>
        /// Indicate if the collection has a item with the key specified
        /// </summary>
        /// <param name="key">Item key</param>
        /// <returns>true if the collection has the item; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(string key) => this.Any(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
        /// <summary>
        /// Gets the item values of a key
        /// </summary>
        /// <param name="key">Item key</param>
        /// <returns>Item values</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] GetValues(string key)
            => this.Where(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).Select(x => x.Value).ToArray();
        /// <summary>
        /// Removes items with a key
        /// </summary>
        /// <param name="key">Item key</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(string key)
        {
            this.Where(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).ToArray().Each(x => base.Remove(x));
        }

        /// <summary>
        /// Returns a string that represent the current object
        /// </summary>
        /// <returns>String that represent the current object</returns>
        public override string ToString() => ToString(true);

        /// <summary>
        /// Returns a string that represent the current object
        /// </summary>
        /// <returns>String that represent the current object</returns>
        /// <param name="urlencoded">Indicate if the string has to be url encoded</param>
        /// <param name="excludeKeys">IDictionary to exclude some keys from the string representation</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual string ToString(bool urlencoded, IDictionary excludeKeys = null)
        {
            if (Count == 0)
                return string.Empty;

            var stringBuilder = new StringBuilder();
            foreach (var item in this)
            {
                var key = item.Key;

                if ((excludeKeys != null) && excludeKeys.Contains(key)) continue;
                var value = item.Value;

                if (urlencoded)
                    key = Uri.EscapeDataString(key);

                if (stringBuilder.Length > 0)
                    stringBuilder.Append('&');

                stringBuilder.Append((key != null) ? (key + "=") : string.Empty);

                if (string.IsNullOrEmpty(value)) continue;
                if (urlencoded)
                    value = Uri.EscapeDataString(value);
                stringBuilder.Append(value);
            }
            return stringBuilder.ToString();
        }

        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FillFromString(string query, bool urlencoded)
        {
            query.SplitAndTrim('&').Select(i => i.SplitAndTrim('=').ToArray()).Select(i => new KeyValue(
                    (urlencoded ? Uri.UnescapeDataString(i[0]) : i[0]).Replace("+", " "),
                    (urlencoded ? Uri.UnescapeDataString(i[1]) : i[1]).Replace("+", " "))).Each(i => base.Add(i));
        }
        #endregion
    }
}
