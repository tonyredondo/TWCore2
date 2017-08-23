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
using System.Diagnostics;
using System.Threading;
using TWCore.Messaging;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Server;

namespace TWCore.Services.Messaging
{
    /// <summary>
    /// Business message processor
    /// </summary>
    public class BusinessMessageProcessor : IMessageProcessor
    {
        static BusinessMessageProcessorSettings settings = Core.GetSettings<BusinessMessageProcessorSettings>();
        ObjectPool<IBusiness> BusinessPool;
        Func<IBusiness> creationFunction;
        int maxMessagesPerQueue;

        #region Static Properties
        /// <summary>
        /// Business item initial count in percent of the total maximum items.
        /// </summary>
        public static float InitialBusinessesPercent { get; set; } = settings.InitialBusinessesPercent;
        /// <summary>
        /// Business preallocation threshold in percent of the total maximum items.
        /// </summary>
        public static float BusinessesPreallocationPercent { get; set; } = settings.BusinessesPreallocationPercent;
        #endregion

        #region Properties
        /// <summary>
        /// Business item initial count
        /// </summary>
        public int BusinessInitialCount { get; private set; }
        /// <summary>
        /// Business preallocation threshold. 
        /// Number of items limit to create new allocations in another Task. Use 0 to disable, if is greater than the initial buffer then the initial buffer is set twice at this value.
        /// </summary>
        public int BusinessPreallocationThreshold { get; private set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Business message processor
        /// </summary>
        /// <param name="pairConfig">Queue server config</param>
        /// <param name="businessCreationFunction">Business item creation function</param>
        public BusinessMessageProcessor(MQPairConfig pairConfig, Func<IBusiness> businessCreationFunction)
        {
            if (pairConfig == null) throw new ArgumentNullException(nameof(pairConfig), "The MQPairConfig can't be null");
            if (businessCreationFunction == null) throw new ArgumentNullException(nameof(businessCreationFunction), "The bussines creation function can't be null");
            creationFunction = businessCreationFunction;
            maxMessagesPerQueue = pairConfig.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue;
            var serverCount = pairConfig.ServerQueues.Count;
            BusinessInitialCount = (int)((maxMessagesPerQueue * serverCount) * InitialBusinessesPercent) + 1; //We start with the 30% of the total businesses
            BusinessPreallocationThreshold = (int)((maxMessagesPerQueue * serverCount) * BusinessesPreallocationPercent) + 1; //The threshold to create new business is in 10%
            AttachStatus();
        }
        /// <summary>
        /// Business message processor
        /// </summary>
        /// <param name="server">Queue server</param>
        /// <param name="businessCreationFunction">Business item creation function</param>
        public BusinessMessageProcessor(IMQueueServer server, Func<IBusiness> businessCreationFunction)
        {
            if (server == null) throw new ArgumentNullException(nameof(server), "The IMQueueServer can't be null");
            if (businessCreationFunction == null) throw new ArgumentNullException(nameof(businessCreationFunction), "The bussines creation function can't be null");
            creationFunction = businessCreationFunction;
            maxMessagesPerQueue = server.Config.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue;
            var serverCount = server.Config.ServerQueues.Count;
            BusinessInitialCount = (int)((maxMessagesPerQueue * serverCount) * InitialBusinessesPercent) + 1; //We start with the 30% of the total businesses
            BusinessPreallocationThreshold = (int)((maxMessagesPerQueue * serverCount) * BusinessesPreallocationPercent) + 1; //The threshold to create new business is in 10%
            AttachStatus();
        }
        /// <summary>
        /// Business message processor
        /// </summary>
        /// <param name="businessInitialCount">Business item initial count</param>
        /// <param name="businessPreallocationThreshold">Number of items limit to create new allocations in another Task. Use 0 to disable, if is greater than the initial buffer then the initial buffer is set twice at this value.</param>
        /// <param name="businessCreationFunction">Business item creation function</param>
        public BusinessMessageProcessor(int businessInitialCount, int businessPreallocationThreshold, Func<IBusiness> businessCreationFunction)
        {
            if (businessCreationFunction == null) throw new ArgumentNullException(nameof(businessCreationFunction), "The bussines creation function can't be null");
            creationFunction = businessCreationFunction;
            BusinessInitialCount = businessInitialCount;
            BusinessPreallocationThreshold = businessPreallocationThreshold;
            AttachStatus();
        }
        void AttachStatus()
        {
            Core.Status.Attach(collection =>
            {
                collection.Add("Maximum businesses per queue", maxMessagesPerQueue);
                collection.Add("Business item initial count in percent of the total maximum items", InitialBusinessesPercent);
                collection.Add("Business preallocation threshold in percent of the total maximum items", BusinessesPreallocationPercent);
                collection.Add("Business initial count", BusinessInitialCount);
                collection.Add("Business preallocation threshold", BusinessPreallocationThreshold);
                collection.Add("Available business on the pool", BusinessPool?.Count);
            });
        }
        #endregion

        #region IMessageProcessor Methods
        /// <summary>
        /// Initialize message processor
        /// </summary>
        public void Init()
        {
            Core.Log.LibDebug("Initializing message processor...");
            Core.Log.LibDebug("Business item initial count in percent of the total maximum items = {0}", InitialBusinessesPercent);
            Core.Log.LibDebug("Business preallocation threshold in percent of the total maximum items = {0}", BusinessesPreallocationPercent);
            Core.Log.LibDebug("Maximum businesses per queue = {0}", maxMessagesPerQueue);
            Core.Log.LibDebug("Business Initial Count = {0}", BusinessInitialCount);
            Core.Log.LibDebug("Business Preallocation Threshold = {0}", BusinessPreallocationThreshold);
            Dispose();
            BusinessPool = new ObjectPool<IBusiness>(pool =>
            {
                var item = creationFunction();
                if (item == null)
                    throw new NullReferenceException("The business creation function returns a null IBusiness value. Please check the creation function.");
                item.Init();
                return item;
            }, null, BusinessInitialCount, PoolResetMode.AfterUse, BusinessPreallocationThreshold);
            Core.Log.LibDebug("Message processor initialized");
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
            var sw = Stopwatch.StartNew();
            var item = BusinessPool.New();
            object response = ResponseMessage.NoResponse;
            try
            {
                response = item.Process(message, cancellationToken);
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            BusinessPool.Store(item);
            Core.Log.LibDebug("Message processed.");
            return response;
        }
        /// <summary>
        /// Dispose all resources
        /// </summary>
        public void Dispose()
        {
            if (BusinessPool != null)
            {
                var businesses = BusinessPool.GetCurrentObjects();
                foreach (var item in businesses)
                    item.Dispose();
                BusinessPool.Clear();
            }
        }
        #endregion
    }
}
