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
        private readonly BlockingCollection<object> _items;
        private IMQueueClient _queueClient;
        private readonly IPool<List<LogItem>> _pool;
        private readonly IPool<List<GroupMetadata>> _poolGroup;
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
            _items = new BlockingCollection<object>();
            _pool = new ReferencePool<List<LogItem>>();
            _poolGroup = new ReferencePool<List<GroupMetadata>>();
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
            if (Interlocked.Increment(ref _count) < 10_000)
            {
                _items.Add(new LogItem
                {
                    InstanceId = Core.InstanceId,
                    Id = item.Id,
                    EnvironmentName = item.EnvironmentName,
                    MachineName = item.MachineName,
                    ApplicationName = item.ApplicationName,
                    ProcessName = item.ProcessName,
                    AssemblyName = item.AssemblyName,
                    TypeName = item.TypeName,
                    GroupName = item.GroupName,
                    Code = item.Code,
                    Exception = item.Exception,
                    Level = item.Level,
                    Message = item.Message,
                    Timestamp = item.Timestamp
                });
            }
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
        /// <summary>
        /// Writes a group metadata item to the storage
        /// </summary>
        /// <param name="item">Group metadata item</param>
        /// <returns>Task process</returns>
        public Task WriteAsync(IGroupMetadata item)
        {
            if (!_enabled) return Task.CompletedTask;
            if (Interlocked.Increment(ref _count) < 10_000)
            {
                _items.Add(new GroupMetadata
                {
                    InstanceId = Core.InstanceId,
                    Timestamp = item.Timestamp,
                    GroupName = item.GroupName,
                    Items = item.Items
                });
            }
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
                if (_items.Count == 0)
                {
                    _processing = false;
                    return;
                }

                var logItemsToSend = _pool.New();
                var metadataToSend = _poolGroup.New();
                while (logItemsToSend.Count < 2048 && metadataToSend.Count < 2048 && _items.TryTake(out var item, 10))
                {
                    if (item is LogItem lItem)
                        logItemsToSend.Add(lItem);
                    else if (item is GroupMetadata gItem)
                        metadataToSend.Add(gItem);
                    Interlocked.Decrement(ref _count);
                }

                Core.Log.LibDebug("Sending {0} log items to the diagnostic queue.", logItemsToSend.Count + metadataToSend.Count);
                if (_queueClient is null)
                {
                    _queueClient = Core.Services.GetQueueClient(_queueName);
                    Core.Status.AttachChild(_queueClient, this);
                }

                var tskLogs = logItemsToSend.Count > 0 ? _queueClient.SendAsync(logItemsToSend) : Task.CompletedTask;
                var tskGroup = metadataToSend.Count > 0 ? _queueClient.SendAsync(metadataToSend) : Task.CompletedTask;

                Task.WaitAll(tskLogs, tskGroup);

                logItemsToSend.Clear();
                metadataToSend.Clear();
                _pool.Store(logItemsToSend);
                _poolGroup.Store(metadataToSend);
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