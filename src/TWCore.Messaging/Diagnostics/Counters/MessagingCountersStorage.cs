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
using TWCore.Diagnostics.Status;
using TWCore.Messaging.RawClient;
using TWCore.Services;
// ReSharper disable ImpureMethodCallOnReadonlyValueField
// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global


namespace TWCore.Diagnostics.Counters.Storages
{
    /// <inheritdoc />
    /// <summary>
    /// Messaging counters storage
    /// </summary>
    [StatusName("Messaging Counters")]
    public class MessagingCountersStorage : ICountersStorage
    {
        private IMQueueRawClient _queueClient;

        #region .ctor
        /// <summary>
        /// Messaging log storage
        /// </summary>
        /// <param name="queueName">Queue pair config name</param>
        public MessagingCountersStorage(string queueName)
        {
            _queueClient = Core.Services.GetQueueRawClient(queueName, true);
        }
        /// <summary>
        /// Messaging log storage
        /// </summary>
        /// <param name="queueClient">Queue client</param>
        public MessagingCountersStorage(IMQueueRawClient queueClient)
        {
            _queueClient = queueClient;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Store counters 
        /// </summary>
        /// <param name="counterItems">Counters items enumerables</param>
        public void Store(List<ICounterItem> counterItems)
        {
            if (_queueClient == null) return;
            try
            {
                _queueClient.SendAsync(counterItems);
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _queueClient?.Dispose();
            _queueClient = null;
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
