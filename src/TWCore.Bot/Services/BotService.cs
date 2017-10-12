#pragma warning disable IDE1006 // Estilos de nombres
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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using TWCore.Bot;
using TWCore.Serialization;
// ReSharper disable CheckNamespace
// ReSharper disable UnusedParameter.Global

namespace TWCore.Services
{
    /// <inheritdoc />
    /// <summary>
    /// Bot Service base
    /// </summary>
    public abstract class BotService : IBotService
    {
        private readonly ISerializer _serializer = SerializerManager.DefaultBinarySerializer;

        /// <inheritdoc />
        /// <summary>
        /// Service bot engine
        /// </summary>
        public IBotEngine Bot { get; private set; }
        /// <inheritdoc />
        /// <summary>
        /// Save tracked chats on disk
        /// </summary>
        public bool SaveTrackedChats { get; protected set; } = false;
        /// <inheritdoc />
        /// <summary>
        /// Tracked chats file path
        /// </summary>
        public string TrackedChatsFilePath { get; protected set; } = "{0}_SavedChats".ApplyFormat(Core.ApplicationName);

        #region IService Methods
        /// <inheritdoc />
        /// <summary>
        /// Get if the service support pause and continue
        /// </summary>
        public bool CanPauseAndContinue => true;
        /// <inheritdoc />
        /// <summary>
        /// On Continue from pause method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async void OnContinue()
        {
            try
            {
                Core.Log.InfoBasic("Continuing Bot service...");
                await Bot.StartListenerAsync().ConfigureAwait(false);
                OnContinued();
                Core.Log.InfoBasic("Bot service has started.");
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// On Pause method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async void OnPause()
        {
            try
            {
                Core.Log.InfoBasic("Pausing Bot service...");
                OnPaused();
                await Bot.StopListenerAsync().ConfigureAwait(false);
                Core.Log.InfoBasic("Bot service has been paused.");
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// On shutdown requested method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnShutdown()
        {
            OnStop();
        }
        /// <inheritdoc />
        /// <summary>
        /// On Service Start method
        /// </summary>
        /// <param name="args">Start arguments</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async void OnStart(string[] args)
        {
            try
            {
                Core.Log.InfoBasic("Starting Bot service...");
                OnInit(args);
                Bot = GetBotEngine();
                if (Bot == null)
                    throw new NullReferenceException("There aren't a Bot engine instance to start, check the GetBotEngine() method return value");
                if (SaveTrackedChats)
                {
                    if (File.Exists(TrackedChatsFilePath))
                    {
                        Try.Do(() =>
                        {
                            Core.Log.InfoBasic("Loading tracked chats file: {0}", TrackedChatsFilePath);
                            var botChats = _serializer.DeserializeFromFile<List<BotChat>>(TrackedChatsFilePath);
                            Bot.TrackedChats.AddRange(botChats);
                            Core.Log.InfoBasic("{0} tracked chats loaded.", botChats?.Count);
                        });
                    }
                    Bot.OnTrackedChatsChanged += (s, e) =>
                    {
                        Try.Do(() =>
                        {
                            Core.Log.InfoBasic("Saving tracked chats file: {0}", TrackedChatsFilePath);
                            var botChats = ((IBotEngine)s).TrackedChats.ToList();
                            _serializer.SerializeToFile(botChats, TrackedChatsFilePath);
                            Core.Log.InfoBasic("{0} tracked chats saved.", botChats?.Count);
                        });
                    };
                }
                await Bot.StartListenerAsync().ConfigureAwait(false);
                OnStarted();
                Core.Log.InfoBasic("Bot service has started.");
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                throw;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// On Service Stops method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async void OnStop()
        {
            try
            {
                Core.Log.InfoBasic("Stopping Bot service...");
                OnFinalizing();
                await Bot.StopListenerAsync().ConfigureAwait(false);
                Core.Log.InfoBasic("Bot service has stopped.");
                OnDispose();
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// On Service Init
        /// </summary>
        /// <param name="args">Service arguments</param>
        protected virtual void OnInit(string[] args) { }
        /// <summary>
        /// On Service Started
        /// </summary>
        protected virtual void OnStarted() { }
        /// <summary>
        /// On Service Paused
        /// </summary>
        protected virtual void OnPaused() { }
        /// <summary>
        /// On Service Continued
        /// </summary>
        protected virtual void OnContinued() { }
        /// <summary>
        /// Gets the Bot Engine 
        /// </summary>
        /// <returns>BotEngine instance</returns>
        protected abstract IBotEngine GetBotEngine();
        /// <summary>
        /// On Service Stop
        /// </summary>
        protected virtual void OnFinalizing() { }
        /// <summary>
        /// On Service Dispose
        /// </summary>
        protected virtual void OnDispose() { }
        #endregion
    }
}
