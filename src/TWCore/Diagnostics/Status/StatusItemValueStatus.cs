﻿/*
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


namespace TWCore.Diagnostics.Status
{
    /// <summary>
    /// Value Status
    /// </summary>
    public enum StatusItemValueStatus
    {
        /// <summary>
        /// Default value, the status is unknown
        /// </summary>
        Unknown,
        /// <summary>
        /// Green status, is a value inside the expected parameters
        /// </summary>
        Green,
        /// <summary>
        /// Yellow status, is a value outside the expected parameters but not critical, but should be a warning
        /// </summary>
        Yellow,
        /// <summary>
        /// Red status, is a value considered critical
        /// </summary>
        Red
    }
}
