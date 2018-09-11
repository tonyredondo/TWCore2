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
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Cache
{
    /// <inheritdoc />
    /// <summary>
    /// Event args of the event when an Item has been removed from the storage
    /// </summary>
    public class ItemRemovedEventArgs : EventArgs
    {
        /// <summary>
        /// Storage item key
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// Storage item tags
        /// </summary>
        public List<string> Tags { get; }

        /// <inheritdoc />
        /// <summary>
        /// Event args of the event when an Item has been removed from the storage
        /// </summary>
        /// <param name="key">Key of the removed item</param>
        /// <param name="tags">Tags of the removed item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemRemovedEventArgs(string key, List<string> tags)
        {
            Key = key;
            Tags = tags;
        }
    }
}
