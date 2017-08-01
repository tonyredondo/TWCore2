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

namespace TWCore.Numerics
{
    /// <summary>
    /// This class implements a group of 2 items.
    /// </summary>
    /// <typeparam name="TItem0">The type of the first item</typeparam>
    /// <typeparam name="TItem1">The type of the second item</typeparam>
    public class ComparableTuple2<TItem0, TItem1> : IComparable<ComparableTuple2<TItem0, TItem1>>
        where TItem0 : IComparable<TItem0>
        where TItem1 : IComparable<TItem1>
    {
        /// <summary>
        /// The first item
        /// </summary>
        public TItem0 Item0
        {
            get;
            private set;
        }

        /// <summary>
        /// The second item.
        /// </summary>
        public TItem1 Item1
        {
            get;
            private set;
        }

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public ComparableTuple2()
        {
        }

        /// <summary>
        /// Constructs a new instance with the same item values as this instance.
        /// </summary>
        /// <param name="group">Base group</param>
        public ComparableTuple2(ComparableTuple2<TItem0, TItem1> group)
        {
            Item0 = group.Item0;
            Item1 = group.Item1;
        }

        /// <summary>
        /// Constructs a new instance with the specified item values.
        /// </summary>
        /// <param name="item0">The first item</param>
        /// <param name="item1">The second item</param>
        public ComparableTuple2(TItem0 item0, TItem1 item1)
        {
            Item0 = item0;
            Item1 = item1;
        }

        #endregion

        #region IComparable<ComparableTuple2> implementation
        /// <summary>
        /// This methods implements the IComparable ComparableTuple2 TItem0, TItem1 interface.
        /// </summary>
        /// <param name="group">The group being compared to this group</param>
        /// <returns>
        /// The value -1 if this groups is less than the passed group.
        /// The value 1 if this group is greater than the passed group.
        /// The value 0 if this group and the passed groups are equal.
        /// </returns>
        public int CompareTo(ComparableTuple2<TItem0, TItem1> group)
        {
            int result = this.Item0.CompareTo(group.Item0);

            if (result == 0)
            {
                result = this.Item1.CompareTo(group.Item1);
            }

            return result;
        }

        #endregion
    }
}
