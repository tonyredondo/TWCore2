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

using NSQCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
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

		#region Fields
		List<NsqProducer> _senders;
		INsqConsumer _receiver;
		string _receiverConsumerTag;
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
			public byte[] Body;
			public readonly ManualResetEventSlim WaitHandler = new ManualResetEventSlim(false);
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
			_senders = new List<NsqProducer>();
			_receiver = null;

			throw new NotImplementedException();
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
		protected override bool OnSend(RequestMessage message)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Receive Method
		protected override ResponseMessage OnReceive(Guid correlationId, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
