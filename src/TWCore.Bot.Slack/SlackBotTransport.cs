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
            var channel = obj.channel != null ? _client.ChannelLookup[obj.channel] : null;
            var user = obj.user != null ? _client.UserLookup[obj.user] : null;
            if (channel == null)
            {
                var channels = GetChannelListAsync().WaitAndResults();
                channel = channels.channels.FirstOrDefault((i, c) => i.id == c, obj.channel);
            }
            if (user == null)
            {
                var users = GetUserListAsync().WaitAndResults();
                user = users.members.FirstOrDefault((i, c) => i.id == c, obj.user);
            }
            if (channel == null)
            {
                Core.Log.Warning("Channel can't be found");
                channel = new Channel
                {
                    id = obj.channel,
                    name = obj.channel
                };
            }
            if (user == null)
            {
                Core.Log.Warning("User can't be found");
                user = new User
                {
                    id = obj.user,
                    name = obj.user
                };
            }
            var message = new BotTextMessage
            {
                Id = obj.id.ToString(),
                Date = obj.ts,
                Text = obj.text,
                Chat = new BotChat
                {
                    ChatType = BotChatType.Channel,
                    Id = channel.id,
                    Name = channel.name
                },
                User = new SlackBotUser
                {
                    Id = user.id,
                    Name = user.name,
                    Color = user.color,
                    Presence = user.presence,
                    Email = user.profile?.email,
                    FirstName = user.profile?.first_name,
                    LastName = user.profile?.last_name,
                    RealName = user.profile?.real_name,
                    Skype = user.profile?.skype,
                    Phone = user.profile?.phone
                }
            };
            TextMessageReceived?.Invoke(this, new EventArgs<BotTextMessage>(message));
        }
        private Task<LoginResponse> ConnectClientAsync()
        {
            var tskSource = new TaskCompletionSource<LoginResponse>();
            IsConnected = false;
            _client.Connect(connected =>
            {
                // This is called once the client has emitted the RTM start command
                IsConnected = true;
                tskSource.TrySetResult(connected);
            }, () =>
            {
                // This is called once the RTM client has connected to the end point
            });
            return tskSource.Task;
        }
        private Task<ChannelListResponse> GetChannelListAsync()
        {
            var tskSource = new TaskCompletionSource<ChannelListResponse>();
            _client.GetChannelList(response => tskSource.TrySetResult(response));
            return tskSource.Task;
        }
        private Task<UserListResponse> GetUserListAsync()
        {
            var tskSource = new TaskCompletionSource<UserListResponse>();
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
            var loginResponse = await ConnectClientAsync().ConfigureAwait(false);
            var channelList = await GetChannelListAsync().ConfigureAwait(false);
            var userList = await GetUserListAsync().ConfigureAwait(false);
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
            var taskSource = new TaskCompletionSource<bool>();
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