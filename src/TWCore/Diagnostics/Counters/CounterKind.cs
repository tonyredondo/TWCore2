﻿/*
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


// ReSharper disable InconsistentNaming
namespace TWCore.Diagnostics.Counters
{
    /// <summary>
    /// Counter Kind
    /// </summary>
    public enum CounterKind
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,
        /// <summary>
        /// Application
        /// </summary>
        Application,
        /// <summary>
        /// Data access
        /// </summary>
        DataAccess,
        /// <summary>
        /// Messaging
        /// </summary>
        Messaging,
        /// <summary>
        /// Cache
        /// </summary>
        Cache,
        /// <summary>
        /// Remote procedure call
        /// </summary>
        RPC,
        /// <summary>
        /// Bot
        /// </summary>
        Bot
    }
}