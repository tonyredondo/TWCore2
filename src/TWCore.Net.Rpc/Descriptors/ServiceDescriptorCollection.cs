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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TWCore.Net.RPC.Descriptors
{
    /// <summary>
    /// Defines a RPC Service
    /// </summary>
    [Serializable, DataContract]
    public class ServiceDescriptorCollection
    {
        /// <summary>
        /// Service descriptors list
        /// </summary>
        [DataMember]
        public Dictionary<string, ServiceDescriptor>  Items { get; set; } = new Dictionary<string, ServiceDescriptor>(StringComparer.Ordinal);

        /// <summary>
        /// Add service descriptor
        /// </summary>
        /// <param name="descriptor">Service Descriptor instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ServiceDescriptor descriptor) => Items[descriptor.Name] = descriptor;
        /// <summary>
        /// Combine with another Descriptor collection
        /// </summary>
        /// <param name="descriptors">Descriptor collection</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Combine(ServiceDescriptorCollection descriptors)
        {
            if (descriptors?.Items == null) return;
            foreach(var item in descriptors.Items)
                Items[item.Key] = item.Value;
        }
    }
}
