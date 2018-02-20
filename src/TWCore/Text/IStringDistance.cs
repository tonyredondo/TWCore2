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

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TWCore.Text
{
    /// <summary>
    /// Define method for calculate distance (string metric) between two strings
    /// </summary>
    public interface IStringDistance
    {
        /// <summary>
        /// Calculate the Difference-Distance between to String
        /// </summary>
        /// <param name="a"> First string to compare</param>
        /// <param name="b"> Second string to compare</param>
        /// <param name="comparer"> IEqualityComparer to use, to support Case Sensitive or not, if null it uses Case Sensitive</param>
        /// <returns>Percent of equality of the two strings</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int CalculateDistance(string a, string b, IEqualityComparer<char> comparer);
    }
}
