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
using System.Linq;
using TWCore.Bot;
using TWCore.Bot.Slack;
using TWCore.Services;
// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Api
{
    public class DiagnosticBotService : BotService
    {
        private BotEngine _engine;
        private string _currentEnvironment;
        private ConcurrentDictionary<string, BotChat> _errorChats;

        protected override IBotEngine GetBotEngine()
        {
            //Core.DebugMode = true;
            var settings = Core.GetSettings<BotSettings>();
            var slackTransport = new SlackBotTransport(settings.SlackToken);
            _engine = new BotEngine(slackTransport);
            _currentEnvironment = settings.DefaultEnvironment;
            _errorChats = new ConcurrentDictionary<string, BotChat>();
            BindCommands();
            return _engine;
        }

        private void BindCommands()
        {
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
                lock(this)
                    added = _errorChats.TryAdd(message.Chat.Id, message.Chat);
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
                    removed = _errorChats.TryRemove(message.Chat.Id, out _);
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

        }

        private class BotSettings : Settings.SettingsBase
        {
            public string SlackToken { get; set; } = "ZX9YXvB6hTwTmurR4QguFgZZ-551556876045-732105390004-bxox".Reverse();
            public string DefaultEnvironment { get; set; } = "Docker";
        }
    }
}