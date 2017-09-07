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
using TWCore.Messaging.Configuration;
using TWCore.Serialization;
// ReSharper disable UnusedMemberInSuper.Global

namespace TWCore.Messaging
{
    /// <inheritdoc />
    /// <summary>
    /// Message queue transport definition
    /// </summary>
    public interface IMQueue : IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets or Sets the client name
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Gets or sets the sender serializer
        /// </summary>
        ISerializer SenderSerializer { get; set; }
        /// <summary>
        /// Gets or sets the receiver serializer
        /// </summary>
        ISerializer ReceiverSerializer { get; set; }
        /// <summary>
        /// Gets the current configuration
        /// </summary>
        MQPairConfig Config { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Initialize client with the configuration
        /// </summary>
        /// <param name="config">Message queue client configuration</param>
        void Init(MQPairConfig config);
        #endregion
    }
}
