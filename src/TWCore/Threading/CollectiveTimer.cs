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
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable MethodSupportsCancellation
// ReSharper disable UnusedMember.Global
#pragma warning disable 420

namespace TWCore.Threading
{
    /// <summary>
    /// Collective Timer
    /// </summary>
    /// <typeparam name="T">Type of instance to apply an action</typeparam>
    public class CollectiveTimer<T>
    {
        private ConcurrentQueue<QueueItem> _queue;
        private Action<T> _action;
        private TimeSpan _timeout;
        private CancellationToken _cancellationToken;


        #region Nested Type
        private readonly struct QueueItem
        {
            public readonly T Item;
            public readonly DateTime Time;

            public QueueItem(T item, DateTime time)
            {
                Item = item;
                Time = time;
            }
        }
        #endregion

        /// <summary>
        /// Collective Timer
        /// </summary>
        /// <param name="action">Action to apply</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CollectiveTimer(Action<T> action, TimeSpan timeout, CancellationToken cancellationToken)
        {
            _queue = new ConcurrentQueue<QueueItem>();
            _action = action;
            _timeout = timeout;
            _cancellationToken = cancellationToken;
            Task.Run(TimerHandler);
        }

        /// <summary>
        /// Enqueue item
        /// </summary>
        /// <param name="item">Item to enqueue</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(T item) =>
            _queue.Enqueue(new QueueItem(item, DateTime.UtcNow));


        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task TimerHandler()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                QueueItem queueItem;
                var now = DateTime.UtcNow;
                while (_queue.TryPeek(out queueItem) && now - queueItem.Time >= _timeout)
                {
                    _queue.TryDequeue(out queueItem);
                    _action(queueItem.Item);
                }

                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }
            _queue = null;
            _action = null;
        }
        #endregion
    }
}
