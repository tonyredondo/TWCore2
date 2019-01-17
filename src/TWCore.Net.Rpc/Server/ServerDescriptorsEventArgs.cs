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
using TWCore.Net.RPC.Descriptors;

namespace TWCore.Net.RPC.Server
{
    /// <inheritdoc />
    /// <summary>
    /// Server descriptor event args, for the event when the server receive a server descriptor request.
    /// </summary>
    public class ServerDescriptorsEventArgs : EventArgs
    {
        /// <summary>
        /// Service descriptor collection
        /// </summary>
        public ServiceDescriptorCollection Descriptors { get; set; }



        #region Statics
        /// <summary>
        /// Server descriptor event args, for the event when the server receive a server descriptor request.
        /// </summary>
        /// <returns>ServerDescriptorsEventArgs instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ServerDescriptorsEventArgs Retrieve()
        {
            return ReferencePool<ServerDescriptorsEventArgs>.Shared.New();
        }
        /// <summary>
        /// Stores a server descriptor args.
        /// </summary>
        /// <param name="value">Method event args value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Store(ServerDescriptorsEventArgs value)
        {
            if (value is null) return;
            value.Descriptors = null;
            ReferencePool<ServerDescriptorsEventArgs>.Shared.Store(value);
        }
        #endregion
    }
}
