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
using System.Threading;
using TWCore.Net.RPC.Grid;

namespace TWCore.Net.RPC.Client.Grid
{
    /// <summary>
    /// Grid node client
    /// </summary>
    public class NodeClient
    {
        NodeProxy nodeProxy;
        internal ManualResetEventSlim availableEvent = new ManualResetEventSlim(true);

        #region Properties
        /// <summary>
        /// Gets a value indicating if this GridNode is available for processing.
        /// </summary>
        /// <value>true if is available; otherwise, false.</value>
        public bool Available => availableEvent.IsSet && nodeProxy.GetIsReady();
        /// <summary>
        /// Gets the node information
        /// </summary>
        /// <value>GridNodeInfo instance with the information of the node.</value>
        public NodeInfo NodeInfo => nodeProxy.GetNodeInfo();
        #endregion

        #region .ctor
        /// <summary>
        /// Grid node client
        /// </summary>
        /// <param name="node">Node proxy</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NodeClient(NodeProxy node)
        {
            nodeProxy = node;
            Core.Status.AttachChild(nodeProxy, this);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Lock this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Lock()
        {
            availableEvent.Reset();
        }
        /// <summary>
        /// Unlock this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnLock()
        {
            availableEvent.Set();
        }
        /// <summary>
        /// Process the arguments in the node
        /// </summary>
        /// <param name="args">Argmuents to be processed</param>
        /// <returns>Grid node client result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NodeClientResult Process(params object[] args)
        {
            availableEvent.Reset();
            object response = nodeProxy.Process(args);
            availableEvent.Set();
            return new NodeClientResult(NodeInfo, response);
        }
        #endregion
    }
}
