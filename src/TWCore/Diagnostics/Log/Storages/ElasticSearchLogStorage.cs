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

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Serialization;
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable SwitchStatementMissingSomeCases

namespace TWCore.Diagnostics.Log.Storages
{
    /// <summary>
    /// ElasticSearch log storage
    /// </summary>
    [StatusName("ElasticSearch Log")]
    public class ElasticSearchLogStorage : ILogStorage
    {
        private readonly Timer _timer;
        private readonly Uri _url;
        private readonly string _indexFormat;
        private volatile bool _processing;
        private int _count;
        private readonly BlockingCollection<SourceData> _logItems;
        private bool _enabled = true;
        private StringBuilder _builderBuffer = new StringBuilder();
        private JsonTextSerializer serializer = new JsonTextSerializer
        {
            EnumsAsStrings = true
        };
        private WebClient _client;

        #region .ctor
        /// <summary>
        /// ElasticSearch log storage
        /// </summary>
        /// <param name="url">Elastic search json api url</param>
        /// <param name="indexFormat">Index format</param>
        /// <param name="periodInSeconds">Fetch period in seconds</param>
        public ElasticSearchLogStorage(string url, string indexFormat, int periodInSeconds = 10)
        {
            _url = new UriBuilder(url) { Path = "_bulk" }.Uri;
            _indexFormat = indexFormat?.ToLowerInvariant();
            _client = new WebClient();
            _logItems = new BlockingCollection<SourceData>();
            var period = TimeSpan.FromSeconds(periodInSeconds);
            _timer = new Timer(TimerCallback, this, period, period);
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
            if (item is LogItem logItem && Interlocked.Increment(ref _count) < 10_000)
                _logItems.Add(new SourceData
                {
                    timestamp = logItem.Timestamp,
                    level = logItem.Level,
                    message = logItem.Message,
                    instanceId = Core.InstanceId,
                    environmentName = logItem.EnvironmentName,
                    machineName = logItem.MachineName,
                    applicationName = logItem.ApplicationName,
                    processName = logItem.ProcessName,
                    processId = logItem.ProcessId,
                    assemblyName = logItem.AssemblyName,
                    code = logItem.Code,
                    groupName = logItem.GroupName,
                    exception = logItem.Exception
                });
            return Task.CompletedTask;
        }

        /// <summary>
        /// Writes a log item empty line
        /// </summary>
        public Task WriteEmptyLineAsync()
            => Task.CompletedTask;

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
            if (_processing) return;
            _processing = true;
            try
            {
                if (_logItems.Count == 0)
                {
                    _processing = false;
                    return;
                }
                
                var count = 0;
                while (count++ < 2048 && _logItems.TryTake(out var item, 10))
                {
                    var index = _indexFormat.ApplyFormat(item.@timestamp, item.applicationName, item.environmentName);
                    _builderBuffer.Append("{\"index\":{\"_index\":\"" + index + "\",\"_type\":\"logevent\"}}\n");
                    _builderBuffer.Append(serializer.SerializeToString(item) + "\n");
                    Interlocked.Decrement(ref _count);
                }
                var data = _builderBuffer.ToString();
                _builderBuffer.Clear();
                Core.Log.LibDebug("Sending {0} log items to elastic search.", count);
                _client.Headers[HttpRequestHeader.Accept] = "application/json";
                _client.Headers[HttpRequestHeader.ContentType] = "application/json";
                var result = _client.UploadString(_url, "POST", data);
            }
            catch (UriFormatException fException)
            {
                Core.Log.Error(fException, $"Disabling {nameof(ElasticSearchLogStorage)}. Reason: {fException.Message}");
                _enabled = false;
                _timer.Dispose();
            }
            catch (Exception ex)
            {
                //
            }
            _processing = false;
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
            public int processId;
            public string assemblyName;
            public string code;
            public string groupName;
            public SerializableException exception;
        }
        #endregion
    }
}
