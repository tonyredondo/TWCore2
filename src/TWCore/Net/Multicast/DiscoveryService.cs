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
using System.Collections.Generic;
using System.Net;
using System.Threading;
using TWCore.Collections;
using TWCore.Security;
using TWCore.Serialization;

namespace TWCore.Net.Multicast
{
    /// <summary>
    /// Discovery Service
    /// </summary>
    public static class DiscoveryService
    {
        private static readonly PeerConnection PeerConnection;
        private static readonly List<RegisteredService> LocalServices;
        private static readonly TimeoutDictionary<Guid, ReceivedService> ReceivedServices;
        private static readonly TimeSpan ServiceTimeout = TimeSpan.FromSeconds(30);
        private static Thread _sendThread;
        private static CancellationTokenSource _tokenSource;
        private static CancellationToken _token;
        private static bool _connected;

        #region Consts
        public const string FrameworkCategory = "FRAMEWORK";
        #endregion

        #region Events
        /// <summary>
        /// Service received event
        /// </summary>
        public static event EventHandler<EventArgs<ReceivedService>> OnServiceReceived;
        /// <summary>
        /// Service expired event
        /// </summary>
        public static event EventHandler<EventArgs<ReceivedService>> OnServiceExpired;
        /// <summary>
        /// New Service received event
        /// </summary>
        public static event EventHandler<EventArgs<ReceivedService>> OnNewServiceReceived;
        #endregion

        #region Properties
        /// <summary>
        /// Port number
        /// </summary>
        public static int Port { get; private set; } = 64128;
        /// <summary>
        /// Multicast Ip Address
        /// </summary>
        public static string MulticastIp { get; private set; } = "230.23.12.83";
        /// <summary>
        /// Messages Serializer
        /// </summary>
        public static ISerializer Serializer { get; set; } = SerializerManager.DefaultBinarySerializer;
        /// <summary>
        /// Has a registered local service
        /// </summary>
        public static bool HasRegisteredLocalService
        {
            get
            {
                lock(LocalServices)
                    return LocalServices.Count > 0;
            }
        }
        #endregion

