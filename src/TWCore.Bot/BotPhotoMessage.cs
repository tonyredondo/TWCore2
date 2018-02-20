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

using System;
using System.IO;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace TWCore.Bot
{
    /// <summary>
    /// Bot photo message definition
    /// </summary>
    public class BotPhotoMessage
    {
        /// <summary>
        /// Message Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Photo Id
        /// </summary>
        public string PhotoId { get; set; }
        /// <summary>
        /// Photo Size
        /// </summary>
        public int PhotoSize { get; set; }
        /// <summary>
        /// Photo Name
        /// </summary>
        public string PhotoName { get; set; }
        /// <summary>
        /// Photo width
        /// </summary>
        public int PhotoWidth { get; set; }
        /// <summary>
        /// Photo Height
        /// </summary>
        public int PhotoHeight { get; set; }
        /// <summary>
        /// Photo Stream
        /// </summary>
        public Lazy<Stream> PhotoStream { get; set; }
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
