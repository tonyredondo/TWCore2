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


namespace TWCore.Messaging
{
    /// <summary>
    /// Default Parameters Keys
    /// </summary>
    public static class ParameterKeys
    {
        /// <summary>
        /// Connection username
        /// </summary>
        public const string Username = "Username";
        /// <summary>
        /// Connection password
        /// </summary>
        public const string Password = "Password";
        /// <summary>
        /// Connection port
        /// </summary>
        public const string Port = "Port";
        /// <summary>
        /// Timeout for each try over a message queue on correlationId
        /// </summary>
        public const string TimeoutForEachTryInMs = "TimeoutForEachTryInMs";
        /// <summary>
        /// Delay time before another message queue read try
        /// </summary>
        public const string DelayForNextTryInMs = "DelayForNextTryInMs";
        /// <summary>
        /// Use message queue lock on read
        /// </summary>
        public const string UseLock = "UseLock";
        /// <summary>
        /// Use a single response queue and not one queue per CorrelationId
        /// </summary>
        public const string SingleResponseQueue = "SingleResponseQueue";
    }
}
