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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TWCore.Messaging;

namespace TWCore.Services.Messaging
{
    /// <summary>
    /// Process messages using different Action delegates for each message type
    /// </summary>
    public class ActionMessageProcessor : IMessageProcessor
    {
        /// <summary>
        /// Registered actions
        /// </summary>
        public Dictionary<Type, Action<object, CancellationToken>> Actions { get; } = new Dictionary<Type, Action<object, CancellationToken>>();

        #region .ctor
        /// <summary>
        /// Process messages using different Action delegates for each message type
        /// </summary>
        public ActionMessageProcessor()
        {
            Core.Status.Attach(collection =>
            {
                collection.Add("Message Types", Actions.Keys.Select(k => k.Name).Join(", "));
            });
        }
        #endregion

        #region Register Actions
        /// <summary>
        /// Register a new action for a message type
        /// </summary>
        /// <typeparam name="T">Type of the message</typeparam>
        /// <param name="action">Action to process the message</param>
        public void RegisterAction<T>(Action<T> action)
        {
            if (action == null) throw new NullReferenceException("You can't register a null Action");
            var messageType = typeof(T);
            var processor = new Action<object, CancellationToken>((obj, cancellation) => action((T)obj));
            Actions[messageType] = processor;
        }

        /// <summary>
        /// Register a new action for a message type
        /// </summary>
        /// <typeparam name="T">Type of the message</typeparam>
        /// <param name="action">Action to process the message</param>
        public void RegisterAction<T>(Action<T, CancellationToken> action)
        {
            if (action == null) throw new NullReferenceException("You can't register a null Action");
            var messageType = typeof(T);
            var processor = new Action<object, CancellationToken>((obj, cancellation) => action((T)obj, cancellation));
            Actions[messageType] = processor;
        }
        #endregion

        /// <summary>
        /// Initialize message processor
        /// </summary>
        public void Init()
        {
        }
        /// <summary>
        /// Process a message using the registered actions
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        public object Process(object message, CancellationToken cancellationToken)
        {
            Core.Log.LibDebug("Processing message...");
            Type msgType = message?.GetType() ?? Actions.Keys.First();
            if (Actions.TryGetValue(msgType, out var processor))
            {
                processor(message, cancellationToken);
                Core.Log.LibDebug("Message processed.");
            }
            else if (Actions.TryGetValue(typeof(object), out processor))
            {
                processor(message, cancellationToken);
                Core.Log.LibDebug("Message processed.");
            }
            else
                Core.Log.Warning("Message can't be processed because not Processor instance couldn't be found. Type = {0}", msgType);
            return ResponseMessage.NoResponse;
        }
        /// <summary>
        /// Dispose all resources
        /// </summary>
        public void Dispose()
        {
            Actions.Clear();
        }
    }
}
