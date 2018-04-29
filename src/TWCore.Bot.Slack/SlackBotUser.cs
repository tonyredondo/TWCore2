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
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace TWCore.Bot.Slack
{
    /// <inheritdoc />
    /// <summary>
    /// Slack bot user
    /// </summary>
    public class SlackBotUser : BotUser
    {
        /// <summary>
        /// User color
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// User presence
        /// </summary>
        public string Presence { get; set; }
        /// <summary>
        /// User email
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// User firstname
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// User lastname
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// User real name
        /// </summary>
        public string RealName { get; set; }
        /// <summary>
        /// User skype contact
        /// </summary>
        public string Skype { get; set; }
        /// <summary>
        /// User phone
        /// </summary>
        public string Phone { get; set; }
    }
}