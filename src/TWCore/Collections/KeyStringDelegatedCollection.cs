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
    /// <summary>
    /// Collection of Items where the Key is calculated from a delegate.
    /// </summary>
    /// <typeparam name="TItem">Item type</typeparam>
    [DataContract]
	[Serializable]
    public class KeyStringDelegatedCollection<TItem> : KeyDelegatedCollection<string, TItem>
    {
        #region Statics
        /// <summary>
        /// Default Key array separator
        /// </summary>
        public static string DefaultKeyArraySeparator { get; set; } = ",";
        #endregion

        #region Properties
        /// <summary>
        /// Key array separator
        /// </summary>
        [XmlAttribute, DataMember]
        public string KeyArraySeparator { get; set; } = DefaultKeyArraySeparator;
        #endregion

        #region .ctor
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyStringDelegatedCollection() { }
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        /// <param name="throwExceptionOnDuplicateKeys">Sets the behavior when adding an item, throwing an exception if the key is duplicated, or ignoring the item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyStringDelegatedCollection(bool throwExceptionOnDuplicateKeys) : base(throwExceptionOnDuplicateKeys) { }
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        /// <param name="enumerable">Enumerable to fill the instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyStringDelegatedCollection(IEnumerable<TItem> enumerable) : base(enumerable) { }
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        /// <param name="enumerable">Enumerable to fill the instance</param>
        /// <param name="throwExceptionOnDuplicateKeys">Sets the behavior when adding an item, throwing an exception if the key is duplicated, or ignoring the item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyStringDelegatedCollection(IEnumerable<TItem> enumerable, bool throwExceptionOnDuplicateKeys) : base(enumerable, throwExceptionOnDuplicateKeys) { }
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyStringDelegatedCollection(Func<TItem, string> keySelector) : base(keySelector) { }
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        /// <param name="throwExceptionOnDuplicateKeys">Sets the behavior when adding an item, throwing an exception if the key is duplicated, or ignoring the item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyStringDelegatedCollection(Func<TItem, string> keySelector, bool throwExceptionOnDuplicateKeys) : base(keySelector, throwExceptionOnDuplicateKeys) { }
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        /// <param name="enumerable">Enumerable to fill the instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyStringDelegatedCollection(Func<TItem, string> keySelector, IEnumerable<TItem> enumerable) : base(keySelector, enumerable) { }
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
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
            KeySelector = KeySelector ?? DefaultKeySelector ?? new Func<TItem, string>(item => IndexOf(item).ToString());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void ClearItems()
        {
            partialKeys.Clear();
            base.ClearItems();
        }
        #endregion

        #region Public Methods
        readonly ConcurrentDictionary<string, string> partialKeys = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// Get if part of the name is contained on the key.
        /// </summary>
        /// <param name="partialKey">Part of the key</param>
        /// <returns>True or false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsPartialKey(string partialKey)
        {
            if (!this.Contains(partialKey))
            {
                var fullKey = partialKeys.GetOrAdd(partialKey, _partialKey =>
                {
                    var _kSeparator = new string[] { KeyArraySeparator };
                    foreach (var i in Items)
                    {
                        var iFullKey = GetKeyForItem(i);
                        var partials = iFullKey?.Trim()?.Split(_kSeparator, StringSplitOptions.RemoveEmptyEntries);
                        if (Array.IndexOf<string>(partials, _partialKey) > -1)
                            return iFullKey;
                    }
                    return null;
                });
                return fullKey != null;
            }
            else
            {
                partialKeys.GetOrAdd(partialKey, partialKey);
                return true;
            }
        }
        /// <summary>
        /// Get value from a partial key of the key string array
        /// </summary>
        /// <param name="partialKey">Part of the key</param>
        /// <returns>True or false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetByPartialKey(string partialKey, out TItem item)
        {
            item = default(TItem);
            if (ContainsPartialKey(partialKey))
            {
                item = this[partialKeys[partialKey]];
                return true;
            }
            return false;
        }
        #endregion
    }
}
