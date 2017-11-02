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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TWCore.Messaging.Client;
using TWCore.Serialization;
using TWCore.Settings;
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable CheckNamespace

namespace TWCore.Diagnostics.Status.Transports
{
    /// <inheritdoc />
    /// <summary>
    /// Messaging Status Transport
    /// </summary>
    public class MessagingStatusTransport : IStatusTransport
    {
        private IMQueueClient _queueClient;
        
        #region Events
        /// <inheritdoc />
        /// <summary>
        /// Handles when a fetch status event has been received
        /// </summary>
        public event FetchStatusDelegate OnFetchStatus;
        #endregion
        
        /// <inheritdoc />
        /// <summary>
        /// Dispose the current object resources
        /// </summary>
        public void Dispose()
        {
        }
    }
}