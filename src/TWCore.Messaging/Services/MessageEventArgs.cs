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
using TWCore.Messaging;

// ReSharper disable CheckNamespace

namespace TWCore.Services
{
    /// <inheritdoc />
    /// <summary>
    /// Message event args
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// Message object instance
        /// </summary>
        public IMessage Message { get; private set; }
        /// <inheritdoc />
        /// <summary>
        /// Message event args
        /// </summary>
        /// <param name="message">Message object</param>
        public MessageEventArgs(IMessage message) => Message = message;
    }
}
