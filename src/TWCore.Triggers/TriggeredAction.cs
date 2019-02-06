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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Triggers
{
    /// <inheritdoc />
    /// <summary>
    /// Action to execute depending of triggers events
    /// </summary>
    public class TriggeredAction : IDisposable
    {
        private static readonly object ParentSync = new object();
        private readonly object _localSync = new object();

        #region Properties
        /// <summary>
        /// Action to execute when the trigger occurs
        /// </summary>
        public Action Action { get; set; }
        /// <summary>
        /// Wait time before listening to another trigger event
        /// </summary>
        public TimeSpan WaitTime { get; set; }
        /// <summary>
        /// Cancellation token
        /// </summary>
        public CancellationToken CancellationToken => _tokenSource?.Token ?? new CancellationToken();
        #endregion

        #region Events
        /// <summary>
        /// Before invoke event
        /// </summary>
        public event EventHandler<TriggeredEventArgs<Action>> BeforeInvoke;
        /// <summary>
        /// After invoke event
        /// </summary>
        public event EventHandler<TriggeredEventArgs<Action>> AfterInvoke;
        /// <summary>
        /// Not invoked cause the value is too recent event
        /// </summary>
        public event EventHandler<TriggeredEventArgs<Action>> NotInvokedTooRecent;
        /// <summary>
        /// On Exception event
        /// </summary>
        public event EventHandler<ExceptionEventArgs> OnException;
        #endregion

        #region Private Fields
        private readonly List<TriggerBase> _triggers;
        private DateTime _lastUpdateTime;
        private Task _triggerTask;
        private CancellationTokenSource _tokenSource;
        #endregion

        #region .ctors
        /// <summary>
        /// Action to execute depending of triggers events
        /// </summary>
        /// <param name="useStaticLock">Use static lock when a trigger occurs</param>
        public TriggeredAction(bool useStaticLock = false)
        {
            _triggers = new List<TriggerBase>();
            if (useStaticLock)
                _localSync = ParentSync;
            WaitTime = TimeSpan.FromSeconds(1);

            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(WaitTime), WaitTime);
                collection.Add("LastUpdateTime", _lastUpdateTime);
                foreach (var trigger in _triggers)
                    Core.Status.AttachChild(trigger, this);
            });
        }
        /// <inheritdoc />
        /// <summary>
        /// Action to execute depending of triggers events
        /// </summary>
        /// <param name="triggeredAction">Triggered action</param>
        /// <param name="useStaticLock">Definies if the trigger has to use an static lock</param>
        public TriggeredAction(Action triggeredAction, bool useStaticLock = false)
            : this(useStaticLock)
        {
            Action = triggeredAction;
        }
        #endregion

        #region Trigger Management
        /// <summary>
        /// Add an update trigger to the collection
        /// </summary>
        /// <param name="trigger">Update trigger object</param>
        public void AddTrigger(TriggerBase trigger)
        {
            if (trigger is null) return;
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
            if (trigger is null) return;
            if (!_triggers.Contains(trigger)) return;
            trigger.OnTriggered -= OnTriggerExecute;
            trigger.Dispose();
            _triggers.Remove(trigger);
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
            lock (ParentSync)
            {
                if (_triggers is null) return;
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
            if (Action is null) return;
            lock (_localSync)
            {
                _tokenSource = new CancellationTokenSource();
                var token = _tokenSource.Token;
                _triggerTask = Task.Run(() =>
                {
                    if (Core.Now - _lastUpdateTime > WaitTime)
                    {
                        _lastUpdateTime = Core.Now;
                        BeforeInvoke?.Invoke(this, new TriggeredEventArgs<Action>(trigger, Action));
                        var loadOk = true;
                        var sw = Stopwatch.StartNew();
                        try
                        {
                            if (CancellationToken.IsCancellationRequested)
                                return;
                            Action();
                            if (CancellationToken.IsCancellationRequested)
                                return;
                            sw.Stop();
                        }
                        catch (Exception ex)
                        {
                            sw.Stop();
                            loadOk = false;
                            OnException?.Invoke(this, new ExceptionEventArgs(ex, sw.Elapsed.TotalMilliseconds));
                        }
                        if (loadOk)
                            AfterInvoke?.Invoke(this, new TriggeredEventArgs<Action>(trigger, Action, sw.Elapsed.TotalMilliseconds));
                    }
                    else
                        NotInvokedTooRecent?.Invoke(this, new TriggeredEventArgs<Action>(trigger, Action));
                }, token);
                try
                {
                    _triggerTask.Wait(token);
                }
                catch
                {
                    // ignored
                }
                _triggerTask = null;
                _tokenSource = null;
            }
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        public void Dispose()
        {
            Unload();
            _tokenSource?.Cancel();
            _triggerTask?.Wait(2000);
            _tokenSource = null;
            _triggerTask = null;
        }
    }
}
