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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using NsqSharp;
using NsqSharp.Api;
using TWCore.Messaging.RawClient;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Exceptions;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable NotAccessedField.Local
// ReSharper disable InconsistentNaming

namespace TWCore.Messaging.NSQ
{
    /// <inheritdoc />
    /// <summary>
    /// NSQ Queue Raw Client
    /// </summary>
    public class NSQueueRawClient : MQueueRawClientBase
    {
        private static readonly ConcurrentDictionary<Guid, NSQueueMessage> ReceivedMessages = new ConcurrentDictionary<Guid, NSQueueMessage>();
        private static readonly NSQMessageHandler MessageHandler = new NSQMessageHandler();
        private static readonly UTF8Encoding Encoding = new UTF8Encoding(false);

        #region Fields
        private List<(MQConnection, ObjectPool<Producer>)> _senders;
        private Consumer _receiver;
        private MQConnection _receiverConnection;
        private MQClientQueues _clientQueues;
        private MQClientSenderOptions _senderOptions;
        private MQClientReceiverOptions _receiverOptions;
        #endregion

        #region Properties
        /// <summary>
        /// Use Single Response Queue
        /// </summary>
        public bool UseSingleResponseQueue { get; private set; }
        #endregion

        #region Nested Type
        private class NSQueueMessage
        {
            public Guid CorrelationId;
            public SubArray<byte> Body;
            public readonly ManualResetEventSlim WaitHandler = new ManualResetEventSlim(false);
            public Consumer Consumer;
            public string Route;
            public string Name;
        }
        private class NSQMessageHandler : IHandler
        {
            public void HandleMessage(NsqSharp.IMessage message)
            {
                (var body, var correlationId, var _) = GetFromRawMessageBody(message.Body);
                Try.Do(message.Finish, false);

                var rMsg = ReceivedMessages.GetOrAdd(correlationId, cId => new NSQueueMessage());
                rMsg.CorrelationId = correlationId;
                rMsg.Body = body;
                rMsg.WaitHandler.Set();
            }
            public void LogFailedMessage(NsqSharp.IMessage message)
            {
            }
        }
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// NSQ Queue Client
        /// </summary>
        public NSQueueRawClient()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 200;
        }
        #endregion


        #region Init and Dispose Methods
        /// <inheritdoc />
        /// <summary>
        /// On client initialization
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnInit()
        {
            OnDispose();
            _senders = new List<(MQConnection, ObjectPool<Producer>)>();
            _receiver = null;


            if (Config != null)
            {
                if (Config.ClientQueues != null)
                {
                    _clientQueues = Config.ClientQueues.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true && c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true)
                                    ?? Config.ClientQueues.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true)
                                    ?? Config.ClientQueues.FirstOrDefault(c => c.EnvironmentName.IsNullOrWhitespace());
                }
                _senderOptions = Config.RequestOptions?.ClientSenderOptions;
                _receiverOptions = Config.ResponseOptions?.ClientReceiverOptions;
                UseSingleResponseQueue = _receiverOptions?.Parameters?[ParameterKeys.SingleResponseQueue].ParseTo(false) ?? false;

