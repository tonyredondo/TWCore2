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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable EventNeverSubscribedTo.Global

namespace TWCore.Collections
{
    /// <summary>
    /// Cache Collection ValueNode Base
    /// </summary>
    /// <typeparam name="TValue">Value type</typeparam>
    public abstract class CacheCollectionValueNode<TValue>
    {
        /// <summary>
        /// Value of the Node
        /// </summary>
        public TValue Value;
        /// <summary>
        /// Create a new Cache collection ValueNode with the given value
        /// </summary>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected CacheCollectionValueNode(TValue value)
        {
            Value = value;
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Cache Collection Object Base
    /// </summary>
    /// <typeparam name="TKey">Collection Key</typeparam>
    /// <typeparam name="TValue">Collection Value</typeparam>
    /// <typeparam name="TValueNode">Valuenode type</typeparam>
    public abstract class CacheCollectionBase<TKey, TValue, TValueNode> : ICacheCollection<TKey, TValue> where TValueNode : CacheCollectionValueNode<TValue>
    {
        #region Fields
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly object _padlock = new object();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _deletes;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _inserts;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _hits;

        /// <summary>
        /// Value Storage
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected readonly Dictionary<TKey, TValueNode> ValueStorage;
        #endregion

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Maximum capacity of the collection
        /// </summary>
        public int Capacity { get; }
        /// <inheritdoc />
        /// <summary>
        /// Number of nodes deleted in the collection
        /// </summary>
        public int Deletes => _deletes;
        /// <inheritdoc />
        /// <summary>
        /// Number of nodes hitted in the collection
        /// </summary>
        public int Hits => _hits;
        /// <inheritdoc />
        /// <summary>
        /// Number of nodes inserted in the collection
        /// </summary>
        public int Inserts => _inserts;
        /// <inheritdoc />
        /// <summary>
        /// Synchronization object
        /// </summary>
        public object SyncLock => _padlock;
        #endregion

        #region Events
        /// <summary>
        /// Node Hitted event
        /// </summary>
        public event CacheNodeEventDelegate<TKey, TValue> NodeHitted;
        /// <inheritdoc />
        /// <summary>
        /// Node Removed event
        /// </summary>
        public event CacheNodeEventDelegate<TKey, TValue> NodeRemoved;
        /// <summary>
        /// Node Inserted event
        /// </summary>
        public event CacheNodeEventDelegate<TKey, TValue> NodeInserted;
        #endregion

        #region .ctor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Collection Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected CacheCollectionBase(int capacity)
        {
            Ensure.GreaterThan(capacity, 0, "Capacity should be greater than 0");
            ValueStorage = new Dictionary<TKey, TValueNode>();
            Capacity = capacity;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Collection Capacity</param>
        /// <param name="comparer">Comparer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected CacheCollectionBase(int capacity, IEqualityComparer<TKey> comparer)
        {
            Ensure.GreaterThan(capacity, 0, "Capacity should be greater than 0");
            ValueStorage = new Dictionary<TKey, TValueNode>(comparer);
            Capacity = capacity;
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Increment hits value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ReportHit(TKey key, TValue value)
        {
            Interlocked.CompareExchange(ref _hits, 0, int.MaxValue);
            Interlocked.Increment(ref _hits);
            NodeHitted?.Invoke(key, value);
        }
        /// <summary>
        /// Increment deletes value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ReportDelete(TKey key, TValue value)
        {
            Interlocked.CompareExchange(ref _deletes, 0, int.MaxValue);
            Interlocked.Increment(ref _deletes);
            NodeRemoved?.Invoke(key, value);
        }
        /// <summary>
        /// Increment inserts value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ReportInsert(TKey key, TValue value)
        {
            Interlocked.CompareExchange(ref _inserts, 0, int.MaxValue);
            Interlocked.Increment(ref _inserts);
            NodeInserted?.Invoke(key, value);
        }
        #endregion

        #region Abstract/Virtual Methods
        /// <summary>
        /// Update List algorithm method
        /// </summary>
        /// <param name="key">Key object</param>
        /// <param name="node">Value node</param>
        protected abstract void UpdateList(TKey key, TValueNode node);
        /// <summary>
        /// Create TValueNode instance
        /// </summary>
        /// <param name="key">Key object instance</param>
        /// <param name="value">Value object instance</param>
        /// <returns>TValueNode object</returns>
        protected abstract TValueNode CreateNode(TKey key, TValue value);
        /// <summary>
        /// Clean internals lists
        /// </summary>
        protected abstract void OnClean();
        /// <summary>
        /// Remove Item
        /// </summary>
        protected abstract void OnNodeRemove(TValueNode node);
        /// <summary>
        /// Get Key from an index value
        /// </summary>
        /// <param name="index">Index of the key</param>
        /// <param name="key">Key value</param>
        /// <returns>True or False if the key was found</returns>
        protected virtual bool OnGetKey(int index, out TKey key) => throw new NotSupportedException();
        /// <summary>
        /// Get the index from a key value
        /// </summary>
        /// <param name="key">Key value</param>
        /// <param name="index">Index of the key</param>
        /// <returns></returns>
        protected virtual bool OnGetIndex(TKey key, out int index) => throw new NotSupportedException();
        #endregion

        #region ICacheCollection
        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value of the key/value pair at the specified index.</returns>
        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                lock (_padlock)
                {
                    if (!ValueStorage.TryGetValue(key, out var nValue)) throw new KeyNotFoundException();
                    UpdateList(key, nValue);
                    return nValue.Value;
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                lock (_padlock)
                {
                    if (ValueStorage.TryGetValue(key, out var nValue))
                        nValue.Value = value;
                    else
                    {
                        nValue = CreateNode(key, value);
                        ValueStorage[key] = nValue;
                    }
                    UpdateList(key, nValue);
                }
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <returns>The value of the key/value pair at the specified index.</returns>
        public TValue this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                lock (_padlock)
                {
                    if (!OnGetKey(index, out var key))
                        return default(TValue);
                    var nValue = ValueStorage[key];
                    UpdateList(key, nValue);
                    return nValue.Value;
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                lock (_padlock)
                {
                    if (!OnGetKey(index, out var key)) return;
                    var nValue = CreateNode(key, value);
                    ValueStorage[key] = nValue;
                    UpdateList(key, nValue);
                }
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Number of nodes in the collection
        /// </summary>
        public int Count => ValueStorage.Count;
        /// <inheritdoc />
        /// <summary>
        /// true if the collection is empty; otherwise, false
        /// </summary>
        public bool IsEmpty => ValueStorage.Count == 0;
        /// <inheritdoc />
        /// <summary>
        /// Gets the collection of Keys
        /// </summary>
        public ICollection<TKey> Keys
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                lock (_padlock)
					return ValueStorage.Keys.ToList();
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the collection of Values
        /// </summary>
        public ICollection<TValue> Values
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                lock (_padlock)
                    return ValueStorage.Values.Select(v => v.Value).ToList();
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Uses the specified functions to add a key/value pair to the Collection if the key does not already exist, 
        /// or to update a key/value pair in the Collection if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <returns>The new value for the key. This will be either be addValue (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            TValue res;
            lock (_padlock)
            {
                if (ValueStorage.TryGetValue(key, out var nValue))
                    nValue.Value = updateValueFactory(key, nValue.Value);
                else
                {
                    nValue = CreateNode(key, addValueFactory(key));
                    ValueStorage[key] = nValue;
                }
                UpdateList(key, nValue);
                res = nValue.Value;
            }
            return res;
        }
        /// <inheritdoc />
        /// <summary>
        /// Adds a key/value pair to the Collection if the key does not already exist, or updates a key/value pair in the Collection 
        /// by using the specified function if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <returns>The new value for the key. This will be either be addValue (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            TValue res;
            lock (_padlock)
            {
                if (ValueStorage.TryGetValue(key, out var nValue))
                    nValue.Value = updateValueFactory(key, nValue.Value);
                else
                {
                    nValue = CreateNode(key, addValue);
                    ValueStorage[key] = nValue;
                }
                UpdateList(key, nValue);
                res = nValue.Value;
            }
            return res;
        }
        /// <inheritdoc />
        /// <summary>
        /// Clears the collection
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            lock (_padlock)
            {
                ValueStorage.Clear();
                OnClean();
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Determines whether the Collection contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the Collection</param>
        /// <returns>true if the Collection contains an element with the specified key; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key)
        {
            lock (_padlock)
                return ValueStorage.ContainsKey(key);
        }
        /// <inheritdoc />
        /// <summary>
        /// Adds a key/value pair to the Collection by using the specified function, if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <returns>The value for the key. This will be either the existing value for the key if the key is already in the dictionary, or the new value for the key as returned by valueFactory if the key was not in the dictionary.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            TValue res;
            lock (_padlock)
            {
                if (!ValueStorage.TryGetValue(key, out var nValue))
                {
                    nValue = CreateNode(key, valueFactory(key));
                    ValueStorage[key] = nValue;
                }
                UpdateList(key, nValue);
                res = nValue.Value;
            }
            return res;
        }
        /// <inheritdoc />
        /// <summary>
        /// Adds a key/value pair to the Collection if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">the value to be added, if the key does not already exist</param>
        /// <returns>The value for the key. This will be either the existing value for the key if the key is already in the dictionary, or the new value if the key was not in the dictionary.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetOrAdd(TKey key, TValue value)
        {
            TValue res;
            lock (_padlock)
            {
                if (!ValueStorage.TryGetValue(key, out var nValue))
                {
                    nValue = CreateNode(key, value);
                    ValueStorage[key] = nValue;
                }
                UpdateList(key, nValue);
                res = nValue.Value;
            }
            return res;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the index of an element inside the collection
        /// </summary>
        /// <param name="key">Key of the element</param>
        /// <returns>Index of the element</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(TKey key)
        {
            lock (_padlock)
            {
                if (OnGetIndex(key, out var idx))
                    return idx;
            }
            return -1;
        }
        /// <inheritdoc />
        /// <summary>
        /// Copies the key and value pairs stored in the Collection to a new array.
        /// </summary>
        /// <returns>A new array containing a snapshot of key and value pairs copied from the Collection.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValuePair<TKey, TValue>[] ToArray()
        {
            lock (_padlock)
                return ValueStorage.Select(i => new KeyValuePair<TKey, TValue>(i.Key, i.Value.Value)).ToArray();
        }
        /// <inheritdoc />
        /// <summary>
        /// Attempts to add the specified key and value to the Collection.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        /// <returns>true if the key/value pair was added to the Collection successfully; false if the key already exists.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(TKey key, TValue value)
        {
            lock (_padlock)
            {
                if (ValueStorage.TryGetValue(key, out var nValue)) return false;
                nValue = CreateNode(key, value);
                ValueStorage[key] = nValue;
                UpdateList(key, nValue);
                return true;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Attempts to get the value associated with the specified key from the Collection.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the object from the Collection that has the specified key, or the default value of , if the operation failed.</param>
        /// <returns>true if the key was found in the Collection; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_padlock)
            {
                if (ValueStorage.TryGetValue(key, out var nValue))
                {
                    UpdateList(key, nValue);
                    value = nValue.Value;
                    return true;
                }
            }
            value = default(TValue);
            return false;
        }
        /// <inheritdoc />
        /// <summary>
        /// Attempts to remove and return the value that has the specified key from the Collection.
        /// </summary>
        /// <param name="key">The key of the element to remove and return.</param>
        /// <param name="value">When this method returns, contains the object removed from the Collection, or the default value of the TValue type if key does not exist.</param>
        /// <returns>true if the object was removed successfully; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(TKey key, out TValue value)
        {
            lock (_padlock)
            {
                if (ValueStorage.TryGetValue(key, out var nValue))
                {
                    ValueStorage.Remove(key);
                    OnNodeRemove(nValue);
                    value = nValue.Value;
                    NodeRemoved?.Invoke(key, value);
                    return true;
                }
            }
            value = default(TValue);
            return false;
        }
        #endregion
    }
}
