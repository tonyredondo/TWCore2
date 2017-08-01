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

namespace TWCore.Collections
{
    /// <summary>
    /// Cache node event delegate
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    public delegate void CacheNodeEventDelegate<TKey, TValue>(TKey key, TValue value);
    /// <summary>
    /// Defines a cache collection object
    /// </summary>
    /// <typeparam name="TKey">Key object type</typeparam>
    /// <typeparam name="TValue">Value object type</typeparam>
    public interface ICacheCollection<TKey, TValue>
    {
        /// <summary>
        /// Synchronization object
        /// </summary>
        object SyncLock { get; }
        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value of the key/value pair at the specified index.</returns>
        TValue this[TKey key] { get; set; }
        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value of the key/value pair at the specified index.</returns>
        TValue this[int key] { get; set; }
        /// <summary>
        /// Maximum capacity of the collection
        /// </summary>
        int Capacity { get; }
        /// <summary>
        /// Number of nodes in the collection
        /// </summary>
        int Count { get; }
        /// <summary>
        /// true if the collection is empty; otherwise, false
        /// </summary>
        bool IsEmpty { get; }
        /// <summary>
        /// Number of nodes hitted in the collection
        /// </summary>
        int Hits { get; }
        /// <summary>
        /// Number of nodes deleted in the collection
        /// </summary>
        int Deletes { get; }
        /// <summary>
        /// Number of nodes inserted in the collection
        /// </summary>
        int Inserts { get; }
        /// <summary>
        /// Gets the collection of Keys
        /// </summary>
        ICollection<TKey> Keys { get; }
        /// <summary>
        /// Gets the collection of Values
        /// </summary>
        ICollection<TValue> Values { get; }
        /// <summary>
        /// Removed by capacity event
        /// </summary>
        event CacheNodeEventDelegate<TKey,TValue> NodeRemoved;
        /// <summary>
        /// Adds a key/value pair to the Collection if the key does not already exist, or updates a key/value pair in the Collection 
        /// by using the specified function if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <returns>The new value for the key. This will be either be addValue (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory);
        /// <summary>
        /// Uses the specified functions to add a key/value pair to the Collection if the key does not already exist, 
        /// or to update a key/value pair in the Collection if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <returns>The new value for the key. This will be either be addValue (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory);
        /// <summary>
        /// Clears the collection
        /// </summary>
        void Clear();
        /// <summary>
        /// Determines whether the Collection contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the Collection</param>
        /// <returns>true if the Collection contains an element with the specified key; otherwise, false.</returns>
        bool ContainsKey(TKey key);
        /// <summary>
        /// Adds a key/value pair to the Collection if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">the value to be added, if the key does not already exist</param>
        /// <returns>The value for the key. This will be either the existing value for the key if the key is already in the dictionary, or the new value if the key was not in the dictionary.</returns>
        TValue GetOrAdd(TKey key, TValue value);
        /// <summary>
        /// Adds a key/value pair to the Collection by using the specified function, if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <returns>The value for the key. This will be either the existing value for the key if the key is already in the dictionary, or the new value for the key as returned by valueFactory if the key was not in the dictionary.</returns>
        TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory);
        /// <summary>
        /// Gets the index of an element inside the collection
        /// </summary>
        /// <param name="key">Key of the element</param>
        /// <returns>Index of the element</returns>
        int IndexOf(TKey key);
        /// <summary>
        /// Copies the key and value pairs stored in the Collection to a new array.
        /// </summary>
        /// <returns>A new array containing a snapshot of key and value pairs copied from the Collection.</returns>
        KeyValuePair<TKey, TValue>[] ToArray();
        /// <summary>
        /// Attempts to add the specified key and value to the Collection.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        /// <returns>true if the key/value pair was added to the Collection successfully; false if the key already exists.</returns>
        bool TryAdd(TKey key, TValue value);
        /// <summary>
        /// Attempts to get the value associated with the specified key from the Collection.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the object from the Collection that has the specified key, or the default value of , if the operation failed.</param>
        /// <returns>true if the key was found in the Collection; otherwise, false.</returns>
        bool TryGetValue(TKey key, out TValue value);
        /// <summary>
        /// Attempts to remove and return the value that has the specified key from the Collection.
        /// </summary>
        /// <param name="key">The key of the element to remove and return.</param>
        /// <param name="value">When this method returns, contains the object removed from the Collection, or the default value of the TValue type if key does not exist.</param>
        /// <returns>true if the object was removed successfully; otherwise, false.</returns>
        bool TryRemove(TKey key, out TValue value);
    }
}