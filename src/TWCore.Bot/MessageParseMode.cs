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

namespace TWCore.Bot
{
    /// <summary>
    /// Message Parse Mode
    /// </summary>
    public enum MessageParseMode
    {
        /// <summary>
        /// Default message (only text)
        /// </summary>
        Default = 0,
        /// <summary>
        /// Markdown message
        /// </summary>
        Markdown = 1,
        /// <summary>
        /// Html message
        /// </summary>
        Html = 2
    }
}
