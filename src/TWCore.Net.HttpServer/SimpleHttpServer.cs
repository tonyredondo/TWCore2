﻿/*
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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Serialization;

namespace TWCore.Net.HttpServer
{
    /// <summary>
    /// Simple Http Server
    /// </summary>
    public class SimpleHttpServer : IDisposable
    {
        TcpListener _listener;
        CancellationTokenSource tokenSource;
        CancellationToken token;
        Task tskListener;
        long RequestCount;
        int Port;
        volatile bool active;

        #region Properties
        /// <summary>
        /// Event triggered when a new request is received
        /// </summary>
        public event OnRequestDelegate OnBeginRequest;
        /// <summary>
        /// Event triggered when the request is finalizing
        /// </summary>
        public event OnRequestDelegate OnEndRequest;
        /// <summary>
        /// Route handlers collection
        /// </summary>
        public Dictionary<HttpMethod, List<RouteHandler>> RouteHandlers { get; private set; } = new Dictionary<HttpMethod, List<RouteHandler>>();
        /// <summary>
        /// Current Web folder, where the files are located.
        /// </summary>
        public string WebFolder { get; set; } = AppContext.BaseDirectory;
        /// <summary>
        /// File extension mime types
        /// </summary>
        public Dictionary<string, string> ExtensionsMimeTypes { get; private set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".exe"] = "application/octet-stream",
            [".gif"] = "image/gif",
            [".png"] = "image/png",
            [".jpe"] = "image/jpeg",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".gz"] = "application/x-gzip",
            [".html"] = "text/html",
            [".htm"] = "text/html",
            [".xml"] = "application/xml",
            [".config"] = "application/xml",
            [".json"] = "application/json",
            [".js"] = "application/javascript",
            [".wav"] = "audio/wav",
            [".m3u"] = "audio/x-mpegurl",
            [".m4a"] = "audio/mp4",
            [".m4v"] = "video/mp4",
            [".mid"] = "audio/mid",
            [".midi"] = "audio/mid",
            [".mp3"] = "audio/mpeg",
            [".mp4"] = "video/mp4",
            [".mpa"] = "video/mpeg",
            [".mpe"] = "video/mpeg",
            [".mpg"] = "video/mpeg",
            [".mpeg"] = "video/mpeg",
            [".svg"] = "image/svg+xml",
            [".svgz"] = "image/svg+xml",
            [".txt"] = "text/plain",
        };
        #endregion

        #region .ctor
        /// <summary>
        /// Simple Http Server
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SimpleHttpServer()
        {
            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(Port), Port);
                collection.Add(nameof(RequestCount), RequestCount);
                collection.Add(nameof(WebFolder), WebFolder);
                foreach (var key in RouteHandlers.Keys)
                {
                    var value = RouteHandlers[key];
                    for (var i = 0; i < value.Count; i++)
                        collection.Add("Route " + i + ":", "[" + key + "] " + value[i].Url);
                }
            });
        }
        /// <summary>
        /// Detructor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~SimpleHttpServer()
        {
            Dispose(false);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Start the http listener
        /// </summary>
        /// <param name="port">Http port</param>
        /// <returns>Task of the start process</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task StartAsync(int port = 80)
        {
            if (!active)
            {
                Core.Log.LibVerbose("Starting HttpServer...");
                Port = port;
                tokenSource = new CancellationTokenSource();
                token = tokenSource.Token;
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _listener.Server.NoDelay = true;
                Factory.SetSocketLoopbackFastPath(_listener.Server);
                _listener.Start();
                tskListener = Task.Factory.StartNew(async () =>
                {
                    while (!token.IsCancellationRequested)
                        ThreadPool.QueueUserWorkItem(ConnectionReceived, await _listener.AcceptTcpClientAsync().ConfigureAwait(false));
                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                active = true;
                Core.Log.LibVerbose("HttpServer Started on {0}.", port);
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }
        /// <summary>
        /// Stops the http listener
        /// </summary>
        /// <returns>Task of the stop process</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task StopAsync()
        {
            if (active)
            {
                Core.Log.LibVerbose("Stopping HttpServer...");
                tokenSource.Cancel();
                await tskListener.ConfigureAwait(false);
                _listener.Stop();
                _listener = null;
                active = false;
                Core.Log.LibVerbose("HttpServer Stopped.");
            }
        }

        #region Add Handlers
        /// <summary>
        /// Adds all defined routes on a HttpController type
        /// </summary>
        /// <typeparam name="T">Type of HttpControllerBase</typeparam>
        /// <returns>SimpleHttpServer instance</returns>
        public SimpleHttpServer AddHttpControllerRoutes<T>() where T : HttpControllerBase
        {
            var type = typeof(T);
            var methods = type.GetRuntimeMethods();
            methods.Each(m =>
            {
                var routes = m.GetCustomAttributes<HttpRouteAttribute>().ToArray();
                if (routes?.Any() == true)
                {
                    routes.Each(route => AddRouteHandler(route.Method, route.Url, context =>
                    {
                        var ctl = Activator.CreateInstance<T>();
                        ctl.Context = context;
                        var mParams = m.GetParameters();
                        if (mParams.Length > 0)
                        {
                            ParameterInfo postObjectParam = null;
                            object postObject = null;
                            var ivkParams = new List<object>();
                            if (context.Request.HasPostObject)
                            {
                                postObjectParam = mParams.FirstOrDefault(p => p.GetCustomAttribute(typeof(PostObjectAttribute)) != null);
                                if (postObjectParam == null && mParams.Length == 1) postObjectParam = mParams[0];
                                if (postObjectParam != null)
                                    postObject = Try.Do(() => context.Request.GetPostObject(postObjectParam.ParameterType));
                            }
                            var dictionary = context.Route.GetRouteParameters(context.Request.Url.AbsolutePath);
                            foreach (var mParam in mParams)
                            {
                                if (mParam == postObjectParam)
                                    ivkParams.Add(postObject);
                                else if (dictionary.ContainsKey(mParam.Name))
                                    ivkParams.Add(dictionary[mParam.Name]);
                                else
                                    ivkParams.Add(null);
                            }

                            var response = m.Invoke(ctl, ivkParams.ToArray());
                            if (response == null) return;
                            var serializer = SerializerManager.GetByMimeType(ctl.Context.Response.ContentType);
                            if (serializer == null && response is string)
                                ctl.Context.Response.Write((string)response);
                            else if (serializer == null && response is ValueType)
                                ctl.Context.Response.Write(response.ToString());
                            else if (serializer == null)
                                serializer = SerializerManager.GetByMimeType(SerializerMimeTypes.Json);
                            if (serializer != null)
                                Try.Do(() => serializer.Serialize(response, response.GetType(), ctl.Context.Response.OutputStream), ex =>
                                {
                                    var sEx = new SerializableException(ex);
                                    serializer.Serialize(sEx, sEx.GetType(), ctl.Context.Response.OutputStream);
                                });
                        }
                        else
                        {
                            var response = m.Invoke(ctl, new object[0]);
                            if (response == null) return;
                            var serializer = SerializerManager.GetByMimeType(ctl.Context.Response.ContentType);
                            if (serializer == null && response is string)
                                ctl.Context.Response.Write((string)response);
                            else if (serializer == null && response is ValueType)
                                ctl.Context.Response.Write(response.ToString());
                            else if (serializer == null)
                                serializer = SerializerManager.GetByMimeType(SerializerMimeTypes.Json);
                            if (serializer != null)
                                Try.Do(() => serializer.Serialize(response, response.GetType(), ctl.Context.Response.OutputStream), ex =>
                                {
                                    var sEx = new SerializableException(ex);
                                    serializer.Serialize(sEx, sEx.GetType(), ctl.Context.Response.OutputStream);
                                });
                        }
                    }));
                }
            });
            return this;
        }
        /// <summary>
        /// Add a route to a controller
        /// </summary>
        /// <typeparam name="T">Type of HttpControllerBase</typeparam>
        /// <param name="method">Http method to bind</param>
        /// <param name="url">Route url to bind</param>
        /// <param name="action">Action to execute</param>
        /// <returns>SimpleHttpServer instance</returns>
        public SimpleHttpServer AddHttpController<T>(HttpMethod method, string url, string action = null) where T : HttpControllerBase
            => AddRouteHandler(method, url, (context) =>
            {
                var ctl = Activator.CreateInstance<T>();
                ctl.Context = context;
                if (action == null)
                    action = url.Substring(1);
                var ctlMethod = ctl.GetType().GetRuntimeMethods().FirstOrDefault(m => m.Name.ToLowerInvariant() == action.ToLowerInvariant());
                if (ctlMethod == null) return;
                var mParams = ctlMethod.GetParameters();
                if (mParams.Length > 0)
                {
                    ParameterInfo postObjectParam = null;
                    object postObject = null;
                    var ivkParams = new List<object>();
                    if (context.Request.HasPostObject)
                    {
                        postObjectParam = mParams.FirstOrDefault(p => p.GetCustomAttribute(typeof(PostObjectAttribute)) != null);
                        if (postObjectParam == null && mParams.Length == 1) postObjectParam = mParams[0];
                        if (postObjectParam != null)
                            postObject = Try.Do(() => context.Request.GetPostObject(postObjectParam.ParameterType));
                    }
                    var dictionary = context.Route.GetRouteParameters(context.Request.Url.AbsolutePath);
                    foreach (var mParam in mParams)
                    {
                        if (mParam == postObjectParam)
                            ivkParams.Add(postObject);
                        else if (dictionary.ContainsKey(mParam.Name))
                            ivkParams.Add(dictionary[mParam.Name]);
                        else
                            ivkParams.Add(null);
                    }

                    var response = ctlMethod.Invoke(ctl, ivkParams.ToArray());
                    if (response == null) return;
                    var serializer = SerializerManager.GetByMimeType(ctl.Context.Response.ContentType);
                    if (serializer == null && response is string)
                        ctl.Context.Response.Write((string)response);
                    else if (serializer == null && response is ValueType)
                        ctl.Context.Response.Write(response.ToString());
                    else if (serializer == null)
                        serializer = SerializerManager.GetByMimeType(SerializerMimeTypes.Json);
                    if (serializer != null)
                        Try.Do(() => serializer.Serialize(response, response.GetType(), ctl.Context.Response.OutputStream), ex =>
                        {
                            var sEx = new SerializableException(ex);
                            serializer.Serialize(sEx, sEx.GetType(), ctl.Context.Response.OutputStream);
                        });
                }
                else
                {
                    var response = ctlMethod.Invoke(ctl, new object[0]);
                    if (response == null) return;
                    var serializer = SerializerManager.GetByMimeType(ctl.Context.Response.ContentType);
                    if (serializer == null && response is string)
                        ctl.Context.Response.Write((string)response);
                    else if (serializer == null && response is ValueType)
                        ctl.Context.Response.Write(response.ToString());
                    else if (serializer == null)
                        serializer = SerializerManager.GetByMimeType(SerializerMimeTypes.Json);
                    if (serializer != null)
                        Try.Do(() => serializer.Serialize(response, response.GetType(), ctl.Context.Response.OutputStream), ex =>
                        {
                            var sEx = new SerializableException(ex);
                            serializer.Serialize(sEx, sEx.GetType(), ctl.Context.Response.OutputStream);
                        });
                }
            });
        /// <summary>
        /// Adds a new route handler
        /// </summary>
        /// <param name="method">Http method</param>
        /// <param name="url">Route url</param>
        /// <param name="handler">Delegate handler</param>
        /// <returns>SimpleHttpServer instance</returns>
        public SimpleHttpServer AddRouteHandler(HttpMethod method, string url, OnRequestHandler handler)
        {
            if (!RouteHandlers.ContainsKey(method))
                RouteHandlers[method] = new List<RouteHandler>();
            RouteHandlers[method].Add(new RouteHandler(method, url, handler));
            return this;
        }
        /// <summary>
        /// Adds a Http GET route
        /// </summary>
        /// <param name="url">Route url</param>
        /// <param name="handler">Delegate handler</param>
        /// <returns>SimpleHttpServer instance</returns>
        public SimpleHttpServer AddGetHandler(string url, OnRequestHandler handler) => AddRouteHandler(HttpMethod.GET, url, handler);
        /// <summary>
        /// Adds a Http POST route
        /// </summary>
        /// <param name="url">Route url</param>
        /// <param name="handler">Delegate handler</param>
        /// <returns>SimpleHttpServer instance</returns>
        public SimpleHttpServer AddPostHandler(string url, OnRequestHandler handler) => AddRouteHandler(HttpMethod.POST, url, handler);
        /// <summary>
        /// Adds a Http PUT route
        /// </summary>
        /// <param name="url">Route url</param>
        /// <param name="handler">Delegate handler</param>
        /// <returns>SimpleHttpServer instance</returns>
        public SimpleHttpServer AddPutHandler(string url, OnRequestHandler handler) => AddRouteHandler(HttpMethod.PUT, url, handler);
        /// <summary>
        /// Adds a Http HEAD route
        /// </summary>
        /// <param name="url">Route url</param>
        /// <param name="handler">Delegate handler</param>
        /// <returns>SimpleHttpServer instance</returns>
        public SimpleHttpServer AddHeadHandler(string url, OnRequestHandler handler) => AddRouteHandler(HttpMethod.HEAD, url, handler);
        /// <summary>
        /// Adds a Http DELETE route
        /// </summary>
        /// <param name="url">Route url</param>
        /// <param name="handler">Delegate handler</param>
        /// <returns>SimpleHttpServer instance</returns>
        public SimpleHttpServer AddDeleteHandler(string url, OnRequestHandler handler) => AddRouteHandler(HttpMethod.DELETE, url, handler);
        #endregion

        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ConnectionReceived(object objClient)
        {
            TcpClient client = (TcpClient)objClient;
            var reqNumber = Interlocked.Increment(ref RequestCount);
            using (var watch = Watch.Create($"HTTP REQUEST START: {reqNumber}", $"HTTP REQUEST END: {reqNumber}"))
            {
                HttpContext context = null;

                try
                {
                    #region Context
                    //watch.Tap("Creating context...");
                    context = new HttpContext(client);
                    if (!context.LoadRequest())
                    {
                        Core.Log.Warning($"{reqNumber} The request could not be loaded.");
                        client.Dispose();
                        return;
                    }
                    watch.Tap("Request Loaded.");
                    #endregion

                    bool handled = false;

                    #region OnBeginRequest
                    if (OnBeginRequest != null)
                    {
                        Core.Log.LibVerbose($"{reqNumber} - On Begin Request Method");
                        OnBeginRequest(context, ref handled);
                    }
                    #endregion

                    #region Route handler
                    var uHandler = RouteHandlers.TryGetValue(context.Request.Method, out var handlers) ? handlers.FirstOrDefault(r => r.Match(context.Request.Url.AbsolutePath)) : null;
                    if (uHandler != null)
                    {
                        watch.Tap("Url Handle: " + uHandler.Method + " " + uHandler.Url);
                        context.Response.StatusCode = HttpResponse.HttpStatusCode.OK;
                        context.Route = uHandler;
                        uHandler.Handler?.Invoke(context);
                        handled = true;
                    }
                    #endregion

                    #region OnEndRequest
                    if (OnEndRequest != null)
                    {
                        Core.Log.LibVerbose($"{reqNumber} - On End Request Method");
                        OnEndRequest(context, ref handled);
                    }
                    #endregion

                    if (!handled)
                    {
                        if (context.Request.Method == HttpMethod.GET && WebFolder.IsNotNullOrEmpty())
                        {
                            #region Not handled and is a file
                            var filePath = context.Request.Url.LocalPath.Substring(1).Replace("/", "\\");
                            filePath = Path.Combine(WebFolder, filePath);
                            if (File.Exists(filePath))
                            {
                                context.Response.StatusCode = HttpResponse.HttpStatusCode.OK;
                                var extension = Path.GetExtension(filePath);
                                if (ExtensionsMimeTypes.TryGetValue(extension, out var ctype))
                                    context.Response.ContentType = ctype;
                                else
                                    context.Response.ContentType = "application/octet-stream";
                                Core.Log.LibVerbose($"{reqNumber} - Sending file: {filePath}...");
                                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                    stream.WriteToStream(context.Response.OutputStream);
                                Core.Log.LibVerbose($"{reqNumber} - File {filePath} sent.");
                                handled = true;
                            }
                            #endregion
                        }

                        if (!handled)
                        {
                            Core.Log.LibVerbose($"{reqNumber} - Response: 404 Not Found");
                            context.Response.StatusCode = HttpResponse.HttpStatusCode.Not_Found;
                            context.Response.Write("404 Not Found");
                        }
                    }
                    context.CloseContext();
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                    if (context != null)
                    {
                        Core.Log.LibVerbose($"{reqNumber} - Response: 500 Internal Server Error");
                        try
                        {
                            context.Response.StatusCode = HttpResponse.HttpStatusCode.Internal_Server_Error;
                            context.Response.Write("500 Internal Server Error");
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }

                try
                {
                    client.Dispose();
                }
                catch (Exception ex)
                {
                    Core.Log.Error(ex, "{0} - Error: {1}", reqNumber, ex.Message);
                }
            }
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls
        /// <summary>
        /// Dispose method
        /// </summary>
        /// <param name="disposing">true if dispossing the managed properties</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (active)
                    StopAsync().Wait();
                disposedValue = true;
            }
        }
        /// <summary>
        /// Dispose method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
