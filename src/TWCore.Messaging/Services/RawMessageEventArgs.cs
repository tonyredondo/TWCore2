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

using System;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace

namespace TWCore.Services
{
    /// <inheritdoc />
    /// <summary>
    /// Message event args
    /// </summary>
    public class RawMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Message correlation Id
        /// </summary>
        public Guid CorrelationId { get; }
        /// <summary>
        /// Message object instance
        /// </summary>
        public MultiArray<byte> Message { get; }
        /// <inheritdoc />
        /// <summary>
        /// Message event args
        /// </summary>
        /// <param name="message">Message object</param>
        /// <param name="correlationId">Correlation Id</param>
        public RawMessageEventArgs(MultiArray<byte> message, Guid correlationId)
        {
            CorrelationId = correlationId;
            Message = message;
        }
    }
}