                if (_clientQueues?.SendQueues?.Any() == true)
                {
                    foreach (var queue in _clientQueues.SendQueues)
                    {
                        _senders.Add((queue, new ObjectPool<Producer>(pool =>
                        {
                            Core.Log.LibVerbose("New Producer from QueueClient");
                            return new Producer(queue.Route);
                        }, null, 1)));
                    }
                }
                if (_clientQueues?.RecvQueue != null)
                {
                    _receiverConnection = _clientQueues.RecvQueue;
                    _receiver = new Consumer(_receiverConnection.Name, _receiverConnection.Name);
                    if (UseSingleResponseQueue)
                    {
                        _receiver.AddHandler(MessageHandler);
                        _receiver.ConnectToNsqd(_receiverConnection.Route);
                    }
                }
            }

            Core.Status.Attach(collection =>
            {
                if (_senders != null)
                    for (var i = 0; i < _senders.Count; i++)
                        collection.Add(nameof(_senders) + " {0} Path".ApplyFormat(i), _senders[i].Item1.Route);
                if (_receiver != null)
                    collection.Add(nameof(_receiver) + " Path", _clientQueues.RecvQueue.Route);
            });
        }
        /// <inheritdoc />
        /// <summary>
        /// On Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnDispose()
        {
            if (_senders != null)
            {
                var producers = _senders.SelectMany(i => i.Item2.GetCurrentObjects()).ToArray();
                Parallel.ForEach(producers, p => p.Stop());
                foreach (var sender in _senders)
                    sender.Item2.Clear();
                _senders.Clear();
                _senders = null;
            }
            if (_receiver == null) return;
            if (UseSingleResponseQueue)
                _receiver.Stop();
            _receiver = null;
        }
        #endregion

        #region Send Method
        /// <inheritdoc />
        /// <summary>
        /// On Send message data
        /// </summary>
        /// <param name="message">Request message instance</param>
        /// <param name="correlationId">Message CorrelationId</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnSend(byte[] message, Guid correlationId)
        {
            if (_senders?.Any() != true)
                throw new NullReferenceException("There aren't any senders queues.");
            if (_senderOptions == null)
                throw new ArgumentNullException("SenderOptions");

            var recvQueue = _clientQueues.RecvQueue;
            var name = recvQueue.Name;
            if (!UseSingleResponseQueue)
                name += "_" + correlationId;

            var body = CreateRawMessageBody(message, correlationId, name);

            foreach ((var queue, var nsqProducerPool) in _senders)
            {
                Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}' with CorrelationId={2}", body.Length, queue.Route + "/" + queue.Name, correlationId);
                var nsqProducer = nsqProducerPool.New();
                nsqProducer.PublishAsync(queue.Name, body).Wait();
                nsqProducerPool.Store(nsqProducer);
            }
            Core.Log.LibVerbose("Message with CorrelationId={0} sent", correlationId);
            return true;
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
        protected override byte[] OnReceive(Guid correlationId, CancellationToken cancellationToken)
        {
            if (_receiver == null)
                throw new NullReferenceException("There is not receiver queue.");
            if (_receiverOptions == null)
                throw new ArgumentNullException("SenderOptions");

            var timeout = TimeSpan.FromSeconds(_receiverOptions.TimeoutInSec);
            var sw = Stopwatch.StartNew();
            var message = ReceivedMessages.GetOrAdd(correlationId, cId => new NSQueueMessage());

            if (!UseSingleResponseQueue)
            {
                message.Name = _receiverConnection.Name + "_" + correlationId;
                message.Route = _receiverConnection.Route;
                message.Consumer = new Consumer(message.Name, message.Name);
                message.Consumer.AddHandler(MessageHandler);
                message.Consumer.ConnectToNsqd(message.Route);

                var waitResult = message.WaitHandler.Wait(timeout, cancellationToken);

                message.Consumer.Stop();
                message.Consumer.DisconnectFromNsqd(message.Route);
                message.Consumer = null;
                var pro = new NsqdHttpClient(message.Route.Replace(":4150", ":4151"), TimeSpan.FromSeconds(60));
                pro.DeleteChannel(message.Name, message.Name);
                pro.DeleteTopic(message.Name);

                if (waitResult)
                {
                    Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}' with CorrelationId={2}", message.Body.Count, _clientQueues.RecvQueue.Name, correlationId);
                    Core.Log.LibVerbose("Correlation Message ({0}) received at: {1}ms", correlationId, sw.Elapsed.TotalMilliseconds);
                    sw.Stop();
                    return (byte[])message.Body;
                }
                else
                    throw new MessageQueueTimeoutException(timeout, correlationId.ToString());
            }

            if (message.WaitHandler.Wait(timeout, cancellationToken))
            {
                Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}' with CorrelationId={2}", message.Body.Count, _clientQueues.RecvQueue.Name, correlationId);
                Core.Log.LibVerbose("Correlation Message ({0}) received at: {1}ms", correlationId, sw.Elapsed.TotalMilliseconds);
                sw.Stop();
                return (byte[])message.Body;
            }
            else
                throw new MessageQueueTimeoutException(timeout, correlationId.ToString());
        }
        #endregion

        #region Static Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte[] CreateRawMessageBody(SubArray<byte> message, Guid correlationId, string name)
        {
            var nameBytes = Encoding.GetBytes(name);
            var nameLength = nameBytes.Length;
            var body = new byte[16 + 4 + nameLength + message.Count];
            Buffer.BlockCopy(correlationId.ToByteArray(), 0, body, 0, 16);
            Buffer.BlockCopy(BitConverter.GetBytes(nameLength), 0, body, 16, 4);
            nameBytes.CopyTo(body, 20);
            message.CopyTo(body, 20 + nameLength);
            return body;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static (SubArray<byte>, Guid, string) GetFromRawMessageBody(byte[] message)
        {
            var body = new SubArray<byte>(message);
            var correlationId = new Guid((byte[])body.Slice(0, 16));
            var nameLength = BitConverter.ToInt32(message, 16);
            var name = Encoding.GetString(message, 20, nameLength);
            var messageBody = body.Slice(20 + nameLength);
            return (messageBody, correlationId, name);
        }
        #endregion
    }
}
