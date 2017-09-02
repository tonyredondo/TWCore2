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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace TWCore.Collections
{
    /// <summary>
    /// Collection of Items where the Key is calculated from a delegate.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TItem">Item type</typeparam>
    [DataContract]
	[Serializable]
    public class KeyDelegatedCollection<TKey, TItem> : KeyedCollection<TKey, TItem>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected object Ilocker = new object();

        #region Statics
        /// <summary>
        /// Default Key Selector
        /// </summary>
        public static Func<TItem, TKey> DefaultKeySelector { get; set; }
        /// <summary>
        /// Default Combine delegate
        /// </summary>
        public static Func<TItem, TItem, TItem> DefaultCombineDelegate { get; set; }
        #endregion

        #region Properties
        /// <summary>
        /// Key Selector
        /// </summary>
        public Func<TItem, TKey> KeySelector = DefaultKeySelector;
        /// <summary>
        /// Combine delegate
        /// </summary>
        public Func<TItem, TItem, TItem> CombineDelegate = DefaultCombineDelegate;
        /// <summary>
        /// Gets or sets the behavior when adding an item, throwing an exception if the key is duplicated, or ignoring the item.
        /// </summary>
        [XmlAttribute, DataMember]
        public bool ThrowExceptionOnDuplicateKeys { get; set; } = true;
        #endregion

        #region .ctor
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyDelegatedCollection()
        {
            OnInit();
        }
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        /// <param name="throwExceptionOnDuplicateKeys">Sets the behavior when adding an item, throwing an exception if the key is duplicated, or ignoring the item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyDelegatedCollection(bool throwExceptionOnDuplicateKeys)
        {
            ThrowExceptionOnDuplicateKeys = throwExceptionOnDuplicateKeys;
            OnInit();
        }
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        /// <param name="enumerable">Enumerable to fill the instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyDelegatedCollection(IEnumerable<TItem> enumerable)
        {
            OnInit();
            AddRange(enumerable);
        }
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        /// <param name="enumerable">Enumerable to fill the instance</param>
        /// <param name="throwExceptionOnDuplicateKeys">Sets the behavior when adding an item, throwing an exception if the key is duplicated, or ignoring the item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyDelegatedCollection(IEnumerable<TItem> enumerable, bool throwExceptionOnDuplicateKeys) : this(throwExceptionOnDuplicateKeys)
        {
            AddRange(enumerable);
        }
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyDelegatedCollection(Func<TItem, TKey> keySelector)
        {
            KeySelector = keySelector;
            if (DefaultKeySelector == null && KeySelector != null)
                DefaultKeySelector = KeySelector;
            OnInit();
        }
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        /// <param name="keySelector">Key selector</param>
        /// <param name="throwExceptionOnDuplicateKeys">Sets the behavior when adding an item, throwing an exception if the key is duplicated, or ignoring the item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyDelegatedCollection(Func<TItem, TKey> keySelector, bool throwExceptionOnDuplicateKeys) 
        {
            ThrowExceptionOnDuplicateKeys = throwExceptionOnDuplicateKeys;
            KeySelector = keySelector;
            if (DefaultKeySelector == null && KeySelector != null)
                DefaultKeySelector = KeySelector;
            OnInit();
        }
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        /// <param name="keySelector">Key selector</param>
        /// <param name="enumerable">Enumerable to fill the instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyDelegatedCollection(Func<TItem, TKey> keySelector, IEnumerable<TItem> enumerable) : this(keySelector)
        {
            AddRange(enumerable);
        }
        /// <summary>
        /// Collection of Items where the Key is calculated from a delegate.
        /// </summary>
        /// <param name="keySelector">Key selector</param>
        /// <param name="enumerable">Enumerable to fill the instance</param>
        /// <param name="throwExceptionOnDuplicateKeys">Sets the behavior when adding an item, throwing an exception if the key is duplicated, or ignoring the item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyDelegatedCollection(Func<TItem, TKey> keySelector, IEnumerable<TItem> enumerable, bool throwExceptionOnDuplicateKeys) : this(keySelector, throwExceptionOnDuplicateKeys)
        {
            AddRange(enumerable);
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Method for initialize the instance, runs after the constructor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnInit()
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Add an item to the Collection
        /// </summary>
        /// <param name="item">Item to be added</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new void Add(TItem item)
        {
            if (ThrowExceptionOnDuplicateKeys)
                base.Add(item);
            else
            {
                lock (Ilocker)
                {
                    if (!Contains(KeySelector(item)))
                        base.Add(item);
                }
            }
        }
        /// <summary>
        /// Adds a range of items to the collection
        /// </summary>
        /// <param name="enumerable">IEnumerable instance to be added.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(IEnumerable<TItem> enumerable)
        {
            if (enumerable != null)
            {
                foreach (var item in enumerable)
                    Add(item);
            }
        }
        /// <summary>
        /// Add an item to the collection and if there is an item with the same key, combine it.
        /// </summary>
        /// <param name="item">Item to add or combine with</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddAndCombine(TItem item)
        {
            var key = KeySelector(item);
            lock (Ilocker)
            {
                if (Contains(key))
                {
                    var oItem = this[key];
                    var nItem = CombineDelegate(oItem, item);
                    Remove(key);
                    base.Add(nItem);
                }
                else
                    base.Add(item);
            }
        }

        /// <summary>
        /// Adds an IEnumerable to the collection and if there is items with the same key, combine it.
        /// </summary>
        /// <param name="enumerable">Enumerable</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddAndCombine(IEnumerable<TItem> enumerable)
        {
            if (enumerable == null) return;
            foreach (var item in enumerable)
                AddAndCombine(item);
        }
        /// <summary>
        /// Try Gets a value inside the collection
        /// </summary>
        /// <param name="key">Key to look at</param>
        /// <param name="item">Item output</param>
        /// <returns>True if the item was found, otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet(TKey key, out TItem item)
        {
            item = default(TItem);
            lock (Ilocker)
            {
                if (!Contains(key)) return false;
                item = this[key];
                return true;
            }
        }
        #endregion

        #region Public Static Methods
        /// <summary>
        /// Gets a KeyDelegatedCollection instance from a combination of two collections.
        /// </summary>
        /// <param name="a">Initial KeyDelegateCollection</param>
        /// <param name="b">Second KeyDelegateCollection</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static KeyDelegatedCollection<TKey, TItem> Combine(KeyDelegatedCollection<TKey, TItem> a, KeyDelegatedCollection<TKey, TItem> b)
        {
            var result = new KeyDelegatedCollection<TKey, TItem>(a.KeySelector, a, a.ThrowExceptionOnDuplicateKeys);
            result.AddAndCombine(b);
            return result;
        }
        #endregion

        #region Overrides
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TKey GetKeyForItem(TItem item)
        {
            var selector = KeySelector ?? DefaultKeySelector;
            Ensure.ReferenceNotNull(selector, "A Key selector for this items can't be found, please declare a local or the default one.");
            return selector(item);
        }
        #endregion
    }
}