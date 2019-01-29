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
using System.Threading;
using TWCore.Messaging.RawClient;
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
    [StatusName("Messaging Status")]
    public class MessagingStatusTransport : IStatusTransport
    {
        private readonly string _queueName;
        private readonly Timer _timer;
        private int _processing = 0;
        private IMQueueRawClient _queueClient;
        private bool _enabled = true;

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
        /// <summary>
        /// Messaging status transport finalizer
        /// </summary>
        ~MessagingStatusTransport()
        {
            Dispose();
        }
        #endregion

        #region Private methods
        private void TimerCallback(object state)
        {
            if (!_enabled) return;
            if (Interlocked.CompareExchange(ref _processing, 1, 0) == 1) return;
            try
            {
                var statusData = OnFetchStatus?.Invoke();
                if (statusData is null)
                {
                    Interlocked.Exchange(ref _processing, 0);
                    return;
                }
                Core.Log.LibDebug("Sending status data to the diagnostic queue.");
                if (_queueClient is null)
                {
                    _queueClient = Core.Services.GetQueueRawClient(_queueName, true);
                    Core.Status.AttachChild(_queueClient, this);
                }
                _queueClient.SendAsync(statusData).WaitAndResults();
            }
            catch (UriFormatException fException)
            {
                Core.Log.Warning($"Disabling {nameof(MessagingStatusTransport)}. Reason: {fException.Message}");
                _enabled = false;
                _timer.Dispose();
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            Interlocked.Exchange(ref _processing, 0);
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
            _queueClient?.Dispose();
        }
        #endregion
    }
}