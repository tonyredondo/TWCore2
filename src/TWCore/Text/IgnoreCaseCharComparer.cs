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

namespace TWCore.Text
{
    /// <summary>
    /// IgnoreCase Comparer, Ignore Cases at char comparisson
    /// </summary>
    public class IgnoreCaseCharComparer : IEqualityComparer<char>
    {
        /// <summary>
        /// Compare two chars and return if equal or not
        /// </summary>
        /// <param name="x"> First char to compare</param>
        /// <param name="y"> Second char to compare</param>
        /// <returns>true if the char are equals; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(char x, char y) 
            => char.ToLowerInvariant(x) == char.ToLowerInvariant(y);
        /// <summary>
        /// Get HahsCode from char
        /// </summary>
        /// <param name="obj"> Char to get Hash Code</param>
        /// <returns>Hash code value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(char obj) => obj.GetHashCode();
    }
}
