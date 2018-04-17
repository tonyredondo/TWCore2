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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using TWCore.Diagnostics.Status;
using TWCore.Net.Multicast;
using TWCore.Serialization;

namespace TWCore.Object.Api.Controllers
{
    public class StatusController : Controller
    {
        private static readonly ReferencePool<WebClient> WebClients = new ReferencePool<WebClient>();
        private static readonly string HtmlPage = typeof(StatusController).Assembly.GetResourceString("Status.htm");
        private static readonly JsonTextSerializer JsonSerializer = new JsonTextSerializer { UseCamelCase = true };

        [HttpGet("api/status/getall.{format}")]
        [HttpGet("api/status/getall")]
        [FormatFilter]
        public async Task<StatusCollection> GetAll()
        {
            var statusHttpServices = DiscoveryService.GetLocalRegisteredServices("STATUS.HTTP").DistinctBy(srv => srv.ApplicationName);
            var collection = new StatusCollection();
            var getTasks = new List<(Task<string> Data, string IpAddress, string Port, WebClient Client)>();
            foreach (var srv in statusHttpServices)
            {
                if (!(srv.Data.GetValue() is Dictionary<string, object> data)) continue;
                if (!data.TryGetValue("Port", out var port)) continue;
                var client = WebClients.New();
                var clientTask = client.DownloadStringTaskAsync($"http://{srv.Addresses[0]}:{port}/xml");
                getTasks.Add((clientTask, srv.Addresses[0].ToString(), port.ToString(), client));
            }
            await Task.WhenAll(getTasks.Select(i => i.Data)).ConfigureAwait(false);
            foreach(var item in getTasks)
            {
                var statusCollection = item.Data.Result.DeserializeFromXml<StatusItemCollection>();
                collection.Statuses.Add(new StatusCollectionItem { Data = statusCollection, IpAddress = item.IpAddress, Port = item.Port });
                WebClients.Store(item.Client);
            }
            collection.Statuses.Sort((x, y) =>
            {
                var cmp = string.Compare(x.Data.EnvironmentName, y.Data.EnvironmentName, StringComparison.Ordinal);
                if (cmp == 0)
                    cmp = string.Compare(x.Data.MachineName, y.Data.MachineName, StringComparison.Ordinal);
                if (cmp == 0)
                    cmp = string.Compare(x.Data.ApplicationName, y.Data.ApplicationName, StringComparison.Ordinal);
                return cmp;
            });
            return collection;
        }

        [HttpGet("api/status/get/{ip}/{port}")]
        [FormatFilter]
        public async Task<ActionResult> Get(string ip, string port)
        {
            var statusHttpServices = DiscoveryService.GetRegisteredServices("STATUS.HTTP").DistinctBy(srv => srv.ApplicationName);
            foreach (var srv in statusHttpServices)
            {
                if (!(srv.Data.GetValue() is Dictionary<string, object> data)) continue;
                if (!data.TryGetValue("Port", out var mPort)) continue;
                if (port != mPort.ToString()) continue;
                if (srv.Addresses.Any(addr => addr.ToString() != ip)) continue;
                var client = WebClients.New();
                var response = await client.DownloadStringTaskAsync($"http://{ip}:{port}/xml").ConfigureAwait(false);
                var statusCollection = response.DeserializeFromXml<StatusItemCollection>();
                WebClients.Store(client);
                return Content(JsonSerializer.SerializeToString(statusCollection), "application/json");
            }
            return Content("{}", "application/json");
        }

        [HttpGet("api/status/get/{port}")]
        [FormatFilter]
        public async Task<ActionResult> Get(string port)
        {
            var statusHttpServices = DiscoveryService.GetLocalRegisteredServices("STATUS.HTTP").DistinctBy(srv => srv.ApplicationName);
            foreach (var srv in statusHttpServices)
            {
                if (!(srv.Data.GetValue() is Dictionary<string, object> data)) continue;
                if (!data.TryGetValue("Port", out var mPort)) continue;
                if (port != mPort.ToString()) continue;
                var client = WebClients.New();
                var response = await client.DownloadStringTaskAsync($"http://localhost:{port}/xml").ConfigureAwait(false);
                var statusCollection = response.DeserializeFromXml<StatusItemCollection>();
                WebClients.Store(client);
                return Content(JsonSerializer.SerializeToString(statusCollection), "application/json");
            }
            return Content("{}", "application/json");
        }

        [HttpGet("status/{port}")]
        public ActionResult Page(int port)
        {
            var html = HtmlPage.Replace("$JSONURL$", $"/api/status/get/{port}");
            return Content(html, "text/html");
        }
        [HttpGet("status/{ip}/{port}")]
        public ActionResult Page(string ip, int port)
        {
            var html = HtmlPage.Replace("$JSONURL$", $"/api/status/get/{ip}/{port}");
            return Content(html, "text/html");
        }
    }



    [DataContract]
    public class StatusCollection
    {
        [XmlAttribute, DataMember]
        public DateTime DateTime { get; set; } = Core.Now;
        [XmlElement("Status"), DataMember]
        public List<StatusCollectionItem> Statuses { get; set; } = new List<StatusCollectionItem>();
    }
    [DataContract]
    public class StatusCollectionItem
    {
        [XmlAttribute, DataMember]
        public string IpAddress { get; set; }
        [XmlAttribute, DataMember]
        public string Port { get; set; }
        [XmlElement(), DataMember]
        public StatusItemCollection Data { get; set; }
    }
}
