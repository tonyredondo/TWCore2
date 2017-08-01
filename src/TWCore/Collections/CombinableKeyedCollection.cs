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
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace TWCore.Collections
{
    /// <summary>
    /// Keyed collection where the items are ICombinables
    /// </summary>
    /// <typeparam name="TKey">Key item type</typeparam>
    /// <typeparam name="TItem">ICombinable Item type</typeparam>
    [DataContract]
    public abstract class CombinableKeyedCollection<TKey, TItem> : KeyedCollection<TKey, TItem>, ICombinable<CombinableKeyedCollection<TKey, TItem>> where TItem : ICombinable<TItem>
    {
        /// <summary>
        /// Gets the combination of the current instance with another item
        /// </summary>
        /// <param name="item">Item to combine with</param>
        /// <returns>Combination between the current instance and the item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual CombinableKeyedCollection<TKey, TItem> Combine(CombinableKeyedCollection<TKey, TItem> item)
        {
            var lst = (CombinableKeyedCollection<TKey, TItem>)Activator.CreateInstance(GetType());
			if (item != null)
            	lst.AddRange(item.Select(i => i.DeepClone()));
            if (this.Any())
            {
                foreach (var innerItem in this)
                {
                    var rN = GetKeyForItem(innerItem);
					if (!lst.Contains(rN))
						lst.Add(innerItem.DeepClone());
                    else
                    {
						TItem oItem = lst[rN];
						lst.Remove(oItem);
                        var rItem = innerItem.Combine(oItem);
                        lst.Add(rItem);
                    }
                };
            }
            return lst;
        }
    }
    /// <summary>
    /// Keyed collection where the items are ICombinables
    /// </summary>
    /// <typeparam name="TItem">ICombinable Item type</typeparam>
    [DataContract]
    public abstract class CombinableKeyedCollection<TItem> : CombinableKeyedCollection<string, TItem> where TItem : ICombinable<TItem> { }
}
