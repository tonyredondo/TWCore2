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
using System.Linq;
using System.Runtime.CompilerServices;

namespace TWCore.Collections
{
    /// <inheritdoc />
    /// <summary>
    /// Key/Value Collection
    /// </summary>
    /// <typeparam name="TKey">Key Type</typeparam>
    /// <typeparam name="TValue">Value Type</typeparam>
	[Serializable]
    public class KeyValueCollection<TKey, TValue> : KeyDelegatedCollection<TKey, KeyValue<TKey, TValue>>
    {
        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Key/Value Collection
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValueCollection() : base(i => i.Key) { }
        /// <inheritdoc />
        /// <summary>
        /// Key/Value Collection
        /// </summary>
        /// <param name="dictionary">Dictionary collection to add</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValueCollection(IDictionary<TKey, TValue> dictionary) : base(i => i.Key)
        {
            if (dictionary == null) return;
            foreach (var item in dictionary)
                Add(new KeyValue<TKey, TValue>(item));
        }
        /// <inheritdoc />
        /// <summary>
        /// Key/Value Collection
        /// </summary>
        /// <param name="col">IEnumerable collection to add</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValueCollection(IEnumerable<KeyValue<TKey, TValue>> col) : base(i => i.Key)
        {
            var keyValues = col as KeyValue<TKey, TValue>[] ?? col?.ToArray();
            if (keyValues?.Any() != true) return;
            foreach (var item in keyValues)
                if (!Contains(item.Key))
                    Add(item);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the value for a key, if the value is not found, return null
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        public new TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                TryGet(key, out var item);
#pragma warning disable IDE0030 // Usar propagación de null
                return (item != null) ? item.Value : default(TValue);
#pragma warning restore IDE0030 // Usar propagación de null
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                lock (Ilocker)
                {
                    if (Contains(key))
                        Remove(key);
                    Add(key, value);
                }
            }
        }
        /// <summary>
        /// Add Key/Value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TKey key, TValue value)
            => base.Add(new KeyValue<TKey, TValue>(key, value));

        /// <summary>
        /// Convert the KeyValueCollection to a Dictionary
        /// </summary>
        /// <returns>Dictionary instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TValue> ToDictionary()
            => this.ToDictionary(k => k.Key, v => v.Value);
        #endregion
    }

    /// <summary>
    /// Key/Value Collection
    /// </summary>
	[Serializable]
    public class KeyValueCollection : KeyStringDelegatedCollection<KeyValue<string, string>>
    {
        #region .ctor
        /// <summary>
        /// Key/Value Collection
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValueCollection() : base(i => i.Key) { }
        /// <summary>
        /// Key/Value Collection
        /// </summary>
        /// <param name="throwExceptionOnDuplicateKeys">Sets the behavior when adding an item, throwing an exception if the key is duplicated, or ignoring the item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValueCollection(bool throwExceptionOnDuplicateKeys) : base(i => i.Key, throwExceptionOnDuplicateKeys) { }

        /// <summary>
        /// Key/Value Collection
        /// </summary>
        /// <param name="dictionary">Dictionary collection to add</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValueCollection(IDictionary<string, string> dictionary) : base(i => i.Key)
        {
            if (dictionary == null) return;
            foreach (var item in dictionary)
                Add(new KeyValue<string, string>(item.Key, item.Value));
        }
        /// <summary>
        /// Key/Value Collection
        /// </summary>
        /// <param name="dictionary">Dictionary collection to add</param>
        /// <param name="throwExceptionOnDuplicateKeys">Sets the behavior when adding an item, throwing an exception if the key is duplicated, or ignoring the item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValueCollection(IDictionary<string, string> dictionary, bool throwExceptionOnDuplicateKeys) : base(i => i.Key, throwExceptionOnDuplicateKeys)
        {
            if (dictionary == null) return;
            foreach (var item in dictionary)
                Add(new KeyValue<string, string>(item.Key, item.Value));
        }
        /// <summary>
        /// Key/Value Collection
        /// </summary>
        /// <param name="col">IEnumerable collection to add</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValueCollection(IEnumerable<KeyValue<string, string>> col) : base(i => i.Key)
        {
            var keyValues = col as KeyValue<string, string>[] ?? col?.ToArray();
            if (keyValues?.Any() != true) return;
            foreach (var item in keyValues)
                if (!Contains(item.Key))
                    Add(new KeyValue<string, string>(item.Key, item.Value));
        }
        /// <summary>
        /// Key/Value Collection
        /// </summary>
        /// <param name="col">IEnumerable collection to add</param>
        /// <param name="throwExceptionOnDuplicateKeys">Sets the behavior when adding an item, throwing an exception if the key is duplicated, or ignoring the item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValueCollection(IEnumerable<KeyValue<string, string>> col, bool throwExceptionOnDuplicateKeys) : base(i => i.Key, throwExceptionOnDuplicateKeys)
        {
            var keyValues = col as KeyValue<string, string>[] ?? col?.ToArray();
            if (keyValues?.Any() != true) return;
            foreach (var item in keyValues)
                if (!Contains(item.Key))
                    Add(new KeyValue<string, string>(item.Key, item.Value));
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the value for a key, if the value is not found, return null
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        public new string this[string key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                TryGet(key, out var item);
                return item?.Value;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                lock (Ilocker)
                {
                    if (Contains(key))
                        Remove(key);
                    Add(key, value);
                }
            }
        }
        /// <summary>
        /// Add Key/Value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(string key, string value)
            => base.Add(new KeyValue<string, string>(key, value));

        /// <summary>
        /// Convert the KeyValueCollection to a Dictionary
        /// </summary>
        /// <returns>Dictionary instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<string, string> ToDictionary()
            => this.ToDictionary(k => k.Key, v => v.Value);
        #endregion
    }
}
