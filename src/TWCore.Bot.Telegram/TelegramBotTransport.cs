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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.IO;
using TBot = Telegram.Bot;
using TBotApi = Telegram.Bot.TelegramBotClient;

namespace TWCore.Bot.Telegram
{
    /// <summary>
    /// Telegram bot transport
    /// </summary>
    public class TelegramBotTransport : IBotTransport
    {
        volatile bool active;
        readonly TBotApi Bot;

        #region Events
        /// <summary>
        /// Event when a text message has been received
        /// </summary>
        public event EventHandler<EventArgs<BotTextMessage>> TextMessageReceived;
        /// <summary>
        /// Event when a photo message has been received
        /// </summary>
        public event EventHandler<EventArgs<BotPhotoMessage>> PhotoMessageReceived;
        #endregion

        #region Properties
        /// <summary>
        /// Bot token
        /// </summary>
        public string Token { get; private set; }
        /// <summary>
        /// Get or set if the web page preview is disabled when sending a message
        /// </summary>
        public bool DisableWebPagePreview { get; set; } = true;
        /// <summary>
        /// Get true if the transport is connected; otherwise, false.
        /// </summary>
        public bool IsConnected => active;
        #endregion

        #region .ctor
        /// <summary>
        /// Telegram bot transport
        /// </summary>
        /// <param name="token">Bot token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TelegramBotTransport(string token)
        {
            Token = token;
            Bot = new TBotApi(token);
            Bot.OnMessage += Bot_OnMessage;
            Bot.OnMessageEdited += Bot_OnMessage;
            Bot.OnReceiveError += Bot_OnReceiveError;
            Bot.OnInlineQuery += Bot_OnInlineQuery;
            Bot.OnInlineResultChosen += Bot_OnInlineResultChosen;
        }
        #endregion

        #region Bot Events Handling

        private void Bot_OnInlineResultChosen(object sender, TBot.Args.ChosenInlineResultEventArgs e)
        {
        }

        private void Bot_OnInlineQuery(object sender, TBot.Args.InlineQueryEventArgs e)
        {
        }

        private void Bot_OnReceiveError(object sender, TBot.Args.ReceiveErrorEventArgs e)
        {
            Core.Log.Write(e.ApiRequestException);
        }

        private void Bot_OnMessage(object sender, TBot.Args.MessageEventArgs e)
        {
            if (e.Message == null) return;

            switch (e.Message.Type)
            {
                case TBot.Types.Enums.MessageType.TextMessage:
                    if (string.IsNullOrWhiteSpace(e.Message.Text)) return;
                    var message = new BotTextMessage
                    {
                        Id = e.Message.MessageId.ToString(),
                        Date = e.Message.Date,
                        Text = e.Message.Text,
                        Chat = new TelegramBotChat
                        {
                            Id = e.Message.Chat.Id.ToString(),
                            ChatType = (BotChatType)e.Message.Chat.Type,
                            Name = e.Message.Chat.Title,
                            Title = e.Message.Chat.Title,
                            Username = e.Message.Chat.Username,
                            FirstName = e.Message.Chat.FirstName,
                            LastName = e.Message.Chat.LastName
                        },
                        User = new TelegramBotUser
                        {
                            Id = e.Message.From.Id.ToString(),
                            Name = e.Message.From.Username,
                            FirstName = e.Message.From.FirstName,
                            LastName = e.Message.From.LastName
                        }
                    };
                    TextMessageReceived?.Invoke(this, new EventArgs<BotTextMessage>(message));
                    break;
                case TBot.Types.Enums.MessageType.PhotoMessage:
                    foreach(var photo in e.Message.Photo)
                    {
                        var photoMessage = new BotPhotoMessage
                        {
                            Id = e.Message.MessageId.ToString(),
                            Date = e.Message.Date,
                            PhotoWidth = photo.Width,
                            PhotoHeight = photo.Height,
                            PhotoId = photo.FileId,
                            PhotoSize = photo.FileSize,
                            PhotoName = photo.FilePath,
                            PhotoStream = new Lazy<Stream>(() =>
                            {
                                var file = Bot.GetFileAsync(photo.FileId).WaitAndResults();
                                return file.FileStream;
                            }),
                            Chat = new TelegramBotChat
                            {
                                Id = e.Message.Chat.Id.ToString(),
                                ChatType = (BotChatType)e.Message.Chat.Type,
                                Name = e.Message.Chat.Title,
                                Title = e.Message.Chat.Title,
                                Username = e.Message.Chat.Username,
                                FirstName = e.Message.Chat.FirstName,
                                LastName = e.Message.Chat.LastName
                            },
                            User = new TelegramBotUser
                            {
                                Id = e.Message.From.Id.ToString(),
                                Name = e.Message.From.Username,
                                FirstName = e.Message.From.FirstName,
                                LastName = e.Message.From.LastName
                            }
                        };
                        PhotoMessageReceived?.Invoke(this, new EventArgs<BotPhotoMessage>(photoMessage));
                    }
                    break;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Connect to the bot server
        /// </summary>
        /// <returns>Async task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task ConnectAsync()
        {
            Bot.StartReceiving();
            active = true;
            return Task.CompletedTask;
        }
        /// <summary>
        /// Disconnect from the bot server
        /// </summary>
        /// <returns>Async task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task DisconnectAsync()
        {
            Bot.StopReceiving();
            active = false;
            return Task.CompletedTask;
        }

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
            if (!active) return;
            await Bot.SendTextMessageAsync(chat.Id.ParseTo<long>(-1), message, 
                disableWebPagePreview: DisableWebPagePreview, 
                parseMode: (TBot.Types.Enums.ParseMode)parseMode).ConfigureAwait(false);
        }
        /// <summary>
        /// Send the typing action to the chat
        /// </summary>
        /// <param name="chat">Chat where the typing action will be sent</param>
        /// <returns>Async task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task SendTypingAsync(BotChat chat)
        {
            if (!active) return;
            await Bot.SendChatActionAsync(chat.Id.ParseTo<long>(-1), TBot.Types.Enums.ChatAction.Typing).ConfigureAwait(false);
        }
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
            if (!active) return;
            if (!File.Exists(fileName)) throw new FileNotFoundException("The filename doesn't exist", fileName);

            using (var ms = new RecycleMemoryStream())
            {
                //var ms = new MemoryStream();
                using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    await fs.CopyToAsync(ms).ConfigureAwait(false);
                ms.Position = 0;
                var fileToSend = new TBot.Types.FileToSend(Path.GetFileName(fileName), ms);

                await Bot.SendPhotoAsync(chat.Id.ParseTo<long>(-1), fileToSend, caption).ConfigureAwait(false);
            }
        }
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
            if (!active) return;
            Ensure.ArgumentNotNull(fileStream, "The fileStream can't be null");
            var fileToSend = new TBot.Types.FileToSend(caption ?? Core.Now.Ticks.ToString(), fileStream);
            await Bot.SendPhotoAsync(chat.Id.ParseTo<long>(-1), fileToSend, caption).ConfigureAwait(false);
        }
        #endregion
    }
}
