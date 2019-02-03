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


using System.Threading.Tasks;

namespace TWCore.Bot
{
    /// <summary>
    /// Delegate to define a handler condition
    /// </summary>
    /// <param name="message">Received text message</param>
    /// <returns>True if the handler is going to handle the message; otherwise false.</returns>
    public delegate bool TextMessageHandlerCondition(string message);
    /// <summary>
    /// Delegate to define the handler action for a message
    /// </summary>
    /// <param name="bot">Bot engine instance</param>
    /// <param name="message">Bot received text message</param>
    /// <returns>True if the handler is not going to allow other handlers to check the message; otherwise false.</returns>
    public delegate Task<bool> TextMessageHandlerAsync(IBotEngine bot, BotTextMessage message);

    /// <summary>
    /// Defines a command handler for bot incoming messages
    /// </summary>
    public class BotCommand
    {
        /// <summary>
        /// Command condition
        /// </summary>
        public TextMessageHandlerCondition Condition;
        /// <summary>
        /// Command handler action
        /// </summary>
        public TextMessageHandlerAsync Handler;
    }
}
