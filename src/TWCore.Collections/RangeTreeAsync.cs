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
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Global

namespace TWCore.Collections
{
    /// <inheritdoc />
    /// <summary>
    /// The async range tree implementation. Keeps a root node and
    /// forwards all queries to it.
    /// Whenenver new items are added or items are removed, the tree
    /// goes "out of sync" and when the next query is started, the tree
    /// is being rebuilt in an async task. During the rebuild, queries are
    /// still done on the old tree plus on the items currently not part of the
    /// tree. If items were removed, these are filtered out.
    /// there is no need to wait for the rebuild to finish in order
    /// to return the query results.
    /// </summary>
    /// <typeparam name="TKey">The type of the range.</typeparam>
    /// <typeparam name="T">The type of the data items.</typeparam>
    public class RangeTreeAsync<TKey, T> : IRangeTree<TKey, T> where TKey : IComparable<TKey> where T : IRangeProvider<TKey>
    {
        private RangeTree<TKey, T> _rangeTree;
        private List<T> _addedItems = new List<T>();
        private List<T> _removedItems = new List<T>();
        private List<T> _addedItemsRebuilding = new List<T>();
        private List<T> _removedItemsRebuilding = new List<T>();
        private readonly IComparer<T> _rangeComparer;
        private Task _rebuildTask;
        private CancellationTokenSource _rebuildTaskCancelSource;
        private bool _isRebuilding;
        private readonly object _locker = new object();

        /// <inheritdoc />
        /// <summary>
        /// All items of the tree.
        /// </summary>
        public IEnumerable<T> Items => _rangeTree.Items.Concat(_addedItemsRebuilding).Concat(_addedItems);

        /// <inheritdoc />
        /// <summary>
        /// Count of all items.
        /// </summary>
        public int Count => _rangeTree.Count + _addedItemsRebuilding.Count + _addedItems.Count;

        /// <summary>
        /// Initializes an empty tree.
        /// </summary>
        public RangeTreeAsync(IComparer<T> rangeComparer)
        {
            _rangeTree = new RangeTree<TKey, T>(rangeComparer) { AutoRebuild = false };
            _rangeComparer = rangeComparer;
        }

        /// <summary>
        /// Initializes a tree with a list of items to be added.
        /// </summary>
        public RangeTreeAsync(IEnumerable<T> items, IComparer<T> rangeComparer)
        {
            _rangeTree = new RangeTree<TKey, T>(items, rangeComparer) { AutoRebuild = false };
            _rangeComparer = rangeComparer;
        }

