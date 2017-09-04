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
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;

namespace TWCore.Cache.Client
{
    /// <summary>
    /// Cache pool item
    /// </summary>
    public class PoolItem : IDisposable
    {
        CancellationTokenSource tokenSource;
        CancellationToken token;
        Task pingTask;

        #region Properties
        /// <summary>
        /// Pool Item Name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Storage instance
        /// </summary>
        public IStorage Storage { get; private set; }
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
        /// <param name="name">Pool item name</param>
        /// <param name="storage">Storage instance</param>
        /// <param name="mode">Storage mode inside the pool</param>
        /// <param name="pingDelay">Delays between ping tries in milliseconds</param>
        /// <param name="pingDelayOnError">Delay after a ping error for next try</param>
        public PoolItem(string name, IStorage storage, StorageItemMode mode, int pingDelay = 5000, int pingDelayOnError = 30000)
        {
            Storage = storage;
            Mode = mode;
            PingTime = 0;
            PingDelay = pingDelay;
            PingDelayOnError = pingDelayOnError;
            Enabled = false;
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
            Name = name ?? storage?.ToString();
			if (storage?.Type == StorageType.Memory)
			{
				Enabled = true;
				PingTime = 0;
			}
			else
			{
	            pingTask = Task.Factory.StartNew(async () =>
	            {
	                try
	                {
	                    while (!token.IsCancellationRequested)
	                    {
	                        try
	                        {
	                            if (Storage != null)
	                            {
	                                using (var watch = Watch.Create())
	                                {
	                                    bool enabled = true;
	                                    Enabled = false;
	                                    try
	                                    {
	                                        enabled = Storage.IsEnabled();
	                                        if (PingResponse == int.MaxValue) PingResponse = 15;
	                                        PingResponse++;
	                                        PingConsecutiveFailure = 0;
	                                    }
	                                    catch (Exception ex)
	                                    {
	                                        Core.Log.Write(ex);
	                                        enabled = false;
	                                        if (PingFailure == int.MaxValue) PingFailure = 15;
	                                        if (PingConsecutiveFailure == int.MaxValue) PingConsecutiveFailure = 15;
	                                        PingFailure++;
	                                        PingConsecutiveFailure++;
	                                    }
	                                    Enabled = enabled;
	                                    PingTime = watch.GlobalElapsedMilliseconds;
	                                    Core.Log.LibVerbose("Ping Task for Pool item node: {0} has Enabled = {1} with a PingTime = {2:0.0000}ms.", name, Enabled, PingTime);
	                                }
	                            }
	                            else
	                                Core.Log.Error("The pool item node: {0} doesn't have any storage associated", name);

	                            await Task.Delay((PingConsecutiveFailure > 15) ? PingDelayOnError : PingDelay, token).ConfigureAwait(false);
	                        }
	                        catch (TaskCanceledException) { }
	                        catch (Exception ex)
	                        {
	                            Core.Log.Write(ex);
	                        }
	                    }
	                    Core.Log.Warning("Ping Task for Pool item node: {0} was terminated.", name);
	                }
	                catch (Exception ex)
	                {
	                    Core.Log.Write(ex);
	                }
	            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
			}
            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(Name), Name);
                collection.Add(nameof(Storage), Storage);
                collection.Add(nameof(Mode), Mode);
                collection.Add(nameof(PingTime), PingTime);
                collection.Add(nameof(PingDelay), PingDelay);
                collection.Add(nameof(PingDelayOnError), PingDelayOnError);
                collection.Add(nameof(PingResponse), PingResponse, StatusItemValueStatus.Green);
                collection.Add(nameof(PingFailure), PingFailure, StatusItemValueStatus.Yellow);
                collection.Add(nameof(PingConsecutiveFailure), PingConsecutiveFailure, PingConsecutiveFailure == 0 ? StatusItemValueStatus.Green : StatusItemValueStatus.Red);
                collection.Add(nameof(Enabled), Enabled, Enabled ? StatusItemValueStatus.Green : StatusItemValueStatus.Red);
                Core.Status.AttachChild(Storage, this);
            });
        }
        /// <summary>
        /// Detructor
        /// </summary>
        ~PoolItem()
        {
            Dispose();
        }
        /// <summary>
        /// Dispose all resources of the instance
        /// </summary>
        public void Dispose()
        {
	        if (pingTask == null) return;
	        Core.Status.DeAttachObject(this);
	        tokenSource.Cancel();
	        try
	        {
		        pingTask.Wait(10000);
	        }
	        catch
	        {
		        // ignored
	        }
	        tokenSource = null;
	        pingTask = null;
	        try
	        {
		        Storage.Dispose();
		        Storage = null;
	        }
	        catch
	        {
		        // ignored
	        }
        }
        #endregion
    }
}
