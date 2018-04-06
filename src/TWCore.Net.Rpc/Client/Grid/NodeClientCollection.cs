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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Threading;

namespace TWCore.Net.RPC.Client.Grid
{
    /// <inheritdoc />
    /// <summary>
    /// Grid node client collection
    /// </summary>
    public class NodeClientCollection : List<NodeClient>
    {
        /// <summary>
        /// Waits for a node to be available and gets it to process data.
        /// </summary>
        /// <param name="timeoutInMilliseconds">Time in milleseconds to wait for a node to be available</param>
        /// <returns>Grid node client to process data</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<NodeClient> WaitForAvailableAsync(int timeoutInMilliseconds = -1)
        {
            NodeClient item = null;
            var waitHandles = this.Select(i => i.AvailableEvent.WaitHandle).ToArray();
            var idx = WaitHandle.WaitAny(waitHandles, timeoutInMilliseconds);
            if (idx != WaitHandle.WaitTimeout)
            {
                item = this[idx];
                if (item.Available)
                    return item;
            }
            await TaskHelper.SleepUntil(() =>
            {
                item = this.FirstOrDefault(i => i.Available);
                return (item != null);
            }, timeoutInMilliseconds).ConfigureAwait(false);
            return item;
        }
    }
}
