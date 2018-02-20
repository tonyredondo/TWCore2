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

namespace TWCore.Diagnostics.Log
{
    /// <summary>
    /// Log item levels
    /// </summary>
    [Flags]
    public enum LogLevel
    {
        /// <summary>
        /// Error level, the current code execution has been aborted.
        /// </summary>
        Error = 1,
        /// <summary>
        /// Warning level, a known problematic issue has occurred, but the execution wasn't aborted.
        /// </summary>
        Warning = 2,
        /// <summary>
        /// Information Level 1, this item contains general information at business level.
        /// </summary>
        InfoBasic = 4,
        /// <summary>
        /// Information Level 2, this item contains detailed information at business level.
        /// </summary>
        InfoMedium = 8,
        /// <summary>
        /// Information Level 3, this item contains atomic information at business level.
        /// </summary>
        InfoDetail = 16,
        /// <summary>
        /// Debug level, this item contains detailed information who could serve for debugging purpose.
        /// </summary>
        Debug = 32,
        /// <summary>
        /// Verbose level, this is the level with more details, information of all things that are happening.
        /// </summary>
        Verbose = 64,
        /// <summary>
        /// Stats level, this level has stats time of the execution.
        /// </summary>
        Stats = 128,
        /// <summary>
        /// This is the debugging level used by internals libraries, not to be used on final services or applications.
        /// </summary>
        LibDebug = 256,
        /// <summary>
        /// This is the verbose level used by internals libraries, not to be used on final services or applications.
        /// </summary>
        LibVerbose = 512
    }
}