        #region .ctor
        static DiscoveryService()
        {
            LocalServices = new List<RegisteredService>();
            ReceivedServices = new TimeoutDictionary<Guid, ReceivedService>();
            ReceivedServices.OnItemTimeout += (s, e) => Try.Do(() => OnServiceExpired?.Invoke(s, new EventArgs<ReceivedService>(e.Value)), false);
            PeerConnection = new PeerConnection();
            PeerConnection.OnReceive += PeerConnection_OnReceive;
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            _connected = false;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Connect to the multicast group
        /// </summary>
        public static void Connect()
        {
            Connect(MulticastIp, Port);
        }
        /// <summary>
        /// Connect to the multicast group
        /// </summary>
        /// <param name="multicastIp">Multicast Ip address</param>
        /// <param name="port">Port</param>
        public static void Connect(string multicastIp, int port)
        {
            _connected = true;
            MulticastIp = multicastIp;
            Port = port;
            PeerConnection.Connect(multicastIp, port);
            _sendThread = new Thread(SendThread)
            {
                Name = "DiscoveryServiceSendThread",
                IsBackground = true
            };
            _sendThread.Start();

        }
        /// <summary>
        /// Disconnect from the multicast group
        /// </summary>
        public static void Disconnect()
        {
            if (!_connected) return;
            _tokenSource?.Cancel();
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            PeerConnection.Disconnect();
            _connected = false;
        }
        /// <summary>
        /// Register service
        /// </summary>
        /// <param name="category">Service category</param>
        /// <param name="name">Service name</param>
        /// <param name="description">Service description</param>
        /// <param name="data">Service additional data</param>
        public static void RegisterService(string category, string name, string description, SerializedObject data)
        {
            var service = new RegisteredService
            {
                Category = category,
                Name = name,
                Description = description,
                MachineName = Core.MachineName,
                ApplicationName = Core.ApplicationName,
                FrameworkVersion = Core.FrameworkVersion,
                EnvironmentName = Core.EnvironmentName,
                Data = data
            };
            var serviceText = service.Category + service.Name + service.Description + service.MachineName + service.ApplicationName + service.FrameworkVersion + service.EnvironmentName;
            service.ServiceId = serviceText.GetHashSHA1Guid();
            lock (LocalServices)
                LocalServices.Add(service);
        }
        /// <summary>
        /// Register service
        /// </summary>
        /// <param name="category">Service category</param>
        /// <param name="name">Service name</param>
        /// <param name="description">Service description</param>
        /// <param name="data">Service additional data</param>
        public static void RegisterService(string category, string name, string description, Dictionary<string, object> data)
            => RegisterService(category, name, description, new SerializedObject(data));
        /// <summary>
        /// Get registered services
        /// </summary>
        /// <returns>Received registered services array</returns>
        public static ReceivedService[] GetRegisteredServices()
        {
            lock (ReceivedServices)
            {
                return ReceivedServices.ToValueArray();
            }
        }
        #endregion

        #region Private Methods
        private static void PeerConnection_OnReceive(object sender, PeerConnectionMessageReceivedEventArgs e)
        {
            var rService = Serializer.Deserialize<RegisteredService>(e.Data);
            var received = new ReceivedService
            {
                ServiceId = rService.ServiceId,
                Category = rService.Category,
                Name = rService.Name,
                Description = rService.Description,
                MachineName = rService.MachineName,
                ApplicationName = rService.ApplicationName,
                FrameworkVersion = rService.FrameworkVersion,
                EnvironmentName = rService.EnvironmentName,
                Data = rService.Data,
                Address = e.Address
            };
            bool exist;
            lock (ReceivedServices)
            {
                exist = ReceivedServices.TryRemove(received.ServiceId, out var _);
                ReceivedServices.TryAdd(received.ServiceId, received, ServiceTimeout);
            }
            var eArgs = new EventArgs<ReceivedService>(received);
            if (!exist)
                OnNewServiceReceived?.Invoke(sender, eArgs);
            OnServiceReceived?.Invoke(sender, eArgs);
        }
        private static void SendThread()
        {
            while (!_token.IsCancellationRequested)
            {
                RegisteredService[] rSrv;
                lock (LocalServices)
                    rSrv = LocalServices.ToArray();
                foreach (var srv in rSrv)
                {
                    var srvValue = Serializer.Serialize(srv);
                    PeerConnection.Send(srvValue);
                    if (_token.IsCancellationRequested)
                        return;
                }
                _token.WhenCanceledAsync().Wait(10000);
            }
        }
        #endregion

        #region Nested Types
        /// <summary>
        /// Registered Service
        /// </summary>
        public class RegisteredService
        {
            /// <summary>
            /// Service Id
            /// </summary>
            public Guid ServiceId { get; set; }
            /// <summary>
            /// Service Category
            /// </summary>
            public string Category { get; set; }
            /// <summary>
            /// Service Name
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// Service Description
            /// </summary>
            public string Description { get; set; }
            /// <summary>
            /// Machine Name
            /// </summary>
            public string MachineName { get; set; }
            /// <summary>
            /// Application Name
            /// </summary>
            public string ApplicationName { get; set; }
            /// <summary>
            /// Framework Version
            /// </summary>
            public string FrameworkVersion { get; set; }
            /// <summary>
            /// Environment Name
            /// </summary>
            public string EnvironmentName { get; set; }
            /// <summary>
            /// Additional Data
            /// </summary>
            public SerializedObject Data { get; set; }
        }
        /// <inheritdoc />
        /// <summary>
        /// Received Service
        /// </summary>
        public class ReceivedService : RegisteredService
        {
            /// <summary>
            /// Remote IpAddress
            /// </summary>
            public IPAddress Address { get; set; }
        }
        #endregion
    }
}
