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
using TWCore.Diagnostics.Counters;
using TWCore.Diagnostics.Counters.Storages;
using TWCore.Diagnostics.Status;
using TWCore.Messaging.Client;
using TWCore.Services;
// ReSharper disable ImpureMethodCallOnReadonlyValueField
// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global


namespace TWCore.Messaging.Diagnostics.Counters.Storages
{
    /// <inheritdoc />
    /// <summary>
    /// Messaging counters storage
    /// </summary>
    [StatusName("Messaging Counters")]
    public class MessagingCountersStorage : ICountersStorage
    {
        private IMQueueClient _queueClient;

        #region .ctor
        /// <summary>
        /// Messaging log storage
        /// </summary>
        /// <param name="queueName">Queue pair config name</param>
        public MessagingCountersStorage(string queueName)
        {
            _queueClient = Core.Services.GetQueueClient(queueName);
        }
        /// <summary>
        /// Messaging log storage finalizer
        /// </summary>
        ~MessagingCountersStorage()
        {
            Dispose();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Store counters 
        /// </summary>
        /// <param name="counterItems">Counters items enumerables</param>
        public void Store(IEnumerable<ICounterItem> counterItems)
        {
            if (_queueClient == null) return;
            try
            {
                var cItems = counterItems.ToList();
                _queueClient.SendAsync(cItems);
            }
            catch(Exception ex)
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
