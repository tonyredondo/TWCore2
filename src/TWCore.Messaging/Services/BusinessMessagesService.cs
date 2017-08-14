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
using TWCore.Messaging.Server;
using TWCore.Services.Messaging;

namespace TWCore.Services
{
	/// <summary>
	/// Business messages service.
	/// </summary>
	public class BusinessMessagesService : MessagingService
	{
		Func<IBusiness> _businessFactory;
		Func<IMQueueServer> _queueServerFactory;

		#region .ctor
		/// <summary>
		/// Business messages service.
		/// </summary>
		/// <param name="businessFactory">Business factory delegate</param>
		public BusinessMessagesService(Func<IBusiness> businessFactory)
		{
			_businessFactory = businessFactory;
			_queueServerFactory = () => Core.Services.GetQueueServer();
		}
		/// <summary>
		/// Business messages service.
		/// </summary>
		/// <param name="businessFactory">Business factory delegate</param>
		/// <param name="queueServerFactory">QueueServer factory delegate</param>
		public BusinessMessagesService(Func<IBusiness> businessFactory, Func<IMQueueServer> queueServerFactory)
		{
			_businessFactory = businessFactory;
			_queueServerFactory = queueServerFactory;
		}
		#endregion

		#region Overrides
		/// <summary>
		/// Gets the message processor
		/// </summary>
		/// <param name="server">Queue server object instance</param>
		/// <returns>Message processor instance</returns>
		protected override IMessageProcessor GetMessageProcessor(IMQueueServer server)
			=> new BusinessMessageProcessor(server, _businessFactory);
		/// <summary>
		/// Gets the queue server object
		/// </summary>
		/// <returns>IMQueueServer object instance</returns>
		protected override IMQueueServer GetQueueServer()
			=> _queueServerFactory();
		#endregion
	}
}