        /// <inheritdoc />
        /// <summary>
        /// Performans a "stab" query with a single value.
        /// All items with overlapping ranges are returned.
        /// </summary>
        public List<T> Query(TKey value)
        {
            // check if we need to start a rebuild task
            if (NeedsRebuild())
                RebuildTree();

            lock (_locker)
            {
                // query the tree (may be out of date)
                var results = _rangeTree.Query(value);

                // add additional results
                results.AddRange(_addedItemsRebuilding.Where(item => item.Range.Contains(value)));
                results.AddRange(_addedItems.Where(item => item.Range.Contains(value)));

                return FilterResults(results);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Performans a range query.
        /// All items with overlapping ranges are returned.
        /// </summary>
        public List<T> Query(Range<TKey> range)
        {
            // check if we need to start a rebuild task
            if (NeedsRebuild())
                RebuildTree();

            lock (_locker)
            {
                // query the tree (may be out of date)
                var results = _rangeTree.Query(range);

                // add additional results
                results.AddRange(_addedItemsRebuilding.Where(item => item.Range.Intersects(range)));
                results.AddRange(_addedItems.Where(item => item.Range.Intersects(range)));

                return FilterResults(results);
            }
        }


        /// <summary>
        /// Filter out results, if items were removed since the last rebuild.
        /// </summary>
        private List<T> FilterResults(List<T> results)
        {
            if (_removedItemsRebuilding.Count <= 0 && _removedItems.Count <= 0) return results;
            var hs = new HashSet<T>(results);
            foreach (var item in _removedItemsRebuilding)
                hs.Remove(item);
            foreach (var item in _removedItems)
                hs.Remove(item);
            results = hs.ToList();

            return results;
        }

        /// <inheritdoc />
        /// <summary>
        /// Adds the specified item.
        /// </summary>
        public void Add(T item)
        {
            lock (_locker)
                _addedItems.Add(item);
        }

        /// <inheritdoc />
        /// <summary>
        /// Adds the specified items.
        /// </summary>
        public void Add(IEnumerable<T> items)
        {
            lock (_locker)
                _addedItems.AddRange(items);
        }

        /// <inheritdoc />
        /// <summary>
        /// Removes the specified item.
        /// </summary>
        public void Remove(T item)
        {
            lock (_locker)
                _removedItems.Add(item);
        }

        /// <inheritdoc />
        /// <summary>
        /// Removes the specified items.
        /// </summary>
        public void Remove(IEnumerable<T> items)
        {
            lock (_locker)
                _removedItems.AddRange(items);
        }

        /// <inheritdoc />
        /// <summary>
        /// Clears the tree (removes all items).
        /// </summary>
        public void Clear()
        {
            lock (_locker)
            {
                _rangeTree.Clear();
                _addedItems = new List<T>();
                _removedItems = new List<T>();
                _addedItemsRebuilding = new List<T>();
                _removedItemsRebuilding = new List<T>();

                _rebuildTaskCancelSource?.Cancel();
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Start the rebuild task if a rebuild is necessary.
        /// </summary>
        public void Rebuild()
        {
            if (NeedsRebuild())
                RebuildTree();
        }

        /// <summary>
        /// Rebuilds the tree by starting an async task.
        /// </summary>
        private void RebuildTree()
        {
            lock (_locker)
            {
                // if a rebuild is in progress return
                if (_isRebuilding || _addedItems.Count == 0)
                    return;

                _isRebuilding = true;
            }

            _rebuildTaskCancelSource = new CancellationTokenSource();

            _rebuildTask = Task.Run(() =>
            {
                lock (_locker)
                {
                    // store the items to be added, we need this if a query takes places
                    // before we are finished rebuilding
                    _addedItemsRebuilding = _addedItems.ToList();
                    _addedItems.Clear();

                    // store the items to be removed ...
                    _removedItemsRebuilding = _removedItemsRebuilding.ToList();
                    _removedItems.Clear();
                }

                // all items of the tree
                var allItems = _rangeTree.Items.ToList();
                allItems.AddRange(_addedItemsRebuilding);

                // we may have to remove some
                foreach (var item in _removedItemsRebuilding)
                    allItems.Remove(item);

                // build the new tree
                var newTree = new RangeTree<TKey, T>(allItems, _rangeComparer) { AutoRebuild = false };

                // if task was not cancelled, set the new tree as the current one
                if (!_rebuildTaskCancelSource.Token.IsCancellationRequested)
                {
                    lock (_locker)
                    {
                        _rangeTree = newTree;
                        _addedItemsRebuilding.Clear();
                        _removedItemsRebuilding.Clear();
                    }
                }
                else
                {
                    // nop
                }
            }, _rebuildTaskCancelSource.Token)
            .ContinueWith(task =>
            {
                // done with rebuilding, do we need to start again?
                _isRebuilding = false;

                if (NeedsRebuild())
                    RebuildTree();
            });
        }

        /// <summary>
        /// Checks whether a rebuild is necessary.
        /// </summary>
        private bool NeedsRebuild()
        {
            lock (_locker)
            {
                // only if count of added or removed items is > 100
                // otherwise, the sequential query is ok
                return _addedItems.Count > 100 || _removedItems.Count > 100;
            }
        }
    }
}
