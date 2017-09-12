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
// ReSharper disable CheckNamespace

namespace TWCore.Services
{
	/// <inheritdoc />
	/// <summary>
	/// Business messages async service.
	/// </summary>
	public class BusinessMessagesServiceAsync<TBusinessAsync> : MessagingServiceAsync where TBusinessAsync : IBusinessAsync, new()
	{
	    #region Properties
	    /// <summary>
	    /// Get or set the IMQueueServer factory, by default is loaded using the queues.xml configuration file
	    /// </summary>
	    public Func<IMQueueServer> QueueServerFactory { get; set; } = () => Core.Services.GetQueueServer();
	    #endregion

        #region Overrides
        /// <inheritdoc />
        /// <summary>
        /// Gets the message processor
        /// </summary>
        /// <param name="server">Queue server object instance</param>
        /// <returns>Message processor instance</returns>
        protected override IMessageProcessorAsync GetMessageProcessorAsync(IMQueueServer server)
			=> new BusinessMessageProcessorAsync(server, () => new TBusinessAsync());
		/// <inheritdoc />
		/// <summary>
		/// Gets the queue server object
		/// </summary>
		/// <returns>IMQueueServer object instance</returns>
		protected override IMQueueServer GetQueueServer()
			=> QueueServerFactory();
		#endregion
	}
}
