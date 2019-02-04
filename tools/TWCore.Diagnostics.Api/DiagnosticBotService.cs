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
using System.Threading;
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
        private ConcurrentDictionary<(string, string, string), ErrorsRegister> _errorsRegistry;

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
            _errorsRegistry = new ConcurrentDictionary<(string, string, string), ErrorsRegister>();
            BindCommands();
            return _engine;
        }

        #region Bot Commands
        private void BindCommands()
        {
            _engine.Commands.Add(msg => msg.Equals("hola", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                await bot.SendTextMessageAsync(message.Chat, ">>>Que tal? " + message.User.Name).ConfigureAwait(false);
                return true;
            });
            _engine.Commands.Add(msg => msg.Equals(".getenv", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                await bot.SendTextMessageAsync(message.Chat, "`The current environment is: " + _currentEnvironment + "`").ConfigureAwait(false);
                return true;
            });
            _engine.Commands.Add(msg => msg.StartsWith(".setenv", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                var arrText = message.Text.SplitAndTrim(" ");
                if (arrText.Length == 2)
                {
                    _currentEnvironment = arrText[1];
                    await bot.SendTextMessageAsync(message.Chat, "`Environment setted to: " + _currentEnvironment + "`").ConfigureAwait(false);
                }
                else
                {
                    await bot.SendTextMessageAsync(message.Chat, ":exclamation: `Invalid syntax.`").ConfigureAwait(false);
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
                    await bot.SendTextMessageAsync(message.Chat, "`Chat was added to the error tracking list.`").ConfigureAwait(false);
                else
                    await bot.SendTextMessageAsync(message.Chat, "`Chat already added to the error tracking list.`").ConfigureAwait(false);

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
                    await bot.SendTextMessageAsync(message.Chat, "`Chat was removed from the error tracking list.`").ConfigureAwait(false);

                return true;
            });

            //*****************************************************************************************************************************************

            _engine.Commands.Add(msg => msg.Equals(".getcounters", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                await bot.SendTextMessageAsync(message.Chat, $"`Querying for counters for {_currentEnvironment}...`").ConfigureAwait(false);
                var counters = await DbHandlers.Instance.Query.GetCounters(_currentEnvironment).ConfigureAwait(false);
                await bot.SendTextMessageAsync(message.Chat, "`Counters:`").ConfigureAwait(false);
                counters = counters.OrderBy(c => c.Application).ThenBy(c => c.Category).ThenBy(c => c.Name).ToList();
                foreach (var batch in counters.Batch(20))
                {
                    var str = batch.Select(c => c.CountersId + " | " + c.Application + "\\" + c.Category + "\\" + c.Name).Join("\n");
                    await bot.SendTextMessageAsync(message.Chat, "```" + str + "```").ConfigureAwait(false);
                }
                return true;
            });
            _engine.Commands.Add(msg => msg.StartsWith(".getcountervalue", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                var arrText = message.Text.SplitAndTrim(" ");
                if (arrText.Length == 2 && Guid.TryParse(arrText[1], out var counterId))
                {
                    var counters = await DbHandlers.Instance.Query.GetCounters(_currentEnvironment).ConfigureAwait(false);
                    var counterItem = counters.FirstOrDefault((c, cid) => c.CountersId == cid, counterId);
                    var counterValues = await DbHandlers.Instance.Query.GetCounterValues(counterId, Core.Now.Date.AddMonths(-1), Core.Now.Date.AddDays(1), 10).ConfigureAwait(false);
                    if (counterValues.Count > 0 && counterItem != null)
                    {
                        var msg = counterValues.Select(v => v.Timestamp + ": " + v.Value).Join("\n");
                        await bot.SendTextMessageAsync(message.Chat, $"`Last 10 counter values for {counterItem.Application}\\{counterItem.Category}\\{counterItem.Name}:`\n```" + msg + "```").ConfigureAwait(false);
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(message.Chat, ":exclamation: `No values were found.`").ConfigureAwait(false);
                    }
                }
                else
                {
                    await bot.SendTextMessageAsync(message.Chat, ":exclamation: `Invalid syntax.`").ConfigureAwait(false);
                }
                return true;
            });

            //*****************************************************************************************************************************************
            DbHandlers.Instance.ErrorLogMessage += Instance_ErrorLogMessage;
        }
        #endregion

        #region Nested Types
        private class ErrorsRegister
        {
            public DateTime LastDate;
            public int Quantity;
        }
        #endregion

        #region Event Handlers
        private void OnConnected(object sender, EventArgs e)
        {
            LoadErrorChats();
        }
        private void OnDisconnected(object sender, EventArgs e)
        {
            SaveErrorChats();
        }
        private void Instance_ErrorLogMessage(object sender, Log.LogItem e)
        {
            if (!string.Equals(e.EnvironmentName, _currentEnvironment, StringComparison.OrdinalIgnoreCase)) return;
            var mKey = (e.EnvironmentName, e.ApplicationName, e.Message);
            var register = _errorsRegistry.GetOrAdd(mKey, _ => new ErrorsRegister());
            if (register.LastDate.Date != Core.Now.Date)
                Interlocked.Exchange(ref register.Quantity, 0);
            else
                Interlocked.Increment(ref register.Quantity);
            if (Core.Now - register.LastDate > TimeSpan.FromSeconds(30))
            {
                register.LastDate = Core.Now;
                var message = $":exclamation: `Error at: {e.Timestamp}`\n>>>```";
                message += $"Application: {e.ApplicationName}\n";
                message += $"Group: {e.GroupName}\n";
                message += $"Numbers of repetitions of the same error today: {register.Quantity}\n";
                message += $"Log: {e.Message}\n";
                if (!string.IsNullOrEmpty(e.TypeName))
                    message += $"Caller: {e.TypeName}\n";
                if (e.Exception != null)
                {
                    message += $"Message: {e.Exception.Message}\n";
                    message += $"Type: {e.Exception.ExceptionType}\n";
                    message += $"Stacktrace: {e.Exception.StackTrace}\n";
                }
                message += "```";
                foreach (var chat in _errorChats.Values)
                {
                    _ = _engine.SendTextMessageAsync(chat, message);
                }
            }
        }
        #endregion

        #region Load & Save Error Chats files.
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
                        _ = _engine.SendTextMessageAsync(chat, $":warning: `Diagnostics bot has started, you're listening to errors on environment: {_currentEnvironment}`");
                    }
                    Core.Log.InfoBasic("Errors chats loaded.");
                }
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
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
        #endregion

        private class BotSettings : Settings.SettingsBase
        {
            public string SlackToken { get; set; } = "ZX9YXvB6hTwTmurR4QguFgZZ-551556876045-732105390004-bxox".Reverse();
            public string DefaultEnvironment { get; set; } = "Docker";
        }
    }
}