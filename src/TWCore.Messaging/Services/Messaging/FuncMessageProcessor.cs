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
    /// Process messages using different Func delegates for each message type
    /// </summary>
    public class FuncMessageProcessor : IMessageProcessor
    {
        /// <summary>
        /// Registered func
        /// </summary>
        public Dictionary<Type, Func<object, CancellationToken, object>> Funcs { get; } = new Dictionary<Type, Func<object, CancellationToken, object>>();

        #region .ctor
        /// <summary>
        /// Process messages using different Func delegates for each message type
        /// </summary>
        public FuncMessageProcessor()
        {
            Core.Status.Attach(collection =>
            {
                collection.Add("Message Types", Funcs.Keys.Select(k => k.Name).Join(", "));
            });
        }
        #endregion

        #region Registers
        /// <summary>
        /// Register a new func for a message type
        /// </summary>
        /// <typeparam name="T">Type of the message</typeparam>
        /// <param name="func">Func to process the message</param>
        public void RegisterFunc<T>(Func<T, object> func)
        {
            if (func == null) throw new NullReferenceException("You can't register a null Func");
            var messageType = typeof(T);
            var processor = new Func<object, CancellationToken, object>((obj, cancellation) => func((T)obj));
            Funcs[messageType] = processor;
        }
        /// <summary>
        /// Register a new func for a message type
        /// </summary>
        /// <typeparam name="T">Type of the message</typeparam>
        /// <typeparam name="R">Type of response</typeparam>
        /// <param name="func">Func to process the message</param>
        public void RegisterFunc<T, R>(Func<T, R> func)
        {
            if (func == null) throw new NullReferenceException("You can't register a null Func");
            var messageType = typeof(T);
            var processor = new Func<object, CancellationToken, object>((obj, cancellation) => func((T)obj));
            Funcs[messageType] = processor;
        }
        /// <summary>
        /// Register a new func for a message type
        /// </summary>
        /// <typeparam name="T">Type of the message</typeparam>
        /// <param name="func">Func to process the message</param>
        public void RegisterFunc<T>(Func<T, CancellationToken, object> func)
        {
            if (func == null) throw new NullReferenceException("You can't register a null Func");
            var messageType = typeof(T);
            var processor = new Func<object, CancellationToken, object>((obj, cancellation) => func((T)obj, cancellation));
            Funcs[messageType] = processor;
        }
        /// <summary>
        /// Register a new func for a message type
        /// </summary>
        /// <typeparam name="T">Type of the message</typeparam>
        /// <typeparam name="R">Type of response</typeparam>
        /// <param name="func">Func to process the message</param>
        public void RegisterFunc<T, R>(Func<T, CancellationToken, R> func)
        {
            if (func == null) throw new NullReferenceException("You can't register a null Func");
            var messageType = typeof(T);
            var processor = new Func<object, CancellationToken, object>((obj, cancellation) => func((T)obj, cancellation));
            Funcs[messageType] = processor;
        }
        #endregion

        /// <summary>
        /// Initialize message processor
        /// </summary>
        public void Init()
        {
        }
        /// <summary>
        /// Process a message using the registered funcs
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        public object Process(object message, CancellationToken cancellationToken)
        {
            Core.Log.LibDebug("Processing message...");
            Type msgType = message?.GetType() ?? Funcs.Keys.First();
            if (Funcs.TryGetValue(msgType, out var processor))
            {
                var response = processor(message, cancellationToken);
                Core.Log.LibDebug("Message processed.");
                return response;
            }
            else if (Funcs.TryGetValue(typeof(object), out processor))
            {
                var response = processor(message, cancellationToken);
                Core.Log.LibDebug("Message processed.");
                return response;
            }
            Core.Log.Warning("Message can't be processed because not Processor instance couldn't be found. Type = {0}", msgType);
            return ResponseMessage.NoResponse;
        }
        /// <summary>
        /// Dispose all resources
        /// </summary>
        public void Dispose()
        {
            Funcs.Clear();
        }
    }
}
