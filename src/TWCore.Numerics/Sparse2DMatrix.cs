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
using System.Collections;
using System.Collections.Generic;
// ReSharper disable RedundantAssignment

namespace TWCore.Numerics
{
    /// <summary>
    /// This class implements a sparse 2 dimensional matrix.
    /// </summary>
    /// <typeparam name="TKey0">The first key type used to index the array items</typeparam>
    /// <typeparam name="TKey1">The second key type used to index the array items</typeparam>
    /// <typeparam name="TValue">The type of the array values</typeparam>
    public class Sparse2DMatrix<TKey0, TKey1, TValue> : IEnumerable<KeyValuePair<ComparableTuple2<TKey0, TKey1>, TValue>>
        where TKey0 : IComparable<TKey0>
        where TKey1 : IComparable<TKey1>
    {
        private Dictionary<ComparableTuple2<TKey0, TKey1>, TValue> m_dictionary;

        /// <summary>
        /// This property stores the default value that is returned if the keys don't exist in the array.
        /// </summary>
        public TValue DefaultValue
        {
            get;
            set;
        }

        /// <summary>
        /// Property to get the count of items in the sparse array.
        /// </summary>
        public int Count
        {
            get
            {
                return m_dictionary.Count;
            }
        }

        #region Constructors
        /// <summary>
        /// Constructor - creates an empty sparse array instance.
        /// </summary>
        public Sparse2DMatrix()
        {
            InitializeDictionary();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="defaultValue">A default value to return if the key is not present.</param>
        public Sparse2DMatrix(TValue defaultValue)
        {
            InitializeDictionary();
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="sparse2DMatrix">The sparse array instance to be copied</param>
        public Sparse2DMatrix(Sparse2DMatrix<TKey0, TKey1, TValue> sparse2DMatrix)
        {
            InitializeDictionary();
            Initialize(sparse2DMatrix);
            DefaultValue = sparse2DMatrix.DefaultValue;
        }

        #endregion

        /// <summary>
        /// Initialize the dictionary to compare items based on key values.
        /// </summary>
        private void InitializeDictionary()
        {
            ComparableTuple2EqualityComparer<TKey0, TKey1> equalityComparer = new ComparableTuple2EqualityComparer<TKey0, TKey1>();
            m_dictionary = new Dictionary<ComparableTuple2<TKey0, TKey1>, TValue>(equalityComparer);
        }

        /// <summary>
        /// Method to copy the data in another Sparse2DMatrix instance to this instance.
        /// </summary>
        /// <param name="sparse2DMatrix">An instance of the Sparse2DMatrix class.</param>
        private void Initialize(Sparse2DMatrix<TKey0, TKey1, TValue> sparse2DMatrix)
        {
            m_dictionary.Clear();

            // Copy each key value pair to the dictionary.
            foreach (KeyValuePair<ComparableTuple2<TKey0, TKey1>, TValue> pair in sparse2DMatrix)
            {
                ComparableTuple2<TKey0, TKey1> newCombinedKey = new ComparableTuple2<TKey0, TKey1>(pair.Key);
                m_dictionary.Add(newCombinedKey, pair.Value);
            }
        }

        /// <summary>
        /// Method to copy the data in this Sparse2DMatrix instance to another instance.
        /// </summary>
        /// <param name="sparse2DMatrix">An instance of the Sparse2DMatrix class.</param>
        public void CopyTo(Sparse2DMatrix<TKey0, TKey1, TValue> sparse2DMatrix)
        {
            sparse2DMatrix.m_dictionary.Clear();

            // Copy each key value pair to the dictionary.
            foreach (KeyValuePair<ComparableTuple2<TKey0, TKey1>, TValue> pair in m_dictionary)
            {
                ComparableTuple2<TKey0, TKey1> newCombinedKey = new ComparableTuple2<TKey0, TKey1>(pair.Key);
                sparse2DMatrix.m_dictionary.Add(newCombinedKey, pair.Value);
            }
        }

        /// <summary>
        /// Property []
        /// </summary>
        /// <param name="key0">The first key used to index the value</param>
        /// <param name="key1">The second key used to index the value</param>
        /// <returns>The 'get' property returns the value at the current key</returns>
        public TValue this[TKey0 key0, TKey1 key1]
        {
            get
            {
                if (!m_dictionary.TryGetValue(CombineKeys(key0, key1), out var value))
                {
                    value = DefaultValue;
                }

                return value;
            }

            set
            {
                m_dictionary[CombineKeys(key0, key1)] = value;
            }
        }
        /// <summary>
        /// Determines whether this matrix contains the specified keys.
        /// </summary>
        /// <param name="key0">The first key used to index the value</param>
        /// <param name="key1">The second key used to index the value</param>
        /// <returns>Returns the value 'true' if and only if the keys exists in this matrix</returns>
        public bool ContainsKey(TKey0 key0, TKey1 key1)
        {
            return m_dictionary.ContainsKey(CombineKeys(key0, key1));
        }

        /// <summary>
        /// Determines whether this matrix contains the specified value.
        /// </summary>
        /// <param name="value">A value</param>
        /// <returns>Returns the value 'true' if and only if the value exists in this matrix</returns>
        public bool ContainsValue(TValue value)
        {
            return m_dictionary.ContainsValue(value);
        }

        /// <summary>
        /// Gets the value for the associated keys.
        /// </summary>
        /// <param name="key0">The first key used to index the value</param>
        /// <param name="key1">The second key used to index the value</param>
        /// <param name="value">An out parameter that contains the value if the key exists</param>
        /// <returns>Returns the value 'true' if and only if the key exists in this matrix</returns>
        public bool TryGetValue(TKey0 key0, TKey1 key1, out TValue value)
        {
            return m_dictionary.TryGetValue(CombineKeys(key0, key1), out value);
        }

        /// <summary>
        /// Removes the value with the specified key from this sparse matrix instance.
        /// </summary>
        /// <param name="key0">The first key of the element to remove</param>
        /// <param name="key1">The second key of the element to remove</param>
        /// <returns>The value 'true' if and only if the element is successfully found and removed.</returns>
        public bool Remove(TKey0 key0, TKey1 key1)
        {
            return m_dictionary.Remove(CombineKeys(key0, key1));
        }

        /// <summary>
        /// Method to clear all values in the sparse array.
        /// </summary>
        public void Clear()
        {
            m_dictionary.Clear();
        }

        /// <summary>
        /// This method must be overridden by the caller to combine the keys.
        /// </summary>
        /// <param name="key0">The first key</param>
        /// <param name="key1">The second key</param>
        /// <returns>A value that combines the keys in a unique fashion</returns>
        public ComparableTuple2<TKey0, TKey1> CombineKeys(TKey0 key0, TKey1 key1)
        {
            return new ComparableTuple2<TKey0, TKey1>(key0, key1);
        }

        /// <summary>
        /// This method must be overridden by the caller to separate a combined key into the two original keys.
        /// </summary>
        /// <param name="combinedKey">A value that combines the keys in a unique fashion</param>
        /// <param name="key0">A reference to the first key</param>
        /// <param name="key1">A reference to the second key</param>
        public void SeparateCombinedKeys(ComparableTuple2<TKey0, TKey1> combinedKey, ref TKey0 key0, ref TKey1 key1)
        {
            key0 = combinedKey.Item0;
            key1 = combinedKey.Item1;
        }

        #region IEnumerable<KeyValuePair<ComparableTuple2<TKey0, TKey1>, TValue>> Members

        /// <summary>
        /// The Generic IEnumerator GetEnumerator method
        /// </summary>
        /// <returns>An enumerator to iterate over all key-value pairs in this sparse array</returns>
        public IEnumerator<KeyValuePair<ComparableTuple2<TKey0, TKey1>, TValue>> GetEnumerator()
        {
            return m_dictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// The non-generic IEnumerator GetEnumerator method
        /// </summary>
        /// <returns>An enumerator to iterate over all key-value pairs in this sparse array</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_dictionary.GetEnumerator();
        }

        #endregion
    }
}
