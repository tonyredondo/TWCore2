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
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
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
    [StatusName("Messaging Log")]
    public class MessagingLogStorage : ILogStorage
    {
        private readonly string _queueName;
        private readonly Timer _timer;
        private volatile bool _processing;
        private int _count;
        private readonly BlockingCollection<LogItem> _logItems;
        private IMQueueClient _queueClient;
        private readonly IPool<List<LogItem>> _pool;
        private bool _enabled = true;

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
            _pool = new ReferencePool<List<LogItem>>();
            var period = TimeSpan.FromSeconds(periodInSeconds);
            _timer = new Timer(TimerCallback, this, period, period);
        }
        /// <summary>
        /// Messaging log storage finalizer
        /// </summary>
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
            if (!_enabled) return Task.CompletedTask;
            if (item is LogItem logItem && Interlocked.Increment(ref _count) < 10_000)
                _logItems.Add(new LogItem
                {
                    InstanceId = Core.InstanceId,
                    Id = logItem.Id,
                    EnvironmentName = logItem.EnvironmentName,
                    MachineName = logItem.MachineName,
                    ApplicationName = logItem.ApplicationName,
                    ProcessId = logItem.ProcessId,
                    ProcessName = logItem.ProcessName,
                    AssemblyName = logItem.AssemblyName,
                    TypeName = logItem.TypeName,
                    GroupName = logItem.GroupName,
                    Code = logItem.Code,
                    Exception = logItem.Exception,
                    Level = logItem.Level,
                    LineNumber = logItem.LineNumber,
                    Message = logItem.Message,
                    Timestamp = logItem.Timestamp
                });
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
            _queueClient?.Dispose();
        }
        #endregion

        #region Private methods
        private void TimerCallback(object state)
        {
            if (_processing) return;
            _processing = true;
            try
            {
                if (_logItems.Count == 0)
                {
                    _processing = false;
                    return;
                }

                var itemsToSend = _pool.New();
                while (itemsToSend.Count < 2048 && _logItems.TryTake(out var item, 10))
                {
                    itemsToSend.Add(item);
                    Interlocked.Decrement(ref _count);
                }

                Core.Log.LibDebug("Sending {0} log items to the diagnostic queue.", itemsToSend.Count);
                if (_queueClient is null)
                {
                    _queueClient = Core.Services.GetQueueClient(_queueName);
                    Core.Status.AttachChild(_queueClient, this);
                }
                _queueClient.SendAsync(itemsToSend).WaitAndResults();

                itemsToSend.Clear();
                _pool.Store(itemsToSend);
            }
            catch (UriFormatException fException)
            {
                Core.Log.Warning($"Disabling {nameof(MessagingLogStorage)}. Reason: {fException.Message}");
                _enabled = false;
                _timer.Dispose();
            }
            catch
            {
                //
            }
            _processing = false;
        }
        #endregion
    }
}