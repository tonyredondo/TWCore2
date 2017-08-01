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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TWCore.Collections
{
    /// <summary>
    /// Timeout dictionary, a dictionary which items are saved and deleted using a timeout
    /// </summary>
    /// <typeparam name="TKey">Type of the Key</typeparam>
    /// <typeparam name="TValue">Type of the Value</typeparam>
    public class TimeoutDictionary<TKey, TValue>
    {
        #region Fields
        ConcurrentDictionary<TKey, TimeoutClass> _dictionary;
        #endregion

        #region Events
        /// <summary>
        /// On Item Timeout event (when item is deleted)
        /// </summary>
        public event EventHandler<TimeOutEventArgs> OnItemTimeout;
        #endregion

        #region Nested Classes
        /// <summary>
        /// Internal dictionary item
        /// </summary>
        class TimeoutClass
        {
            public TValue Value;
            public Task Task;
            public TimeSpan Timeout;
            public CancellationTokenSource TokenSource;
        }
        /// <summary>
        /// TimeOut Event Argument
        /// </summary>
        public class TimeOutEventArgs : EventArgs
        {
            /// <summary>
            /// Key of the item
            /// </summary>
            public TKey Key { get; internal set; }
            /// <summary>
            /// Value of the item
            /// </summary>
            public TValue Value { get; internal set; }
            /// <summary>
            /// Definied timeout
            /// </summary>
            public TimeSpan TimeOut { get; internal set; }
        }
        #endregion

        #region .ctor
        /// <summary>
        /// Timeout dictionary, a dictionary which items are saved and deleted using a timeout
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeoutDictionary()
        {
            _dictionary = new ConcurrentDictionary<TKey, TimeoutClass>();
        }
        #endregion

        #region ConcurrentDictionary Implementation
        /// <summary>
        /// Uses the specified functions to add a key/value pair to the TimeoutDictionary 
        /// if the key does not already exist, or to update a key/value pair in the TimeoutDictionary 
        /// if the key already exists. 
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue AddOrUpdate(TKey key, Func<TKey, (TValue, TimeSpan)> addValueFactory, Func<TKey, (TValue, TimeSpan), (TValue, TimeSpan)> updateValueFactory)
        {
            return _dictionary.AddOrUpdate(key, k =>
            {
                var val = addValueFactory(k);
                return Create(k, val.Item1, val.Item2);
            }, (k, t) =>
            {
                t.TokenSource.Cancel();
                var val = updateValueFactory(k, (t.Value, t.Timeout));
                return Create(k, val.Item1, val.Item2);
            }).Value;
        }
        /// <summary>
        /// Adds a key/value pair to the TimeoutDictionary 
        /// if the key does not already exist, or updates a key/value pair in the TimeoutDictionary 
        /// by using the specified function if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="valueTimeout">The value timeout</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <returns>The new value for the key. This will be either be addValue (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue AddOrUpdate(TKey key, TValue addValue, TimeSpan valueTimeout, Func<TKey, (TValue, TimeSpan), (TValue, TimeSpan)> updateValueFactory)
        {
            return _dictionary.AddOrUpdate(key, k => Create(k, addValue, valueTimeout), (k, t) =>
            {
                t.TokenSource.Cancel();
                var val = updateValueFactory(k, (t.Value, t.Timeout));
                return Create(k, val.Item1, val.Item2);
            }).Value;
        }
        /// <summary>
        /// Removes all keys and values from the TimeoutDictionary
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            foreach (var item in _dictionary)
                item.Value?.TokenSource?.Cancel();
            _dictionary.Clear();
        }
        /// <summary>
        /// Determines whether the TimeoutDictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the  TimeoutDictionary</param>
        /// <returns>true if TimeoutDictionary contains an element with the specified key; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);
        /// <summary>
        /// Returns an enumerator that iterates through the TimeoutDictionary.
        /// </summary>
        /// <returns>An enumerator for the TimeoutDictionary.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new TimeoutDictionaryEnumerator(_dictionary.GetEnumerator());
        /// <summary>
        /// Adds a key/value pair to the TimeoutDictionary
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">the value to be added, if the key does not already exist</param>
        /// <param name="valueTimeout">The value timeout</param>
        /// <returns>The value for the key. This will be either the existing value for the key if the key is already in the dictionary, or the new value if the key was not in the dictionary.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetOrAdd(TKey key, TValue value, TimeSpan valueTimeout)
            => _dictionary.GetOrAdd(key, k => Create(key, value, valueTimeout)).Value;
        /// <summary>
        /// Adds a key/value pair to the TimeoutDictionary by using the specified function, if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <returns>The value for the key. This will be either the existing value for the key if the key is already in the dictionary, or the new value for the key as returned by valueFactory if the key was not in the dictionary.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetOrAdd(TKey key, Func<TKey, (TValue, TimeSpan)> valueFactory)
        {
            return _dictionary.GetOrAdd(key, k =>
            {
                var val = valueFactory(k);
                return Create(k, val.Item1, val.Item2);
            }).Value;
        }
        /// <summary>
        /// Copies the key and value pairs stored in the TimeoutDictionary to a new array.
        /// </summary>
        /// <returns>A new array containing a snapshot of key and value pairs copied from the TimeoutDictionary.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValuePair<TKey, TValue>[] ToArray()
            => _dictionary.ToArray().Select(k => new KeyValuePair<TKey, TValue>(k.Key, k.Value.Value)).ToArray();

        /// <summary>
        /// Attempts to add the specified key and value to the TimeoutDictionary
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        /// <param name="valueTimeout">The value timeout.</param>
        /// <returns>true if the key/value pair was added to the TimeoutDictionary successfully; false if the key already exists.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(TKey key, TValue value, TimeSpan valueTimeout)
            => _dictionary.TryAdd(key, Create(key, value, valueTimeout));

        /// <summary>
        /// Attempts to get the value associated with the specified key from the TimeoutDictionary.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">hen this method returns, contains the object from the TimeoutDictionary that has the specified key, or the default value of , if the operation failed.</param>
        /// <returns>true if the key was found in the TimeoutDictionary; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value)
        {
            var res = _dictionary.TryGetValue(key, out var val);
            value = val != null ? val.Value : default(TValue);
            return res;
        }

        /// <summary>
        /// Attempts to remove and return the value that has the specified key from the TimeoutDictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove and return.</param>
        /// <param name="value">When this method returns, contains the object removed from the TimeoutDictionary, or the default value of the TValue type if key does not exist.</param>
        /// <returns>true if the item has been removed from the dictionary; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(TKey key, out TValue value)
        {
            var res = _dictionary.TryRemove(key, out var val);
            value = val != null ? val.Value : default(TValue);
            return res;
        }

        /// <summary>
        /// Compares the existing value for the specified key with a specified value, and if they are equal, updates the key with a third value.
        /// </summary>
        /// <param name="key">The key whose value is compared with comparisonValue and possibly replaced.</param>
        /// <param name="newValue">The value that replaces the value of the element that has the specified key if the comparison results in equality.</param>
        /// <param name="comparisonValue">The value that is compared to the value of the element that has the specified key.</param>
        /// <returns>true if the value with key was equal to comparisonValue and was replaced with newValue; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            if (_dictionary.TryGetValue(key, out var val))
            {
                if (object.Equals(val.Value, comparisonValue))
                {
                    var res = _dictionary.TryUpdate(key, Create(key, newValue, val.Timeout), val);
                    val?.TokenSource?.Cancel();
                    return res;
                }
            }
            return false;
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        TimeoutClass Create(TKey key, TValue value, TimeSpan valueTimeout)
        {
            var cts = new CancellationTokenSource();
            return new TimeoutClass
            {
                Value = value,
                TokenSource = cts,
                Timeout = valueTimeout,
                Task = Task.Delay(valueTimeout, cts.Token).ContinueWith((task, obj) =>
                {
                    var objArray = (object[])obj;
                    var mDictio = (ConcurrentDictionary<TKey, TimeoutClass>)objArray[0];
                    var mKey = (TKey)objArray[1];
                    if (mDictio != null && mDictio.TryRemove(mKey, out var tVal))
                        OnItemTimeout?.Invoke(this, new TimeOutEventArgs { Key = mKey, Value = tVal.Value, TimeOut = tVal.Timeout });

                }, new object[] { _dictionary, key }, CancellationToken.None,
                        TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
                        TaskScheduler.Default)
            };
        }

        private class TimeoutDictionaryEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            IEnumerator<KeyValuePair<TKey, TimeoutClass>> _enumerator;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TimeoutDictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TimeoutClass>> enumerator) 
                => _enumerator = enumerator;

            public KeyValuePair<TKey, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    var current = _enumerator.Current;
                    return new KeyValuePair<TKey, TValue>(current.Key, current.Value.Value);
                }
            }

            object IEnumerator.Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    var current = _enumerator.Current;
                    return new KeyValuePair<TKey, TValue>(current.Key, current.Value.Value);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _enumerator.Dispose();
                _enumerator = null;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => _enumerator.MoveNext();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => _enumerator.Reset();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the number of elements contained in the dictionary.
        /// </summary>
        public int Count => _dictionary.Count;
        #endregion
    }
}
