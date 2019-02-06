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
using TWCore.Diagnostics.Counters;
using TWCore.Serialization;
using TWCore.Services;
// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Api
{
    public class DiagnosticBotService : BotService
    {
        private static BotSettings Settings = Core.GetSettings<BotSettings>();

        const string ErrorChatsFile = "errorchats.json";

        private BotEngine _engine;
        private string _currentEnvironment;
        private ConcurrentDictionary<string, BotChat> _errorChats;
        private ConcurrentDictionary<(string, string, string), ErrorsRegister> _errorsRegistry;

        protected override IBotEngine GetBotEngine()
        {
            var slackTransport = new SlackBotTransport(Settings.SlackToken);
            _engine = new BotEngine(slackTransport);
            _engine.OnConnected += OnConnected;
            _engine.OnDisconnected += OnDisconnected;
            _currentEnvironment = Settings.DefaultEnvironment;
            _errorChats = new ConcurrentDictionary<string, BotChat>();
            _errorsRegistry = new ConcurrentDictionary<(string, string, string), ErrorsRegister>();
            BindCommands();
            return _engine;
        }

        #region Bot Commands
        private void BindCommands()
        {
            //*****************************************************************************************************************************************

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

            _engine.Commands.Add(msg => msg.StartsWith(".getcounters", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                var arrText = message.Text.SplitAndTrim(" ");
                if (arrText.Length == 2 && Enum.TryParse(typeof(CounterKind), arrText[1], out var counterKind))
                {
                    var cKind = (CounterKind)counterKind;
                    await bot.SendTextMessageAsync(message.Chat, $"`Querying {cKind} counters for {_currentEnvironment}...`").ConfigureAwait(false);
                    var counters = await DbHandlers.Instance.Query.GetCounters(_currentEnvironment).ConfigureAwait(false);
                    await bot.SendTextMessageAsync(message.Chat, "`Counters:`").ConfigureAwait(false);
                    counters = counters.Where(c => c.Kind == cKind).OrderBy(c => c.Application).ThenBy(c => c.Category).ThenBy(c => c.Name).ToList();
                    foreach (var batch in counters.Batch(20))
                    {
                        var str = batch.Select(c => c.CountersId + " | " + c.Application + "\\" + c.Category + "\\" + c.Name).Join("\n");
                        await bot.SendTextMessageAsync(message.Chat, "```" + str + "```").ConfigureAwait(false);
                    }
                }
                else
                {
                    await bot.SendTextMessageAsync(message.Chat, $":exclamation: `You need to specify a counter kind like: {CounterKind.Application}, {CounterKind.Bot}, {CounterKind.Cache}, {CounterKind.DataAccess}, {CounterKind.Messaging}, {CounterKind.RPC}, {CounterKind.Unknown}.`").ConfigureAwait(false);
                }
                return true;
            });
            _engine.Commands.Add(msg => msg.StartsWith(".getcountervalue", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                var arrText = message.Text.SplitAndTrim(" ");
                if (arrText.Length == 2 && Guid.TryParse(arrText[1], out var counterId))
                {
                    var counterItem = await DbHandlers.Instance.Query.GetCounter(counterId).ConfigureAwait(false);
                    if (counterItem != null)
                        await bot.SendTextMessageAsync(message.Chat, $"`Today latest 20 counter values for {counterItem.Application}\\{counterItem.Category}\\{counterItem.Name} [{counterItem.Type}]:`").ConfigureAwait(false);

                    var counterValues = await DbHandlers.Instance.Query.GetCounterValues(counterId, Core.Now.Date.AddSeconds(-1), Core.Now.Date.AddDays(1)).ConfigureAwait(false);
                    if (counterValues.Count > 0)
                    {
                        var msg = "```" + counterValues.Take(20).Select(v => v.Timestamp + ": " + v.Value).Join("\n") + "\n\n";
                        if (counterItem.Type == CounterType.Average)
                        {
                            var avg = counterValues.Average(i => Convert.ToDecimal(i.Value));
                            msg += $"Total average for today: {avg}";
                        }
                        else if (counterItem.Type == CounterType.Cumulative)
                        {
                            var total = counterValues.Sum(i => Convert.ToDecimal(i.Value));
                            msg += $"Total cumulative for today: {total}";
                        }
                        msg += "```";

                        await bot.SendTextMessageAsync(message.Chat, msg).ConfigureAwait(false);
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

            _engine.Commands.Add(msg => msg.Equals(".help", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                var helpMsg = $"`Available commands:`\n";
                helpMsg += ">>>```.getenv : Get current environment.\n";
                helpMsg += ".setenv : Set current environment.\n";
                helpMsg += ".trackerrors : Sets the current chat to tracking errors.\n";
                helpMsg += ".untrackerrors : Remove the current chat to the tracking errors list.\n";
                helpMsg += ".getcounters : Get available counters.\n";
                helpMsg += ".getcountervalue : Get values for a counter.\n```";
                await bot.SendTextMessageAsync(message.Chat, helpMsg).ConfigureAwait(false);
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
                    if (!string.IsNullOrEmpty(e.Exception.StackTrace))
                        message += $"Stacktrace: \n{e.Exception.StackTrace}\n";
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
                if (!Directory.Exists(Settings.DataFolder))
                    Directory.CreateDirectory(Settings.DataFolder);

                var pFileName = Path.Combine(Settings.DataFolder, ErrorChatsFile);
                if (File.Exists(pFileName))
                {
                    Core.Log.InfoBasic("Loading error chats...");
                    var keys = pFileName.DeserializeFromJsonFile<string[]>();
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
                if (!Directory.Exists(Settings.DataFolder))
                    Directory.CreateDirectory(Settings.DataFolder);
                var pFileName = Path.Combine(Settings.DataFolder, ErrorChatsFile);
                Core.Log.InfoBasic("Saving error chats...");
                var keys = _errorChats.Keys.ToArray();
                keys.SerializeToJsonFile(pFileName);
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
            public string DataFolder { get; set; } = "./botdata";
        }
    }
}