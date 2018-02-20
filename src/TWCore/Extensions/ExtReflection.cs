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

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
// ReSharper disable CheckNamespace

namespace TWCore
{
    /// <summary>
    /// Reflection Extensions
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Gets if a type is Assignable
        /// </summary>
        /// <param name="item">Current Type</param>
        /// <param name="from">Type from</param>
        /// <returns>True if is assignable, otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAssignableFrom(this Type item, Type from)
        {
            if (item != null && from != null)
                return item.GetTypeInfo().IsAssignableFrom(from.GetTypeInfo());
            return false;
        }
    }
}
