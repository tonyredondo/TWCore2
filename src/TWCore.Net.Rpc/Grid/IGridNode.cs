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

namespace TWCore.Net.RPC.Grid
{
    /// <summary>
    /// Grid node definition
    /// </summary>
    public interface IGridNode
    {
        /// <summary>
        /// Node information
        /// </summary>
        /// <returns>GridNodeInfo instance</returns>
        NodeInfo GetNodeInfo();
        /// <summary>
        /// Gets if node is available to process.
        /// </summary>
        /// <returns>true if the node is ready; otherwise, false.</returns>
        bool GetIsReady();
        /// <summary>
        /// Node Init Method
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <returns>Output object</returns>
        object Init(params object[] args);
        /// <summary>
        /// Start the process execution
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <returns>Response object</returns>
        object Process(params object[] args);
    }
}
