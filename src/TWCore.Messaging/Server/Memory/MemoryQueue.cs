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
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Threading;
using Thread = System.Threading.Thread;

namespace TWCore.Messaging
{
    /// <summary>
    /// Memory Queue
    /// </summary>
    public class MemoryQueue
    {
        private readonly ConcurrentQueue<Guid> _messageQueue = new ConcurrentQueue<Guid>();
        private readonly ConcurrentDictionary<Guid, Message> _messageStorage = new ConcurrentDictionary<Guid, Message>();
        private TaskCompletionSource<bool> _queueTask = new TaskCompletionSource<bool>();
        
        public class Message
        {
            public object Value;
            public readonly TaskCompletionSource<bool> TaskSource = new TaskCompletionSource<bool>();
        }

        #region Queue Methods
        /// <summary>
        /// Enqueue an object to the queue
        /// </summary>
        /// <param name="correlationId">CorrelationId value</param>
        /// <param name="value">Object value</param>
        /// <returns>true if the item was enqueued; otherwise false.</returns>
        public bool Enqueue(Guid correlationId, object value)
        {
            var message = _messageStorage.GetOrAdd(correlationId, id => new Message());
            message.Value = value;
            message.TaskSource.TrySetResult(true);
            _messageQueue.Enqueue(correlationId);
            _queueTask.TrySetResult(true);
            return true;
        }
        /// <summary>
        /// Dequeue an object from the queue
        /// </summary>
        /// <param name="cancellationToken">CancellationToken value</param>
        /// <returns>Object value</returns>
        public async Task<Message> DequeueAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_messageQueue.TryDequeue(out var correlationId))
                    {
                        if (!_messageStorage.TryRemove(correlationId, out var message)) continue;
                        Interlocked.Exchange(ref _queueTask, new TaskCompletionSource<bool>());
                        return message;
                    }
                    await _queueTask.Task.HandleCancellationAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            catch
            {
                //
            }
            return null;
        }
        /// <summary>
        /// Dequeue an object from the queue using the correlationId value
        /// </summary>
        /// <param name="correlationId">CorrelationId value</param>
        /// <param name="waitTime">Time to wait for the value</param>
        /// <param name="cancellationToken">CancellationToken value</param>
        /// <returns>Object value</returns>
        public async Task<Message> DequeueAsync(Guid correlationId, int waitTime, CancellationToken cancellationToken)
        {
            try
            {
                var message = _messageStorage.GetOrAdd(correlationId, id => new Message());
                var wTask = message.TaskSource.Task;
                var rTask = await Task.WhenAny(wTask, Task.Delay(waitTime, cancellationToken)).ConfigureAwait(false);
                if (rTask != wTask) return null;
                _messageStorage.TryRemove(correlationId, out var _);
                return message;
            }
            catch
            {
                //
            }
            return null;
        }
        #endregion
    }
}