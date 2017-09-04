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
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace TWCore.Collections
{
    /// <summary>
    /// Represents a range of values. 
    /// Both values must be of the same type and comparable.
    /// </summary>
    /// <typeparam name="T">Type of the values.</typeparam>
    public struct Range<T> : IComparable<Range<T>> where T : IComparable<T>
    {
        /// <summary>
        /// Range From
        /// </summary>
        public T From { get; set; }
        /// <summary>
        /// Range To
        /// </summary>
        public T To { get; set; }

        /// <summary>
        /// Initializes a new <see cref="Range&lt;T&gt;"/> instance.
        /// </summary>
        public Range(T value) : this()
        {
            From = value;
            To = value;
        }

        /// <summary>
        /// Initializes a new <see cref="Range&lt;T&gt;"/> instance.
        /// </summary>
        public Range(T from, T to) : this()
        {
            From = from;
            To = to;
        }

        /// <summary>
        /// Whether the value is contained in the range. 
        /// Border values are considered inside.
        /// </summary>
        public bool Contains(T value) => value.CompareTo(From) >= 0 && value.CompareTo(To) <= 0;

        /// <summary>
        /// Whether the value is contained in the range. 
        /// Border values are considered outside.
        /// </summary>
        public bool ContainsExclusive(T value) => value.CompareTo(From) > 0 && value.CompareTo(To) < 0;

        /// <summary>
        /// Whether two ranges intersect each other.
        /// </summary>
        public bool Intersects(Range<T> other) => other.To.CompareTo(From) >= 0 && other.From.CompareTo(To) <= 0;

        /// <summary>
        /// Whether two ranges intersect each other.
        /// </summary>
        public bool IntersectsExclusive(Range<T> other) => other.To.CompareTo(From) > 0 && other.From.CompareTo(To) < 0;

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString() => string.Format("{0} - {1}", From, To);
        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            var hash = 23;
            hash = hash * 37 + From.GetHashCode();
            hash = hash * 37 + To.GetHashCode();
            return hash;
        }

        #region IComparable<Range<T>> Members

        /// <summary>
        /// Returns -1 if this range's From is less than the other, 1 if greater.
        /// If both are equal, To is compared, 1 if greater, -1 if less.
        /// 0 if both ranges are equal.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public int CompareTo(Range<T> other)
        {
            if (From.CompareTo(other.From) < 0)
                return -1;
            else if (From.CompareTo(other.From) > 0)
                return 1;
            else if (To.CompareTo(other.To) < 0)
                return -1;
            else if (To.CompareTo(other.To) > 0)
                return 1;
            else
                return 0;
        }

        #endregion
    }

    /// <summary>
    /// Static helper class to create Range instances.
    /// </summary>
    public static class Range
    {
        /// <summary>
        /// Creates and returns a new <see cref="Range&lt;T&gt;"/> instance.
        /// </summary>
        public static Range<T> Create<T>(T from, T to) where T : IComparable<T>
            => new Range<T>(from, to);
    }

    /// <summary>
    /// Interface for classes which provide a range.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRangeProvider<T> where T : IComparable<T>
    {
        /// <summary>
        /// Range of values
        /// </summary>
        Range<T> Range { get; }
    }
}
