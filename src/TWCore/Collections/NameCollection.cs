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

using System.Runtime.CompilerServices;

namespace TWCore.Collections
{
    /// <summary>
    /// Interface for NameCollection items
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface INameItem
    {
        /// <summary>
        /// Key of the item
        /// </summary>
        string Name { get; set; }
    }
    /// <summary>
    /// Collection of INameItems
    /// </summary>
    public class NameCollection<TItem> : KeyStringDelegatedCollection<TItem> where TItem : INameItem
    {
        /// <summary>
        /// Collection of INameItems
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NameCollection() : base(item => item.Name) { }
        /// <summary>
        /// Collection of INameItems
        /// </summary>
        /// <param name="throwExceptionOnDuplicateKeys">Sets the behavior when adding an item, throwing an exception if the key is duplicated, or ignoring the item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NameCollection(bool throwExceptionOnDuplicateKeys) : base(item => item.Name, throwExceptionOnDuplicateKeys) { }
    }
}
