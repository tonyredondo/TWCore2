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

namespace TWCore.IO
{
    /// <summary>
    /// FileObject event args
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    public class FileObjectEventArgs<T> : EventArgs
    {
        /// <summary>
        /// File path for the object
        /// </summary>
        public string FilePath { get; private set; }
        /// <summary>
        /// Old identifier
        /// </summary>
        public Guid OldId { get; private set; }
        /// <summary>
        /// Old object value
        /// </summary>
        public T OldValue { get; private set; }
        /// <summary>
        /// New identifier
        /// </summary>
        public Guid NewId { get; private set; }
        /// <summary>
        /// New object value
        /// </summary>
        public T NewValue { get; private set; }

        /// <summary>
        /// FileObject event args
        /// </summary>
        /// <param name="filePath">File path for the object</param>
        /// <param name="oldId">Old identifier</param>
        /// <param name="oldValue">Old object value</param>
        /// <param name="newId">New identifier</param>
        /// <param name="newValue">New object value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FileObjectEventArgs(string filePath, Guid oldId, T oldValue, Guid newId, T newValue)
        {
            FilePath = filePath;
            OldId = oldId;
            OldValue = oldValue;
            NewId = newId;
            NewValue = newValue;
        }
    }
}
