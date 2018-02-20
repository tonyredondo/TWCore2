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

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
// ReSharper disable once UnusedMember.Global
// ReSharper disable InheritdocConsiderUsage

namespace TWCore.Threading
{
    /// <summary>
    /// Consumer for enumerables in async
    /// </summary>
    /// <typeparam name="T">Type of the enumerable</typeparam>
    public class AsyncConsumerEnumerable<T> : IEnumerable<T>
    {
        private readonly List<Task<IEnumerable<T>>> _tskList;

        #region .ctor
        /// <summary>
        /// Consumer for enumerables in async
        /// </summary>
        public AsyncConsumerEnumerable()
        {
            _tskList = new List<Task<IEnumerable<T>>>();
        }
        #endregion

        #region Enumerator
        /// <inheritdoc />
        /// <summary>
        /// Enumerator for the produced data.
        /// </summary>
        /// <returns>ConsumerEnumerator instance instance</returns>
        public IEnumerator<T> GetEnumerator()
        {
            lock (_tskList)
                return new ConsumerAsyncEnumerator(_tskList.ToArray());
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        #region Producer Methods
        /// <summary>
        /// Adds a task to produce an array of items
        /// </summary>
        /// <param name="task">Task to produce the array of items</param>
        public void Add(Task<T[]> task)
        {
            lock (_tskList)
                _tskList.Add(Task.Run(async () => (IEnumerable<T>)await task.ConfigureAwait(false)));
        }
        /// <summary>
        /// Adds a task to produce an IEnumerable of items
        /// </summary>
        /// <param name="task">Task to produce the IEnumerable of items</param>
        public void Add(Task<IEnumerable<T>> task)
        {
            lock (_tskList)
                _tskList.Add(task);
        }
        /// <summary>
        /// Adds a task to produce one item
        /// </summary>
        /// <param name="task">Task to produce an item</param>
        public void Add(Task<T> task)
        {
            lock (_tskList)
                _tskList.Add(Task.Run(async () => (IEnumerable<T>)new[] { await task.ConfigureAwait(false) }));
        }
        /// <summary>
        /// Adds an IEnumerable instance
        /// </summary>
        /// <param name="enumerable">Enumerable instance</param>
        public void Add(IEnumerable<T> enumerable)
        {
            lock (_tskList)
                _tskList.Add(Task.FromResult(enumerable));
        }
        /// <summary>
        /// Adds an item instance
        /// </summary>
        /// <param name="item">Item instance</param>
        public void Add(T item)
        {
            lock (_tskList)
                _tskList.Add(Task.FromResult((IEnumerable<T>)new[] { item }));
        }
        #endregion

        #region Nested Classes
        private class ConsumerAsyncEnumerator : IEnumerator<T>
        {
            private Task<IEnumerable<T>>[] _tskArray;
            private int _index;
            private IEnumerator<T> _innerEnumerator;
            public T Current { get; private set; }
            object IEnumerator.Current => Current;

            public ConsumerAsyncEnumerator(Task<IEnumerable<T>>[] tsks)
                => _tskArray = tsks;

            public bool MoveNext() => OnMoveNextAsync().WaitAndResults();
            public void Reset()
            {
                _index = 0;
                _innerEnumerator = null;
            }
            public void Dispose()
            {
                _tskArray = null;
            }

            private async Task<bool> OnMoveNextAsync()
            {
                if (_tskArray == null) return false;
                while (true)
                {
                    if (_innerEnumerator == null)
                    {
                        if (_tskArray.Length == 0) return false;
                        if (_tskArray.Length <= _index) return false;
                        var tsk = _tskArray[_index];
                        var innerEnumerable = await tsk.ConfigureAwait(false);
                        _innerEnumerator = innerEnumerable.GetEnumerator();
                        _index++;
                    }
                    if (_innerEnumerator.MoveNext())
                    {
                        Current = _innerEnumerator.Current;
                        return true;
                    }
                    _innerEnumerator = null;
                }
            }
        }
        #endregion
    }
}