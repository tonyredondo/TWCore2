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
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace TWCore.Net.HttpServer
{
    /// <summary>
    /// Delegate for the request handler
    /// </summary>
    /// <param name="context"></param>
    public delegate void OnRequestHandler(HttpContext context);
    /// <summary>
    /// Delegate for the unhandled request events
    /// </summary>
    /// <param name="context"></param>
    /// <param name="handled"></param>
    /// <param name="cancellationToken">Connection cancellation token</param>
    public delegate void OnRequestDelegate(HttpContext context, ref bool handled, CancellationToken cancellationToken);

    /// <summary>
    /// Http context
    /// </summary>
    public class HttpContext
    {
        /// <summary>
        /// Http request container
        /// </summary>
        public HttpRequest Request { get; private set; }
        /// <summary>
        /// Http response container
        /// </summary>
        public HttpResponse Response { get; private set; }
        /// <summary>
        /// Route handler
        /// </summary>
        public RouteHandler Route { get; internal set; }

        /// <summary>
        /// Http context
        /// </summary>
        /// <param name="socketClient">Socket handling the context</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal HttpContext(TcpClient socketClient)
        {
            Request = new HttpRequest(socketClient, this);
            Response = new HttpResponse(socketClient, this);
        }

        /// <summary>
        /// Loads the Request container
        /// </summary>
        /// <returns>true if the request container was loaded successfully; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool LoadRequest()
        {
            try
            {
                return Request.Process();
            }
            catch(System.IO.IOException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                return false;
            }
        }
        /// <summary>
        /// Close Current Context
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CloseContext()
        {
            Response.EventStream.Write(new byte[0], 0, 0);
            Response.EventStream.Flush();
            Response.EventStream.Dispose();
        }
    }
}
