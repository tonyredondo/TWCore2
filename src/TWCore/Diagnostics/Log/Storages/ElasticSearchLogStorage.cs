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

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Diagnostics.Status;
using TWCore.Serialization;
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable SwitchStatementMissingSomeCases
// ReSharper disable InconsistentNaming
// ReSharper disable NotAccessedField.Local

namespace TWCore.Diagnostics.Log.Storages
{
    /// <summary>
    /// ElasticSearch log storage
    /// </summary>
    [StatusName("ElasticSearch Log")]
    public class ElasticSearchLogStorage : ILogStorage
    {
        private readonly Timer _timer;
        private readonly string _originalUrl;
        private readonly Uri _url;
        private readonly string _indexFormat;
        private readonly BlockingCollection<object> _sourceItems;
        private readonly StringBuilder _builderBuffer = new();
        private readonly JsonTextSerializer serializer = new()
        {
            EnumsAsStrings = true
        };
#if NET5_0_OR_GREATER
        private readonly HttpClient _client;
#else
        private readonly WebClient _client;
#endif
        private int _count;
        private bool _enabled = true;
        private int _inProcess;

        #region .ctor
        /// <summary>
        /// ElasticSearch log storage
        /// </summary>
        /// <param name="url">Elastic search json api url</param>
        /// <param name="indexFormat">Index format</param>
        /// <param name="periodInSeconds">Fetch period in seconds</param>
        public ElasticSearchLogStorage(string url, string indexFormat, int periodInSeconds = 10)
        {
            _originalUrl = url;
            _url = new UriBuilder(url) { Path = "_bulk" }.Uri;
            _indexFormat = indexFormat;
#if NET5_0_OR_GREATER
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
#else
            _client = new WebClient();
#endif
            _sourceItems = new BlockingCollection<object>();
            var period = TimeSpan.FromSeconds(periodInSeconds);
            _timer = new Timer(TimerCallback, this, period, period);
            Core.Status.Attach(collection =>
            {
                collection.Add("Configuration",
                    new StatusItemValueItem("Url", _originalUrl),
                    new StatusItemValueItem("IndexFormat", _indexFormat),
                    new StatusItemValueItem("Period In Seconds", periodInSeconds)
                );
                collection.Add("Remaining Items", _count, true);
            }, this);
        }
        /// <summary>
        /// ElasticSearch log storage finalizer
        /// </summary>
        ~ElasticSearchLogStorage()
        {
            Dispose();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Writes a log item to the storage
        /// </summary>
        /// <param name="item">Log Item</param>
        public Task WriteAsync(ILogItem item)
        {
            if (!_enabled) return Task.CompletedTask;
            if (Interlocked.Increment(ref _count) < 10_000)
            {
                _sourceItems.Add(new SourceData
                {
                    timestamp = item.Timestamp,
                    level = item.Level,
                    message = item.Message,
                    instanceId = Core.InstanceId,
                    environmentName = item.EnvironmentName,
                    machineName = item.MachineName,
                    applicationName = item.ApplicationName,
                    processName = item.ProcessName,
                    assemblyName = item.AssemblyName,
                    code = item.Code,
                    groupName = item.GroupName,
                    exception = item.Exception
                });
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Writes a log item empty line
        /// </summary>
        public Task WriteEmptyLineAsync()
            => Task.CompletedTask;
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
                _sourceItems.Add(new GroupSourceData
                {
                    instanceId = item.InstanceId,
                    timestamp = item.Timestamp,
                    groupName = item.GroupName,
                    items = item.Items,
                });
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _timer.Dispose();
            TimerCallback(this);
        }
        #endregion

        #region Private Methods
        private void TimerCallback(object state)
        {
            if (Interlocked.CompareExchange(ref _inProcess, 1, 0) == 1) return;
            try
            {
                if (_sourceItems.Count == 0)
                {
                    Interlocked.Exchange(ref _inProcess, 0);
                    return;
                }

                var count = 0;
                while (count++ < 2048 && _sourceItems.TryTake(out var item, 10))
                {
                    string index = null;
                    if (item is SourceData sDataItem)
                        index = string.Format(_indexFormat, sDataItem.@timestamp, sDataItem.applicationName, sDataItem.environmentName).ToLowerInvariant();
                    else if (item is GroupSourceData gDataItem)
                        index = string.Format(_indexFormat, gDataItem.@timestamp, Core.ApplicationName, Core.EnvironmentName).ToLowerInvariant();
                    _builderBuffer.Append("{\"index\":{\"_index\":\"" + index + "\",\"_type\":\"logevent\"}}\n");
                    _builderBuffer.Append(serializer.SerializeToString(item) + "\n");
                    Interlocked.Decrement(ref _count);
                }
                var data = _builderBuffer.ToString();
                _builderBuffer.Clear();
                Core.Log.LibDebug("Sending {0} log items to elastic search.", count);
#if NET5_0_OR_GREATER
                _client.Send(new HttpRequestMessage(HttpMethod.Post, _url)
                {
                    Content = new StringContent(data, Encoding.UTF8, "application/json")
                });
#else
                _client.Headers[HttpRequestHeader.Accept] = "application/json";
                _client.Headers[HttpRequestHeader.ContentType] = "application/json";
                _client.UploadString(_url, "POST", data);
#endif
            }
            catch (UriFormatException fException)
            {
                Core.Log.Error(fException, $"Disabling {nameof(ElasticSearchLogStorage)}. Reason: {fException.Message}");
                _enabled = false;
                _timer.Dispose();
            }
            catch (Exception)
            {
                //
            }
            Interlocked.Exchange(ref _inProcess, 0);
        }

        private class SourceData
        {
            [JsonProperty("@timestamp")]
            public DateTime @timestamp;
            public LogLevel level;
            public string message;
            public Guid instanceId;
            public string environmentName;
            public string machineName;
            public string applicationName;
            public string processName;
            public string assemblyName;
            public string code;
            public string groupName;
            public SerializableException exception;
        }
        private class GroupSourceData
        {
            [JsonProperty("@timestamp")]
            public DateTime @timestamp;
            public Guid instanceId;
            public string groupName;
            public KeyValue[] items;
        }
        #endregion
    }
}
