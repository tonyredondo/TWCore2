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

using TWCore.Messaging.Server;
using TWCore.Services.Messaging;

namespace TWCore.Services
{
    /// <summary>
    /// Defines a Messaging service async version
    /// </summary>
    public interface IMessagingServiceAsync : IService
    {
        /// <summary>
        /// Messaging queue server
        /// </summary>
        IMQueueServer QueueServer { get; }
        /// <summary>
        /// Message processor
        /// </summary>
        IMessageProcessorAsync Processor { get; }
    }
}
