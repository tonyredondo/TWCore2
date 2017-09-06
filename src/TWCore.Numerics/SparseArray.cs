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

namespace TWCore.Numerics
{
    /// <inheritdoc />
    /// <summary>
    /// This class implements a sparse array.
    /// </summary>
    /// <typeparam name="TKey">The key type used to index the array items</typeparam>
    /// <typeparam name="TValue">The type of the array values</typeparam>
    public class SparseArray<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : IComparable<TKey>
    {
        private readonly Dictionary<TKey, TValue> _mDictionary;

        /// <summary>
        /// This property stores the default value that is returned if the key doesn't exist.
        /// </summary>
        public TValue DefaultValue { get; set; }

        /// <summary>
        /// Property to get the count of items in the sparse array.
        /// </summary>
        public int Count => _mDictionary.Count;

        #region Constructors
        /// <summary>
        /// Constructor - creates an empty sparse array instance.
        /// </summary>
        public SparseArray()
        {
            _mDictionary = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="defaultValue">A default value to return if the key is not present.</param>
        public SparseArray(TValue defaultValue)
        {
            _mDictionary = new Dictionary<TKey, TValue>();
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="sparseArray">The sparse array instance to be copied</param>
        public SparseArray(SparseArray<TKey, TValue> sparseArray)
        {
            _mDictionary = new Dictionary<TKey, TValue>();
            Initialize(sparseArray);
            DefaultValue = sparseArray.DefaultValue;
        }

        #endregion

        /// <summary>
        /// Method to copy the data in another SparseArray instance to this instance.
        /// </summary>
        /// <param name="sparseArray">An instance of the SparseArray class.</param>
        private void Initialize(SparseArray<TKey, TValue> sparseArray)
        {
            _mDictionary.Clear();

            // Copy each key value pair to the new dictionary.
            foreach (KeyValuePair<TKey, TValue> pair in sparseArray)
                _mDictionary.Add(pair.Key, pair.Value);
        }

        /// <summary>
        /// Method to copy the data in this SparseArray instance to another instance.
        /// </summary>
        /// <param name="sparseArray">An instance of the SparseArray class.</param>
        public void CopyTo(SparseArray<TKey, TValue> sparseArray)
        {
            sparseArray._mDictionary.Clear();

            // Copy each key value pair to the new dictionary.
            foreach (KeyValuePair<TKey, TValue> pair in _mDictionary)
                sparseArray._mDictionary.Add(pair.Key, pair.Value);
        }

        /// <summary>
        /// Property []
        /// </summary>
        /// <param name="key">The key used to index the value</param>
        /// <returns>The 'get' property returns the value at the current key</returns>
        public TValue this[TKey key]
        {
            get
            {
                if (!_mDictionary.TryGetValue(key, out var value))
                    value = DefaultValue;

                return value;
            }
            set => _mDictionary[key] = value;
        }

        /// <summary>
        /// Determines whether this sparse array contains the specified key.
        /// </summary>
        /// <param name="key">A key</param>
        /// <returns>Returns the value 'true' if and only if the key exists in this sparse array</returns>
        public bool ContainsKey(TKey key) => _mDictionary.ContainsKey(key);

        /// <summary>
        /// Determines whether this sparse array contains the specified value.
        /// </summary>
        /// <param name="value">A value</param>
        /// <returns>Returns the value 'true' if and only if the value exists in this sparse array</returns>
        public bool ContainsValue(TValue value) => _mDictionary.ContainsValue(value);

        /// <summary>
        /// Gets the value for the associated key.
        /// </summary>
        /// <param name="key">The key of the value to get</param>
        /// <param name="value">An out parameter that contains the value if the key exists</param>
        /// <returns>Returns the value 'true' if and only if the key exists in this sparse array</returns>
        public bool TryGetValue(TKey key, out TValue value) => _mDictionary.TryGetValue(key, out value);

        /// <summary>
        /// Removes the value with the specified key from this sparse array instance.
        /// </summary>
        /// <param name="key">The key of the element to remove</param>
        /// <returns>The value 'true' if and only if the element is successfully found and removed.</returns>
        public bool Remove(TKey key) => _mDictionary.Remove(key);

        /// <summary>
        /// Method to clear all values in the sparse array.
        /// </summary>
        public void Clear() => _mDictionary.Clear();

        #region IEnumerable<KeyValuePair<TKey, TValue>> Members

        /// <summary>
        /// The Generic IEnumerator GetEnumerator method
        /// </summary>
        /// <returns>An enumerator to iterate over all key-value pairs in this sparse array</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _mDictionary.GetEnumerator();

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// The non-generic IEnumerator GetEnumerator method
        /// </summary>
        /// <returns>An enumerator to iterate over all key-value pairs in this sparse array</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _mDictionary.Values.GetEnumerator();

        /// <summary>
        /// Property to get the key-value pair at the current enumerator position.
        /// </summary>
        public TValue Current => _mDictionary.Values.GetEnumerator().Current;

        /// <summary>
        /// Method to move to the next enumerator position.
        /// </summary>
        /// <returns>true if the move on the enumerator could be done; otherwise, false.</returns>
        public bool MoveNext() => _mDictionary.Values.GetEnumerator().MoveNext();

        #endregion
    }

}
