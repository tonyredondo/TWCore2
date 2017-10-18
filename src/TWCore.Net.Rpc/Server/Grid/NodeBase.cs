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
using System.Runtime.CompilerServices;
using TWCore.Net.RPC.Grid;
using TWCore.Net.RPC.Server.Transports;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TWCore.Net.RPC.Server.Grid
{
    /// <inheritdoc cref="IGridNode" />
    /// <summary>
    /// Grid Node base class
    /// </summary>
    public abstract class NodeBase : IGridNode, IDisposable
    {
        private readonly NodeInfo _nodeInfo = new NodeInfo { Id = Guid.NewGuid() };

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Node information
        /// </summary>
        /// <returns>GridNodeInfo instance</returns>
        public NodeInfo GetNodeInfo() => _nodeInfo;
        /// <inheritdoc />
        /// <summary>
        /// Gets if node is available to process.
        /// </summary>
        /// <returns>true if the node is ready; otherwise, false.</returns>
        public bool GetIsReady() => IsReady;
        /// <summary>
        /// Node Type
        /// </summary>
        public string Type 
        { 
            get => _nodeInfo.Type;
            set => _nodeInfo.Type = value;
        }
        /// <summary>
        /// Node Service Name
        /// </summary>
        public string ServiceName 
        { 
            get => _nodeInfo.ServiceName;
            set => _nodeInfo.ServiceName = value;
        }
        #endregion

        #region .ctor
        /// <summary>
        /// Grid Node base class
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected NodeBase()
        {
            Type = GetType().GetTypeName();
            ServiceName = GetType().Name;

            Core.Status.Attach(collection =>
            {
                collection.Add("NodeInfo", _nodeInfo);
                collection.Add("IsReady", IsReady);
                collection.Add("Type", Type);
                collection.Add("ServiceName", ServiceName);
            });
        }
        #endregion

        #region Public Methods
        /// <inheritdoc />
        /// <summary>
        /// Node Init Method
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <returns>Output object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Init(params object[] args)
        {
            Core.Log.LibDebug("Init call received.");
            return OnInit(args);
        }
        /// <inheritdoc />
        /// <summary>
        /// Start the process execution
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <returns>Response object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Process(params object[] args)
        {
            Core.Log.LibDebug("Process call received.");
            return OnProcess(args);
        }
        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Core.Log.LibDebug("Disposing the node.");
            OnDispose();
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// On Init call of the grid node
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <returns>Output object</returns>
        protected abstract object OnInit(params object[] args);
        /// <summary>
        /// On process execution call
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <returns>Response object</returns>
        protected abstract object OnProcess(params object[] args);
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected virtual void OnDispose() { }
        #endregion

        #region RPC Server Methods
        /// <summary>
        /// Gets if node is available to process.
        /// </summary>
        public bool IsReady { get; protected set; } = false;
        /// <summary>
        /// Gets the IRPCServer for this instance
        /// </summary>
        /// <param name="transport">Transport server to use with the rpc server.</param>
        /// <returns>IRPCServer instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IRPCServer GetRPCServer(ITransportServer transport)
        {
            var rpcServer = new RPCServer(transport);
            rpcServer.AddService(typeof(IGridNode), this);
            return rpcServer;
        }
        #endregion
    }
}
