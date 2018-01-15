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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Messaging.Client;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Exceptions;

// ReSharper disable NotAccessedField.Local
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Messaging
{
    /// <inheritdoc />
    /// <summary>
    /// Memory Queue Client
    /// </summary>
    public class MemoryQueueClient : MQueueClientBase
    {
        #region Fields
        private MemoryQueue _receiver;
        private MQClientQueues _clientQueues;
        private MQClientSenderOptions _senderOptions;
        private MQClientReceiverOptions _receiverOptions;
        private int _receiverOptionsTimeout;
        private bool _cloneObject;
        #endregion

        #region Init and Dispose Methods
        /// <inheritdoc />
        /// <summary>
        /// On client initialization
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnInit()
        {
            _receiver = null;

            if (Config != null)
            {
                if (Config.ClientQueues != null)
                {
                    _clientQueues = Config.ClientQueues.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true && c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true)
                                    ?? Config.ClientQueues.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true)
                                    ?? Config.ClientQueues.FirstOrDefault(c => c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true)
                                    ?? Config.ClientQueues.FirstOrDefault(c => c.EnvironmentName.IsNullOrWhitespace());
                }
                _senderOptions = Config.RequestOptions?.ClientSenderOptions;
                _receiverOptions = Config.ResponseOptions?.ClientReceiverOptions;
                _receiverOptionsTimeout = _receiverOptions?.TimeoutInSec ?? 20;

                if (_receiverOptions?.Parameters?.Contains("Clone") == true)
                    _cloneObject = _receiverOptions.Parameters["Clone"].ParseTo(false);

                if (_clientQueues != null)
                {
                    if (_clientQueues.RecvQueue != null)
                        _receiver = MemoryQueueManager.GetQueue(_clientQueues.RecvQueue.Route, _clientQueues.RecvQueue.Name);
                    foreach (var sender in _clientQueues.SendQueues)
                        MemoryQueueManager.GetQueue(sender.Route, sender.Name);
                }
            }
            Core.Status.AttachObject(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// On Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnDispose()
        {
            _receiver = null;
        }
        #endregion

        #region Send Method
        /// <inheritdoc />
        /// <summary>
        /// On Send message data
        /// </summary>
        /// <param name="message">Request message instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override Task<bool> OnSendAsync(RequestMessage message)
        {
            if (_clientQueues.SendQueues?.Any() != true)
                throw new NullReferenceException("There aren't any senders queues.");
            if (_senderOptions == null)
                throw new ArgumentNullException("SenderOptions");

            if (message.Header.ResponseQueue == null)
            {
                var recvQueue = _clientQueues.RecvQueue;
                if (recvQueue != null)
                {
                    message.Header.ResponseQueue = new MQConnection(recvQueue.Route, recvQueue.Name) { Parameters = recvQueue.Parameters };
                    message.Header.ResponseExpected = true;
                    message.Header.ResponseTimeoutInSeconds = _receiverOptions?.TimeoutInSec ?? -1;
                }
                else
                {
                    message.Header.ResponseExpected = false;
                    message.Header.ResponseTimeoutInSeconds = -1;
                }
            }

            foreach (var sender in _clientQueues.SendQueues)
            {
                var sQueue = MemoryQueueManager.GetQueue(sender.Route, sender.Name);
                Core.Log.LibVerbose("Sending message to the memory Queue '{0}' with CorrelationId={1}", sender.Route + "/" + sender.Name, message.CorrelationId);
                sQueue.Enqueue(message.CorrelationId, message);
            }

            return Task.FromResult(true);
        }
        #endregion

        #region Receive Method
        /// <inheritdoc />
        /// <summary>
        /// On Receive message data
        /// </summary>
        /// <param name="correlationId">Correlation Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response message instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override Task<ResponseMessage> OnReceiveAsync(Guid correlationId, CancellationToken cancellationToken)
        {
            if (_receiver == null)
                throw new NullReferenceException("There is not receiver queue.");

            var sw = Stopwatch.StartNew();
            var message = _receiver.Dequeue(correlationId, _receiverOptionsTimeout * 1000, cancellationToken);
            if (message == null)
                throw new MessageQueueTimeoutException(TimeSpan.FromSeconds(_receiverOptionsTimeout), correlationId.ToString());
            if (message.Value == null)
                throw new MessageQueueNotFoundException("The Message can't be retrieved, null body on CorrelationId = " + correlationId);

            var response = message.Value as ResponseMessage;

            if (response?.Body != null && _cloneObject)
                response.Body = response.Body.DeepClone();

            Core.Log.LibVerbose("Received message from the memory Queue '{0}' with CorrelationId={1} received at: {2}ms", _clientQueues.RecvQueue.Name, correlationId, sw.Elapsed.TotalMilliseconds);
            return Task.FromResult(response);
        }
        #endregion
    }
}