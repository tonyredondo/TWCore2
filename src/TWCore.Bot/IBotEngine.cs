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
using System.Threading.Tasks;
using TWCore.Collections;
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace TWCore.Bot
{
    /// <summary>
    /// Bot Engine interface
    /// </summary>
    public interface IBotEngine
    {
        /// <summary>
        /// Current chats
        /// </summary>
        KeyStringDelegatedCollection<BotChat> Chats { get; }
        /// <summary>
        /// Commands collection
        /// </summary>
        BotCommandCollection Commands { get; }
        /// <summary>
        /// Get true if the transport is connected; otherwise, false.
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// Tracked chats
        /// </summary>
        KeyStringDelegatedCollection<BotChat> TrackedChats { get; }
        /// <summary>
        /// Bot transport
        /// </summary>
        IBotTransport Transport { get; }
        /// <summary>
        /// Current users
        /// </summary>
        KeyStringDelegatedCollection<BotUser> Users { get; }

        /// <summary>
        /// Event when the tracked chats has been changed
        /// </summary>
        event EventHandler OnTrackedChatsChanged;
        /// <summary>
        /// Event when a photo message has been received
        /// </summary>
        event EventHandler<EventArgs<BotPhotoMessage>> OnPhotoMessageReceived;

        /// <summary>
        /// Get if a chat is being tracked
        /// </summary>
        /// <param name="chat">Chat to check</param>
        /// <returns>true if the chat is being tracked; otherwise false.</returns>
        bool IsChatTracked(BotChat chat);
        /// <summary>
        /// Sends a text message
        /// </summary>
        /// <param name="chat">Chat where the message will be sent</param>
        /// <param name="message">Message to send</param>
		/// <param name="parseMode">Message ParseMode</param>
        /// <returns>Async task</returns>
		Task SendTextMessageAsync(BotChat chat, string message, MessageParseMode parseMode = MessageParseMode.Default);
		/// <summary>
		/// Sends a text message to all tracked chats
		/// </summary>
		/// <param name="message">Message to send</param>
		/// <param name="parseMode">Message ParseMode</param>
		/// <returns>Async task</returns>
		Task SendTextMessageToTrackedChatsAsync(string message, MessageParseMode parseMode = MessageParseMode.Default);
		/// <summary>
		/// Send the typing action to the chat
		/// </summary>
		/// <param name="chat">Chat where the typing action will be sent</param>
		/// <returns>Async task</returns>
		Task SendTypingAsync(BotChat chat);
		/// <summary>
		/// Send the typing action to all tracked chats
		/// </summary>
		/// <returns>Async task</returns>
		Task SendTypingToTrackedChatsAsync();
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
        /// Sends a photo message to all tracked chats
        /// </summary>
        /// <param name="fileName">Photo Filename</param>
        /// <param name="caption">Photo Caption</param>
        /// <returns>Async task</returns>
        Task SendPhotoMessageToTrackedChatsAsync(string fileName, string caption = null);
        /// <summary>
        /// Sends a photo message to all tracked chats
        /// </summary>
        /// <param name="fileStream">Photo stream</param>
        /// <param name="caption">Photo Caption</param>
        /// <returns>Async task</returns>
        Task SendPhotoMessageToTrackedChatsAsync(Stream fileStream, string caption = null);
        /// <summary>
        /// Start the bot engine listener.
        /// </summary>
        /// <returns>Async task</returns>
        Task StartListenerAsync();
        /// <summary>
        /// Stop the bot engine listener
        /// </summary>
        /// <returns>Async task</returns>
        Task StopListenerAsync();
        /// <summary>
        /// Track a chat
        /// </summary>
        /// <param name="chat">Chat to be tracked</param>
        /// <returns>true if the chat was added to the tracking; otherwise false.</returns>
        bool TrackChat(BotChat chat);
        /// <summary>
        /// Untrack all chats
        /// </summary>
        void UnTrackAllChats();
        /// <summary>
        /// Untrack a chat
        /// </summary>
        /// <param name="chat">Chat to be untracked</param>
        /// <returns>true if the chat was remove from the tracking; otherwise false.</returns>
        bool UnTrackChat(BotChat chat);
    }
}