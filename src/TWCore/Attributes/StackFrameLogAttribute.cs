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
using System.Runtime.CompilerServices;
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CheckNamespace

namespace TWCore
{
    /// <inheritdoc />
    /// <summary>
    /// Set Stackframe Log settings (for debugging lib)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Delegate)]
    public sealed class StackFrameLogAttribute : Attribute
    {
        /// <summary>
        /// Name of the class to show in the log.
        /// </summary>
        public string ClassName { get; }

        /// <inheritdoc />
        /// <summary>
        /// Set Stackframe Log settings (for debugging lib)
        /// </summary>
        public StackFrameLogAttribute() { }
        /// <inheritdoc />
        /// <summary>
        /// Set Stackframe Log settings (for debugging lib)
        /// </summary>
        /// <param name="className">Name of the class to show in the log</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackFrameLogAttribute(string className)
        {
            ClassName = className;
        }
    }
}
