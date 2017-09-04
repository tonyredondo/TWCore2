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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using TWCore.Bot;
using TWCore.Bot.Telegram;
using TWCore.Serialization;
using TWCore.Settings;

namespace TWCore.Diagnostics.Log.Storages
{
    [SettingsContainer("TelegramBotLog")]
    public class TelegramBotLogStorage : SettingsBase, ILogStorage
    {
        ISerializer serializer = SerializerManager.DefaultBinarySerializer;

        /// <summary>
        /// Bot Engine
        /// </summary>
        static IBotEngine Bot { get; set; }
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
        /// Flag to send Stack Trace in message
        /// </summary>
        public bool SendStackTrace { get; set; } = false;


        #region .ctor
        /// <summary>
        /// Telegram Bot Log Storage
        /// </summary>
        public TelegramBotLogStorage()
        {
            Bot = GetBotEngine();
            Bot.StartListenerAsync();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IBotEngine GetBotEngine()
        {
            var telegramTransport = new TelegramBotTransport(BotToken);
            var bEngine = new BotEngine(telegramTransport);

            var header = string.Format("Machine Name: {0}\r\nApplication Name: {1}\r\n", Core.MachineName, Core.ApplicationDisplayName);

            bEngine.Commands.Add(txt => txt.Trim().StartsWith("/TrackLog"), (api, msg) =>
            {
                api.TrackChat(msg.Chat);
                api.SendTextMessageAsync(msg.Chat, header + "Tracking");
                return true;
            });
            bEngine.Commands.Add(txt => txt.Trim().StartsWith("/UntrackLog"), (api, msg) =>
            {
                api.TrackChat(msg.Chat);
                api.SendTextMessageAsync(msg.Chat, header + "Untracked");
                return true;
            });
            bEngine.Commands.Add(txt => txt.Trim().StartsWith("/HelloLog"), (api, msg) =>
            {
                api.SendTextMessageAsync(msg.Chat, header + "Hello");
                return true;
            });

            if (SaveTrackedChats)
            {
                if (File.Exists(TrackedChatsFilePath))
                {
                    Try.Do(() =>
                    {
                        Core.Log.InfoBasic("Loading tracked chats file: {0}", TrackedChatsFilePath);
                        var botChats = serializer.DeserializeFromFile<List<BotChat>>(TrackedChatsFilePath);
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
                        serializer.SerializeToFile(botChats, TrackedChatsFilePath);
                        Core.Log.InfoBasic("{0} tracked chats saved.", botChats?.Count);
                    });
                };
            }

            return bEngine;
        }
        #endregion

        /// <summary>
        /// Writes a log item to the storage
        /// </summary>
        /// <param name="item">Log Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async void Write(ILogItem item)
        {
            if (!LevelAllowed.HasFlag(item.Level)) return;
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
        /// <summary>
        /// Writes a log item empty line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEmptyLine()
        {
        }

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
