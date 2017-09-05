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
using System.Linq;
using System.Runtime.CompilerServices;

namespace TWCore.Text
{
    /// <inheritdoc />
    /// <summary>
    /// Calculate the Difference-Distance between to String
    /// Using Damerau–Levenshtein : https://en.wikipedia.org/wiki/Damerau%E2%80%93Levenshtein_distance
    /// </summary>
    public class DamerauLevenshteinStringDistance : IStringDistance
    {
        /// <inheritdoc />
        /// <summary>
        /// Calculate the Difference-Distance between to String
        /// Using Damerau–Levenshtein : https://en.wikipedia.org/wiki/Damerau%E2%80%93Levenshtein_distance
        /// </summary>
        /// <param name="a"> First string to compare</param>
        /// <param name="b"> Second string to compare</param>
        /// <param name="comparer"> IEqualityComparer to use.</param>
        /// <returns>Percent of equality of the two strings</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CalculateDistance(string a, string b, IEqualityComparer<char> comparer)
        {
            var lenA = a.Length;
            var lenB = b.Length;

            var matrix = new int[lenA + 1, lenB + 1];
            for (var i = 0; i <= lenA; i++)
                matrix[i, 0] = i;
            for (var j = 0; j <= lenB; j++)
                matrix[0, j] = j;

            for (var i = 1; i <= lenA; i++)
            {
                for (var j = 1; j <= lenB; j++)
                {
                    var cost = comparer.Equals(b[j - 1], a[i - 1]) ? 0 : 1;
                    var vals = new[] {
                        matrix[i - 1, j] + 1,
                        matrix[i, j - 1] + 1,
                        matrix[i - 1, j - 1] + cost
                    };
                    matrix[i, j] = vals.Min();
                    if (i > 1 && j > 1 && comparer.Equals(a[i - 1], b[j - 2]) && comparer.Equals(a[i - 2], b[j - 1]))
                        matrix[i, j] = Math.Min(matrix[i, j], matrix[i - 2, j - 2] + cost);
                }
            }
            return matrix[lenA, lenB];
        }
    }
}
