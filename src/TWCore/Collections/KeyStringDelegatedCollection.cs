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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace TWCore.Collections
{
    internal class KeyStringDelegatedCollection
    {
        /// <summary>
        /// Default Key array separator
        /// </summary>
        public static string DefaultKeyArraySeparator { get; set; } = ",";
    }
    /// <inheritdoc />
    /// <summary>
    /// Collection of Items where the Key is calculated from a delegate.
    /// </summary>
    /// <typeparam name="TItem">Item type</typeparam>
    [DataContract]
	[Serializable]
    public class KeyStringDelegatedCollection<TItem> : KeyDelegatedCollection<string, TItem>
    {
        #region Properties
        /// <summary>
        /// Key array separator
        /// </summary>
        [XmlAttribute, DataMember]
        public string KeyArraySeparator { get; set; } = KeyStringDelegatedCollection.DefaultKeyArraySeparator;
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyStringDelegatedCollection() { }
        /// <inheritdoc />
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        /// <param name="throwExceptionOnDuplicateKeys">Sets the behavior when adding an item, throwing an exception if the key is duplicated, or ignoring the item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyStringDelegatedCollection(bool throwExceptionOnDuplicateKeys) : base(throwExceptionOnDuplicateKeys) { }
        /// <inheritdoc />
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        /// <param name="enumerable">Enumerable to fill the instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyStringDelegatedCollection(IEnumerable<TItem> enumerable) : base(enumerable) { }
        /// <inheritdoc />
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        /// <param name="enumerable">Enumerable to fill the instance</param>
        /// <param name="throwExceptionOnDuplicateKeys">Sets the behavior when adding an item, throwing an exception if the key is duplicated, or ignoring the item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyStringDelegatedCollection(IEnumerable<TItem> enumerable, bool throwExceptionOnDuplicateKeys) : base(enumerable, throwExceptionOnDuplicateKeys) { }
        /// <inheritdoc />
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyStringDelegatedCollection(Func<TItem, string> keySelector) : base(keySelector) { }
        /// <inheritdoc />
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        /// <param name="keySelector">Key Selector</param>
        /// <param name="throwExceptionOnDuplicateKeys">Sets the behavior when adding an item, throwing an exception if the key is duplicated, or ignoring the item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyStringDelegatedCollection(Func<TItem, string> keySelector, bool throwExceptionOnDuplicateKeys) : base(keySelector, throwExceptionOnDuplicateKeys) { }
        /// <inheritdoc />
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        /// <param name="keySelector">Key Selector</param>
        /// <param name="enumerable">Enumerable to fill the instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyStringDelegatedCollection(Func<TItem, string> keySelector, IEnumerable<TItem> enumerable) : base(keySelector, enumerable) { }
        /// <inheritdoc />
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        /// <param name="keySelector">Key Selector</param>
        /// <param name="enumerable">Enumerable to fill the instance</param>
        /// <param name="throwExceptionOnDuplicateKeys">Sets the behavior when adding an item, throwing an exception if the key is duplicated, or ignoring the item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyStringDelegatedCollection(Func<TItem, string> keySelector, IEnumerable<TItem> enumerable, bool throwExceptionOnDuplicateKeys) :
            base(keySelector, enumerable, throwExceptionOnDuplicateKeys)
        { }
        #endregion

        #region Overrides
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnInit()
        {
            base.OnInit();
            KeySelector = KeySelector ?? DefaultKeySelector ?? (item => IndexOf(item).ToString());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void ClearItems()
        {
            _partialKeys.Clear();
            base.ClearItems();
        }
        #endregion

        #region Public Methods
        private readonly ConcurrentDictionary<string, string> _partialKeys = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// Get if part of the name is contained on the key.
        /// </summary>
        /// <param name="partialKey">Part of the key</param>
        /// <returns>True or false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsPartialKey(string partialKey)
        {
            if (!Contains(partialKey))
            {
                var fullKey = _partialKeys.GetOrAdd(partialKey, pKey =>
                {
                    var kSeparator = new[] { KeyArraySeparator };
                    foreach (var i in Items)
                    {
                        var iFullKey = GetKeyForItem(i);
                        var partials = iFullKey?.Trim()?.Split(kSeparator, StringSplitOptions.RemoveEmptyEntries);
                        if (Array.IndexOf(partials, pKey) > -1)
                            return iFullKey;
                    }
                    return null;
                });
                return fullKey != null;
            }
            _partialKeys.GetOrAdd(partialKey, partialKey);
            return true;
        }

        /// <summary>
        /// Get value from a partial key of the key string array
        /// </summary>
        /// <param name="partialKey">Part of the key</param>
        /// <param name="item">ITem</param>
        /// <returns>True or false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetByPartialKey(string partialKey, out TItem item)
        {
            item = default(TItem);
            if (!ContainsPartialKey(partialKey)) return false;
            item = this[_partialKeys[partialKey]];
            return true;
        }
        #endregion
    }
}
