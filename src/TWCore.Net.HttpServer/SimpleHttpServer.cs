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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Serialization;
// ReSharper disable MethodSupportsCancellation
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
#pragma warning disable 4014

namespace TWCore.Net.HttpServer
{
    /// <inheritdoc />
    /// <summary>
    /// Simple Http Server
    /// </summary>
    public sealed class SimpleHttpServer : IDisposable
    {
        private TcpListener _listener;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        private Task _tskListener;
        private long _requestCount;
        private int _port;
        private volatile bool _active;

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
            [".txt"] = "text/plain"
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
                collection.Add(nameof(_port), _port);
                collection.Add(nameof(_requestCount), _requestCount);
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
            if (_active) return Task.CompletedTask;

            Core.Log.LibVerbose("Starting HttpServer...");
            _port = port;
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listener.Server.NoDelay = true;
            Factory.SetSocketLoopbackFastPath(_listener.Server);
            _listener.Start();
            _tskListener = TcpListenerAsync();
            _active = true;
            Core.Log.LibVerbose("HttpServer Started on {0}.", port);
            return Task.CompletedTask;
        }
        /// <summary>
        /// Stops the http listener
        /// </summary>
        /// <returns>Task of the stop process</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task StopAsync()
        {
            if (_active)
            {
                Core.Log.LibVerbose("Stopping HttpServer...");
                _tokenSource.Cancel();
                await _tskListener.ConfigureAwait(false);
                _listener.Stop();
                _listener = null;
                _active = false;
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
            foreach (var m in methods)
            {
                var routes = m.GetCustomAttributes<HttpRouteAttribute>().ToArray();
                if (routes.Any())
                {
                    foreach (var route in routes)
                    {
                        AddRouteHandler(route.Method, route.Url, context =>
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
                                    if (postObjectParam is null && mParams.Length == 1) postObjectParam = mParams[0];
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
                                if (response is null) return;
                                var serializer = SerializerManager.GetByMimeType(ctl.Context.Response.ContentType);
                                switch (serializer)
                                {
                                    case null when response is string:
                                        ctl.Context.Response.Write((string) response);
                                        break;
                                    case null when response is ValueType:
                                        ctl.Context.Response.Write(response.ToString());
                                        break;
                                    case null:
                                        serializer = SerializerManager.GetByMimeType(SerializerMimeTypes.Json);
                                        break;
                                }

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
                                if (response is null) return;
                                var serializer = SerializerManager.GetByMimeType(ctl.Context.Response.ContentType);
                                switch (serializer)
                                {
                                    case null when response is string:
                                        ctl.Context.Response.Write((string) response);
                                        break;
                                    case null when response is ValueType:
                                        ctl.Context.Response.Write(response.ToString());
                                        break;
                                    case null:
                                        serializer = SerializerManager.GetByMimeType(SerializerMimeTypes.Json);
                                        break;
                                }

                                if (serializer != null)
                                    Try.Do(() => serializer.Serialize(response, response.GetType(), ctl.Context.Response.OutputStream), ex =>
                                    {
                                        var sEx = new SerializableException(ex);
                                        serializer.Serialize(sEx, sEx.GetType(), ctl.Context.Response.OutputStream);
                                    });
                            }
                        });
                    }
                }
            };
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
            => AddRouteHandler(method, url, context =>
            {
                var ctl = Activator.CreateInstance<T>();
                ctl.Context = context;
                if (action is null)
                    action = url.Substring(1);
                var ctlMethod = ctl.GetType().GetRuntimeMethods().FirstOrDefault((m, mAction) => string.Equals(m.Name, mAction, StringComparison.InvariantCultureIgnoreCase), action);
                if (ctlMethod is null) return;
                var mParams = ctlMethod.GetParameters();
                if (mParams.Length > 0)
                {
                    ParameterInfo postObjectParam = null;
                    object postObject = null;
                    var ivkParams = new List<object>();
                    if (context.Request.HasPostObject)
                    {
                        postObjectParam = mParams.FirstOrDefault(p => p.GetCustomAttribute(typeof(PostObjectAttribute)) != null);
                        if (postObjectParam is null && mParams.Length == 1) postObjectParam = mParams[0];
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
                    if (response is null) return;
                    var serializer = SerializerManager.GetByMimeType(ctl.Context.Response.ContentType);
                    switch (serializer)
                    {
                        case null when response is string:
                            ctl.Context.Response.Write((string)response);
                            break;
                        case null when response is ValueType:
                            ctl.Context.Response.Write(response.ToString());
                            break;
                        case null:
                            serializer = SerializerManager.GetByMimeType(SerializerMimeTypes.Json);
                            break;
                    }
                    if (serializer != null)
                        Try.Do(() => serializer.Serialize(response, response.GetType(), ctl.Context.Response.OutputStream), ex =>
                        {
                            var sEx = new SerializableException(ex);
                            serializer.Serialize(sEx, sEx.GetType(), ctl.Context.Response.OutputStream);
                        });
                }
                else
                {
                    var response = ctlMethod.Invoke(ctl, Array.Empty<object>());
                    if (response is null) return;
                    var serializer = SerializerManager.GetByMimeType(ctl.Context.Response.ContentType);
                    switch (serializer)
                    {
                        case null when response is string:
                            ctl.Context.Response.Write((string)response);
                            break;
                        case null when response is ValueType:
                            ctl.Context.Response.Write(response.ToString());
                            break;
                        case null:
                            serializer = SerializerManager.GetByMimeType(SerializerMimeTypes.Json);
                            break;
                    }
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
        private async Task TcpListenerAsync()
        {
            var tokenTask = _token.WhenCanceledAsync();
            while (!_token.IsCancellationRequested)
            {
                var listenerTask = _listener.AcceptTcpClientAsync();
                var result = await Task.WhenAny(listenerTask, tokenTask).ConfigureAwait(false);
                if (result == tokenTask) break;
                Task.Factory.StartNew(ConnectionReceived, listenerTask.Result, _token);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ConnectionReceived(object objClient)
        {
            var client = (TcpClient)objClient;
            var cts = new CancellationTokenSource();
            var reqNumber = Interlocked.Increment(ref _requestCount);
            //using (var watch = Watch.Create($"HTTP REQUEST START: {reqNumber}", $"HTTP REQUEST END: {reqNumber}"))
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
                    //watch.Tap("Request Loaded.");
                    #endregion

                    var handled = false;

                    #region OnBeginRequest
                    if (OnBeginRequest != null)
                    {
                        Core.Log.LibVerbose($"{reqNumber} - On Begin Request Method");
                        OnBeginRequest(context, ref handled, cts.Token);
                    }
                    #endregion

                    #region Route handler
                    var uHandler = RouteHandlers.TryGetValue(context.Request.Method, out var handlers) ? handlers.FirstOrDefault((r, absPath) => r.Match(absPath), context.Request.Url.AbsolutePath) : null;
                    if (uHandler != null)
                    {
                        //watch.Tap("Url Handle: " + uHandler.Method + " " + uHandler.Url);
                        Core.Log.LibDebug("Url Handle: " + uHandler.Method + " " + uHandler.Url);
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
                        OnEndRequest(context, ref handled, cts.Token);
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
                                context.Response.ContentType = ExtensionsMimeTypes.TryGetValue(extension, out var ctype) ? ctype : "application/octet-stream";
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
                    cts.Cancel();
                    context.CloseContext();
                }
                catch (Exception ex)
                {
                    cts.Cancel();
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
        private bool _disposedValue; // To detect redundant calls
        /// <summary>
        /// Dispose method
        /// </summary>
        /// <param name="disposing">true if dispossing the managed properties</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing) { }
            if (_active)
                StopAsync().Wait();
            _disposedValue = true;
        }
        /// <inheritdoc />
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
