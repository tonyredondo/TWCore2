﻿/*
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
using System.Runtime.CompilerServices;
using TWCore.Messaging.Client;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.RawClient;
using TWCore.Messaging.RawServer;
using TWCore.Messaging.Server;
using TWCore.Serialization;
using TWCore.Settings;
// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedMember.Global

namespace TWCore.Services
{
    /// <summary>
    /// Core Services Extensions
    /// </summary>
    public static class CoreServicesExtensions
    {
        private static MQueuesConfiguration _queues;
        private static MQPairConfig _queueServer;
        private static bool _init;

        #region Init
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Init()
        {
            if (_init) return;
            _init = true;

            var queueSettings = Core.GetSettings<QueueConfigurationSettings>();
            if (string.IsNullOrEmpty(queueSettings.ConfigFile)) return;
            queueSettings.ServerName = queueSettings.ServerName ?? Core.ApplicationName;

            var queuesConfigFile = queueSettings.ConfigFile;
            queuesConfigFile = queuesConfigFile?.Replace("{EnvironmentName}", Core.EnvironmentName);
            queuesConfigFile = queuesConfigFile?.Replace("{MachineName}", Core.MachineName);
            queuesConfigFile = queuesConfigFile?.Replace("{ApplicationName}", Core.ApplicationName);
            queuesConfigFile = queuesConfigFile?.Replace("{InstanceId}", Core.InstanceIdString);
            queuesConfigFile = Factory.ResolveLowLowFilePath(queuesConfigFile);
            Core.Log.InfoBasic("Loading queues configuration: {0}", queuesConfigFile);

            try
            {
                var value = queuesConfigFile.ReadTextFromFile();
                value = Core.ReplaceSettingsTemplate(value);
                value = Core.ReplaceEnvironmentTemplate(value);
                var serializer = SerializerManager.GetByFileName<ITextSerializer>(queuesConfigFile);
                _queues = serializer.DeserializeFromString<MQueuesConfiguration>(value);
            }
            catch (Exception ex)
            {
                throw new Exception($"The Queues config file: {queuesConfigFile} can't be deserialized.", ex);
            }

            if (_queues is null)
            {
                Core.Log.Warning("The Queues configuration file is null or empty.");
                return;
            }

            if (_queues.Items?.Contains(queueSettings.ServerName) == true)
                _queueServer = _queues.Items[queueSettings.ServerName];
        }
        #endregion

        #region Queue methods
        /// <summary>
        /// Gets a queue client instance
        /// </summary>
        /// <param name="services">CoreServices instance</param>
        /// <param name="queuePairName">Queue config pair name</param>
        /// <param name="sendOnly">Send only</param>
        /// <returns>IMQueueClient instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IMQueueClient GetQueueClient(this CoreServices services, string queuePairName, bool sendOnly = false)
        {
            Init();
            if (_queues?.Items?.Contains(queuePairName) == true)
                return _queues.Items[queuePairName].GetClient(sendOnly);
            throw new NullReferenceException($"The Queue Pair Name: {queuePairName} not found in the configuration file.");
        }
        /// <summary>
        /// Gets a queue raw client instance
        /// </summary>
        /// <param name="services">CoreServices instance</param>
        /// <param name="queuePairName">Queue config pair name</param>
        /// <param name="sendOnly">Send only</param>
        /// <returns>IMQueueClient instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IMQueueRawClient GetQueueRawClient(this CoreServices services, string queuePairName, bool sendOnly = false)
        {
            Init();
            if (_queues?.Items?.Contains(queuePairName) == true)
                return _queues.Items[queuePairName].GetRawClient(sendOnly);
            throw new NullReferenceException($"The Queue Pair Name: {queuePairName} not found in the configuration file.");
        }
        /// <summary>
        /// Gets the queue server object
        /// </summary>
        /// <param name="services">Current services instance</param>
        /// <param name="responseServer">true if the server must be a response server</param>
        /// <returns>IMQueueServer object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IMQueueServer GetQueueServer(this CoreServices services, bool responseServer = false)
        {
            Init();
            if (_queueServer is null)
            {
                var queueSettings = Core.GetSettings<QueueConfigurationSettings>();
                throw new KeyNotFoundException($"The Queue server name: {queueSettings.ServerName} couldn't be found or loaded in the queue configuration file.");
            }
            return _queueServer.GetServer(responseServer);
        }
        /// <summary>
        /// Gets the queue server object
        /// </summary>
        /// <param name="services">Current services instance</param>
        /// <param name="queuePairName">Queue config pair name</param>
        /// <param name="responseServer">true if the server must be a response server</param>
        /// <returns>IMQueueServer object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IMQueueServer GetQueueServer(this CoreServices services, string queuePairName, bool responseServer = false)
        {
            Init();
            if (_queues.Items?.Contains(queuePairName) != true)
                throw new KeyNotFoundException(
                    $"The Queue server name: {queuePairName} couldn't be found in the queue configuration file.");
            
            var serverPair = _queues.Items[queuePairName];
            return serverPair.GetServer(responseServer);
        }

        /// <summary>
        /// Gets the queue raw server object
        /// </summary>
        /// <param name="services">Current services instance</param>
        /// <param name="responseServer">true if the server must be a response server</param>
        /// <returns>IMQueueRawServer object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IMQueueRawServer GetQueueRawServer(this CoreServices services, bool responseServer = false)
        {
            Init();
            if (_queueServer is null)
            {
                var queueSettings = Core.GetSettings<QueueConfigurationSettings>();
                throw new KeyNotFoundException($"The Queue server name: {queueSettings.ServerName} couldn't be found or loaded in the queue configuration file.");
            }
            return _queueServer.GetRawServer(responseServer);
        }
        /// <summary>
        /// Gets the queue raw server object
        /// </summary>
        /// <param name="services">Current services instance</param>
        /// <param name="queuePairName">Queue config pair name</param>
        /// <param name="responseServer">true if the server must be a response server</param>
        /// <returns>IMQueueServer object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IMQueueRawServer GetQueueRawServer(this CoreServices services, string queuePairName, bool responseServer = false)
        {
            Init();
            if (_queues.Items?.Contains(queuePairName) != true)
                throw new KeyNotFoundException(
                    $"The Queue server name: {queuePairName} couldn't be found in the queue configuration file.");
            
            var serverPair = _queues.Items[queuePairName];
            return serverPair.GetRawServer(responseServer);
        }
        
        /// <summary>
        /// Gets the Default Queues Configuration
        /// </summary>
        /// <returns>QueuesConfiguration instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MQueuesConfiguration GetDefaultQueuesConfiguration(this CoreServices services)
        {
            Init();
            return _queues;
        }
        #endregion

        #region Nested Settings Type
        [SettingsContainer("Core.Services.Queue")]
        private class QueueConfigurationSettings : SettingsBase
        {
            public string ConfigFile { get; set; }
            public string ServerName { get; set; }
        }
        #endregion
    }
}
