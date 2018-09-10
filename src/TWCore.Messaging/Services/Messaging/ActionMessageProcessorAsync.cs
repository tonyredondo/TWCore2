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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Messaging;
// ReSharper disable CheckNamespace

namespace TWCore.Services.Messaging
{
    /// <summary>
    /// Async action message delegate
    /// </summary>
    /// <typeparam name="T">Type of message to process</typeparam>
    /// <param name="message">Message to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async task</returns>
    public delegate Task ActionMessageAsyncDelegate<in T>(T message, CancellationToken cancellationToken);

    /// <inheritdoc />
    /// <summary>
    /// Process messages using different Action delegates for each message type
    /// </summary>
    [StatusName("Processor")]
    public class ActionMessageProcessorAsync : IMessageProcessorAsync
    {
        /// <summary>
        /// Registered actions
        /// </summary>
        public Dictionary<Type, ActionMessageAsyncDelegate<object>> Actions { get; } = new Dictionary<Type, ActionMessageAsyncDelegate<object>>();

        #region .ctor
        /// <summary>
        /// Process messages using different Action delegates for each message type
        /// </summary>
        public ActionMessageProcessorAsync()
        {
            Core.Status.Attach(collection =>
            {
                collection.Add("Type", "Action");
                collection.Add("Message Types", Actions.Keys.Select(k => k.Name).Join(", "));
            });
        }
        #endregion

        /// <summary>
        /// Register a new action for a message type
        /// </summary>
        /// <typeparam name="T">Type of the message</typeparam>
        /// <param name="action">Action to process the message</param>
        public void RegisterAction<T>(ActionMessageAsyncDelegate<T> action)
        {
            if (action == null) throw new NullReferenceException("You can't register a null Action");
            var messageType = typeof(T);
            var processor = new ActionMessageAsyncDelegate<object>((obj, cancellation) => action((T)obj, cancellation));
            Actions[messageType] = processor;
        }

        /// <inheritdoc />
        /// <summary>
        /// Initialize message processor
        /// </summary>
        public void Init()
        {
        }
        /// <inheritdoc />
        /// <summary>
        /// Process a message using the registered actions
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        public async Task<object> ProcessAsync(object message, CancellationToken cancellationToken)
        {
            Core.Log.LibDebug("Processing message...");
            var msgType = message?.GetType() ?? Actions.Keys.First();
            if (Actions.TryGetValue(msgType, out var processor))
            {
                await processor(message, cancellationToken).ConfigureAwait(false);
                Core.Log.LibDebug("Message processed.");
            }
            else if (Actions.TryGetValue(typeof(object), out processor))
            {
                await processor(message, cancellationToken).ConfigureAwait(false);
                Core.Log.LibDebug("Message processed.");
            }
            else
                Core.Log.Warning("Message can't be processed because not Processor instance couldn't be found. Type = {0}", msgType);
            return ResponseMessage.NoResponse;
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        public void Dispose()
        {
            Actions.Clear();
        }
    }
}
