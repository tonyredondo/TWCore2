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

using System.Collections.Generic;
using System.Runtime.CompilerServices;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Text
{
    /// <summary>
    /// String comparisson helper
    /// </summary>
    public class StringCompare
    {
        /// <summary>
        /// Default Comparer, Case Sensitive
        /// </summary>
        public static IEqualityComparer<char> DefaultComparer => new DefaultCharComparer();
        /// <summary>
        /// IgnoreCase Comparer, Ignore Cases
        /// </summary>
        public static IEqualityComparer<char> IgnoreCaseComparer => new IgnoreCaseCharComparer();

        /// <summary>
        /// Compare two strings and gives the equality in percent.
        /// Using Damerau-Levenshtein 
        /// </summary>
        /// <param name="a">First string to compare</param>
        /// <param name="b">Second string to compare</param>
        /// <param name="distanceAlgo">String Distance algorithm to use</param>
        /// <param name="comparer"> IEqualityComparer to use, to support Case Sensitive or not, if null it uses Case Sensitive</param>
        /// <returns>Percent of equality of the two strings</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double EqualPercent(string a, string b, IStringDistance distanceAlgo, IEqualityComparer<char> comparer)
        {
            comparer = comparer ?? DefaultComparer;
            string longer = a, shorter = b;
            if (a.Length < b.Length)
            {
                longer = b; shorter = a;
            }
            int longerLength = longer.Length;
            if (longerLength == 0)
                return 1.0;
            return (longerLength - distanceAlgo.CalculateDistance(longer, shorter, comparer)) / (double)longerLength;
        }
    }
}
