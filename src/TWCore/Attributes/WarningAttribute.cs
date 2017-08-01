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
using System.Runtime.CompilerServices;

namespace TWCore
{
    /// <summary>
    /// Indicates that a class or method is incomplete and should not be used
    /// </summary>
    [Obsolete("Warning code is used, Please check the code for the reason.")]
    [AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Interface | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class, Inherited = false)]
    public sealed class WarningAttribute : GenericStringAttribute
    {
        /// <summary>
        /// Indicates that a class or method is incomplete and should not be used
        /// </summary>
        /// <param name="value">Comments of the attribute</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WarningAttribute(string value = null) : base(value) { }
    }
}
