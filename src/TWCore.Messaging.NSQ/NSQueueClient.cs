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
using NSQCore;
using TWCore.Messaging.Client;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Exceptions;

namespace TWCore.Messaging.NSQ
{
	/// <summary>
	/// NSQ Queue Client
	/// </summary>
	public class NSQueueClient : MQueueClientBase
	{
		static readonly ConcurrentDictionary<Guid, NSQueueMessage> ReceivedMessages = new ConcurrentDictionary<Guid, NSQueueMessage>();
		readonly ConcurrentDictionary<Guid, (INsqConsumer, NsqProducer)> CorrelationIdConsumers = new ConcurrentDictionary<Guid, (INsqConsumer, NsqProducer)>();

		#region Fields
		List<(MQConnection, NsqProducer)> _senders;
		INsqConsumer _receiver;
		MQConnection _receiverConnection;
		MQClientQueues _clientQueues;
		MQClientSenderOptions _senderOptions;
		MQClientReceiverOptions _receiverOptions;
		#endregion

		#region Properties
		/// <summary>
		/// Use Single Response Queue
		/// </summary>
		public bool UseSingleResponseQueue { get; private set; }
		#endregion

		#region Nested Type
		class NSQueueMessage
		{
			public Guid CorrelationId;
			public SubArray<byte> Body;
			public readonly ManualResetEventSlim WaitHandler = new ManualResetEventSlim(false);
		}
		#endregion

		#region .ctor
		/// <summary>
		/// NSQ Queue Client
		/// </summary>
		public NSQueueClient()
		{
			System.Net.ServicePointManager.DefaultConnectionLimit = 200;
		}
		#endregion


		#region Init and Dispose Methods
		/// <summary>
		/// On client initialization
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override void OnInit()
		{
			OnDispose();
			_senders = new List<(MQConnection, NsqProducer)>();
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
				UseSingleResponseQueue = _receiverOptions.Parameters?[ParameterKeys.SingleResponseQueue].ParseTo(false) ?? false;

				if (_clientQueues?.SendQueues?.Any() == true)
				{
					foreach (var queue in _clientQueues.SendQueues)
					{
						var options = ConsumerOptions.Parse(queue.Route);
						options.Topic = queue.Name;
						options.Channel = queue.Name;
						_senders.Add((queue, new NsqProducer(options.NsqEndPoint.Host, 4151)));
					}
				}
				if (_clientQueues?.RecvQueue != null)
				{
					_receiverConnection = _clientQueues.RecvQueue;
					var options = ConsumerOptions.Parse(_receiverConnection.Route);
					options.Topic = _receiverConnection.Name;
					options.Channel = _receiverConnection.Name;
					options.ClientId = Guid.NewGuid().ToString();
					_receiver = NsqConsumer.Create(options);
					if (UseSingleResponseQueue)
					{
						_receiver.ConnectAndWaitAsync(async message =>
						{
							var body = new SubArray<byte>(message.Body);
							var correlationId = new Guid((byte[])body.Slice(0, 16));
							var messageBody = body.Slice(16);
							var rMsg = ReceivedMessages.GetOrAdd(correlationId, cId => new NSQueueMessage());
							rMsg.CorrelationId = correlationId;
							rMsg.Body = messageBody;
							rMsg.WaitHandler.Set();
							await message.FinishAsync().ConfigureAwait(false);
						}).WaitAsync();
						_receiver.SetMaxInFlightAsync(1).WaitAsync();
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
		/// <summary>
		/// On Dispose
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override void OnDispose()
		{
			if (_senders != null)
			{
				_senders.Clear();
				_senders = null;
			}
			if (_receiver != null)
			{
				_receiver.Dispose();
				_receiver = null;
			}
		}
		#endregion

		#region Send Method
		/// <summary>
		/// On Send message data
		/// </summary>
		/// <param name="message">Request message instance</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override bool OnSend(RequestMessage message)
		{
			if (_senders?.Any() != true)
				throw new NullReferenceException("There aren't any senders queues.");
			if (_senderOptions == null)
				throw new ArgumentNullException("SenderOptions");

			var recvQueue = _clientQueues.RecvQueue;
			if (message.Header.ResponseQueue == null)
			{
				if (recvQueue != null)
				{
					message.Header.ResponseQueue = new MQConnection(recvQueue.Route, recvQueue.Name) { Parameters = recvQueue.Parameters };
					message.Header.ResponseExpected = true;
					message.Header.ResponseTimeoutInSeconds = _receiverOptions?.TimeoutInSec ?? -1;
					if (!UseSingleResponseQueue)
					{
						message.Header.ResponseQueue.Name += "_" + message.CorrelationId;
					}
				}
				else
				{
					message.Header.ResponseExpected = false;
					message.Header.ResponseTimeoutInSeconds = -1;
				}
			}
			var data = SenderSerializer.Serialize(message);
			var body = new byte[data.Count + 16];
			Buffer.BlockCopy(message.CorrelationId.ToByteArray(), 0, body, 0, 16);
			data.CopyTo(body, 16);

			foreach ((var queue, var nsqProducer) in _senders)
			{
				Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}' with CorrelationId={2}", body.Length, queue.Route + "/" + queue.Name, message.Header.CorrelationId);
				nsqProducer.PublishAsync(queue.Name, body);
			}
			Core.Log.LibVerbose("Message with CorrelationId={0} sent", message.Header.CorrelationId);
			return true;
		}
		#endregion

		#region Receive Method
		/// <summary>
		/// On Receive message data
		/// </summary>
		/// <param name="correlationId">Correlation Id</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Response message instance</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override ResponseMessage OnReceive(Guid correlationId, CancellationToken cancellationToken)
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
				var _recName = _receiverConnection.Name + "_" + correlationId;
				var options = ConsumerOptions.Parse(_receiverConnection.Route);
				options.Topic = _recName;
				options.Channel = _recName;

				var rcv = NsqConsumer.Create(options);
				var pro = new NsqProducer(options.NsqEndPoint.Host, 4151);
				CorrelationIdConsumers.TryAdd(correlationId, (rcv, pro));
				rcv.ConnectAndWaitAsync(async msg =>
				{
					var body = new SubArray<byte>(msg.Body);
					var crId = new Guid((byte[])body.Slice(0, 16));
					var messageBody = body.Slice(16);
					var rMsg = ReceivedMessages.GetOrAdd(crId, cId => new NSQueueMessage());
					rMsg.CorrelationId = crId;
					rMsg.Body = messageBody;
					rMsg.WaitHandler.Set();
					await msg.FinishAsync().ConfigureAwait(false);
					if (CorrelationIdConsumers.TryRemove(crId, out var _val))
					{
						_val.Item1.Dispose();
						await _val.Item2.DeleteTopicAsync(_recName).ConfigureAwait(false);
					}
				}).WaitAsync();
				rcv.SetMaxInFlightAsync(1).WaitAsync();
			}

			if (message.WaitHandler.Wait(timeout, cancellationToken))
			{
				if (message.Body == null)
					throw new MessageQueueNotFoundException("The Message can't be retrieved, null body on CorrelationId = " + correlationId.ToString());

				Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}' with CorrelationId={2}", message.Body.Count, _clientQueues.RecvQueue.Name, correlationId);
				var response = ReceiverSerializer.Deserialize<ResponseMessage>(message.Body);
				Core.Log.LibVerbose("Correlation Message ({0}) received at: {1}ms", correlationId, sw.Elapsed.TotalMilliseconds);
				sw.Stop();
				sw = null;
				return response;
			}
			else
				throw new MessageQueueTimeoutException(timeout, correlationId.ToString());
		}
		#endregion
	}
}
