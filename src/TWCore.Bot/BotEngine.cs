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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Collections;

namespace TWCore.Bot
{
    /// <inheritdoc />
    /// <summary>
    /// Default bot engine
    /// </summary>
    public class BotEngine : IBotEngine
    {
        #region Events
        /// <summary>
        /// Event when the tracked chats has been changed
        /// </summary>
        public event EventHandler OnTrackedChatsChanged;
        /// <summary>
        /// Event when a photo message has been received
        /// </summary>
        public event EventHandler<EventArgs<BotPhotoMessage>> OnPhotoMessageReceived;
        #endregion

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Bot transport
        /// </summary>
        public IBotTransport Transport { get; private set; }
        /// <inheritdoc />
        /// <summary>
        /// Current chats
        /// </summary>
        public KeyStringDelegatedCollection<BotChat> Chats { get; } = new KeyStringDelegatedCollection<BotChat>(c => c.Id, false);
        /// <inheritdoc />
        /// <summary>
        /// Current users
        /// </summary>
        public KeyStringDelegatedCollection<BotUser> Users { get; } = new KeyStringDelegatedCollection<BotUser>(c => c.Id, false);
        /// <inheritdoc />
        /// <summary>
        /// Tracked chats
        /// </summary>
        public KeyStringDelegatedCollection<BotChat> TrackedChats { get; } = new KeyStringDelegatedCollection<BotChat>(c => c.Id, false);
        /// <inheritdoc />
        /// <summary>
        /// Commands collection
        /// </summary>
        public BotCommandCollection Commands { get; } = new BotCommandCollection();
        /// <inheritdoc />
        /// <summary>
        /// Get true if the transport is connected; otherwise, false.
        /// </summary>
        public bool IsConnected => Transport?.IsConnected ?? false;
        #endregion

        #region .ctor
        /// <summary>
        /// Default bot engine
        /// </summary>
        /// <param name="transport">Bot transport</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BotEngine(IBotTransport transport)
        {
            Transport = transport;
            Transport.TextMessageReceived += Transport_TextMessageReceived;
            Transport.PhotoMessageReceived += Transport_PhotoMessageReceived;
        }
        #endregion

