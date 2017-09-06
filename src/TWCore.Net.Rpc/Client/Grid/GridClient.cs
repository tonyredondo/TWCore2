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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
// ReSharper disable UnassignedField.Global

namespace TWCore.Net.RPC.Client.Grid
{
    /// <summary>
    /// Grid client
    /// </summary>
    public class GridClient
    {
        private readonly object _waitLock = new object();

        #region Properties
        /// <summary>
        /// Gets the Node items collection
        /// </summary>
        public NodeClientCollection Items { get; } = new NodeClientCollection();
        #endregion

        #region Events
        /// <summary>
        /// Event triggered when a node response is received.
        /// </summary>
        public EventHandler<EventArgs<NodeClientResult>> OnNodeResults;
        #endregion

        #region .ctor
        /// <summary>
        /// Grid client
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GridClient()
        {
            Core.Status.Attach(collection =>
            {
                collection.Add("Items Count", Items.Count);
                foreach (var item in Items)
                    Core.Status.AttachChild(item, this);
            });
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds a node from a transport
        /// </summary>
        /// <param name="transport">Transport to connect to the node</param>
        /// <param name="args">Arguments to send to the node Init method</param>
        /// <returns>Node response from on Init call</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<object> AddNodeAsync(ITransportClient transport, params object[] args)
        {
            Core.Log.LibVerbose("Adding Node and initializing");
            var client = new RPCClient(transport);
            var node = await client.CreateProxyAsync<NodeProxy>().ConfigureAwait(false);
            var response = node.Init(args);
            Items.Add(new NodeClient(node));
            Core.Log.LibVerbose("Node was initializated and added to the collection.");
            return response;
        }
        /// <summary>
        /// Process the specified args using an available node.
        /// </summary>
        /// <param name="args">Arguments to be processed by the node</param>
        /// <returns>Process results</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NodeClientResult Process(params object[] args)
        {
            if (Items.Count <= 0) return null;
            NodeClient item;
            Core.Log.LibDebug("Selecting an available node...");
            lock (_waitLock)
            {
                item = Items.WaitForAvailable();
                item.Lock();
            }
            Core.Log.LibDebug("Calling process on Node '{0}'", item.NodeInfo.Id);
            var response = item.Process(args);
            Core.Log.LibDebug("Received response from Node '{0}'", item.NodeInfo.Id);
            OnNodeResults?.Invoke(this, new EventArgs<NodeClientResult>(response));
            return response;
        }
        /// <summary>
        /// Processes the batch on the available nodes of the grid.
        /// Each element on the enumerable will be processed on a node of the grid.
        /// </summary>
        /// <param name="argsCollection">The arguments batch collection.</param>
        /// <returns>The IEnumerable results from the nodes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<NodeClientResult> ProcessBatch(IEnumerable<object[]> argsCollection)
        {
            Ensure.ArgumentNotNull(argsCollection);
            var collection = argsCollection as object[][] ?? argsCollection.ToArray();
            Core.Log.Debug("Processing batch of {0} elements", collection.Length);
            var cbag = new ConcurrentBag<NodeClientResult>();
            Parallel.ForEach(collection, new ParallelOptions { MaxDegreeOfParallelism = collection.Length }, args => cbag.Add(Process(args)));
            return cbag;
        }
        #endregion
    }
}
