﻿/*
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
using TWCore.Collections;
using TWCore.Serialization;

namespace TWCore.Net.RPC.Client.Transports
{
    /// <summary>
    /// Http Transport client factory
    /// </summary>
    public class HttpTransportClientFactory : TransportClientFactoryBase
    {
        /// <summary>
        /// Create a new Transport from a KeyValueCollection parameters
        /// </summary>
        /// <param name="parameters">Parameters to create the transport</param>
        /// <returns>Transport instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ITransportClient CreateTransport(KeyValueCollection parameters)
        {
            var url = parameters["Url"];
            var serializerMimeType = parameters["SerializerMimeType"];
            var serializer = SerializerManager.GetByMimeType(serializerMimeType);
            return new HttpTransportClient(url, serializer);
        }
    }
}
