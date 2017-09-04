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
    /// Action to execute depending of triggers events
    /// </summary>
    public abstract class TriggeredActionBase : IDisposable
    {
        readonly static object _parentSync = new object();
        readonly object localSync = new object();
        Task triggerTask;
        CancellationTokenSource tokenSource;

        /// <summary>
        /// Action to execute when the trigger occurs
        /// </summary>
        protected abstract void OnAction();

        /// <summary>
        /// Wait time before listening to another trigger event
        /// </summary>
        public TimeSpan WaitTime { get; set; }
        /// <summary>
        /// Cancellation token
        /// </summary>
        public CancellationToken CancellationToken => tokenSource?.Token ?? new CancellationToken();


        #region Events
        /// <summary>
        /// Before invoke event
        /// </summary>
        public event EventHandler<TriggeredEventArgs<TriggeredActionBase>> BeforeInvoke;
        /// <summary>
        /// After invoke event
        /// </summary>
        public event EventHandler<TriggeredEventArgs<TriggeredActionBase>> AfterInvoke;
        /// <summary>
        /// Not invoked cause the value is too recent event
        /// </summary>
        public event EventHandler<TriggeredEventArgs<TriggeredActionBase>> NotInvokedTooRecent;
        /// <summary>
        /// On Exception event
        /// </summary>
        public event EventHandler<ExceptionEventArgs> OnException;
        #endregion

        #region Private Fields
        DateTime _lastUpdateTime;
        List<TriggerBase> Triggers;
        #endregion

        #region .ctors
        /// <summary>
        /// Action to execute depending of triggers events
        /// </summary>
        /// <param name="useStaticLock">Use static lock when a trigger occurs</param>
        public TriggeredActionBase(bool useStaticLock = false)
        {
            Triggers = new List<TriggerBase>();
            if (useStaticLock)
                localSync = _parentSync;
            WaitTime = TimeSpan.FromSeconds(1);

            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(WaitTime), WaitTime);
                collection.Add("LastUpdateTime", _lastUpdateTime);
                foreach (var trigger in Triggers)
                    Core.Status.AttachChild(trigger, this);
            });
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~TriggeredActionBase()
        {
            Dispose();
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

        #region Loader
        /// <summary>
        /// Execute the instance loader
        /// </summary>
        public void Execute()
        {
            OnTriggerExecute(new LocalLoaderTrigger());
        }
        /// <summary>
        /// Unload and dispose all the registered triggers
        /// </summary>
        public void Unload()
        {
            lock (_parentSync)
            {
                if (Triggers != null)
                {
                    foreach (var trigger in Triggers)
                    {
                        trigger.OnTriggered -= OnTriggerExecute;
                        trigger.Dispose();
                    }
                    Triggers.Clear();
                }
            }
        }
        #endregion

        #region Private Methods
        private void OnTriggerExecute(TriggerBase trigger)
        {
            lock (localSync)
            {
                tokenSource = new CancellationTokenSource();
                var token = tokenSource.Token;
                triggerTask = Task.Run(() =>
                {
                    if (Core.Now - _lastUpdateTime > WaitTime)
                    {
                        _lastUpdateTime = Core.Now;
                        BeforeInvoke?.Invoke(this, new TriggeredEventArgs<TriggeredActionBase>(trigger, this));
                        bool loadOK = true;
                        var sw = Stopwatch.StartNew();
                        try
                        {
                            if (CancellationToken.IsCancellationRequested)
                                return;
                            OnAction();
                            if (CancellationToken.IsCancellationRequested)
                                return;
                            sw.Stop();
                        }
                        catch (Exception ex)
                        {
                            sw.Stop();
                            loadOK = false;
                            OnException?.Invoke(this, new ExceptionEventArgs(ex, sw.Elapsed.TotalMilliseconds));
                        }
                        if (loadOK)
                            AfterInvoke?.Invoke(this, new TriggeredEventArgs<TriggeredActionBase>(trigger, this, sw.Elapsed.TotalMilliseconds));
                    }
                    else
                        NotInvokedTooRecent?.Invoke(this, new TriggeredEventArgs<TriggeredActionBase>(trigger, this));
                }, token);
                try
                {
                    triggerTask.Wait(token);
                }
                catch
                {
                    // ignored
                }
                triggerTask = null;
                tokenSource = null;
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
            triggerTask?.Wait(2000);
            tokenSource = null;
            triggerTask = null;
        }
    }
}
