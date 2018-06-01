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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Messaging.Client;
using TWCore.Services;
// ReSharper disable ImpureMethodCallOnReadonlyValueField
// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Log.Storages
{
    /// <inheritdoc />
    /// <summary>
    /// Messaging log storage
    /// </summary>
    public class MessagingLogStorage : ILogStorage
    {
        private readonly string _queueName;
        private readonly Timer _timer;
		private readonly BlockingCollection<LogItem> _logItems;
		private IMQueueClient _queueClient;

        #region .ctor
        /// <summary>
        /// Messaging log storage
        /// </summary>
        /// <param name="queueName">Queue pair config name</param>
        /// <param name="periodInSeconds">Fetch period in seconds</param>
        public MessagingLogStorage(string queueName, int periodInSeconds)
        {
            _queueName = queueName;
			_logItems = new BlockingCollection<LogItem>();
			_queueClient = Core.Services.GetQueueClient(_queueName);
            var period = TimeSpan.FromSeconds(periodInSeconds);
            _timer = new Timer(TimerCallback, this, period, period);
        }
		~MessagingLogStorage()
		{
			Dispose();
		}
        #endregion
        
        #region Public methods
        /// <inheritdoc />
        /// <summary>
        /// Writes a log item to the storage
        /// </summary>
        /// <param name="item">Log Item</param>
        public Task WriteAsync(ILogItem item)
        {
			if (item is LogItem logItem)
				_logItems.Add(logItem.DeepClone());
            return Task.CompletedTask;
        }
        /// <inheritdoc />
        /// <summary>
        /// Writes a log item empty line
        /// </summary>
        public Task WriteEmptyLineAsync()
        {
            return Task.CompletedTask;
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose method
        /// </summary>
        public void Dispose()
        {
            _timer.Dispose();
            TimerCallback(this);
			_queueClient.Dispose();
        }
        #endregion
        
        #region Private methods
        private void TimerCallback(object state)
        {
            try
            {
				var itemsToSend = new List<LogItem>();
				while (itemsToSend.Count < 500 && _logItems.TryTake(out var item, 10))
					itemsToSend.Add(item);

                Core.Log.LibDebug("Sending {0} log items to the diagnostic queue.", itemsToSend.Count);
                _queueClient.SendAsync(itemsToSend).WaitAndResults();
            }
            catch
            {
				//
            }
        }
        #endregion
    }
}