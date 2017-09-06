﻿/*
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
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace TWCore.Bot.Telegram
{
    /// <inheritdoc />
    /// <summary>
    /// Telegram bot chat specific
    /// </summary>
    public class TelegramBotChat : BotChat
    {
        /// <summary>
        /// Chat title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Chat username
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Chat first name
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Chat last name
        /// </summary>
        public string LastName { get; set; }
    }
}
