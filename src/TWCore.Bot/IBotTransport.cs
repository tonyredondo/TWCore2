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
using System.IO;
using System.Threading.Tasks;
// ReSharper disable UnusedMemberInSuper.Global

namespace TWCore.Bot
{
    /// <summary>
    /// Defines a bot transport
    /// </summary>
    public interface IBotTransport
    {
        /// <summary>
        /// Event when a text message has been received
        /// </summary>
        event EventHandler<EventArgs<BotTextMessage>> TextMessageReceived;
        /// <summary>
        /// Event when a photo message has been received
        /// </summary>
        event EventHandler<EventArgs<BotPhotoMessage>> PhotoMessageReceived;
        /// <summary>
        /// Get true if the transport is connected; otherwise, false.
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// Bot token
        /// </summary>
        string Token { get; }
        /// <summary>
        /// Connect to the bot server
        /// </summary>
        /// <returns>Async task</returns>
        Task ConnectAsync();
        /// <summary>
        /// Sends a text message
        /// </summary>
        /// <param name="chat">Chat where the message will be sent</param>
        /// <param name="message">Message to send</param>
        /// <param name="parseMode">Message parse mode</param>
        /// <returns>Async task</returns>
        Task SendTextMessageAsync(BotChat chat, string message, MessageParseMode parseMode = MessageParseMode.Default);
        /// <summary>
        /// Sends a photo message
        /// </summary>
        /// <param name="chat">Chat where the message will be sent</param>
        /// <param name="fileName">Photo Filename</param>
        /// <param name="caption">Photo Caption</param>
        /// <returns>Async task</returns>
        Task SendPhotoMessageAsync(BotChat chat, string fileName, string caption = null);
        /// <summary>
        /// Sends a photo message
        /// </summary>
        /// <param name="chat">Chat where the message will be sent</param>
        /// <param name="fileStream">Photo stream</param>
        /// <param name="caption">Photo Caption</param>
        /// <returns>Async task</returns>
        Task SendPhotoMessageAsync(BotChat chat, Stream fileStream, string caption = null);
        /// <summary>
        /// Send the typing action to the chat
        /// </summary>
        /// <param name="chat">Chat where the typing action will be sent</param>
        /// <returns>Async task</returns>
        Task SendTypingAsync(BotChat chat);
        /// <summary>
        /// Disconnect from the bot server
        /// </summary>
        /// <returns>Async task</returns>
        Task DisconnectAsync();
    }
}
