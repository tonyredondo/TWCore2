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

using System;

namespace TWCore.Bot
{
    /// <summary>
    /// Bot text message definition
    /// </summary>
    public class BotTextMessage
    {
        /// <summary>
        /// Message id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Message content
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Message Date
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Chat where the message was received
        /// </summary>
        public BotChat Chat { get; set; }
        /// <summary>
        /// User who sent the message
        /// </summary>
        public BotUser User { get; set; }
    }
}
