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

namespace TWCore.Diagnostics.Status
{
    /// <inheritdoc />
    /// <summary>
    /// Attribute to define the name of the StatusItem of the Status library
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class StatusNameAttribute : Attribute
    {
        /// <summary>
        /// Name to show in the status library
        /// </summary>
        public string Name { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// Attribute to define the name of the StatusItem of the Status library
        /// </summary>
        /// <param name="name">Name to show in the status library</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatusNameAttribute(string name)
        {
            Name = name;
        }
    }
}
