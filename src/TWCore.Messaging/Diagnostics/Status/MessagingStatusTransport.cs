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
using System.Threading;
using TWCore.Messaging.Client;
using TWCore.Services;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable CheckNamespace

namespace TWCore.Diagnostics.Status.Transports
{
    /// <inheritdoc />
    /// <summary>
    /// Messaging Status Transport
    /// </summary>
    public class MessagingStatusTransport : IStatusTransport
    {
        private readonly string _queueName;
        private readonly Timer _timer;
        
        #region Events
        /// <inheritdoc />
        /// <summary>
        /// Handles when a fetch status event has been received
        /// </summary>
        public event FetchStatusDelegate OnFetchStatus;
        #endregion

        #region .ctor
        /// <summary>
        /// Messaging status transport
        /// </summary>
        /// <param name="queueName">Queue pair config name</param>
        /// <param name="periodInSeconds">Fetch period in seconds</param>
        public MessagingStatusTransport(string queueName, int periodInSeconds)
        {
            _queueName = queueName;
            var period = TimeSpan.FromSeconds(periodInSeconds);
            _timer = new Timer(TimerCallback, this, period, period);
        }
        #endregion

        #region Private methods
        private static void TimerCallback(object state)
        {
            try
            {
                var mStatus = (MessagingStatusTransport) state;
                var statusData = mStatus.OnFetchStatus?.Invoke();
                if (statusData == null) return;
                Core.Log.LibDebug("Sending status data to the diagnostic queue.");
                var queueClient = Core.Services.GetQueueClient(mStatus._queueName);
                queueClient.SendAsync(statusData).WaitAndResults();
                queueClient.Dispose();
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
        #endregion
        
        #region Dispose
        /// <inheritdoc />
        /// <summary>
        /// Dispose the current object resources
        /// </summary>
        public void Dispose()
        {
            _timer.Dispose();
        }
        #endregion
    }
}