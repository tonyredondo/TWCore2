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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TWCore.Net.HttpServer;
using TWCore.Net.Multicast;
using TWCore.Serialization;
using TWCore.Settings;
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

// ReSharper disable CheckNamespace

namespace TWCore.Diagnostics.Status.Transports
{
    /// <inheritdoc />
    /// <summary>
    /// Http server status transport
    /// </summary>
    public class HttpStatusTransport : IStatusTransport
    {
        private const int MaxNumberOfTries = 3;
        private readonly int _numberOfTries;
        private readonly Guid _discoveryServiceId;
        private SimpleHttpServer _httpServer;

        #region Events
        /// <summary>
        /// Handles when a fetch status event has been received
        /// </summary>
        public event FetchStatusDelegate OnFetchStatus;
        #endregion

        #region .ctor
        /// <summary>
        /// Http server status transport
        /// </summary>
        /// <param name="port">Status http port</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HttpStatusTransport(int port = 80)
        {
            var htmlPage = this.GetAssembly().GetResourceString("Status.htm");
            var xmlSerializer = new XmlTextSerializer();
            var jsonSerializer = new JsonTextSerializer() { UseCamelCase = true };
            _httpServer = new SimpleHttpServer();
            _httpServer.AddGetHandler("/", ctx =>
            {
                ctx.Response.Write(htmlPage);
            });
            _httpServer.AddGetHandler("/xml", ctx =>
            {
                if (OnFetchStatus == null) return;
                var statuses = OnFetchStatus.Invoke();
                ctx.Response.ContentType = SerializerMimeTypes.Xml;
                xmlSerializer.Serialize(statuses, ctx.Response.OutputStream);
            });
            _httpServer.AddGetHandler("/json", ctx =>
            {
                if (OnFetchStatus == null) return;
                var statuses = OnFetchStatus.Invoke();
                ctx.Response.ContentType = SerializerMimeTypes.Json;
                jsonSerializer.Serialize(statuses, ctx.Response.OutputStream);
            });
            _httpServer.AddGetHandler("/gccollect", ctx => 
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                ctx.Response.ContentType = SerializerMimeTypes.Json;
                ctx.Response.Write("true");
            });
            _httpServer.AddGetHandler("/discovery", ctx =>
            {
                var services = DiscoveryService.GetRegisteredServices();
                var statusServices = services.Where(s => s.Category == DiscoveryService.FrameworkCategory && s.Name == "STATUS.HTTP").ToArray();
                ctx.Response.WriteLine("<html><head><title>Discovered Status Services</title></head><body style='padding:30px;'><h1 style='text-align:center;'>Discovered status services</h1>");
                foreach(var g in statusServices.GroupBy(s => new { s.EnvironmentName, s.MachineName }).OrderBy(s => s.Key.EnvironmentName))
                {
                    ctx.Response.WriteLine($"<h3>Environment: {g.Key.EnvironmentName} - Machine: {g.Key.MachineName}</h3>");
                    ctx.Response.WriteLine("<ul>");
                    foreach (var ss in g)
                    {
                        var dct = (Dictionary<string, object>)ss.Data.GetValue();
                        ctx.Response.WriteLine("<li style='list-style-type: none;'>");
                        foreach (var ssAddress in ss.Addresses)
                        {
                            ctx.Response.WriteLine($"<a href='http://{ssAddress.ToString()}:{dct["Port"]}/' target='_blank' style='text-decoration: none;color: blue;'>{ssAddress.ToString()}</a> /");
                        }
                        ctx.Response.WriteLine($" {ss.ApplicationName}</li>");
                    }
                    ctx.Response.WriteLine("</ul>");
                }
                ctx.Response.WriteLine("</body></html>");
            });
            var connected = false;
            while (true)
            {
                try
                {
                    _numberOfTries++;
                    _httpServer.StartAsync(port).Wait();
                    connected = true;
                    break;
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                    if (_numberOfTries > MaxNumberOfTries)
                        break;
                    Factory.Thread.Sleep(1000);
                }
            }
            if (!connected)
            {
                Core.Log.Error("There was a problem trying to bind the Http Server on port {0}, please check the exception messages", port);
            }
            else
            {
                var settings = Core.GetSettings<HttpStatusSettings>();
                if (settings.Discovery)
                {
                    _discoveryServiceId = DiscoveryService.RegisterService(DiscoveryService.FrameworkCategory, "STATUS.HTTP", "Status engine http transport service", new Dictionary<string, object>
                    {
                        ["Port"] = port
                    });
                }
            }
            Core.Status.DeAttachObject(_httpServer);
        }
        /// <summary>
        /// Detructor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~HttpStatusTransport()
        {
            DiscoveryService.UnregisterService(_discoveryServiceId);
            Dispose();
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Try.Do(() =>
            {
                _httpServer?.StopAsync().Wait();
                _httpServer = null;
            }, false);
        }
        #endregion

        #region Nested Type
        [SettingsContainer("Core")]
        private class HttpStatusSettings : SettingsBase
        {
            /// <summary>
            /// Enable or disable the discovery of the status transport
            /// </summary>
            [SettingsKey("Status.Http.Discovery")]
            public bool Discovery { get; set; } = true;
        }
        #endregion
    }
}
