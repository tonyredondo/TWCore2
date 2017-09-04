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
using TWCore.Net.RPC.Grid;

namespace TWCore.Net.RPC.Client.Grid
{
    /// <summary>
    /// Grid node proxy object
    /// </summary>
    public class NodeProxy : RPCProxy, IGridNode
    {
        NodeInfo _info;

        /// <summary>
        /// Node information
        /// </summary>
        /// <returns>GridNodeInfo instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NodeInfo GetNodeInfo()
        {
            if (_info == null)
				_info = Invoke<NodeInfo>();
            return _info;
        }
        /// <summary>
        /// Gets if node is available to process.
        /// </summary>
        /// <returns>true if the node is ready; otherwise, false.</returns>
		public bool GetIsReady() => Invoke<bool>();
        /// <summary>
        /// Node Init Method
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <returns>Output object</returns>
		public object Init(params object[] args) => Invoke(args);
        /// <summary>
        /// Start the process execution
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <returns>Response object</returns>
		public object Process(params object[] args) => Invoke(args);
    }
}
