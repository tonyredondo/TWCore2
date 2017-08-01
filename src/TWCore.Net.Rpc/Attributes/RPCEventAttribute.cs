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

using System;

namespace TWCore.Net.RPC.Attributes
{
    /// <summary>
    /// Define the event as a RPC Event so a handler is associated in order to sent the event triggering to the client.
    /// </summary>
    [AttributeUsage(AttributeTargets.Event)]
    public sealed class RPCEventAttribute : Attribute
    {
        /// <summary>
        /// Fire triggering message scope
        /// </summary>
        public RPCMessageScope Scope { get; set; }
        /// <summary>
        /// In case of scope sets to Hub, the Comma separated Hub name list to fire the event
        /// </summary>
        public string HubName { get; set; }

        /// <summary>
        /// Define the event as a RPC Event so a handler is associated in order to sent the event triggering to the client.
        /// </summary>
        /// <param name="scope">RPC Message scope for the event</param>
        public RPCEventAttribute(RPCMessageScope scope)
        {
            Scope = scope;
        }
    }
}
