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

using System.Threading.Tasks;
using TWCore.Net.RPC.Descriptors;
using TWCore.Net.RPC.Server.Transports;

// ReSharper disable InconsistentNaming

namespace TWCore.Net.RPC.Server
{
    /// <summary>
    /// Define a RPC Server
    /// </summary>
    public interface IRPCServer
    {
        /// <summary>
        /// Service descriptor collection
        /// </summary>
        ServiceDescriptorCollection Descriptors { get; }
        /// <summary>
        /// Server transport
        /// </summary>
        ITransportServer Transport { get; set; }
        /// <summary>
        /// true if the RPC server is running; otherwise, false.
        /// </summary>
        bool Running { get; }
        /// <summary>
        /// Starts the RPC server listener
        /// </summary>
        /// <returns>Task with the result of the start</returns>
        Task StartAsync();
        /// <summary>
        /// Stops the RPC server listener
        /// </summary>
        /// <returns>Task with the result of the stop</returns>
        Task StopAsync();
    }
}
