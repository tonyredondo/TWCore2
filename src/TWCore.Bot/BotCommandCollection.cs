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
using System.Runtime.CompilerServices;

namespace TWCore.Bot
{
    /// <summary>
    /// Collection of bot commands
    /// </summary>
    public class BotCommandCollection : List<BotCommand>
    {
        /// <summary>
        /// Adds a new bot command to the collection
        /// </summary>
        /// <param name="condition">Command condition</param>
        /// <param name="handler">Command handler action</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TextMessageHandlerCondition condition, TextMessageHandler handler)
            => Add(new BotCommand { Condition = condition, Handler = handler });
    }
}
