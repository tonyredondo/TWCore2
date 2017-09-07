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
using System.Runtime.CompilerServices;

namespace TWCore.Text
{
    /// <summary>
    /// Calculate the Difference-Distance between to String
    /// Using Levenshtein : https://en.wikipedia.org/wiki/Levenshtein_distance
    /// </summary>
    public class LevenshteinStringDistance : IStringDistance
    {
        /// <summary>
        /// Calculate the Difference-Distance between to String
        /// Using Levenshtein : https://en.wikipedia.org/wiki/Levenshtein_distance
        /// </summary>
        /// <param name="a"> First string to compare</param>
        /// <param name="b"> Second string to compare</param>
        /// <param name="comparer"> IEqualityComparer to use.</param>
        /// <returns>Percent of equality of the two strings</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CalculateDistance(string a, string b, IEqualityComparer<char> comparer)
        {
            var costs = new int[b.Length + 1];
            for (var i = 0; i <= a.Length; i++)
            {
                var lastValue = i;
                for (var j = 0; j <= b.Length; j++)
                {
                    if (i == 0)
                        costs[j] = j;
                    else
                    {
                        if (j > 0)
                        {
                            var newValue = costs[j - 1];
                            if (!comparer.Equals(a[i - 1], b[j - 1]))
                                newValue = Math.Min(Math.Min(newValue, lastValue), costs[j]) + 1;
                            costs[j - 1] = lastValue;
                            lastValue = newValue;
                        }
                    }
                }
                if (i > 0)
                    costs[b.Length] = lastValue;
            }
            return costs[b.Length];
        }
    }
}
