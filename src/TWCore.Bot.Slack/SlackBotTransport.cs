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
using SlackAPI;
using SlackAPI.WebSocketMessages;
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedVariable
// ReSharper disable UnusedMember.Global

namespace TWCore.Bot.Slack
{
    /// <inheritdoc />
    /// <summary>
    /// Slack Bot Transport
    /// </summary>
    public class SlackBotTransport : IBotTransport
    {
        private SlackSocketClient _client;
        
        #region Events
        /// <inheritdoc />
        /// <summary>
        /// Event when a text message has been received
        /// </summary>
        public event EventHandler<EventArgs<BotTextMessage>> TextMessageReceived;
        /// <inheritdoc />
        /// <summary>
        /// Event when a photo message has been received
        /// </summary>
        public event EventHandler<EventArgs<BotPhotoMessage>> PhotoMessageReceived;
        #endregion
        
        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Get true if the transport is connected; otherwise, false.
        /// </summary>
        public bool IsConnected { get; private set; }
        /// <inheritdoc />
        /// <summary>
        /// Bot token
        /// </summary>
        public string Token { get; set; }
        #endregion
        
        #region .ctor
        /// <summary>
        /// Slack Bot Transport
        /// </summary>
        public SlackBotTransport()
        {
        }
        /// <summary>
        /// Slack Bot Transport
        /// </summary>
        /// <param name="token">Authorization token</param>
        public SlackBotTransport(string token)
        {
            Token = token;
        }
        #endregion
        
        #region Private Methods
        private void ClientOnOnMessageReceived(NewMessage obj)
        {
            try
            {
                _client.ConversationLookup.TryGetValue(obj.channel, out var conversation);
                _client.UserLookup.TryGetValue(obj.user, out var user);

                var chatType = BotChatType.Channel;
                var chatId = obj.channel;
                var chatName = obj.channel;

                if (conversation is Channel cnn)
                {
                    chatType = cnn.is_group ? BotChatType.Group : BotChatType.Channel;
                    chatName = cnn.name;
                }
                else if (conversation is DirectMessageConversation dmc)
                {
                    chatType = BotChatType.Private;
                    if (_client.UserLookup.TryGetValue(dmc.user, out var privateUser))
                        chatName = privateUser.name;
                    else
                        chatName = dmc.user;
                }

                var message = new BotTextMessage
                {
                    Id = obj.id.ToString(),
                    Date = obj.ts,
                    Text = obj.text,
                    Chat = new BotChat
                    {
                        ChatType = chatType,
                        Id = chatId,
                        Name = chatName
                    },
                    User = new SlackBotUser
                    {
                        Id = user?.id ?? obj.user,
                        Name = user?.name ?? obj.user,
                        Color = user?.color,
                        Presence = user?.presence,
                        Email = user?.profile?.email,
                        FirstName = user?.profile?.first_name,
                        LastName = user?.profile?.last_name,
                        RealName = user?.profile?.real_name,
                        Skype = user?.profile?.skype,
                        Phone = user?.profile?.phone
                    }
                };
                TextMessageReceived?.Invoke(this, new EventArgs<BotTextMessage>(message));
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
        private Task<LoginResponse> ConnectClientAsync()
        {
            var tskSource = new TaskCompletionSource<LoginResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
            IsConnected = false;
            Core.Log.LibDebug("Starting connection...");
            _client.Connect(connected =>
            {
                // This is called once the client has emitted the RTM start command
                Core.Log.LibDebug("RTM started.");
                IsConnected = true;
                tskSource.TrySetResult(connected);
            }, () =>
            {
                // This is called once the RTM client has connected to the end point
                Core.Log.LibDebug("Socket connected, waiting for RTM start.");
            });
            return tskSource.Task;
        }
        private Task<ChannelListResponse> GetChannelListAsync()
        {
            var tskSource = new TaskCompletionSource<ChannelListResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
            _client.GetChannelList(response => tskSource.TrySetResult(response));
            return tskSource.Task;
        }
        private Task<UserListResponse> GetUserListAsync()
        {
            var tskSource = new TaskCompletionSource<UserListResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
            _client.GetUserList(response => tskSource.TrySetResult(response));
            return tskSource.Task;
        }
        #endregion
        
        #region Public Methods
        /// <inheritdoc />
        /// <summary>
        /// Connect to the bot server
        /// </summary>
        /// <returns>Async task</returns>
        public async Task ConnectAsync()
        {
            if (_client != null && _client.IsConnected) return;
            _client = new SlackSocketClient(Token);
            _client.OnMessageReceived += ClientOnOnMessageReceived;
            Core.Log.LibDebug("Connecting...");
            var loginResponse = await ConnectClientAsync().ConfigureAwait(false);
            Core.Log.LibDebug("Connected, getting channels list...");
            var channelList = await GetChannelListAsync().ConfigureAwait(false);
            Core.Log.LibDebug("Getting users list...");
            var userList = await GetUserListAsync().ConfigureAwait(false);
            Core.Log.LibDebug("Connection done.");
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends a text message
        /// </summary>
        /// <param name="chat">Chat where the message will be sent</param>
        /// <param name="message">Message to send</param>
        /// <param name="parseMode">Message parse mode</param>
        /// <returns>Async task</returns>
        public Task SendTextMessageAsync(BotChat chat, string message, MessageParseMode parseMode = MessageParseMode.Default)
        {
            var taskSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _client.SendMessage(received => taskSource.TrySetResult(true), chat.Id, message);
            return taskSource.Task;
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends a photo message
        /// </summary>
        /// <param name="chat">Chat where the message will be sent</param>
        /// <param name="fileName">Photo Filename</param>
        /// <param name="caption">Photo Caption</param>
        /// <returns>Async task</returns>
        public Task SendPhotoMessageAsync(BotChat chat, string fileName, string caption = null)
        {
            return Task.CompletedTask;
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends a photo message
        /// </summary>
        /// <param name="chat">Chat where the message will be sent</param>
        /// <param name="fileStream">Photo stream</param>
        /// <param name="caption">Photo Caption</param>
        /// <returns>Async task</returns>
        public Task SendPhotoMessageAsync(BotChat chat, Stream fileStream, string caption = null)
        {
            return Task.CompletedTask;
        }
        /// <inheritdoc />
        /// <summary>
        /// Send the typing action to the chat
        /// </summary>
        /// <param name="chat">Chat where the typing action will be sent</param>
        /// <returns>Async task</returns>
        public Task SendTypingAsync(BotChat chat)
        {
            _client.SendTyping(chat.Id);
            return Task.CompletedTask;
        }
        /// <inheritdoc />
        /// <summary>
        /// Disconnect from the bot server
        /// </summary>
        /// <returns>Async task</returns>
        public Task DisconnectAsync()
        {
            if (_client.IsConnected)
                _client.CloseSocket();
            IsConnected = false;
            return Task.CompletedTask;
        }
        #endregion
    }
}