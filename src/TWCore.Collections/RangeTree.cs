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
// ReSharper disable PossibleMultipleEnumeration

namespace TWCore.Collections
{
    /// <summary>
    /// Range tree interface.
    /// </summary>
    /// <typeparam name="TKey">The type of the range.</typeparam>
    /// <typeparam name="T">The type of the data items.</typeparam>
    public interface IRangeTree<TKey, T> where TKey : IComparable<TKey> where T : IRangeProvider<TKey>
    {
        /// <summary>
        /// All items of the tree.
        /// </summary>
        IEnumerable<T> Items { get; }
        /// <summary>
        /// Count of all items.
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Performans a "stab" query with a single value.
        /// All items with overlapping ranges are returned.
        /// </summary>
        List<T> Query(TKey value);
        /// <summary>
        /// Performans a range query.
        /// All items with overlapping ranges are returned.
        /// </summary>
        List<T> Query(Range<TKey> range);
        /// <summary>
        /// Rebuilds the tree if it is out of sync.
        /// </summary>
        void Rebuild();
        /// <summary>
        /// Adds the specified item. Tree will go out of sync.
        /// </summary>
        void Add(T item);
        /// <summary>
        /// Adds the specified items. Tree will go out of sync.
        /// </summary>
        void Add(IEnumerable<T> items);
        /// <summary>
        /// Removes the specified item. Tree will go out of sync.
        /// </summary>
        void Remove(T item);
        /// <summary>
        /// Removes the specified items. Tree will go out of sync.
        /// </summary>
        void Remove(IEnumerable<T> items);
        /// <summary>
        /// Clears the tree (removes all items).
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// The standard range tree implementation. Keeps a root node and
    /// forwards all queries to it.
    /// Whenenver new items are added or items are removed, the tree 
    /// goes "out of sync" and is rebuild when it's queried next.
    /// </summary>
    /// <typeparam name="TKey">The type of the range.</typeparam>
    /// <typeparam name="T">The type of the data items.</typeparam>
    public class RangeTree<TKey, T> : IRangeTree<TKey, T> where TKey : IComparable<TKey> where T : IRangeProvider<TKey>
    {
        private RangeTreeNode<TKey, T> _root;
        private List<T> _items;
        private bool _isInSync;
        private bool _autoRebuild;
        private IComparer<T> _rangeComparer;

        /// <summary>
        /// Whether the tree is currently in sync or not. If it is "out of sync"
        /// you can either rebuild it manually (call Rebuild) or let it rebuild
        /// automatically when you query it next.
        /// </summary>
        public bool IsInSync
        {
            get { return _isInSync; }
        }

        /// <summary>
        /// All items of the tree.
        /// </summary>
        public IEnumerable<T> Items
        {
            get { return _items; }
        }

        /// <summary>
        /// Count of all items.
        /// </summary>
        public int Count
        {
            get { return _items.Count; }
        }

        /// <summary>
        /// Whether the tree should be rebuild automatically. Defaults to true.
        /// </summary>
        public bool AutoRebuild
        {
            get { return _autoRebuild; }
            set { _autoRebuild = value; }
        }

        /// <summary>
        /// Initializes an empty tree.
        /// </summary>
        public RangeTree(IComparer<T> rangeComparer)
        {
            _rangeComparer = rangeComparer;
            _root = new RangeTreeNode<TKey, T>(rangeComparer);
            _items = new List<T>();
            _isInSync = true;
            _autoRebuild = true;
        }

        /// <summary>
        /// Initializes a tree with a list of items to be added.
        /// </summary>
        public RangeTree(IEnumerable<T> items, IComparer<T> rangeComparer)
        {
            _rangeComparer = rangeComparer;
            _root = new RangeTreeNode<TKey, T>(items, rangeComparer);
            _items = items.ToList();
            _isInSync = true;
            _autoRebuild = true;
        }

        /// <summary>
        /// Performans a "stab" query with a single value.
        /// All items with overlapping ranges are returned.
        /// </summary>
        public List<T> Query(TKey value)
        {
            if (!_isInSync && _autoRebuild)
                Rebuild();

            return _root.Query(value);
        }

        /// <summary>
        /// Performans a range query.
        /// All items with overlapping ranges are returned.
        /// </summary>
        public List<T> Query(Range<TKey> range)
        {
            if (!_isInSync && _autoRebuild)
                Rebuild();

            return _root.Query(range);
        }

        /// <summary>
        /// Rebuilds the tree if it is out of sync.
        /// </summary>
        public void Rebuild()
        {
            if (_isInSync)
                return;

            _root = new RangeTreeNode<TKey, T>(_items, _rangeComparer);
            _isInSync = true;
        }

        /// <summary>
        /// Adds the specified item. Tree will go out of sync.
        /// </summary>
        public void Add(T item)
        {
            _isInSync = false;
            _items.Add(item);
        }

        /// <summary>
        /// Adds the specified items. Tree will go out of sync.
        /// </summary>
        public void Add(IEnumerable<T> items)
        {
            _isInSync = false;
            _items.AddRange(items);
        }

        /// <summary>
        /// Removes the specified item. Tree will go out of sync.
        /// </summary>
        public void Remove(T item)
        {
            _isInSync = false;
            _items.Remove(item);
        }

        /// <summary>
        /// Removes the specified items. Tree will go out of sync.
        /// </summary>
        public void Remove(IEnumerable<T> items)
        {
            _isInSync = false;

            foreach (var item in items)
                _items.Remove(item);
        }

        /// <summary>
        /// Clears the tree (removes all items).
        /// </summary>
        public void Clear()
        {
            _root = new RangeTreeNode<TKey, T>(_rangeComparer);
            _items = new List<T>();
            _isInSync = true;
        }
    }

    /// <summary>
    /// Default IRangeProvider comparer
    /// </summary>
    public class DefaultRangeProviderComparer<T, K> : IComparer<T> where T : IRangeProvider<K> where K : IComparable<K>
    {
        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>A signed integer that indicates the relative values of x and y, as shown in the 
        /// following table.Value Meaning Less than zerox is less than y.Zerox equals y.Greater 
        /// than zerox is greater than y.
        /// </returns>
        public virtual int Compare(T x, T y)
        {
            return x.Range.CompareTo(y.Range);
        }
    }
}
