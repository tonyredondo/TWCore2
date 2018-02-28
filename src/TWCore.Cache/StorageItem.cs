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
using System.Runtime.Serialization;
using TWCore.Serialization;

namespace TWCore.Cache
{
    /// <inheritdoc />
    /// <summary>
    /// Storage item
    /// </summary>
    [DataContract, Serializable]
    public sealed class StorageItem : IDisposable
    {
        /// <summary>
        /// Gets or sets the item meta data
        /// </summary>
        [DataMember]
        public StorageItemMeta Meta { get; set; }
        /// <summary>
        /// Gets or sets the data in the item
        /// </summary>
        [DataMember]
        public SerializedObject Data { get; set; }
        /// <summary>
        /// Gets the deserialized Data Value
        /// </summary>
        [NonSerialize]
        public object Value => Data?.GetValue();

        #region .ctor
        /// <summary>
        /// Storage item
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem() { }
        /// <summary>
        /// Storage item
        /// </summary>
        /// <param name="meta">Item metadata</param>
        /// <param name="data">Item data</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem(StorageItemMeta meta, SerializedObject data = null)
        {
            Meta = meta;
            Data = data;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~StorageItem()
        {
            Dispose();
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Meta = null;
            Data = null;
        }
        #endregion
    }
}
