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
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Net.RPC.Client.Grid
{
    /// <summary>
    /// Grid node client process result
    /// </summary>
    public class NodeClientResult
    {
        /// <summary>
        /// Gets the node information who executed the process
        /// </summary>
        public NodeInfo NodeInfo { get; }
        /// <summary>
        /// Process result data
        /// </summary>
        public object Data { get; }
        /// <summary>
        /// Grid node client process result
        /// </summary>
        /// <param name="nodeInfo">Information of the executed node</param>
        /// <param name="data">Process result data</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal NodeClientResult(NodeInfo nodeInfo, object data)
        {
            NodeInfo = nodeInfo;
            Data = data;
        }
    }
}
