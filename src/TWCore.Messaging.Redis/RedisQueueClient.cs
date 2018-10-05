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

using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Messaging.Client;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Exceptions;
using TWCore.Threading;

// ReSharper disable NotAccessedField.Local
// ReSharper disable InconsistentNaming

namespace TWCore.Messaging.Redis
{
	/// <inheritdoc />
	/// <summary>
	/// Redis Queue Client
	/// </summary>
	public class RedisQueueClient : MQueueClientBase
	{


		#region Nested Type
		private class Message
		{
			public Guid CorrelationId;
			public MultiArray<byte> Body;
			public readonly AsyncManualResetEvent WaitHandler = new AsyncManualResetEvent(false);
			public string Route;
			public string Name;
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
			//new RedisClient();
		}
		/// <inheritdoc />
		/// <summary>
		/// On Dispose
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override void OnDispose()
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}
		#endregion
	}
}
