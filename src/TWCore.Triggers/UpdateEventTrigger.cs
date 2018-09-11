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
using System.Threading;
using TWCore.Diagnostics.Status;
// ReSharper disable EventNeverSubscribedTo.Global

namespace TWCore.Triggers
{
    /// <summary>
    /// Update event trigger delegate
    /// </summary>
    /// <param name="environment">Environment name</param>
    /// <param name="applicationName">Application name</param>
    /// <param name="machineName">Machine name</param>
    /// <param name="triggerName">Trigger name</param>
    /// <returns>True if the trigger must be trigged; otherwise, false.</returns>
    public delegate bool UpdateEventTriggerDelegate(string environment, string applicationName, string machineName, string triggerName);

    /// <summary>
    /// Update event trigger
    /// </summary>
    [StatusName("Update Event Trigger")]
    public class UpdateEventTrigger : TriggerBase
    {
        private volatile bool _processing;
        private CancellationTokenSource _tokenSource;
        private Timer _timer;
        private readonly string _triggerName;

        #region Events
        /// <summary>
        /// Event to check if an update has been made and the trigger must be triggered.
        /// </summary>
        public event UpdateEventTriggerDelegate OnEventTriggerCheck;
        #endregion

        #region Properties
        /// <summary>
        /// TimeSpan Check Frequency
        /// </summary>
        public TimeSpan CheckFrequency { get; set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Update event trigger with a default check frequency of 60 sec
        /// </summary>
        /// <param name="triggerName">Trigger name</param>
        public UpdateEventTrigger(string triggerName)
        {
            _triggerName = triggerName;
            CheckFrequency = TimeSpan.FromSeconds(60);
        }
        /// <summary>
        /// Update event trigger
        /// </summary>
        /// <param name="triggerName">Trigger name</param>
        /// <param name="checkFrequency">Check frequency timespan</param>
        public UpdateEventTrigger(string triggerName, TimeSpan checkFrequency)
        {
            _triggerName = triggerName;
            CheckFrequency = checkFrequency;
        }
        #endregion

        #region Overrides
        /// <summary>
        /// On trigger init
        /// </summary>
        protected override void OnInit()
        {
            Core.Log.LibVerbose("{0}: OnInit(), setting frequency {1}", GetType().Name, CheckFrequency);
            _tokenSource = new CancellationTokenSource();
            _timer = new Timer(obj =>
            {
                if (_processing) return;
                _processing = true;
                var tSource = (CancellationTokenSource)obj;
                if (tSource.Token.IsCancellationRequested) return;
                Core.Log.LibVerbose("{0}: Trigger call", GetType().Name);
                if (OnEventTriggerCheck == null) return;
                try
                {
                    var shouldUpdate = OnEventTriggerCheck(Core.EnvironmentName, Core.ApplicationName, Core.MachineName, _triggerName);
                    if (shouldUpdate)
                        Trigger();
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
                _processing = false;
            }, _tokenSource, CheckFrequency, CheckFrequency);
        }
        /// <summary>
        /// On trigger finalize
        /// </summary>
        protected override void OnFinalize()
        {
            Core.Log.LibVerbose("{0}: OnFinalize()", GetType().Name);
            _tokenSource?.Cancel();
            if (_timer == null) return;
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _timer.Dispose();
            _timer = null;
        }
        #endregion
    }
}
