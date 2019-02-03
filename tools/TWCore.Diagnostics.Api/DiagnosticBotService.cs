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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TWCore.Bot;
using TWCore.Bot.Slack;
using TWCore.Serialization;
using TWCore.Services;
// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Api
{
    public class DiagnosticBotService : BotService
    {
        const string ErrorChatsFile = "errorchats.json";

        private BotEngine _engine;
        private string _currentEnvironment;
        private ConcurrentDictionary<string, BotChat> _errorChats;

        protected override IBotEngine GetBotEngine()
        {
            //Core.DebugMode = true;
            var settings = Core.GetSettings<BotSettings>();
            var slackTransport = new SlackBotTransport(settings.SlackToken);
            _engine = new BotEngine(slackTransport);
            _engine.OnConnected += OnConnected;
            _engine.OnDisconnected += OnDisconnected;
            _currentEnvironment = settings.DefaultEnvironment;
            _errorChats = new ConcurrentDictionary<string, BotChat>();
            BindCommands();

            Task.Delay(20000).ContinueWith(_ =>
            {
                Instance_ErrorLogMessage(this, new Log.LogItem
                {
                    ApplicationName = "app 1",
                    Timestamp = Core.Now,
                    GroupName = Guid.NewGuid().ToString(),
                    EnvironmentName = "Docker",
                    Exception = new SerializableException(new Exception("Error de test")),
                });
            });

            return _engine;
        }

        private void OnConnected(object sender, EventArgs e)
        {
            LoadErrorChats();
        }
        private void OnDisconnected(object sender, EventArgs e)
        {
            SaveErrorChats();
        }

        private void BindCommands()
        {
            _engine.Commands.Add(msg => msg.Equals("hola", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                await bot.SendTextMessageAsync(message.Chat, ">>>Que tal? " + message.User.Name).ConfigureAwait(false);
                return true;
            });
            _engine.Commands.Add(msg => msg.Equals(".getenv", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                await bot.SendTextMessageAsync(message.Chat, ">>>The current environment is: " + _currentEnvironment).ConfigureAwait(false);
                return true;
            });
            _engine.Commands.Add(msg => msg.StartsWith(".setenv", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                var arrText = message.Text.SplitAndTrim(" ");
                if (arrText.Length == 2)
                {
                    _currentEnvironment = arrText[1];
                    await bot.SendTextMessageAsync(message.Chat, ">>>Environment setted to: " + _currentEnvironment).ConfigureAwait(false);
                }
                else
                {
                    await bot.SendTextMessageAsync(message.Chat, ">Invalid syntax.").ConfigureAwait(false);
                }
                return true;
            });

            //*****************************************************************************************************************************************

            _engine.Commands.Add(msg => msg.Equals(".trackerrors", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                var added = false;
                lock (this)
                {
                    added = _errorChats.TryAdd(message.Chat.Id, message.Chat);
                    SaveErrorChats();
                }
                if (added)
                    await bot.SendTextMessageAsync(message.Chat, ">>>Chat was added to the error tracking list.").ConfigureAwait(false);
                else
                    await bot.SendTextMessageAsync(message.Chat, ">>>Chat already added to the error tracking list.").ConfigureAwait(false);

                return true;
            });
            _engine.Commands.Add(msg => msg.Equals(".untrackerrors", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                var removed = false;
                lock (this)
                {
                    removed = _errorChats.TryRemove(message.Chat.Id, out _);
                    SaveErrorChats();
                }
                if (removed)
                    await bot.SendTextMessageAsync(message.Chat, ">>>Chat was removed from the error tracking list.").ConfigureAwait(false);

                return true;
            });

            //*****************************************************************************************************************************************

            _engine.Commands.Add(msg => msg.Equals(".getcounters", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                await bot.SendTextMessageAsync(message.Chat, $">>>Querying for counters for {_currentEnvironment}...").ConfigureAwait(false);
                var counters = await DbHandlers.Instance.Query.GetCounters(_currentEnvironment).ConfigureAwait(false);
                await bot.SendTextMessageAsync(message.Chat, ">>>Counters: ").ConfigureAwait(false);
                foreach (var batch in counters.Batch(30))
                {
                    var str = batch.Select(c => c.Category + "\\" + c.Name).Join("\n");
                    await bot.SendTextMessageAsync(message.Chat, "```" + str + "```").ConfigureAwait(false);
                }
                return true;
            });

            //*****************************************************************************************************************************************
            DbHandlers.Instance.ErrorLogMessage += Instance_ErrorLogMessage;
        }

        private void Instance_ErrorLogMessage(object sender, Log.LogItem e)
        {
            var message = $"```Error at: {e.Timestamp}\nEnvironment: {e.EnvironmentName}\nApplication: {e.ApplicationName}\nGroup: {e.GroupName}\nText: {e.Message}\nMessage: {e.Exception?.Message}\nType: {e.Exception?.ExceptionType}\nStacktrace: {e.Exception?.StackTrace}```";
            foreach (var chat in _errorChats.Values)
            {
                _ = _engine.SendTextMessageAsync(chat, message);
            }
        }

        private void SaveErrorChats()
        {
            try
            {
                Core.Log.InfoBasic("Saving error chats...");
                var keys = _errorChats.Keys.ToArray();
                keys.SerializeToJsonFile(ErrorChatsFile);
                Core.Log.InfoBasic("Errors chats saved.");
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
        private void LoadErrorChats()
        {
            try
            {
                if (File.Exists(ErrorChatsFile))
                {
                    Core.Log.InfoBasic("Loading error chats...");
                    var keys = ErrorChatsFile.DeserializeFromJsonFile<string[]>();
                    foreach(var key in keys)
                    {
                        if (_engine.Chats.TryGet(key, out var chat))
                        {
                            Core.Log.InfoBasic("Adding chat: {0} - {1}", key, chat.Name);
                        }
                        else
                        {
                            Core.Log.InfoBasic("Adding chat: {0}", key);
                            chat = new BotChat
                            {
                                Id = key,
                                ChatType = BotChatType.Private,
                                Name = key
                            };
                        }
                        _errorChats.TryAdd(key, chat);
                    }
                    Core.Log.InfoBasic("Errors chats loaded.");
                }
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }

        private class BotSettings : Settings.SettingsBase
        {
            public string SlackToken { get; set; } = "ZX9YXvB6hTwTmurR4QguFgZZ-551556876045-732105390004-bxox".Reverse();
            public string DefaultEnvironment { get; set; } = "Docker";
        }
    }
}