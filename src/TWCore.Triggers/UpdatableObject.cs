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
    internal static class UpdatableObject
    {
        public static readonly object ParentSync = new object();
    }
    
    /// <summary>
	/// Class to handle an object instances that updates based on triggers
	/// </summary>
	/// <typeparam name="T">Type of instance</typeparam>
	public class UpdatableObject<T> : IDisposable where T : class
    {
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
        public CancellationToken CancellationToken => _tokenSource?.Token ?? CancellationToken.None;

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
        private readonly object _localSync = new object();
        private Task _triggerTask;
        private CancellationTokenSource _tokenSource;
        private DateTime _lastUpdateTime;
        private Func<T> _instanceLoader;
        private readonly List<TriggerBase> _triggers;
        #endregion

        #region .ctors
        /// <summary>
        /// Class to handle an object instances that updates based on triggers
        /// </summary>
        /// <param name="useStaticLock">Use static lock when a trigger occurs</param>
        public UpdatableObject(bool useStaticLock = false)
        {
            _triggers = new List<TriggerBase>();
            if (useStaticLock)
                _localSync = UpdatableObject.ParentSync;
            MinTimeOfInstance = TimeSpan.FromSeconds(1);

            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(MinTimeOfInstance), MinTimeOfInstance);
                collection.Add("LastUpdateTime", _lastUpdateTime);
                foreach (var trigger in _triggers)
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
            if (trigger == null) return;
            if (_triggers.Contains(trigger)) return;
            trigger.OnTriggered += OnTriggerExecute;
            trigger.Init();
            _triggers.Add(trigger);
        }
        /// <summary>
        /// Remove an update trigger from the collection
        /// </summary>
        /// <param name="trigger">Update trigger object</param>
        public void RemoveTrigger(TriggerBase trigger)
        {
            if (trigger == null) return;
            if (!_triggers.Contains(trigger)) return;
            trigger.OnTriggered -= OnTriggerExecute;
            trigger.Dispose();
            _triggers.Remove(trigger);
        }
        #endregion

        #region Instance Loader
        /// <summary>
        /// Sets a new Instance Loader/Updater function
        /// </summary>
        /// <param name="instanceLoader">Loader/Updater function</param>
        public void SetInstanceLoader(Func<T> instanceLoader)
        {
            _instanceLoader = instanceLoader;
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
            lock (UpdatableObject.ParentSync)
            {
                foreach (var trigger in _triggers)
                {
                    trigger.OnTriggered -= OnTriggerExecute;
                    trigger.Dispose();
                }
                _triggers.Clear();
            }
        }
        #endregion

        #region Private Methods
        private void OnTriggerExecute(TriggerBase trigger)
        {
            if (_instanceLoader != null)
                Task.Factory.StartNew(obj => InnerOnTriggerExecute((TriggerBase)obj), trigger, CancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        private void InnerOnTriggerExecute(TriggerBase trigger)
        {
            if (_instanceLoader == null) return;
            lock (_localSync)
            {
                Core.Log.LibVerbose("Trigger execution was received, executing update.");
                if (Core.Now - _lastUpdateTime > MinTimeOfInstance)
                {
                    _lastUpdateTime = Core.Now;
                    var newInstance = default(T);
                    BeforeUpdate?.Invoke(this, new TriggeredEventArgs<WeakReference<T>>(trigger, new WeakReference<T>(Instance)));
                    var loadOk = true;
                    var sw = Stopwatch.StartNew();
                    try
                    {
                        if (CancellationToken.IsCancellationRequested)
                            return;
                        newInstance = _instanceLoader();
                        if (CancellationToken.IsCancellationRequested)
                            return;
                        sw.Stop();
                    }
                    catch (Exception ex)
                    {
                        sw.Stop();
                        Core.Log.Write(ex);
                        loadOk = false;
                        OnException?.Invoke(this, new ExceptionEventArgs(ex, sw.Elapsed.TotalMilliseconds));
                    }
                    if (!loadOk || CancellationToken.IsCancellationRequested) return;
                    var oldInstance = Instance;
                    Instance = newInstance;
                    Core.Log.LibVerbose("The update was done sucessfully.");
                    if (oldInstance is IDisposable oldDisposable)
                    {
                        try
                        {
                            Core.Log.LibDebug("Disposing old instance.");
                            oldDisposable.Dispose();
                        }
                        catch (Exception oDispEx)
                        {
                            Core.Log.Error(oDispEx, "Error disposing the old instance.");
                        }
                    }
                    AfterUpdate?.Invoke(this, new TriggeredEventArgs<WeakReference<T>>(trigger, new WeakReference<T>(Instance), sw.Elapsed.TotalMilliseconds));
                }
                else
                {
                    Core.Log.LibVerbose("The updatable object wasn't update, another update was made recently.");
                    NotUpdatedTooRecent?.Invoke(this, new TriggeredEventArgs<WeakReference<T>>(trigger, new WeakReference<T>(Instance)));
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
            _tokenSource?.Cancel();
            _triggerTask?.Wait(5000);
            _tokenSource = null;
            _triggerTask = null;
        }
    }
}
