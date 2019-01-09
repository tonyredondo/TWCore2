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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Net.RPC.Attributes;
using TWCore.Serialization;
using TWCore.Threading;

namespace TWCore.Net.RPC.Server.Transports
{
    /// <inheritdoc cref="ITransportServer" />
    /// <summary>
    /// Collection of transport servers
    /// </summary>
    public sealed class TransportServerCollection : ObservableCollection<ITransportServer>, ITransportServer
    {
        private string _name;
        
        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Transport name, should be the same name for Server and Client
        /// </summary>
        public string Name => _name;
        /// <inheritdoc />
        /// <summary>
        /// true if the transport server can send the service descriptor; otherwise, false
        /// </summary>
        public bool EnableGetDescriptors
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.Any(i => i.EnableGetDescriptors);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                foreach (var i in this)
                    i.EnableGetDescriptors = value;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Serializer to encode and decode the incoming and outgoing data
        /// </summary>
        public ISerializer Serializer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.FirstOrDefault()?.Serializer;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                foreach (var i in this)
                    i.Serializer = value;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Event that fires when a Descriptor request is received.
        /// </summary>
        public event EventHandler<ServerDescriptorsEventArgs> OnGetDescriptorsRequest;
        /// <summary>
        /// Event that fires when a Method call is received
        /// </summary>
        public AsyncEvent<MethodEventArgs> OnMethodCallAsync { get; set; }
        /// <summary>
        /// Event that fires when a Method response is sent
        /// </summary>
        public event EventHandler<RPCResponseMessage> OnResponseSent;
        /// <summary>
        /// Event that fires when a client connects.
        /// </summary>
        public event EventHandler<ClientConnectEventArgs> OnClientConnect;

        /// <summary>
        /// Collection Changed
        /// </summary>
        public override event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => base.CollectionChanged += value;
            remove => base.CollectionChanged -= value;
        }
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Collection of transport servers
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TransportServerCollection()
        {
            CollectionChanged += (s, e) =>
            {
                if ((e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace) && e.NewItems != null)
                {
                    foreach (ITransportServer item in e.NewItems)
                    {
                        Core.Log.LibVerbose("Adding {0} transport server", item.GetType().Name);
                        item.OnClientConnect += Item_OnClientConnect;
                        item.OnGetDescriptorsRequest += Item_OnGetDescriptorsRequest;
                        item.OnMethodCallAsync += Item_OnMethodCallAsync;
                        item.OnResponseSent += Item_OnResponseSent;
                    }
                }
                if ((e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace) && e.OldItems != null)
                {
                    foreach (ITransportServer item in e.OldItems)
                    {
                        Core.Log.LibVerbose("Removing {0} transport server", item.GetType().Name);
                        item.OnClientConnect -= Item_OnClientConnect;
                        item.OnGetDescriptorsRequest -= Item_OnGetDescriptorsRequest;
                        item.OnMethodCallAsync -= Item_OnMethodCallAsync;
                        item.OnResponseSent -= Item_OnResponseSent;
                    }
                }
                
                _name = string.Join(";", this.Select(i => i.Name).ToArray());
            };
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Item_OnClientConnect(object sender, ClientConnectEventArgs e)
            => OnClientConnect?.InvokeAsync(sender, e);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Item_OnGetDescriptorsRequest(object sender, ServerDescriptorsEventArgs e)
            => OnGetDescriptorsRequest?.InvokeAsync(sender, e);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Task Item_OnMethodCallAsync(object sender, MethodEventArgs e)
        {
            return OnMethodCallAsync is null ? Task.CompletedTask : OnMethodCallAsync.InvokeAsync(sender, e);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Item_OnResponseSent(object sender, RPCResponseMessage message)
            => OnResponseSent?.InvokeAsync(sender, message);
        #endregion

        #region Public Methods
        /// <inheritdoc />
        /// <summary>
        /// Send a fire event trigger to a RPC client.
        /// </summary>
        /// <param name="eventAttribute">Event attribute</param>
        /// <param name="clientId">Client identifier</param>
        /// <param name="serviceName">Service name of the event</param>
        /// <param name="eventName">Event name</param>
        /// <param name="sender">Sender information</param>
        /// <param name="e">Event args</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task FireEventAsync(RPCEventAttribute eventAttribute, Guid clientId, string serviceName, string eventName, object sender, EventArgs e)
        {
            var count = Count;
            if (count == 0) 
                return Task.CompletedTask;
            if (count == 1)
                return this[0].FireEventAsync(eventAttribute, clientId, serviceName, eventName, sender, e);
            var fArray = new Task[count];
            for(var i = 0; i < count; i++)
                fArray[i] = this[i].FireEventAsync(eventAttribute, clientId, serviceName, eventName, sender, e);
            return Task.WhenAll(fArray);
        }
        /// <inheritdoc />
        /// <summary>
        /// Starts the server listener
        /// </summary>
        /// <returns>Task as result of the startup process</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task StartListenerAsync()
        {
            var count = Count;
            if (count == 0) 
                return Task.CompletedTask;
            if (count == 1)
                return this[0].StartListenerAsync();
            var fArray = new Task[count];
            for(var i = 0; i < count; i++)
                fArray[i] = this[i].StartListenerAsync();
            return Task.WhenAll(fArray);
        }
        /// <inheritdoc />
        /// <summary>
        /// Stops the server listener
        /// </summary>
        /// <returns>Task as result of the stop process</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task StopListenerAsync()
        {
            var count = Count;
            if (count == 0) 
                return Task.CompletedTask;
            if (count == 1)
                return this[0].StopListenerAsync();
            var fArray = new Task[count];
            for(var i = 0; i < count; i++)
                fArray[i] = this[i].StopListenerAsync();
            return Task.WhenAll(fArray);
        }
        #endregion
    }
}