        #region Public Methods
        /// <inheritdoc />
        /// <summary>
        /// Track a chat
        /// </summary>
        /// <param name="chat">Chat to be tracked</param>
        /// <returns>true if the chat was added to the tracking; otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrackChat(BotChat chat)
        {
			if (chat == null) return false;
            lock (TrackedChats)
            {
				if (TrackedChats.Contains(chat.Id)) return false;
                TrackedChats.Add(chat);
                Core.Log.LibVerbose("Chat Tracked");
                OnTrackedChatsChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Untrack a chat
        /// </summary>
        /// <param name="chat">Chat to be untracked</param>
        /// <returns>true if the chat was remove from the tracking; otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool UnTrackChat(BotChat chat)
        {
			if (chat == null) return false;
            lock (TrackedChats)
            {
				if (!TrackedChats.Contains(chat.Id)) return false;
                TrackedChats.Remove(chat.Id);
                Core.Log.LibVerbose("Chat UnTracked");
                OnTrackedChatsChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Untrack all chats
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnTrackAllChats()
        {
            lock (TrackedChats)
                TrackedChats.Clear();
            OnTrackedChatsChanged?.Invoke(this, EventArgs.Empty);
            Core.Log.LibVerbose("All Chats UnTracked");
        }
        /// <inheritdoc />
        /// <summary>
        /// Get if a chat is being tracked
        /// </summary>
        /// <param name="chat">Chat to check</param>
        /// <returns>true if the chat is being tracked; otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsChatTracked(BotChat chat)
        {
            lock (TrackedChats)
                return chat != null && TrackedChats.Contains(chat.Id);
        }

        /// <inheritdoc />
        /// <summary>
        /// Start the bot engine listener.
        /// </summary>
        /// <returns>Async task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task StartListenerAsync()
        {
            Core.Log.LibVerbose("Starting Bot engine listener...");
            await Transport.ConnectAsync().ConfigureAwait(false);
            Core.Log.LibVerbose("Started.");
        }
        /// <inheritdoc />
        /// <summary>
        /// Stop the bot engine listener
        /// </summary>
        /// <returns>Async task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task StopListenerAsync()
        {
            Core.Log.LibVerbose("Stopping Bot engine listener...");
            await Transport.DisconnectAsync().ConfigureAwait(false);
            Core.Log.LibVerbose("Stopped.");
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends a text message
        /// </summary>
        /// <param name="chat">Chat where the message will be sent</param>
        /// <param name="message">Message to send</param>
        /// <param name="parseMode">Message parse mode</param>
        /// <returns>Async task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task SendTextMessageAsync(BotChat chat, string message, MessageParseMode parseMode = MessageParseMode.Default)
        {
			if (!IsConnected) return;
            Core.Log.LibVerbose("Sending text message. ChatId = {0}, Message = {1}", chat?.Id, message);
            await Transport.SendTextMessageAsync(chat, message, parseMode).ConfigureAwait(false);
            Core.Log.LibVerbose("Message sent.");
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends a text message to all tracked chats
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="parseMode">Message parse mode</param>
        /// <returns>Async task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task SendTextMessageToTrackedChatsAsync(string message, MessageParseMode parseMode = MessageParseMode.Default)
        {
			if (!IsConnected) return;
            Core.Log.LibVerbose("Sending text message to all tracked chats: Chats Count = {0} Message = {1}", TrackedChats.Count, message);
            foreach (var chat in TrackedChats)
				await Transport.SendTextMessageAsync(chat, message, parseMode).ConfigureAwait(false);
            Core.Log.LibVerbose("Messages sent.");
        }
		/// <inheritdoc />
		/// <summary>
		/// Send the typing action to the chat
		/// </summary>
		/// <param name="chat">Chat where the typing action will be sent</param>
		/// <returns>Async task</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task SendTypingAsync(BotChat chat)
		{
			if (!IsConnected) return;
			Core.Log.LibVerbose("Sending typing notification. ChatId = {0}", chat?.Id);
			await Transport.SendTypingAsync(chat).ConfigureAwait(false);
		}
		/// <inheritdoc />
		/// <summary>
		/// Send the typing action to all tracked chats
		/// </summary>
		/// <returns>Async task</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task SendTypingToTrackedChatsAsync()
		{
			if (!IsConnected) return;
			Core.Log.LibVerbose("Sending typing notification to all tracked chats: Chats Count = {0}", TrackedChats.Count);
			foreach (var chat in TrackedChats)
				await Transport.SendTypingAsync(chat).ConfigureAwait(false);
		}
        /// <inheritdoc />
        /// <summary>
        /// Sends a photo message
        /// </summary>
        /// <param name="chat">Chat where the message will be sent</param>
        /// <param name="fileName">Photo Filename</param>
        /// <param name="caption">Photo Caption</param>
        /// <returns>Async task</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task SendPhotoMessageAsync(BotChat chat, string fileName, string caption = null)
        {
            if (!IsConnected) return;
            Core.Log.LibVerbose("Sending photo message. ChatId = {0}, FileName = {1}, Caption = {2}", chat?.Id, fileName, caption);
            await Transport.SendPhotoMessageAsync(chat, fileName, caption).ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends a photo message
        /// </summary>
        /// <param name="chat">Chat where the message will be sent</param>
        /// <param name="fileStream">Photo stream</param>
        /// <param name="caption">Photo Caption</param>
        /// <returns>Async task</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task SendPhotoMessageAsync(BotChat chat, Stream fileStream, string caption = null)
        {
            if (!IsConnected) return;
            Core.Log.LibVerbose("Sending photo message. ChatId = {0}, Caption = {1}", chat?.Id, caption);
            await Transport.SendPhotoMessageAsync(chat, fileStream, caption).ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends a photo message to all tracked chats
        /// </summary>
        /// <param name="fileName">Photo Filename</param>
        /// <param name="caption">Photo Caption</param>
        /// <returns>Async task</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task SendPhotoMessageToTrackedChatsAsync(string fileName, string caption = null)
        {
            if (!IsConnected) return;
            Core.Log.LibVerbose("Sending photo message to all tracked chats. FileName = {0}, Caption = {1}", fileName, caption);
			foreach (var chat in TrackedChats)
                await Transport.SendPhotoMessageAsync(chat, fileName, caption).ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends a photo message to all tracked chats
        /// </summary>
        /// <param name="fileStream">Photo stream</param>
        /// <param name="caption">Photo Caption</param>
        /// <returns>Async task</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task SendPhotoMessageToTrackedChatsAsync(Stream fileStream, string caption = null)
        {
            if (!IsConnected) return;
            Core.Log.LibVerbose("Sending photo message to all tracked chats. Caption = {0}", caption);
            foreach (var chat in TrackedChats)
                await Transport.SendPhotoMessageAsync(chat, fileStream, caption).ConfigureAwait(false);
        }
        #endregion

        #region Private Methods
        private void Transport_TextMessageReceived(object sender, EventArgs<BotTextMessage> e)
        {
            if (e?.Item1?.Text?.IsNullOrEmpty() != false) return;

            Core.Log.LibVerbose("Message received, looking for command...");
            var message = e.Item1;
            var sCommands = Commands.Where((cmd, sMessage) => cmd.Condition(sMessage.Text), message).ToArray();
            if (sCommands.Any())
            {
                #region Get Chat
                if (!Chats.Contains(message.Chat.Id))
                    Chats.Add(message.Chat);
                #endregion

                #region Get User
                if (!Users.Contains(message.User.Id))
                    Users.Add(message.User);
                #endregion

                foreach (var sCommand in sCommands)
                {
                    Core.Log.LibVerbose("Executing command for: {0}", message.Text);
                    var res = sCommand.Handler(this, message);
                    Core.Log.LibVerbose("Command executed.");
                    if (res)
                        break;
                }
            }
            else
                Core.Log.LibVerbose("Command not found for the message: {0}", message.Text);
        }
        private void Transport_PhotoMessageReceived(object sender, EventArgs<BotPhotoMessage> e)
        {
            Core.Log.LibVerbose("Photo Message received...");
            var message = e.Item1;

            #region Get Chat
            if (!Chats.Contains(message.Chat.Id))
                Chats.Add(message.Chat);
            #endregion

            #region Get User
            if (!Users.Contains(message.User.Id))
                Users.Add(message.User);
            #endregion

            OnPhotoMessageReceived?.Invoke(this, e);
        }
        #endregion
    }
}
