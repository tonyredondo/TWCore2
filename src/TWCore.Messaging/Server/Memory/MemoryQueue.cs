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
using TWCore.Threading;

namespace TWCore.Messaging
{
    /// <summary>
    /// Memory Queue
    /// </summary>
    public class MemoryQueue
    {
        #region Fields
        private readonly object _locker = new object();
        private readonly Dictionary<int, Message> _messages = new Dictionary<int, Message>();
        private readonly Dictionary<Guid, int> _correlationMessages = new Dictionary<Guid, int>();
        private readonly Dictionary<Guid, ManualResetEventSlim> _correlationEvents = new Dictionary<Guid, ManualResetEventSlim>();
        private readonly ManualResetEventSlim _enqueueEvent = new ManualResetEventSlim(false);
        private int _enqueueIndex;
        private int _dequeueIndex;
        private Guid _lastGuid;
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
            lock(_locker)
            {
                _messages.Add(_enqueueIndex, message);
                _correlationMessages.Add(correlationId, _enqueueIndex);
                if (!_correlationEvents.TryGetValue(correlationId, out var cEvent))
                {
                    cEvent = new ManualResetEventSlim();
                    _correlationEvents.Add(correlationId, cEvent);
                }
                Interlocked.Increment(ref _enqueueIndex);
                cEvent.Set();
            }
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
                    lock(_locker)
                    {
                        if (_messages.TryGetValue(_dequeueIndex, out var mResult))
                        {
                            _messages.Remove(_dequeueIndex);
                            _correlationMessages.Remove(mResult.CorrelationId);
                            _correlationEvents.Remove(mResult.CorrelationId);
                            Interlocked.Increment(ref _dequeueIndex);
                            _lastGuid = mResult.CorrelationId;
                            return mResult;
                        }
                        if (_enqueueIndex > _dequeueIndex)
                        {
                            Interlocked.Increment(ref _dequeueIndex);
                        }
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
                ManualResetEventSlim cEvent;
                lock(_locker)
                {
                    if (!_correlationEvents.TryGetValue(correlationId, out cEvent))
                    {
                        cEvent = new ManualResetEventSlim();
                        _correlationEvents.Add(correlationId, cEvent);
                    }
                }
                if (!cEvent.Wait(waitTime, cancellationToken)) return null;
                lock (_locker)
                {
                    if (_correlationMessages.TryGetValue(correlationId, out var cResult) && _messages.TryGetValue(cResult, out var mResult))
                    {
                        _correlationMessages.Remove(correlationId);
                        _correlationEvents.Remove(correlationId);
                        _messages[cResult] = null;
                        return mResult;
                    }
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