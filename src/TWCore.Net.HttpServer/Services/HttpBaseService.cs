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

using System.Runtime.CompilerServices;
using TWCore.Net.HttpServer;
// ReSharper disable CheckNamespace
// ReSharper disable InheritdocConsiderUsage

namespace TWCore.Services
{
    /// <summary>
    /// Http Based Service
    /// </summary>
    public abstract class HttpBaseService : HttpControllerBase, IService
    {
        #region Fields
        private SimpleHttpServer _httpServer;
        #endregion

        #region Properties
        /// <summary>
        /// Get if the service support pause and continue
        /// </summary>
        public bool CanPauseAndContinue => true;
        /// <summary>
        /// Http binding port
        /// </summary>
        public int Port { get; protected set; } = 8085;
        /// <summary>
        /// Web folder, where the files are located.
        /// </summary>
        public string WebFolder { get; protected set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Http Based Service
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected HttpBaseService() { }
        /// <summary>
        /// Http Based Service
        /// </summary>
        /// <param name="port">Http binding port</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected HttpBaseService(int port)
        {
            Port = port;
        }
        /// <summary>
        /// Http Based Service
        /// </summary>
        /// <param name="port">Http binding port</param>
        /// <param name="webFolder">Web folder, where the files are located.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HttpBaseService(int port, string webFolder)
        {
            Port = port;
            WebFolder = webFolder;
        }
        #endregion

        #region Methods
        /// <summary>
        /// On Continue from pause method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IService.OnContinue()
        {
            _httpServer.StartAsync(Port);
        }
        /// <summary>
        /// On Pause method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IService.OnPause()
        {
            _httpServer.StopAsync().WaitAsync();
        }
        /// <summary>
        /// On shutdown requested method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IService.OnShutdown()
        {
            _httpServer.StopAsync().WaitAsync();
        }
        /// <summary>
        /// On Service Start method
        /// </summary>
        /// <param name="args">Start arguments</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IService.OnStart(string[] args)
        {
            _httpServer = new SimpleHttpServer();
            WebFolder = _httpServer.WebFolder;
            OnBinding(_httpServer);
            WebFolder = _httpServer.WebFolder;
            _httpServer.StartAsync(Port);
        }
        /// <summary>
        /// On Service Stops method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IService.OnStop()
        {
            _httpServer.StopAsync().WaitAsync();
        }
        #endregion

        /// <summary>
        /// On Http Server Binding
        /// </summary>
        /// <param name="server">SimpleHttpServer instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnBinding(SimpleHttpServer server) { }
    }
}
