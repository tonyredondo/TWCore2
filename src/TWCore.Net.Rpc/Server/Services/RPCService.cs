#pragma warning disable IDE1006 // Estilos de nombres
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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Net.RPC.Server;
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable UnusedParameter.Global

namespace TWCore.Services
{
    /// <inheritdoc />
    /// <summary>
    /// RPC Service base
    /// </summary>
    public abstract class RPCService : IRPCService
    {
        private CancellationTokenSource _cts;

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// RPC Server to start
        /// </summary>
        public RPCServer Server { get; private set; }
        /// <summary>
        /// Service cancellation token
        /// </summary>
        public CancellationToken ServiceToken { get; private set; }
        /// <inheritdoc />
        /// <summary>
        /// Get if the service support pause and continue
        /// </summary>
        public bool CanPauseAndContinue => true;
        #endregion

        #region .ctor
        /// <summary>
        /// RPC Service
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected RPCService()
        {
        }
        #endregion

        #region Public Methods
        /// <inheritdoc />
        /// <summary>
        /// On Continue from pause method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async void OnContinue()
        {
            try
            {
                Core.Log.InfoBasic("Continuing RPC service...");
                _cts = new CancellationTokenSource();
                ServiceToken = _cts.Token;
                await Server.StartAsync().ConfigureAwait(false);
                Core.Log.InfoBasic("RPC service has started.");
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// On Pause method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async void OnPause()
        {
            try
            {
                Core.Log.InfoBasic("Pausing RPC service...");
                _cts?.Cancel();
                await Server.StopAsync().ConfigureAwait(false);
                Core.Log.InfoBasic("RPC service has been paused.");
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// On shutdown requested method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnShutdown()
        {
            OnStop();
        }
        /// <inheritdoc />
        /// <summary>
        /// On Service Start method
        /// </summary>
        /// <param name="args">Start arguments</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async void OnStart(string[] args)
        {
            try
            {
                Core.Log.InfoBasic("Starting RPC service...");
                _cts = new CancellationTokenSource();
                ServiceToken = _cts.Token;
                OnInit(args);
                Server = await GetRPCServerAsync().ConfigureAwait(false);
                if (Server is null)
                    throw new NullReferenceException("The RPCServer can't be null, nothing to start. Check your GetRPCServer method implementation.");
                Core.Status.AttachChild(Server, this);
                await Server.StartAsync().ConfigureAwait(false);
                Core.Log.InfoBasic("RPC service has started.");
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                throw;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// On Service Stops method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async void OnStop()
        {
            try
            {
                _cts?.Cancel();
                OnFinalizing();
                Core.Log.InfoBasic("Stopping RPC service...");
                await Server.StopAsync().ConfigureAwait(false);
                Core.Log.InfoBasic("RPC service has stopped.");
                OnDispose();
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// On Service Init
        /// </summary>
        /// <param name="args">Service arguments</param>
        protected virtual void OnInit(string[] args) { }
        /// <summary>
        /// On Service Stop
        /// </summary>
        protected virtual void OnFinalizing() { }
        /// <summary>
        /// On Service Dispose
        /// </summary>
        protected virtual void OnDispose() { }
        /// <summary>
        /// Gets the RPCServer 
        /// </summary>
        /// <returns>RPCServer instance</returns>
        protected abstract Task<RPCServer> GetRPCServerAsync();
        #endregion
    }
}
