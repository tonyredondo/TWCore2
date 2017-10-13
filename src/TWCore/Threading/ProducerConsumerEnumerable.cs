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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable once UnusedMember.Global
// ReSharper disable InheritdocConsiderUsage
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Local

namespace TWCore.Threading
{
    /// <summary>
    /// Producer / Consumer schema enumerable
    /// </summary>
    /// <typeparam name="T">Type of the enumerable</typeparam>
    public class ProducerConsumerEnumerable<T> : IEnumerable<T>, IDisposable
    {
        private readonly object _padlock = new object();
        private readonly Func<ProducerMethods, CancellationToken, Task> _producerFunc;
        private readonly List<T> _collection;
        private readonly CancellationTokenSource _tokenSource;
        private readonly AsyncManualResetEvent _producerAddEvent;
        private readonly AsyncManualResetEvent _producerEndEvent;
        private bool _started;
        private bool _disposed;

        #region .ctor
        /// <summary>
        /// Producer / Consumer schema enumerable
        /// </summary>
        /// <param name="producerFunc">Func to perform for the production of data</param>
        public ProducerConsumerEnumerable(Func<ProducerMethods, CancellationToken, Task> producerFunc)
        {
            _started = false;
            _producerFunc = producerFunc;
            _collection = new List<T>();
            _tokenSource = new CancellationTokenSource();
            _producerAddEvent = new AsyncManualResetEvent(false);
            _producerEndEvent = new AsyncManualResetEvent(false);
        }
        /// <summary>
        /// Producer / Consumer schema enumerable
        /// </summary>
        /// <param name="actionFunc">Action to perform for the production of data</param>
        public ProducerConsumerEnumerable(Action<ProducerMethods, CancellationToken> actionFunc)
        {
            _started = false;
            _producerFunc = (pMethods, token) => Task.Run(() => actionFunc(pMethods, token));
            _collection = new List<T>();
            _tokenSource = new CancellationTokenSource();
            _producerAddEvent = new AsyncManualResetEvent(false);
            _producerEndEvent = new AsyncManualResetEvent(false);
        }
        ~ProducerConsumerEnumerable()
        {
            Dispose();
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose instance
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _producerAddEvent?.Dispose();
            _producerEndEvent?.Dispose();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts the producer thread.
        /// </summary>
        public void StartProducer()
        {
            if (_started) return;
            _started = true;
            _producerEndEvent.Reset();
            Task.Run(async () =>
            {
                try
                {
                    var prodMethods = new ProducerMethods(this);
                    await _producerFunc(prodMethods, _tokenSource.Token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
                await _producerEndEvent.SetAsync().ConfigureAwait(false);
            }, _tokenSource.Token);
        }
        /// <summary>
        /// Async version of ForEach
        /// </summary>
        /// <param name="itemAction">Action for each element</param>
        /// <returns>ForEach Task</returns>
        public async Task ForEachAsync(Action<T> itemAction)
        {
            var consumer = new ConsumerEnumerator(this);
            while(await consumer.MoveNextAsync().ConfigureAwait(false))
                itemAction(consumer.Current);
        }
        /// <summary>
        /// Async version of ForEach
        /// </summary>
        /// <param name="itemAction">Action for each element with index</param>
        /// <returns>ForEach Task</returns>
        public async Task ForEachAsync(Action<T, int> itemAction)
        {
            var consumer = new ConsumerEnumerator(this);
            var idx = 0;
            while (await consumer.MoveNextAsync().ConfigureAwait(false))
                itemAction(consumer.Current, idx++);
        }
        /// <summary>
        /// Async version of ForEach
        /// </summary>
        /// <param name="itemFunc">Func for each element to support tasks</param>
        /// <returns>ForEach Task</returns>
        public async Task ForEachAsync(Func<T, Task> itemFunc)
        {
            var consumer = new ConsumerEnumerator(this);
            while (await consumer.MoveNextAsync().ConfigureAwait(false))
                await itemFunc(consumer.Current).ConfigureAwait(false);
        }
        /// <summary>
        /// Async version of ForEach
        /// </summary>
        /// <param name="itemFunc">Func for each element with index to support tasks</param>
        /// <returns>ForEach Task</returns>
        public async Task ForEachAsync(Func<T, int, Task> itemFunc)
        {
            var consumer = new ConsumerEnumerator(this);
            var idx = 0;
            while (await consumer.MoveNextAsync().ConfigureAwait(false))
                await itemFunc(consumer.Current, idx++).ConfigureAwait(false);
        }
        /// <summary>
        /// Async version of ToArray
        /// </summary>
        /// <returns>Array tasks</returns>
        public async Task<T[]> ToArrayAsync()
        {
            var consumer = new ConsumerEnumerator(this);
            var lst = new List<T>();
            while (await consumer.MoveNextAsync().ConfigureAwait(false))
                lst.Add(consumer.Current);
            return lst.ToArray();
        }
        #endregion

        #region Enumerator
        /// <inheritdoc />
        /// <summary>
        /// Enumerator for the produced data.
        /// </summary>
        /// <returns>ConsumerEnumerator instance instance</returns>
        public IEnumerator<T> GetEnumerator() => new ConsumerEnumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        #region Nested Classes
        /// <summary>
        /// Methods to be called from the producer thread
        /// </summary>
        public class ProducerMethods
        {
            private readonly ProducerConsumerEnumerable<T> _data;

            #region .ctor
            internal ProducerMethods(ProducerConsumerEnumerable<T> data)
            {
                _data = data;
            }
            #endregion

            #region Methods
            /// <summary>
            /// Adds an item to the enumerable
            /// </summary>
            /// <param name="item">Item instance</param>
            public void Add(T item)
            {
                lock (_data._padlock)
                    _data._collection.Add(item);
                _data._producerAddEvent.Set();
            }
            /// <summary>
            /// Adds an item to the enumerable
            /// </summary>
            /// <param name="item">Item instance</param>
            public Task AddAsync(T item)
            {
                lock (_data._padlock)
                    _data._collection.Add(item);
                return _data._producerAddEvent.SetAsync();
            }
            /// <summary>
            /// Adds a IEnumerable to the internal enumerable
            /// </summary>
            /// <param name="enumerable">Enumerable of items</param>
            public void AddRange(IEnumerable<T> enumerable)
            {
                lock (_data._padlock)
                    _data._collection.AddRange(enumerable);
                _data._producerAddEvent.Set();
            }
            /// <summary>
            /// Adds a IEnumerable to the internal enumerable
            /// </summary>
            /// <param name="enumerable">Enumerable of items</param>
            public Task AddRangeAsync(IEnumerable<T> enumerable)
            {
                lock (_data._padlock)
                    _data._collection.AddRange(enumerable);
                return _data._producerAddEvent.SetAsync();
            }
            #endregion
        }
        private class ConsumerEnumerator : IEnumerator<T>
        {
            private readonly ProducerConsumerEnumerable<T> _data;
            private int _index;
            public T Current { get; private set; }
            object IEnumerator.Current => Current;

            public ConsumerEnumerator(ProducerConsumerEnumerable<T> data)
            {
                _data = data;
            }

            public async Task<bool> MoveNextAsync()
            {
                _data.StartProducer();
                while (_data._collection.Count - _index == 0)
                {
                    var addTask = _data._producerAddEvent.WaitAsync();
                    var endTask = _data._producerEndEvent.WaitAsync(_data._tokenSource.Token);
                    var waitTask = await Task.WhenAny(endTask, addTask).ConfigureAwait(false);
                    if (waitTask == endTask)
                        return false;
                }
                lock (_data._padlock)
                {
                    _data._producerAddEvent.Reset();
                    if (_data._collection.Count > _index)
                    {
                        Current = _data._collection[_index++];
                        return true;
                    }
                    Core.Log.Error("The index is out of the range of the collection.");
                    return false;
                }
            }

            public bool MoveNext() 
                => MoveNextAsync().WaitAndResults();

            public void Reset()
                => _index = 0;

            public void Dispose()
            {
            }
        }
        #endregion
    }
}