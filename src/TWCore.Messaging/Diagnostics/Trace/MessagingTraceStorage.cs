﻿/*
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
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Messaging.RawClient;
using TWCore.Serialization;
using TWCore.Services;
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable UnusedMember.Global
// ReSharper disable IntroduceOptionalParameters.Global

namespace TWCore.Diagnostics.Trace.Storages
{
    /// <inheritdoc />
    /// <summary>
    /// Messaging Trace Storage
    /// </summary>
    [StatusName("Messaging Trace")]
    public class MessagingTraceStorage : ITraceStorage
    {
        private readonly string _queueName;
        private readonly Timer _timer;
        private int _processing;
        private int _count;
        private readonly bool _sendCompleteTrace;
        private readonly BlockingCollection<MessagingTraceItem> _traceItems;
        private IMQueueRawClient _queueClient;
        private readonly IPool<List<MessagingTraceItem>> _pool;
        private bool _enabled = true;

        #region .ctor
        /// <summary>
        /// Messaging trace storage
        /// </summary>
        /// <param name="queueName">Queue pair config name</param>
        /// <param name="periodInSeconds">Fetch period in seconds</param>
        /// <param name="sendCompleteTrace">Sends the complete trace</param>
        public MessagingTraceStorage(string queueName, int periodInSeconds, bool sendCompleteTrace)
        {
            _queueName = queueName;
            _traceItems = new BlockingCollection<MessagingTraceItem>();
            _sendCompleteTrace = sendCompleteTrace;
            _pool = new ReferencePool<List<MessagingTraceItem>>();
            var period = TimeSpan.FromSeconds(periodInSeconds);
            _timer = new Timer(TimerCallback, this, period, period);
        }
        #endregion

        #region Public methods
        /// <inheritdoc />
        /// <summary>
        /// Writes a trace item to the storage
        /// </summary>
        /// <param name="item">Trace item</param>
        public Task WriteAsync(TraceItem item)
        {
            if (!_enabled) return Task.CompletedTask;
            if (Interlocked.Increment(ref _count) < 1_000)
                _traceItems.Add(new MessagingTraceItem
                {
                    EnvironmentName = Core.EnvironmentName,
                    MachineName = Core.MachineName,
                    ApplicationName = Core.ApplicationName,
                    InstanceId = Core.InstanceId,
                    GroupName = item.GroupName,
                    Id = item.Id,
                    Tags = item.Tags,
                    Timestamp = item.Timestamp,
                    TraceName = item.TraceName,
                    TraceObject = _sendCompleteTrace ? new SerializedObject(item.TraceObject) : null
                });
            return Task.CompletedTask;
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose the current object resources
        /// </summary>
        public void Dispose()
        {
            _timer.Dispose();
            TimerCallback(this);
            _queueClient?.Dispose();
        }
        #endregion

        #region Private methods
        private void TimerCallback(object state)
        {
            if (Interlocked.CompareExchange(ref _processing, 1, 0) == 1) return;
            try
            {
                if (_traceItems.Count == 0)
                {
                    Interlocked.Exchange(ref _processing, 0);
                    return;
                }

                var itemsToSend = _pool.New();
                while (itemsToSend.Count < 2048 && _traceItems.TryTake(out var item, 10))
                {
                    itemsToSend.Add(item);
                    Interlocked.Decrement(ref _count);
                }

                Core.Log.LibDebug("Sending {0} trace items to the diagnostic queue.", itemsToSend.Count);
                if (_queueClient is null)
                {
                    _queueClient = Core.Services.GetQueueRawClient(_queueName, true);
                    Core.Status.AttachChild(_queueClient, this);
                }
                _queueClient.SendAsync(itemsToSend).WaitAndResults();

                itemsToSend.Clear();
                _pool.Store(itemsToSend);
            }
            catch (UriFormatException fException)
            {
                Core.Log.Warning($"Disabling {nameof(MessagingTraceStorage)}. Reason: {fException.Message}");
                _enabled = false;
                _timer.Dispose();
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            Interlocked.Exchange(ref _processing, 0);
        }
        #endregion
    }
}