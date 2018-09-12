﻿/*
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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Messaging;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Server;

// ReSharper disable CheckNamespace

namespace TWCore.Services.Messaging
{
    /// <inheritdoc />
    /// <summary>
    /// Business message processor
    /// </summary>
    [StatusName("Processor")]
    public class BusinessMessageProcessorAsync : IMessageProcessorAsync
    {
        private static readonly BusinessMessageProcessorSettings Settings = Core.GetSettings<BusinessMessageProcessorSettings>();
        private ObjectPool<IBusinessAsync> _businessPool;
        private readonly Func<IBusinessAsync> _creationFunction;
        private readonly int _maxMessages;
        private readonly int _maxMessagesPerQueue;

        #region Static Properties
        /// <summary>
        /// Business item initial count in percent of the total maximum items.
        /// </summary>
        public static float InitialBusinessesPercent { get; set; } = Settings.InitialBusinessesPercent;
        #endregion

        #region Properties
        /// <summary>
        /// Business item initial count
        /// </summary>
        public int BusinessInitialCount { get; private set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Business message processor
        /// </summary>
        /// <param name="pairConfig">Queue server config</param>
        /// <param name="businessCreationFunction">Business item creation function</param>
        public BusinessMessageProcessorAsync(MQPairConfig pairConfig, Func<IBusinessAsync> businessCreationFunction)
        {
            _creationFunction = businessCreationFunction ?? 
                throw new ArgumentNullException(nameof(businessCreationFunction), "The bussines creation function can't be null");
            _maxMessagesPerQueue = pairConfig?.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue ?? 
                throw new ArgumentNullException(nameof(pairConfig), "The MQPairConfig can't be null");
            var serverQueues = pairConfig.ServerQueues?.FirstOf(
                c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true && c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true,
                c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true,
                c => c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true,
                c => c.EnvironmentName.IsNullOrWhitespace());

            _maxMessages = (serverQueues?.RecvQueues?.Count ?? 0) * _maxMessagesPerQueue;
            BusinessInitialCount = (int)(_maxMessages * InitialBusinessesPercent) + 1; AttachStatus();
        }
        /// <summary>
        /// Business message processor
        /// </summary>
        /// <param name="server">Queue server</param>
        /// <param name="businessCreationFunction">Business item creation function</param>
        public BusinessMessageProcessorAsync(IMQueueServer server, Func<IBusinessAsync> businessCreationFunction)
        {
            _creationFunction = businessCreationFunction ?? 
                throw new ArgumentNullException(nameof(businessCreationFunction), "The bussines creation function can't be null");
            _maxMessagesPerQueue = server?.Config.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue ?? 
                throw new ArgumentNullException(nameof(server), "The IMQueueServer can't be null");
            var serverQueues = server.Config.ServerQueues?.FirstOf(
                c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true && c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true,
                c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true,
                c => c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true,
                c => c.EnvironmentName.IsNullOrWhitespace());

            _maxMessages = (serverQueues?.RecvQueues?.Count ?? 0) * _maxMessagesPerQueue;
            BusinessInitialCount = (int)(_maxMessages * InitialBusinessesPercent) + 1;
            AttachStatus();
        }
        /// <summary>
        /// Business message processor
        /// </summary>
        /// <param name="businessInitialCount">Business item initial count</param>
        /// <param name="businessCreationFunction">Business item creation function</param>
        public BusinessMessageProcessorAsync(int businessInitialCount, Func<IBusinessAsync> businessCreationFunction)
        {
            _creationFunction = businessCreationFunction ?? 
                throw new ArgumentNullException(nameof(businessCreationFunction), "The bussines creation function can't be null");
            BusinessInitialCount = businessInitialCount;
            AttachStatus();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AttachStatus()
        {
            Core.Status.Attach(collection =>
            {
                collection.Add("Type", "Business");
                collection.Add("Maximum businesses per queue", _maxMessagesPerQueue);
                collection.Add("Maximum total businesses", _maxMessages);
                collection.Add("Business item initial count in percent of the total maximum items", InitialBusinessesPercent);
                collection.Add("Business initial count", BusinessInitialCount);
            });
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IBusinessAsync BusinessCreate(ObjectPool<IBusinessAsync> pool)
        {
            var item = _creationFunction();
            if (item is null)
                throw new NullReferenceException("The business creation function returns a null IBusiness value. Please check the creation function.");
            item.Init();
            return item;
        }

        #region IMessageProcessorAsync Methods
        /// <inheritdoc />
        /// <summary>
        /// Initialize message processor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            Core.Log.LibDebug("Initializing message processor...");
            Core.Log.LibDebug("Business item initial count in percent of the total maximum items = {0}", InitialBusinessesPercent);
            Core.Log.LibDebug("Maximum businesses per queue = {0}", _maxMessagesPerQueue);
            Core.Log.LibDebug("Maximum businesses = {0}", _maxMessages);
            Core.Log.LibDebug("Business Initial Count = {0}", BusinessInitialCount);
            Dispose();
            _businessPool = new ObjectPool<IBusinessAsync>(BusinessCreate, null, BusinessInitialCount);
            Core.Log.LibDebug("Message processor initialized");
        }
        /// <inheritdoc />
        /// <summary>
        /// Process a message using the registered funcs
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<object> ProcessAsync(object message, CancellationToken cancellationToken)
        {
            var item = _businessPool.New();
            var response = ResponseMessage.NoResponse;
            try
            {
                response = await item.ProcessAsync(message, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            _businessPool.Store(item);
            return response;
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_businessPool is null) return;
            var businesses = _businessPool.GetCurrentObjects();
            foreach (var item in businesses)
                item.Dispose();
            _businessPool.Clear();
        }
        #endregion
    }
}
