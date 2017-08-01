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
using System.Collections.Generic;

namespace TWCore.Numerics
{
    /// <summary>
    /// This class implements the IEqualityComparer ComparableTuple2 TItem0, TItem1 interface
    /// to allow using ComparableTuple2 ComparableTuple2 TItem0, TItem1  class instances as keys in a dictionary.
    /// </summary>
    /// <typeparam name="TItem0">The type of the first item</typeparam>
    /// <typeparam name="TItem1">The type of the second item</typeparam>
    public class ComparableTuple2EqualityComparer<TItem0, TItem1> : IEqualityComparer<ComparableTuple2<TItem0, TItem1>>
        where TItem0 : IComparable<TItem0>
        where TItem1 : IComparable<TItem1>
    {
        /// <summary>
        /// Compares the items in this group for equality.
        /// </summary>
        /// <param name="groupA">Group to compare</param>
        /// <param name="groupB">Group to compare</param>
        /// <returns>true if are equal groups; otherwise, false.</returns>
        public bool Equals(ComparableTuple2<TItem0, TItem1> groupA,
                           ComparableTuple2<TItem0, TItem1> groupB)
        {
            return ((groupA.Item0.Equals(groupB.Item0))
                && (groupA.Item1.Equals(groupB.Item1)));
        }

        /// <summary>
        /// Returns a hash code for an object.
        /// </summary>
        /// <param name="group">An object of type ComparableTuple2</param>
        /// <returns>A hash code for the object.</returns>
        public int GetHashCode(ComparableTuple2<TItem0, TItem1> group)
        {
            int hash0 = group.Item0.GetHashCode();
            int hash1 = group.Item1.GetHashCode();
            int hash = 577 * hash0 + 599 * hash1;
            return hash.GetHashCode();
        }
    }
}
