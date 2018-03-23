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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TWCore.Collections;
using TWCore.Security;
using TWCore.Serialization;
// ReSharper disable ReturnTypeCanBeEnumerable.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable EventNeverSubscribedTo.Global

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
        private static Task _sendThread;
        private static CancellationTokenSource _tokenSource;
        private static CancellationToken _token;
        private static bool _connected;

        #region Consts
        public const string FrameworkCategory = "FRAMEWORK";
        public const string AppCategory = "APP";
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
            _sendThread = SendThreadAsync();
        }
        /// <summary>
        /// Disconnect from the multicast group
        /// </summary>
        public static void Disconnect()
        {
            if (!_connected) return;
            _tokenSource?.Cancel();
            _sendThread?.WaitAsync();
            PeerConnection.Disconnect();
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            _connected = false;
        }

        #region Register Service
        /// <summary>
        /// Register service
        /// </summary>
        /// <param name="category">Service category</param>
        /// <param name="name">Service name</param>
        /// <param name="description">Service description</param>
        /// <param name="data">Service additional data</param>
        /// <returns>Service Id</returns>
        public static Guid RegisterService(string category, string name, string description, SerializedObject data)
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
            {
                if (LocalServices.All(s => s.ServiceId != service.ServiceId))
                    LocalServices.Add(service);
            }
            return service.ServiceId;
        }
        /// <summary>
        /// Register service
        /// </summary>
        /// <param name="category">Service category</param>
        /// <param name="name">Service name</param>
        /// <param name="description">Service description</param>
        /// <param name="getData">Service additional data func</param>
        /// <returns>Service Id</returns>
        public static Guid RegisterService(string category, string name, string description, Func<SerializedObject> getData)
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
                GetDataFunc = getData
            };
            var serviceText = service.Category + service.Name + service.Description + service.MachineName + service.ApplicationName + service.FrameworkVersion + service.EnvironmentName;
            service.ServiceId = serviceText.GetHashSHA1Guid();
            lock (LocalServices)
            {
                if (LocalServices.All(s => s.ServiceId != service.ServiceId))
                    LocalServices.Add(service);
            }
            return service.ServiceId;
        }
        /// <summary>
        /// Register service
        /// </summary>
        /// <param name="category">Service category</param>
        /// <param name="name">Service name</param>
        /// <param name="description">Service description</param>
        /// <param name="data">Service additional data</param>
        /// <returns>Service Id</returns>
        public static Guid RegisterService(string category, string name, string description, Dictionary<string, object> data)
            => RegisterService(category, name, description, new SerializedObject(data));
        /// <summary>
        /// Register service
        /// </summary>
        /// <param name="category">Service category</param>
        /// <param name="name">Service name</param>
        /// <param name="description">Service description</param>
        /// <param name="getData">Service additional data func</param>
        /// <returns>Service Id</returns>
        public static Guid RegisterService(string category, string name, string description, Func<Dictionary<string, object>> getData)
            => RegisterService(category, name, description, () => getData != null ? new SerializedObject(getData()) : null);
        #endregion

        #region Unregister Service
        /// <summary>
        /// Unregister service
        /// </summary>
        /// <param name="serviceId">Service Id</param>
        /// <returns>True if a service was removed; otherwise, false.</returns>
        public static bool UnregisterService(Guid serviceId)
        {
            lock (LocalServices)
                return LocalServices.RemoveAll(s => s.ServiceId == serviceId) > 0;
        }
        #endregion

        #region Get Registered Services
        /// <summary>
        /// Get registered services
        /// </summary>
        /// <returns>Received registered services array</returns>
        public static ReceivedService[] GetRegisteredServices()
        {
            lock (ReceivedServices)
                return ReceivedServices.ToValueArray();
        }
        /// <summary>
        /// Get registered services
        /// </summary>
        /// <returns>Received registered services array</returns>
        public static ReceivedService[] GetLocalRegisteredServices()
        {
            lock (ReceivedServices)
                return ReceivedServices.ToValueArray().Where(s => s.MachineName == Core.MachineName).ToArray();
        }
        /// <summary>
        /// Get registered services
        /// </summary>
        /// <param name="name">Service name</param>
        /// <returns>Received registered services array</returns>
        public static ReceivedService[] GetRegisteredServices(string name)
        {
            lock (ReceivedServices)
                return ReceivedServices.ToValueArray().Where(r => r.Name == name).ToArray();
        }
        /// <summary>
        /// Get registered services
        /// </summary>
        /// <param name="name">Service name</param>
        /// <returns>Received registered services array</returns>
        public static ReceivedService[] GetLocalRegisteredServices(string name)
        {
            lock (ReceivedServices)
                return ReceivedServices.ToValueArray().Where(s => s.MachineName == Core.MachineName && s.Name == name).ToArray();
        }
        #endregion

        #endregion

        #region Private Methods
        private static void PeerConnection_OnReceive(object sender, PeerConnectionMessageReceivedEventArgs e)
        {
            try
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
                    Addresses = new[] {e.Address}
                };
                bool exist;
                lock (ReceivedServices)
                {
                    exist = ReceivedServices.TryRemove(received.ServiceId, out var oldReceived);
                    if (exist)
                        received.Addresses = received.Addresses.Concat(oldReceived.Addresses).Distinct().ToArray();
                    ReceivedServices.TryAdd(received.ServiceId, received, ServiceTimeout);
                }

                var eArgs = new EventArgs<ReceivedService>(received);
                if (!exist)
                    OnNewServiceReceived?.Invoke(sender, eArgs);
                OnServiceReceived?.Invoke(sender, eArgs);
            }
            catch(Exception)
            {
                //
            }
        }
        private static async Task SendThreadAsync()
        {
            while (!_token.IsCancellationRequested)
            {
                List<RegisteredService> tmpList;
                lock (LocalServices)
                    tmpList = new List<RegisteredService>(LocalServices);

                foreach (var srv in tmpList)
                {
                    if (srv.GetDataFunc != null)
                        srv.Data = srv.GetDataFunc();
                    var srvValue = Serializer.Serialize(srv);
                    await PeerConnection.SendAsync(srvValue).ConfigureAwait(false);
                    if (_token.IsCancellationRequested)
                        return;
                }
                await Task.Delay(10000, _token).ConfigureAwait(false);
            }
        }
        #endregion

        #region Nested Types
        /// <summary>
        /// Registered Service
        /// </summary>
        [Serializable, DataContract]
        public class RegisteredService
        {
            [NonSerialize, NonSerialized]
            public Func<SerializedObject> GetDataFunc;

            #region Properties
            /// <summary>
            /// Service Id
            /// </summary>
            [DataMember, XmlAttribute]
            public Guid ServiceId { get; set; }
            /// <summary>
            /// Service Category
            /// </summary>
            [DataMember, XmlAttribute]
            public string Category { get; set; }
            /// <summary>
            /// Service Name
            /// </summary>
            [DataMember, XmlAttribute]
            public string Name { get; set; }
            /// <summary>
            /// Service Description
            /// </summary>
            [DataMember]
            public string Description { get; set; }
            /// <summary>
            /// Machine Name
            /// </summary>
            [DataMember, XmlAttribute]
            public string MachineName { get; set; }
            /// <summary>
            /// Application Name
            /// </summary>
            [DataMember, XmlAttribute]
            public string ApplicationName { get; set; }
            /// <summary>
            /// Framework Version
            /// </summary>
            [DataMember, XmlAttribute]
            public string FrameworkVersion { get; set; }
            /// <summary>
            /// Environment Name
            /// </summary>
            [DataMember, XmlAttribute]
            public string EnvironmentName { get; set; }
            /// <summary>
            /// Additional Data
            /// </summary>
            [DataMember]
            public SerializedObject Data { get; set; }
            #endregion
        }
        /// <inheritdoc />
        /// <summary>
        /// Received Service
        /// </summary>
        [Serializable]
        public class ReceivedService : RegisteredService
        {
            /// <summary>
            /// Remote IpAddresses
            /// </summary>
            public IPAddress[] Addresses { get; set; }
        }
        #endregion
    }
}
