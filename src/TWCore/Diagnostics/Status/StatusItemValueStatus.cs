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
        /// Ok status, is a value inside the expected parameters
        /// </summary>
        Ok,
        /// <summary>
        /// Warning status, is a value outside the expected parameters but not critical, but should be a warning
        /// </summary>
        Warning,
        /// <summary>
        /// Error status, is a value considered critical
        /// </summary>
        Error
    }
}
