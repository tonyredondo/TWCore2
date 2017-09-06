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
    /// Handles all the FileObjects events from all instances 
    /// </summary>
    public static class FileObjectEvents
    {
        /// <summary>
        /// Event occurs when a file object instance chamges
        /// </summary>
        public static event EventHandler<FileObjectEventArgs<object>> OnFileObjectChanged;
        /// <summary>
        /// Event occurs when a file object throws an exception
        /// </summary>
        public static event EventHandler<EventArgs<Exception>> OnException;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void FireFileObjectChanged(object sender, string filePath, Guid oldId, object oldValue, Guid newId, object newValue)
        {
            OnFileObjectChanged?.Invoke(sender, new FileObjectEventArgs<object>(filePath, oldId, oldValue, newId, newValue));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void FireException(object sender, Exception ex)
        {
            OnException?.Invoke(sender, new EventArgs<Exception>(ex));
        }
    }
}
