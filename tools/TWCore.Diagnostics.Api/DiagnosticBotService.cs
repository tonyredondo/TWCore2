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
        private static readonly JsonTextSerializer JsonSerializer = new JsonTextSerializer
        {
            Indent = true,
            EnumsAsStrings = true,
            UseCamelCase = true
        };
        private static BotSettings Settings = Core.GetSettings<BotSettings>();
        const string BotConfigFile = "botconfig.json";
        const string UrlPathPattern = "/#/diagnostics/search?env={0}&term={1}";

        private BotEngine _engine;
        private string _currentEnvironment;
        private string _currentDiagnosticsUrl;
        private ConcurrentDictionary<string, BotChat> _errorChats;
        private ConcurrentDictionary<Guid, BotCounterAlerts> _counterAlerts;
        private ConcurrentDictionary<(string, string, string, string, CounterType, CounterUnit), BotCounterAlerts> _counterAlertsComparer;
        private ConcurrentDictionary<(string, string, string), ErrorsRegister> _errorsRegistry;

        protected override IBotEngine GetBotEngine()
        {
            var slackTransport = new SlackBotTransport(Settings.SlackToken);
            _engine = new BotEngine(slackTransport);
            _engine.OnConnected += OnConnected;
            _engine.OnDisconnected += OnDisconnected;
            _currentEnvironment = Settings.DefaultEnvironment;
            _currentDiagnosticsUrl = Settings.DefaultDiagnosticsUrl;
            _errorChats = new ConcurrentDictionary<string, BotChat>();
            _counterAlerts = new ConcurrentDictionary<Guid, BotCounterAlerts>();
            _counterAlertsComparer = new ConcurrentDictionary<(string, string, string, string, CounterType, CounterUnit), BotCounterAlerts>();
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
                    SaveBotConfig();
                    await bot.SendTextMessageAsync(message.Chat, "`Environment setted to: " + _currentEnvironment + "`").ConfigureAwait(false);
                }
                else
                {
                    await bot.SendTextMessageAsync(message.Chat, ":exclamation: `Invalid syntax.`").ConfigureAwait(false);
                }
                return true;
            });

            //*****************************************************************************************************************************************

            _engine.Commands.Add(msg => msg.Equals(".geturl", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                await bot.SendTextMessageAsync(message.Chat, "`The current diagnostics url is: " + _currentDiagnosticsUrl + "`").ConfigureAwait(false);
                return true;
            });
            _engine.Commands.Add(msg => msg.StartsWith(".seturl", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                var arrText = message.Text.SplitAndTrim(" ");
                if (arrText.Length == 2)
                {
                    _currentDiagnosticsUrl = arrText[1].Replace("<", string.Empty).Replace(">", string.Empty);
                    SaveBotConfig();
                    await bot.SendTextMessageAsync(message.Chat, "`Diagnostics url setted to: " + _currentDiagnosticsUrl + "`").ConfigureAwait(false);
                }
                else
                {
                    await bot.SendTextMessageAsync(message.Chat, ":exclamation: `Invalid syntax.`").ConfigureAwait(false);
                }
                return true;
            });
            _engine.Commands.Add(msg => msg.StartsWith(".getlink", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                var arrText = message.Text.SplitAndTrim(" ");
                if (arrText.Length == 2)
                {
                    var group = arrText[1];
                    await bot.SendTextMessageAsync(message.Chat, $"*<{_currentDiagnosticsUrl + string.Format(UrlPathPattern, _currentEnvironment, group)}|View Transaction>*").ConfigureAwait(false);
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
                    SaveBotConfig();
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
                    SaveBotConfig();
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

            _engine.Commands.Add(msg => msg.StartsWith(".addcounteralert", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                var arrText = message.Text.SplitAndTrim(" ");
                if (arrText.Length == 2 && Guid.TryParse(arrText[1], out var counterId))
                {
                    var counterItem = await DbHandlers.Instance.Query.GetCounter(counterId).ConfigureAwait(false);
                    if (counterItem != null)
                    {
                        var cAlert = _counterAlerts.GetOrAdd(counterId, _ =>
                        {
                            var counter = new BotCounterAlerts
                            {
                                Environment = _currentEnvironment,
                                Application = counterItem.Application,
                                Category = counterItem.Category,
                                Name = counterItem.Name,
                                Type = counterItem.Type,
                                Unit = counterItem.Unit
                            };
                            _counterAlertsComparer.TryAdd((counter.Environment, counter.Application, counter.Category, counter.Name, counter.Type, counter.Unit), counter);
                            return counter;
                        });
                        lock (cAlert.Chats)
                        {
                            if (!cAlert.Chats.Contains(message.Chat.Id))
                            {
                                cAlert.Chats.Add(message.Chat.Id);
                                _ = bot.SendTextMessageAsync(message.Chat, $"`This chat is tracking changes on counter: {counterItem.CountersId} | {counterItem.Application}\\{counterItem.Category}\\{counterItem.Name} [{counterItem.Type}]`");
                            }
                            else
                            {
                                _ = bot.SendTextMessageAsync(message.Chat, $"`This chat is already tracking changes on counter: {counterItem.CountersId} | {counterItem.Application}\\{counterItem.Category}\\{counterItem.Name} [{counterItem.Type}]`");
                            }
                        }
                        SaveBotConfig();
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(message.Chat, ":exclamation: `Counter not found.`").ConfigureAwait(false);
                    }
                }
                else
                {
                    await bot.SendTextMessageAsync(message.Chat, ":exclamation: `Invalid syntax.`").ConfigureAwait(false);
                }
                return true;
            });
            _engine.Commands.Add(msg => msg.StartsWith(".removecounteralert", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                var arrText = message.Text.SplitAndTrim(" ");
                if (arrText.Length == 2 && Guid.TryParse(arrText[1], out var counterId))
                {
                    var counterItem = await DbHandlers.Instance.Query.GetCounter(counterId).ConfigureAwait(false);
                    if (counterItem != null)
                    {
                        var cAlert = _counterAlerts.GetOrAdd(counterId, _ =>
                        {
                            var counter = new BotCounterAlerts
                            {
                                Environment = _currentEnvironment,
                                Application = counterItem.Application,
                                Category = counterItem.Category,
                                Name = counterItem.Name,
                                Type = counterItem.Type,
                                Unit = counterItem.Unit
                            };
                            _counterAlertsComparer.TryAdd((counter.Environment, counter.Application, counter.Category, counter.Name, counter.Type, counter.Unit), counter);
                            return counter;
                        });
                        lock (cAlert.Chats)
                        {
                            if (cAlert.Chats.Remove(message.Chat.Id))
                            {
                                _ = bot.SendTextMessageAsync(message.Chat, $"`Tracking changes removed on counter: {counterItem.CountersId} | {counterItem.Application}\\{counterItem.Category}\\{counterItem.Name} [{counterItem.Type}]`");
                            }
                        }
                        SaveBotConfig();
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(message.Chat, ":exclamation: `Counter not found.`").ConfigureAwait(false);
                    }
                }
                else
                {
                    await bot.SendTextMessageAsync(message.Chat, ":exclamation: `Invalid syntax.`").ConfigureAwait(false);
                }
                return true;
            });
            _engine.Commands.Add(msg => msg.StartsWith(".setcounteralertmessage", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                var arrText = message.Text.SplitAndTrim(" ");
                if (Guid.TryParse(arrText[1], out var counterId))
                {
                    var counterItem = await DbHandlers.Instance.Query.GetCounter(counterId).ConfigureAwait(false);
                    if (counterItem != null)
                    {
                        var cAlert = _counterAlerts.GetOrAdd(counterId, _ =>
                        {
                            var counter = new BotCounterAlerts
                            {
                                Environment = _currentEnvironment,
                                Application = counterItem.Application,
                                Category = counterItem.Category,
                                Name = counterItem.Name,
                                Type = counterItem.Type,
                                Unit = counterItem.Unit
                            };
                            _counterAlertsComparer.TryAdd((counter.Environment, counter.Application, counter.Category, counter.Name, counter.Type, counter.Unit), counter);
                            return counter;
                        });
                        cAlert.Message = arrText.Skip(2).Join(" ");
                        await bot.SendTextMessageAsync(message.Chat, $"`Alert message on counter: {counterItem.CountersId} | {counterItem.Application}\\{counterItem.Category}\\{counterItem.Name} [{counterItem.Type}] was setted to: {cAlert.Message}`").ConfigureAwait(false);
                        SaveBotConfig();
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(message.Chat, ":exclamation: `Counter not found.`").ConfigureAwait(false);
                    }
                }
                else
                {
                    await bot.SendTextMessageAsync(message.Chat, ":exclamation: `Invalid syntax.`").ConfigureAwait(false);
                }
                return true;
            });
            _engine.Commands.Add(msg => msg.Equals(".listcountersalerts", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                if (_counterAlerts.Count > 0)
                {
                    string buffer = $"```Counters alerts:\n";
                    foreach (var cAlert in _counterAlerts)
                    {
                        if (cAlert.Value.Chats.Contains(message.Chat.Id))
                        {
                            var counterItem = await DbHandlers.Instance.Query.GetCounter(cAlert.Key).ConfigureAwait(false);
                            buffer += $"{counterItem.CountersId} | {counterItem.Application}\\{counterItem.Category}\\{counterItem.Name} [{counterItem.Type}] => {cAlert.Value.Message}\n";
                        }
                    }
                    buffer += "```";
                    await bot.SendTextMessageAsync(message.Chat, buffer).ConfigureAwait(false);
                }
                else
                {
                    await bot.SendTextMessageAsync(message.Chat, ":exclamation: `There are not counters alerts.`").ConfigureAwait(false);
                }
                return true;
            });

            //*****************************************************************************************************************************************

            _engine.Commands.Add(msg => msg.Equals(".help", StringComparison.OrdinalIgnoreCase), async (bot, message) =>
            {
                var helpMsg = $"`Available commands:`\n";
                helpMsg += ">>>```.getenv : Get current environment.\n";
                helpMsg += ".setenv : Set current environment.\n";
                helpMsg += ".geturl : Gets the current diagnostics url.\n";
                helpMsg += ".seturl : Sets the current diagnostics url.\n";
                helpMsg += ".trackerrors : Sets the current chat to tracking errors.\n";
                helpMsg += ".untrackerrors : Remove the current chat to the tracking errors list.\n";
                helpMsg += ".getcounters : Get available counters.\n";
                helpMsg += ".getcountervalue : Get values for a counter.\n";
                helpMsg += ".addcounteralert : Adds this chat to a counter change alert.\n";
                helpMsg += ".removecounteralert : Removes this chat from a counter change alert.\n";
                helpMsg += ".listcountersalerts : List counters alerts for this chat.\n";
                helpMsg += ".setcounteralertmessage : Sets a counter change alert message.\n```";
                await bot.SendTextMessageAsync(message.Chat, helpMsg).ConfigureAwait(false);
                return true;
            });

            //*****************************************************************************************************************************************
            DbHandlers.Instance.ErrorLogMessage += Instance_ErrorLogMessage;
            DbHandlers.Instance.CounterReceived += Instance_CounterReceived;
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
            if (!LoadBotConfig())
                SaveBotConfig();
        }
        private void OnDisconnected(object sender, EventArgs e)
        {
            SaveBotConfig();
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
                if (!string.IsNullOrEmpty(e.GroupName))
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
                if (!string.IsNullOrEmpty(e.GroupName) && !string.IsNullOrWhiteSpace(_currentDiagnosticsUrl))
                {
                    message += $"\n*<{_currentDiagnosticsUrl + string.Format(UrlPathPattern, e.EnvironmentName, e.GroupName)}|View Transaction>*";
                }
                foreach (var chat in _errorChats.Values)
                {
                    _ = _engine.SendTextMessageAsync(chat, message);
                }
            }
        }
        private void Instance_CounterReceived(object sender, ICounterItem e)
        {
            if (_counterAlertsComparer.TryGetValue((e.Environment, e.Application, e.Category, e.Name, e.Type, e.Unit), out var alert))
            {
                lock(alert.Chats)
                {
                    foreach(var chat in alert.Chats)
                        _ = _engine.SendTextMessageAsync(new BotChat { Id = chat }, "`" + (alert.Message ?? $"{alert.Application}\\{alert.Category}\\{alert.Name} [{alert.Type}] has a new value.") + "`");
                }
            }
        }
        #endregion

        #region Load & Save Config File.
        private bool LoadBotConfig()
        {
            try
            {
                if (!Directory.Exists(Settings.DataFolder))
                    Directory.CreateDirectory(Settings.DataFolder);

                var pFileName = Path.Combine(Settings.DataFolder, BotConfigFile);
                if (File.Exists(pFileName))
                {
                    Core.Log.InfoBasic("Loading bot config...");

                    var botConfig = JsonSerializer.DeserializeFromFile<BotConfig>(pFileName);
                    if (botConfig != null)
                    {
                        //Environment
                        if (!string.IsNullOrEmpty(botConfig.Environment))
                            _currentEnvironment = botConfig.Environment;

                        //Diagnostics url
                        if (!string.IsNullOrEmpty(botConfig.DiagnosticsUrl))
                            _currentDiagnosticsUrl = botConfig.DiagnosticsUrl;

                        //Error Chats
                        if (botConfig.ErrorChats != null)
                        {
                            foreach (var key in botConfig.ErrorChats)
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
                        }

                        //Counter Warnings
                        if (botConfig.CounterAlerts != null)
                        {
                            foreach (var item in botConfig.CounterAlerts)
                            {
                                var counter = item.Value;
                                _counterAlerts.TryAdd(item.Key, counter);
                                _counterAlertsComparer.TryAdd((counter.Environment, counter.Application, counter.Category, counter.Name, counter.Type, counter.Unit), counter);
                            }
                        }

                        return true;
                    }
                    else
                        return false;

                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            finally
            {
                Core.Log.InfoBasic("Bot config loaded.");
            }
            return false;
        }
        private void SaveBotConfig()
        {
            try
            {
                if (!Directory.Exists(Settings.DataFolder))
                    Directory.CreateDirectory(Settings.DataFolder);
                var pFileName = Path.Combine(Settings.DataFolder, BotConfigFile);
                Core.Log.InfoBasic("Saving Bot config...");

                var botConfig = new BotConfig
                {
                    Environment = _currentEnvironment,
                    DiagnosticsUrl = _currentDiagnosticsUrl,
                    ErrorChats = _errorChats.Keys.ToList(),
                    CounterAlerts = _counterAlerts.MapTo(cd =>
                    {
                        var dictio = new Dictionary<Guid, BotCounterAlerts>();
                        foreach (var item in cd)
                            dictio[item.Key] = item.Value;
                        return dictio;
                    })
                };
                JsonSerializer.SerializeToFile(botConfig, pFileName);
                Core.Log.InfoBasic("Bot config saved.");
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
            public string DefaultDiagnosticsUrl { get; set; }
            public string DataFolder { get; set; } = "./botdata";
        }

        //Config file
        public class BotConfig
        {
            public string Environment { get; set; }
            public string DiagnosticsUrl { get; set; }
            public List<string> ErrorChats { get; set; } = new List<string>();
            public Dictionary<Guid, BotCounterAlerts> CounterAlerts { get; set; } = new Dictionary<Guid, BotCounterAlerts>();
        }
        public class BotCounterAlerts
        {
            public List<string> Chats { get; set; } = new List<string>();
            public string Message { get; set; }
            public string Environment { get; set; }
            public string Application { get; set; }
            public string Category { get; set; }
            public string Name { get; set; }
            public CounterType Type { get; set; }
            public CounterUnit Unit { get; set; }
        }
    }
}