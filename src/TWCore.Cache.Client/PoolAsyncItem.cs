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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
// ReSharper disable MethodSupportsCancellation

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace TWCore.Cache.Client
{
    /// <inheritdoc />
    /// <summary>
    /// Cache pool item
    /// </summary>
    [StatusName("Pool Item")]
    public class PoolAsyncItem : IDisposable
    {
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        private Task _pingTask;

        #region Properties
        /// <summary>
        /// Enabled property has changed.
        /// </summary>
        public event EventHandler<bool> EnabledChanged;

        /// <summary>
        /// Pool Item index
        /// </summary>
        public int Index { get; private set; }
        /// <summary>
        /// Pool Item Name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Storage instance
        /// </summary>
        public IStorageAsync Storage { get; private set; }
        /// <summary>
        /// Storage mode inside the pool
        /// </summary>
        public StorageItemMode Mode { get; private set; }
        /// <summary>
        /// Lastest ping time
        /// </summary>
        public double PingTime { get; private set; }
        /// <summary>
        /// Gets if the storages is enabled (got recent ping response)
        /// </summary>
        public bool Enabled { get; private set; }
        /// <summary>
        /// Number of successful ping responses
        /// </summary>
        public int PingResponse { get; private set; }
        /// <summary>
        /// Number of ping failures
        /// </summary>
        public int PingFailure { get; private set; }
        /// <summary>
        /// Number of consecutive ping failures
        /// </summary>
        public int PingConsecutiveFailure { get; private set; }
        /// <summary>
        /// Delays between ping tries in milliseconds
        /// </summary>
        public int PingDelay { get; set; }
        /// <summary>
        /// Delay after a ping error for next try
        /// </summary>
        public int PingDelayOnError { get; set; }
        #endregion

        #region .ctor

        /// <summary>
        /// Cache pool item
        /// </summary>
        /// <param name="index">Item index</param>
        /// <param name="name">Pool item name</param>
        /// <param name="storage">Storage instance</param>
        /// <param name="mode">Storage mode inside the pool</param>
        /// <param name="pingDelay">Delays between ping tries in milliseconds</param>
        /// <param name="pingDelayOnError">Delay after a ping error for next try</param>
        public PoolAsyncItem(int index, string name, IStorageAsync storage, StorageItemMode mode, int pingDelay = 5000, int pingDelayOnError = 30000)
        {
            Index = index;
            Storage = storage;
            Mode = mode;
            PingTime = 0;
            PingDelay = pingDelay;
            PingDelayOnError = pingDelayOnError;
            Enabled = false;
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            Name = name ?? storage?.ToString();
            if (storage?.Type == StorageType.Memory)
            {
                Enabled = true;
                PingTime = 0;
            }
            else
                _pingTask = PingTaskAsync();

            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(Name), Name);
                collection.Add(nameof(Storage), Storage);
                collection.Add(nameof(Mode), Mode);
                collection.Add(nameof(PingTime), PingTime);
                collection.Add(nameof(PingDelay), PingDelay);
                collection.Add(nameof(PingDelayOnError), PingDelayOnError);
                collection.Add(nameof(PingResponse), PingResponse, StatusItemValueStatus.Ok);
                collection.Add(nameof(PingFailure), PingFailure, StatusItemValueStatus.Warning);
                collection.Add(nameof(PingConsecutiveFailure), PingConsecutiveFailure, PingConsecutiveFailure == 0 ? StatusItemValueStatus.Ok : StatusItemValueStatus.Error);
                collection.Add(nameof(Enabled), Enabled, Enabled ? StatusItemValueStatus.Ok : StatusItemValueStatus.Error);
                Core.Status.AttachChild(Storage, this);
            });
        }

        /// <summary>
        /// Detructor
        /// </summary>
        ~PoolAsyncItem()
        {
            Dispose();
        }

        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources of the instance
        /// </summary>
        public void Dispose()
        {
            if (_pingTask is null) return;
            Core.Status.DeAttachObject(this);
            _tokenSource.Cancel();
            try
            {
                _pingTask.Wait(5000);
            }
            catch
            {
                // ignored
            }

            _tokenSource = null;
            _pingTask = null;
            Storage.Dispose();
            Storage = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task PingTaskAsync()
        {
            var tokenTask = _token.WhenCanceledAsync();
            var sw = new Stopwatch();
            while (!_token.IsCancellationRequested)
            {
                try
                {
                    if (Storage is null)
                    {
                        Core.Log.Error("The cache pool item node: {0} doesn't have any storage associated", Name);
                        break;
                    }
                    var lastEnabled = Enabled;
                    try
                    {
                        sw.Restart();
                        var stoTask = Storage.IsEnabledAsync();
                        var rTask = await Task.WhenAny(stoTask, tokenTask).ConfigureAwait(false);
                        if (rTask == tokenTask) break;
                        Enabled = stoTask.Result;
                        sw.Stop();
                        if (PingResponse == int.MaxValue) PingResponse = 15;
                        PingResponse++;
                        PingConsecutiveFailure = 0;
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Error("Cache Ping Error on {0}: {1}", Name, ex.InnerException?.Message ?? ex.Message);
                        Enabled = false;
                        if (PingFailure == int.MaxValue) PingFailure = 15;
                        if (PingConsecutiveFailure == int.MaxValue) PingConsecutiveFailure = 15;
                        PingFailure++;
                        PingConsecutiveFailure++;
                    }
                    PingTime = sw.Elapsed.TotalMilliseconds;

                    if (Enabled != lastEnabled)
                        EnabledChanged?.Invoke(this, Enabled);
                    Core.Log.LibVerbose("Cache Ping Task for Pool item node: {0} has Enabled = {1} with a PingTime = {2:0.0000}ms", Name, Enabled, PingTime);

                    var waitDelay = PingConsecutiveFailure < 15 ? PingDelayOnError : PingDelay;
                    if (Enabled == false)
                        Core.Log.Warning("{0} is Disabled due connection issues, Trying a new ping on: {1}ms", Name, waitDelay);
                    if (!lastEnabled && Enabled)
                        Core.Log.InfoBasic("{0} is connected.", Name);
                    await Task.Delay(waitDelay, _token).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
            }
            Core.Log.Warning("Ping Task for Pool item node: {0} was terminated.", Name);
        }
        #endregion
    }
}