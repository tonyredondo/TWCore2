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
using TWCore.Messaging.Client;
using TWCore.Services;

// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable UnusedMember.Global
// ReSharper disable IntroduceOptionalParameters.Global

namespace TWCore.Diagnostics.Trace.Storages
{
    /// <inheritdoc />
    /// <summary>
    /// Messaging Trace Storage
    /// </summary>
    public class MessagingTraceStorage : ITraceStorage
    {
        private readonly object _locker = new object();
        private readonly IMQueueClient _queueClient;
        private readonly Timer _timer;
        private readonly bool _sendCompleteTrace;
        private readonly List<TraceItem> _traceItems;
        
        #region .ctor
        /// <summary>
        /// Messaging trace storage
        /// </summary>
        /// <param name="queueName">Queue pair config name</param>
        /// <param name="periodInSeconds">Fetch period in seconds</param>
        /// <param name="sendCompleteTrace">Sends the complete trace</param>
        public MessagingTraceStorage(string queueName, int periodInSeconds, bool sendCompleteTrace)
        {
            _queueClient = Core.Services.GetQueueClient(queueName);
            _traceItems = new List<TraceItem>();
            _sendCompleteTrace = sendCompleteTrace;
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
        public void Write(TraceItem item)
        {
            lock (_locker)
            {
                _traceItems.Add(item);
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose the current object resources
        /// </summary>
        public void Dispose()
        {
            _timer?.Dispose();
            TimerCallback(this);
            _queueClient?.Dispose();
        }
        #endregion
        
        #region Private methods
        private static void TimerCallback(object state)
        {
            try
            {
                var mStatus = (MessagingTraceStorage) state;
                List<MessagingTraceItem> itemsToSend;
                lock (mStatus._locker)
                {
                    itemsToSend = new List<MessagingTraceItem>(mStatus._traceItems.Select(i => new MessagingTraceItem
                    {
                        EnvironmentName = Core.EnvironmentName,
                        MachineName = Core.MachineName,
                        ApplicationName = Core.ApplicationName,
                        ApplicationDisplayName = Core.ApplicationDisplayName,
                        GroupName = i.GroupName,
                        Id = i.Id,
                        Timestamp = i.Timestamp,
                        TraceName = i.TraceName,
                        TraceObject = mStatus._sendCompleteTrace ? i.TraceObject : null
                    }));
                    mStatus._traceItems.Clear();
                }
                mStatus._queueClient.Send(itemsToSend);
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
        #endregion
    }
}