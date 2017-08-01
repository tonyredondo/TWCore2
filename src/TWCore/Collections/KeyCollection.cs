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
using System.Runtime.CompilerServices;

namespace TWCore.Collections
{
    /// <summary>
    /// Interface for KeyCollection items
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IKeyItem<TKey>
    {
        /// <summary>
        /// Key of the item
        /// </summary>
        TKey Key { get; set; }
    }
    /// <summary>
    /// Collection of IKeyItems
    /// </summary>
	[Serializable]
    public class KeyCollection<TKey, TItem> : KeyDelegatedCollection<TKey, TItem> where TItem : IKeyItem<TKey>
    {
        /// <summary>
        /// Collection of IKeyItems
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyCollection() : base(item => item.Key) { }
    }
    /// <summary>
    /// Collection of IKeyItem with strings
    /// </summary>
	[Serializable]
    public class KeyCollection<TItem> : KeyStringDelegatedCollection<TItem> where TItem : IKeyItem<string>
    {
        /// <summary>
        /// Collection of IKeyItem with strings
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyCollection() : base(item => item.Key) { }
    }
}
