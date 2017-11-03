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
using System.Threading;
using TWCore.Messaging.Client;
using TWCore.Services;
// ReSharper disable ImpureMethodCallOnReadonlyValueField

// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Log.Storages
{
    /// <inheritdoc />
    /// <summary>
    /// Messaging log storage
    /// </summary>
    public class MessagingLogStorage : ILogStorage
    {
        private readonly object _locker = new object();
        private readonly string _queueName;
        private readonly Timer _timer;
        private readonly List<LogItem> _logItems;

        #region .ctor
        /// <summary>
        /// Messaging log storage
        /// </summary>
        /// <param name="queueName">Queue pair config name</param>
        /// <param name="periodInSeconds">Fetch period in seconds</param>
        public MessagingLogStorage(string queueName, int periodInSeconds)
        {
            _queueName = queueName;
            _logItems = new List<LogItem>();
            var period = TimeSpan.FromSeconds(periodInSeconds);
            _timer = new Timer(TimerCallback, this, period, period);
        }
        #endregion
        
        #region Public methods
        /// <inheritdoc />
        /// <summary>
        /// Writes a log item to the storage
        /// </summary>
        /// <param name="item">Log Item</param>
        public void Write(ILogItem item)
        {
            lock (_locker)
            {
                if (item is LogItem logItem) 
                {
                    Core.Log.LibDebug("LogItem with level: {0} added. {1}", logItem.Level, logItem.Message);
                    _logItems.Add(logItem);
                }
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Writes a log item empty line
        /// </summary>
        public void WriteEmptyLine()
        {
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose method
        /// </summary>
        public void Dispose()
        {
            _timer.Dispose();
            TimerCallback(this);
        }
        #endregion
        
        #region Private methods
        private static void TimerCallback(object state)
        {
            try
            {
                var mStatus = (MessagingLogStorage) state;
                List<LogItem> itemsToSend;
                lock (mStatus._locker)
                {
                    if (mStatus._logItems.Count == 0) return;
                    itemsToSend = new List<LogItem>(mStatus._logItems);
                    mStatus._logItems.Clear();
                }
                Core.Log.LibDebug("Sending {0} log items to the diagnostic queue.", itemsToSend.Count);
                var queueClient = Core.Services.GetQueueClient(mStatus._queueName);
                queueClient.Send(itemsToSend);
                queueClient.Dispose();
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
        #endregion
    }
}