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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Bot;
using TWCore.Bot.Telegram;
using TWCore.Serialization;
using TWCore.Settings;
// ReSharper disable CheckNamespace
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Log.Storages
{
    /// <summary>
    /// Telegram Bot Log Storage
    /// </summary>
    [SettingsContainer("TelegramBotLog")]
    public class TelegramBotLogStorage : SettingsBase, ILogStorage
    {
        private readonly ISerializer _serializer = SerializerManager.DefaultBinarySerializer;

        /// <summary>
        /// Bot Engine
        /// </summary>
        private static IBotEngine Bot { get; set; }
        /// <summary>
        /// Save tracked chats on disk
        /// </summary>
        public bool SaveTrackedChats { get; protected set; } = false;
        /// <summary>
        /// Tracked chats file path
        /// </summary>
        public string TrackedChatsFilePath { get; protected set; } = "{0}_TelegramBotLogChats".ApplyFormat(Core.ApplicationName);
        /// <summary>
        /// Token to use by the Telegram Bot
        /// </summary>
        public string BotToken { get; set; }
        /// <summary>
        /// Log Level to send to tracked chats
        /// </summary>
        public LogLevel LevelAllowed { get; set; } = LogLevel.Error;
        /// <summary>
        /// Send Group Metadata
        /// </summary>
        public bool SendGroupMetadata { get; set; }
        /// <summary>
        /// Flag to send Stack Trace in message
        /// </summary>
        public bool SendStackTrace { get; set; } = false;


        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Telegram Bot Log Storage
        /// </summary>
        public TelegramBotLogStorage()
        {
            Bot = GetBotEngine();
            Bot.StartListenerAsync();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IBotEngine GetBotEngine()
        {
            var telegramTransport = new TelegramBotTransport(BotToken);
            var bEngine = new BotEngine(telegramTransport);

            var header = string.Format("Machine Name: {0}\r\nApplication Name: {1}\r\n", Core.MachineName, Core.ApplicationDisplayName);

            bEngine.Commands.Add(txt => txt.Trim().StartsWith("/TrackLog"), async (api, msg) =>
            {
                api.TrackChat(msg.Chat);
                await api.SendTextMessageAsync(msg.Chat, header + "Tracking").ConfigureAwait(false);
                return true;
            });
            bEngine.Commands.Add(txt => txt.Trim().StartsWith("/UntrackLog"), async (api, msg) =>
            {
                api.TrackChat(msg.Chat);
                await api.SendTextMessageAsync(msg.Chat, header + "Untracked").ConfigureAwait(false);
                return true;
            });
            bEngine.Commands.Add(txt => txt.Trim().StartsWith("/HelloLog"), async (api, msg) =>
            {
                await api.SendTextMessageAsync(msg.Chat, header + "Hello").ConfigureAwait(false);
                return true;
            });

            if (SaveTrackedChats)
            {
                if (File.Exists(TrackedChatsFilePath))
                {
                    Try.Do(() =>
                    {
                        Core.Log.InfoBasic("Loading tracked chats file: {0}", TrackedChatsFilePath);
                        var botChats = _serializer.DeserializeFromFile<List<BotChat>>(TrackedChatsFilePath);
                        bEngine.TrackedChats.AddRange(botChats);
                        Core.Log.InfoBasic("{0} tracked chats loaded.", botChats?.Count);
                    });
                }
                bEngine.OnTrackedChatsChanged += (s, e) =>
                {
                    Try.Do(() =>
                    {
                        Core.Log.InfoBasic("Saving tracked chats file: {0}", TrackedChatsFilePath);
                        var botChats = ((IBotEngine)s).TrackedChats.ToList();
                        _serializer.SerializeToFile(botChats, TrackedChatsFilePath);
                        Core.Log.InfoBasic("{0} tracked chats saved.", botChats.Count);
                    });
                };
            }

            return bEngine;
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Writes a log item to the storage
        /// </summary>
        /// <param name="item">Log Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task WriteAsync(ILogItem item)
        {
            if ((LevelAllowed & item.Level) == 0) return;
            var msg = string.Format("{0}\r\nMachine Name: {1} [{2}]\r\nAplicationName: {3}\r\nMessage: {4}", item.Timestamp.ToString("dd/MM/yyyy HH:mm:ss"), item.MachineName, item.EnvironmentName, item.ApplicationName, item.Message);
            if (item.Exception != null)
            {
                if (!string.IsNullOrEmpty(item.Exception.ExceptionType))
                    msg += "\r\nException: " + item.Exception.ExceptionType;
                if (SendStackTrace && !string.IsNullOrEmpty(item.Exception?.StackTrace))
                    msg += "\r\nStack Trace: " + item.Exception.StackTrace;
                if (item.Exception.StackTrace.Contains("TelegramBotTransport.<<ConnectAsync>"))
                    return;
            }
            await Bot.SendTextMessageToTrackedChatsAsync(msg).ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Writes a log item empty line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task WriteEmptyLineAsync()
        {
            return Task.CompletedTask;
        }
        /// <summary>
        /// Writes a group metadata item to the storage
        /// </summary>
        /// <param name="item">Group metadata item</param>
        /// <returns>Task process</returns>
        public async Task WriteAsync(IGroupMetadata item)
        {
            if (!SendGroupMetadata) return;
            var msg = string.Format("{0}\r\nMachine Name: {1} [{2}]\r\nAplicationName: {3}\r\nGroup: {4}\r\n", item.Timestamp.ToString("dd/MM/yyyy HH:mm:ss"), Core.MachineName, Core.EnvironmentName, Core.ApplicationName, item.GroupName);
            if (item.Items != null)
            {
                var count = item.Items.Length;
                for (var i = 0; i < count; i++)
                {
                    var keyValue = item.Items[i];
                    msg += string.Format("{0}={1}\r\n", keyValue.Key, keyValue.Value);
                }
            }
            await Bot.SendTextMessageToTrackedChatsAsync(msg).ConfigureAwait(false);
        }


        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Bot.StopListenerAsync().WaitAsync();
            Bot = null;
        }
    }
}
