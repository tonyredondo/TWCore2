﻿/*
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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TWCore.Collections
{
    /// <summary>
    /// Represents a dictionary mapping keys to values.
    /// </summary>
    /// 
    /// <remarks>
    /// Provides the plumbing for the portions of IDictionary[TKey TValue] which can reasonably be implemented without any
    /// dependency on the underlying representation of the dictionary.
    /// </remarks>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(PREFIX + "DictionaryDebugView`2" + SUFFIX)]
    public abstract class BaseDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private const string PREFIX = "System.Collections.Generic.Mscorlib_";
        private const string SUFFIX = ", mscorlib, Version = 2.0.0.0, Culture = neutral, PublicKeyToken = b77a5c561934e089";

        private KeyCollection keys;
        private ValueCollection values;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected BaseDictionary() { }

        public abstract int Count { get; }
        public abstract void Clear();
        public abstract void Add(TKey key, TValue value);
        public abstract bool ContainsKey(TKey key);
        public abstract bool Remove(TKey key);
        public abstract bool TryGetValue(TKey key, out TValue value);
        public abstract IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();
        protected abstract void SetValue(TKey key, TValue value);

        public bool IsReadOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return false; }
        }

        public ICollection<TKey> Keys
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (this.keys == null)
                    this.keys = new KeyCollection(this);
                return this.keys;
            }
        }

        public ICollection<TValue> Values
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (this.values == null)
                    this.values = new ValueCollection(this);
                return this.values;
            }
        }

        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!this.TryGetValue(key, out var value))
                    throw new KeyNotFoundException();
                return value;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                SetValue(key, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (!this.TryGetValue(item.Key, out var value))
                return false;

            return EqualityComparer<TValue>.Default.Equals(value, item.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            Copy(this, array, arrayIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!this.Contains(item))
                return false;


            return this.Remove(item.Key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private abstract class Collection<T> : ICollection<T>
        {
            protected readonly IDictionary<TKey, TValue> dictionary;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected Collection(IDictionary<TKey, TValue> dictionary)
            {
                this.dictionary = dictionary;
            }

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get { return this.dictionary.Count; }
            }

            public bool IsReadOnly
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get { return true; }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(T[] array, int arrayIndex)
            {
                Copy(this, array, arrayIndex);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public virtual bool Contains(T item)
            {
                foreach (T element in this)
                    if (EqualityComparer<T>.Default.Equals(element, item))
                        return true;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IEnumerator<T> GetEnumerator()
            {
                foreach (KeyValuePair<TKey, TValue> pair in this.dictionary)
                    yield return GetItem(pair);
            }

            protected abstract T GetItem(KeyValuePair<TKey, TValue> pair);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Remove(T item)
            {
                throw new NotSupportedException("Collection is read - only.");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(T item)
            {
                throw new NotSupportedException("Collection is read - only.");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                throw new NotSupportedException("Collection is read - only.");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy(PREFIX + "DictionaryKeyCollectionDebugView`2" + SUFFIX)]
        private class KeyCollection : Collection<TKey>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public KeyCollection(IDictionary<TKey, TValue> dictionary)
                    : base(dictionary) { }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override TKey GetItem(KeyValuePair<TKey, TValue> pair)
            {
                return pair.Key;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override bool Contains(TKey item)
            {
                return this.dictionary.ContainsKey(item);
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy(PREFIX + "DictionaryValueCollectionDebugView`2" + SUFFIX)]
        private class ValueCollection : Collection<TValue>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueCollection(IDictionary<TKey, TValue> dictionary)
                    : base(dictionary) { }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override TValue GetItem(KeyValuePair<TKey, TValue> pair)
            {
                return pair.Value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Copy<T>(ICollection<T> source, T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException("arrayIndex");

            if (array.Length - arrayIndex < source.Count)
                throw new ArgumentException("Destination array is not large enough.Check array.Length and arrayIndex.");

            foreach (T item in source)
                array[arrayIndex++] = item;
        }

    }
}
