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
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore
{
    /// <inheritdoc />
    /// <summary>
    /// Exception event args object
    /// </summary>
    public class ExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// Exception object
        /// </summary>
        public Exception Exception { get; }
        /// <summary>
        /// Complementary Data
        /// </summary>
        public object Data { get; }

        #region Constructor
        /// <inheritdoc />
        /// <summary>
        /// Exception event args
        /// </summary>
        /// <param name="exception">Exception object</param>
        /// <param name="data">Complementary data object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ExceptionEventArgs(Exception exception, object data = null)
        {
            Exception = exception;
            Data = data;
        }
        #endregion
    }
}
