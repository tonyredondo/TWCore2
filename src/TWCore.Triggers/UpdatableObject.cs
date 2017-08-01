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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TWCore.Triggers
{
    /// <summary>
	/// Class to handle an object instances that updates based on triggers
	/// </summary>
	/// <typeparam name="T">Type of instance</typeparam>
	public class UpdatableObject<T> : IDisposable where T : class
    {
        readonly static object _parentSync = new object();
        readonly object localSync = new object();
        Task triggerTask;
        CancellationTokenSource tokenSource;

        /// <summary>
        /// Current Instance
        /// </summary>
        public T Instance { get; private set; }
        /// <summary>
        /// Minimum time of the instance (The minimum time between triggers events)
        /// </summary>
        public TimeSpan MinTimeOfInstance { get; set; }
        /// <summary>
        /// Cancellation token
        /// </summary>
        public CancellationToken CancellationToken => tokenSource?.Token ?? CancellationToken.None;

        #region Events
        /// <summary>
        /// Before update event
        /// </summary>
        public event EventHandler<TriggeredEventArgs<WeakReference<T>>> BeforeUpdate;
        /// <summary>
        /// After update event
        /// </summary>
        public event EventHandler<TriggeredEventArgs<WeakReference<T>>> AfterUpdate;
        /// <summary>
        /// Not update cause the value is too recent event
        /// </summary>
        public event EventHandler<TriggeredEventArgs<WeakReference<T>>> NotUpdatedTooRecent;
        /// <summary>
        /// On Exception event
        /// </summary>
        public event EventHandler<ExceptionEventArgs> OnException;
        #endregion

        #region Private Fields
        DateTime _lastUpdateTime;
        Func<T> InstanceLoader;
        List<TriggerBase> Triggers;
        #endregion

        #region .ctors
        /// <summary>
        /// Class to handle an object instances that updates based on triggers
        /// </summary>
        /// <param name="useStaticLock">Use static lock when a trigger occurs</param>
        public UpdatableObject(bool useStaticLock = false)
        {
            Triggers = new List<TriggerBase>();
            if (useStaticLock)
                localSync = _parentSync;
            MinTimeOfInstance = TimeSpan.FromSeconds(1);

            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(MinTimeOfInstance), MinTimeOfInstance);
                collection.Add("LastUpdateTime", _lastUpdateTime);
                foreach (var trigger in Triggers)
                    Core.Status.AttachChild(trigger, this);
            });
        }
        /// <summary>
        /// Class to handle an object instances that updates based on triggers
        /// </summary>
        /// <param name="instanceLoader">Instance loader/update function</param>
        /// <param name="useStaticLock">Definies if the trigger has to use an static lock</param>
        public UpdatableObject(Func<T> instanceLoader, bool useStaticLock = false)
            : this(useStaticLock)
        {
            SetInstanceLoader(instanceLoader);
        }
        #endregion

        #region Trigger Management
        /// <summary>
        /// Add an update trigger to the collection
        /// </summary>
        /// <param name="trigger">Update trigger object</param>
        public void AddTrigger(TriggerBase trigger)
        {
            if (trigger != null)
            {
                if (!Triggers.Contains(trigger))
                {
                    trigger.OnTriggered += OnTriggerExecute;
                    trigger.Init();
                    Triggers.Add(trigger);
                }
            }
        }
        /// <summary>
        /// Remove an update trigger from the collection
        /// </summary>
        /// <param name="trigger">Update trigger object</param>
        public void RemoveTrigger(TriggerBase trigger)
        {
            if (trigger != null)
            {
                if (Triggers.Contains(trigger))
                {
                    trigger.OnTriggered -= OnTriggerExecute;
                    trigger.Dispose();
                    Triggers.Remove(trigger);
                }
            }
        }
        #endregion

        #region Instance Loader
        /// <summary>
        /// Sets a new Instance Loader/Updater function
        /// </summary>
        /// <param name="instanceLoader">Loader/Updater function</param>
        public void SetInstanceLoader(Func<T> instanceLoader)
        {
            InstanceLoader = instanceLoader;
        }
        /// <summary>
        /// Execute the instance loader
        /// </summary>
        public void Load()
        {
            InnerOnTriggerExecute(new LocalLoaderTrigger());
        }
        /// <summary>
        /// Unload and dispose all the registered triggers
        /// </summary>
        public void Unload()
        {
            lock (_parentSync)
            {
                foreach (var trigger in Triggers)
                {
                    trigger.OnTriggered -= OnTriggerExecute;
                    trigger.Dispose();
                }
                Triggers.Clear();
            }
        }
        #endregion

        #region Private Methods
        void OnTriggerExecute(TriggerBase trigger)
        {
            if (InstanceLoader != null)
                Task.Factory.StartNew(obj => InnerOnTriggerExecute((TriggerBase)obj), trigger, CancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        void InnerOnTriggerExecute(TriggerBase trigger)
        {
            if (InstanceLoader != null)
            {
                lock (localSync)
                {
                    Core.Log.LibVerbose("Trigger execution was received, executing update.");
                    if (Core.Now - _lastUpdateTime > MinTimeOfInstance)
                    {
                        _lastUpdateTime = Core.Now;
                        T newInstance = default(T);
                        BeforeUpdate?.Invoke(this, new TriggeredEventArgs<WeakReference<T>>(trigger, new WeakReference<T>(Instance)));
                        bool loadOK = true;
                        var sw = Stopwatch.StartNew();
                        try
                        {
                            if (CancellationToken.IsCancellationRequested)
                                return;
                            newInstance = InstanceLoader();
                            if (CancellationToken.IsCancellationRequested)
                                return;
                            sw.Stop();
                        }
                        catch (Exception ex)
                        {
                            sw.Stop();
                            Core.Log.Write(ex);
                            loadOK = false;
                            OnException?.Invoke(this, new ExceptionEventArgs(ex, sw.Elapsed.TotalMilliseconds));
                        }
                        if (loadOK && !CancellationToken.IsCancellationRequested)
                        {
                            var oldInstance = Instance;
                            Instance = newInstance;
                            Core.Log.LibVerbose("The update was done sucessfully.");
                            if (oldInstance is IDisposable)
                            {
                                try
                                {
                                    Core.Log.LibDebug("Disposing old instance.");
                                    ((IDisposable)oldInstance).Dispose();
                                }
                                catch (Exception oDispEx)
                                {
                                    Core.Log.Error(oDispEx, "Error disposing the old instance.");
                                }
                                finally
                                {
                                    oldInstance = default(T);
                                }
                            }
                            AfterUpdate?.Invoke(this, new TriggeredEventArgs<WeakReference<T>>(trigger, new WeakReference<T>(Instance), sw.Elapsed.TotalMilliseconds));
                        }
                    }
                    else
                    {
                        Core.Log.LibVerbose("The updatable object wasn't update, another update was made recently.");
                        NotUpdatedTooRecent?.Invoke(this, new TriggeredEventArgs<WeakReference<T>>(trigger, new WeakReference<T>(Instance)));
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Dispose all resources
        /// </summary>
        public void Dispose()
        {
            Unload();
            tokenSource?.Cancel();
            triggerTask?.Wait(5000);
            tokenSource = null;
            triggerTask = null;
        }
    }
}
