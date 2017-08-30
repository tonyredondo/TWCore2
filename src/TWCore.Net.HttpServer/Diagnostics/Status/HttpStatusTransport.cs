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
using System.Runtime.CompilerServices;
using TWCore.Net.HttpServer;
using TWCore.Net.Multicast;
using TWCore.Serialization;
using TWCore.Settings;

namespace TWCore.Diagnostics.Status.Transports
{
    /// <summary>
    /// Http server status transport
    /// </summary>
    public class HttpStatusTransport : IStatusTransport
    {
        SimpleHttpServer httpServer;
        int maxNumberOfTries = 3;
        int numberOfTries;
        string htmlPage;
        ISerializer xmlSerializer;
        ISerializer jsonSerializer;

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
            htmlPage = this.GetAssembly().GetResourceString("Status.htm");
            xmlSerializer = new XmlTextSerializer();
            jsonSerializer = new JsonTextSerializer() { UseCamelCase = false };
            httpServer = new SimpleHttpServer();
            httpServer.AddGetHandler("/", ctx =>
            {
                ctx.Response.Write(htmlPage);
            });
            httpServer.AddGetHandler("/xml", ctx =>
            {
                if (OnFetchStatus != null)
                {
                    var statuses = OnFetchStatus.Invoke();
                    ctx.Response.ContentType = SerializerMimeTypes.Xml;
                    xmlSerializer.Serialize(statuses, ctx.Response.OutputStream);
                }
            });
            httpServer.AddGetHandler("/json", ctx =>
            {
                if (OnFetchStatus != null)
                {
                    var statuses = OnFetchStatus.Invoke();
                    ctx.Response.ContentType = SerializerMimeTypes.Json;
                    jsonSerializer.Serialize(statuses, ctx.Response.OutputStream);
                }
            });
            httpServer.AddGetHandler("/gccollect", ctx => 
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                ctx.Response.ContentType = SerializerMimeTypes.Json;
                ctx.Response.Write("true");
            });
            bool connected = false;
            while (true)
            {
                try
                {
                    numberOfTries++;
                    httpServer.StartAsync(port).Wait();
                    connected = true;
                    break;
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                    if (numberOfTries > maxNumberOfTries)
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
                    DiscoveryService.RegisterService(DiscoveryService.FRAMEWORK_CATEGORY, "STATUS", "Status engine http transport service", new Dictionary<string, object>
                    {
                        ["Port"] = port
                    });
                }
            }
            Core.Status.DeAttachObject(httpServer);
        }
        /// <summary>
        /// Detructor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~HttpStatusTransport()
        {
            Dispose();
        }
        /// <summary>
        /// Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Try.Do(() =>
            {
                httpServer?.StopAsync().Wait();
                httpServer = null;
            }, false);
        }
        #endregion

        #region Nested Type
        [SettingsContainer("Core")]
        class HttpStatusSettings : SettingsBase
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
