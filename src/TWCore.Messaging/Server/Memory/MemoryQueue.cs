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
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable MethodSupportsCancellation

// ReSharper disable CheckNamespace

namespace TWCore.Messaging
{
    /// <summary>
    /// Memory Queue
    /// </summary>
    public class MemoryQueue
    {
        private static readonly ConcurrentDictionary<CancellationToken, Task> CancellationTasks = new ConcurrentDictionary<CancellationToken, Task>();
        private readonly ConcurrentQueue<Guid> _messageQueue = new ConcurrentQueue<Guid>();
        private readonly ConcurrentDictionary<Guid, Message> _messageStorage = new ConcurrentDictionary<Guid, Message>();
        private TaskCompletionSource<bool> _queueTask = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        
        /// <summary>
        /// Memory Queue message
        /// </summary>
        public class Message
        {
            /// <summary>
            /// Value
            /// </summary>
            public object Value;
            /// <summary>
            /// Task completion source
            /// </summary>
            public readonly TaskCompletionSource<bool> TaskSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
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
                var tokenTask = CancellationTasks.GetOrAdd(cancellationToken, token => token.WhenCanceledAsync());
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_messageQueue.TryDequeue(out var correlationId))
                    {
                        if (!_messageStorage.TryRemove(correlationId, out var message)) continue;
                        Interlocked.Exchange(ref _queueTask, new TaskCompletionSource<bool>());
                        return message;
                    }
                    await Task.WhenAny(_queueTask.Task, tokenTask).ConfigureAwait(false);
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
                var delayCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, CancellationToken.None);
                var wTask = message.TaskSource.Task;
                var dTask = Task.Delay(waitTime, delayCancellation.Token);
                var rTask = await Task.WhenAny(wTask, dTask).ConfigureAwait(false);
                if (rTask != wTask) return null;
                delayCancellation.Cancel();
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