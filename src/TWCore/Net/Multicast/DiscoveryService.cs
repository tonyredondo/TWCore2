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
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TWCore.Collections;
using TWCore.Security;
using TWCore.Serialization;
using TWCore.Threading;
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
        private static readonly List<RegisteredServiceContainer> LocalServices;
        private static readonly TimeoutDictionary<Guid, ReceivedService> ReceivedServices;
        private static readonly TimeSpan ServiceTimeout = TimeSpan.FromSeconds(70);
        private static AsyncLock ReceivedServicesLock = new AsyncLock();
        private static Timer _sendTimer;
        private static bool _connected;

        #region Consts
        /// <summary>
        /// Framework category constant
        /// </summary>
        public const string FrameworkCategory = "FRAMEWORK";
        /// <summary>
        /// App category constant
        /// </summary>
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
        /// Has a registered local service
        /// </summary>
        public static bool HasRegisteredLocalService { get; private set; }
        /// <summary>
        /// Serializer used to send the data
        /// </summary>
        public static ISerializer Serializer { get; set; } = SerializerManager.DefaultBinarySerializer;
        #endregion

        #region .ctor
        static DiscoveryService()
        {
            LocalServices = new List<RegisteredServiceContainer>();
            ReceivedServices = new TimeoutDictionary<Guid, ReceivedService>();
            ReceivedServices.OnItemTimeout += (s, e) => Try.Do(vTuple => OnServiceExpired?.Invoke(vTuple.s, new EventArgs<ReceivedService>(vTuple.e.Value)), (s, e), false);
            PeerConnection = new PeerConnection();
            PeerConnection.OnReceiveAsync += PeerConnection_OnReceiveAsync;
            _connected = false;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Connect to the multicast group
        /// </summary>
        /// <param name="enableReceive">Enable receive multicast</param>
        public static void Connect(bool enableReceive = true)
        {
            Connect(MulticastIp, Port, enableReceive);
        }
        /// <summary>
        /// Connect to the multicast group
        /// </summary>
        /// <param name="multicastIp">Multicast Ip address</param>
        /// <param name="port">Port</param>
        /// <param name="enableReceive">Enable receive multicast</param>
        public static void Connect(string multicastIp, int port, bool enableReceive = true)
        {
            _connected = true;
            MulticastIp = multicastIp;
            Port = port;
            PeerConnection.Connect(multicastIp, port, enableReceive);
            _sendTimer = new Timer(SendTimerCallback, null, 0, 30000);
        }
        /// <summary>
        /// Disconnect from the multicast group
        /// </summary>
        public static void Disconnect()
        {
            if (!_connected) return;
            _sendTimer?.Dispose();
            _sendTimer = null;
            PeerConnection.Disconnect();
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
                InstanceId = Core.InstanceId,
                MachineName = Core.MachineName,
                ApplicationName = Core.ApplicationName,
                FrameworkVersion = Core.FrameworkVersion,
                EnvironmentName = Core.EnvironmentName,
                Data = data
            };
            var serviceText = service.InstanceId + service.Category + service.Name + service.Description + service.MachineName + service.ApplicationName + service.FrameworkVersion + service.EnvironmentName;
            service.ServiceId = serviceText.GetHashSHA1Guid();
            lock (LocalServices)
            {
                if (LocalServices.All((s, serviceId) => s.Service.ServiceId != serviceId, service.ServiceId))
                {
                    var sObj = new SerializedObject(service, Serializer);
                    var sObjArr = sObj.ToMultiArray();
                    LocalServices.Add(new RegisteredServiceContainer(service, Serializer, sObjArr));
                    HasRegisteredLocalService = true;
                }
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
                InstanceId = Core.InstanceId,
                MachineName = Core.MachineName,
                ApplicationName = Core.ApplicationName,
                FrameworkVersion = Core.FrameworkVersion,
                EnvironmentName = Core.EnvironmentName,
                GetDataFunc = getData
            };
            var serviceText = service.InstanceId + service.Category + service.Name + service.Description + service.MachineName + service.ApplicationName + service.FrameworkVersion + service.EnvironmentName;
            service.ServiceId = serviceText.GetHashSHA1Guid();
            lock (LocalServices)
            {
                if (LocalServices.All((s, serviceId) => s.Service.ServiceId != serviceId, service.ServiceId))
                {
                    LocalServices.Add(new RegisteredServiceContainer(service, Serializer, MultiArray<byte>.Empty));
                    HasRegisteredLocalService = true;
                }
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
                return LocalServices.RemoveAll(s => s.Service.ServiceId == serviceId) > 0;
        }
        #endregion

        #region Get Registered Services
        /// <summary>
        /// Get registered services
        /// </summary>
        /// <returns>Received registered services array</returns>
        public static ReceivedService[] GetRegisteredServices()
        {
            using (ReceivedServicesLock.GetLock())
                    return ReceivedServices.ToValueArray();
        }
        /// <summary>
        /// Get registered services
        /// </summary>
        /// <returns>Received registered services array</returns>
        public static ReceivedService[] GetLocalRegisteredServices()
        {
            using (ReceivedServicesLock.GetLock())
                return ReceivedServices.ToValueArray().Where(s => s.MachineName == Core.MachineName).ToArray();
        }
        /// <summary>
        /// Get registered services
        /// </summary>
        /// <param name="name">Service name</param>
        /// <returns>Received registered services array</returns>
        public static ReceivedService[] GetRegisteredServices(string name)
        {
            using (ReceivedServicesLock.GetLock())
                return ReceivedServices.ToValueArray().Where((r, mName) => r.Name == mName, name).ToArray();
        }
        /// <summary>
        /// Get registered services
        /// </summary>
        /// <param name="name">Service name</param>
        /// <returns>Received registered services array</returns>
        public static ReceivedService[] GetLocalRegisteredServices(string name)
        {
            using (ReceivedServicesLock.GetLock())
                return ReceivedServices.ToValueArray().Where((s, mName) => s.MachineName == Core.MachineName && s.Name == mName, name).ToArray();
        }
        #endregion

        #endregion

        #region Private Methods
        private static async Task PeerConnection_OnReceiveAsync(object sender, PeerConnectionMessageReceivedEventArgs e)
        {
            try
            {
                using (var serObj = SerializedObject.FromReadOnlySequence(e.Data))
                {
                    if (serObj is null) return;
                    if (!(serObj.GetValue() is RegisteredService rService)) return;

                    try
                    {
                        var received = new ReceivedService
                        {
                            ServiceId = rService.ServiceId,
                            InstanceId = rService.InstanceId,
                            Category = rService.Category,
                            Name = rService.Name,
                            Description = rService.Description,
                            MachineName = rService.MachineName,
                            ApplicationName = rService.ApplicationName,
                            FrameworkVersion = rService.FrameworkVersion,
                            EnvironmentName = rService.EnvironmentName,
                            Data = rService.Data,
                            Addresses = new[] { e.Address }
                        };
                        bool exist;
                        using (await ReceivedServicesLock.LockAsync().ConfigureAwait(false))
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
                    catch (Exception)
                    {
                        //
                    }
                }
            }
            catch(Exception)
            {
                //
            }
        }

        private static List<MultiArray<byte>> ServicesBytesList = new List<MultiArray<byte>>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SendTimerCallback(object state)
        {
            lock (LocalServices)
            {
                foreach (var srv in LocalServices)
                {
                    if (srv.Service.GetDataFunc != null)
                    {
                        srv.Service.Data = srv.Service.GetDataFunc();
                        using (var serObj = new SerializedObject(srv.Service, Serializer))
                            ServicesBytesList.Add(serObj.ToMultiArray());
                    }
                    else
                    {
                        if (srv.DataToSend.IsEmpty || srv.Serializer != Serializer)
                        {
                            using (var serObj = new SerializedObject(srv.Service, Serializer))
                                srv.DataToSend = serObj.ToMultiArray();
                            srv.Serializer = Serializer;
                        }
                        ServicesBytesList.Add(srv.DataToSend);
                    }
                }
                
                foreach(var bytes in ServicesBytesList)
                    PeerConnection.SendAsync(bytes).WaitAsync();
                
                ServicesBytesList.Clear();
            }
        }
        #endregion

        #region Nested Types
        private class RegisteredServiceContainer
        {
            public RegisteredService Service;
            public ISerializer Serializer;
            public MultiArray<byte> DataToSend;

            public RegisteredServiceContainer(RegisteredService service, ISerializer serializer, MultiArray<byte> dataToSend)
            {
                Service = service;
                Serializer = serializer;
                DataToSend = dataToSend;
            }
        }
        /// <summary>
        /// Registered Service
        /// </summary>
        [Serializable, DataContract]
        public class RegisteredService
        {
            /// <summary>
            /// Func to get the data about the service
            /// </summary>
            [NonSerialize, NonSerialized]
            public Func<SerializedObject> GetDataFunc;

            #region Properties
            /// <summary>
            /// Service Id
            /// </summary>
            [DataMember, XmlAttribute]
            public Guid ServiceId { get; set; }
            /// <summary>
            /// Instance Id
            /// </summary>
            [DataMember, XmlAttribute]
            public Guid InstanceId { get; set; }
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
