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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TWCore.Messaging
{
    /// <summary>
    /// Memory Queue
    /// </summary>
    public class MemoryQueue
    {
        #region Fields
        private readonly ConcurrentDictionary<int, Message> _messages = new ConcurrentDictionary<int, Message>();
        private readonly ConcurrentDictionary<Guid, int> _correlationMessages = new ConcurrentDictionary<Guid, int>();
        private readonly ManualResetEventSlim _enqueueEvent = new ManualResetEventSlim(false);
        private int _enqueueIndex;
        private int _dequeueIndex;
        #endregion

        public class Message
        {
            public Guid CorrelationId;
            public object Value;
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
            var message = new Message { CorrelationId = correlationId, Value = value };
            if (_messages.TryAdd(_enqueueIndex, message))
                _correlationMessages.TryAdd(correlationId, _enqueueIndex);
            Interlocked.Increment(ref _enqueueIndex);
            _enqueueEvent.Set();
            return true;
        }
        /// <summary>
        /// Dequeue an object from the queue
        /// </summary>
        /// <param name="cancellationToken">CancellationToken value</param>
        /// <returns>Object value</returns>
        public Message Dequeue(CancellationToken cancellationToken)
        {
            try
            {
                while (_enqueueIndex > _dequeueIndex || _enqueueEvent.Wait(Timeout.Infinite, cancellationToken))
                {
                    if (_messages.TryRemove(_dequeueIndex, out var mResult))
                    {
                        if (mResult == null)
                        {
                            Interlocked.Increment(ref _dequeueIndex);
                            continue;
                        }
                        _correlationMessages.TryRemove(mResult.CorrelationId, out var _);
                        Interlocked.Increment(ref _dequeueIndex);
                        return mResult;
                    }
                    _enqueueEvent.Reset();

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
        public Message Dequeue(Guid correlationId, int waitTime, CancellationToken cancellationToken)
        {
            try
            {
                while (_enqueueIndex > _dequeueIndex || _enqueueEvent.Wait(waitTime, cancellationToken))
                {
                    if (_correlationMessages.TryGetValue(correlationId, out var cResult))
                    {
                        if (_messages.TryGetValue(cResult, out var mResult))
                        {
                            _correlationMessages.TryRemove(correlationId, out var _);
                            _messages.TryUpdate(cResult, null, mResult);
                            return mResult;
                        }
                        continue;
                    }
                    _enqueueEvent.Reset();
                }
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