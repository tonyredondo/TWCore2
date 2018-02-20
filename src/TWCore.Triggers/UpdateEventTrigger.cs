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
// ReSharper disable EventNeverSubscribedTo.Global

namespace TWCore.Triggers
{
    public delegate bool UpdateEventTriggerDelegate(string environment, string applicationName, string machineName, string triggerName);

    public class UpdateEventTrigger : TriggerBase
    {
        private CancellationTokenSource _tokenSource;
        private Timer _timer;
        private readonly string _triggerName;

        #region Events
        public event UpdateEventTriggerDelegate OnEventTriggerCheck;
        #endregion

        #region Properties
        /// <summary>
        /// TimeSpan Check Frequency
        /// </summary>
        public TimeSpan CheckFrequency { get; set; }
        #endregion

        #region .ctor
        public UpdateEventTrigger(string triggerName)
        {
            _triggerName = triggerName;
            CheckFrequency = TimeSpan.FromSeconds(60);
        }
        public UpdateEventTrigger(string triggerName, TimeSpan checkFrequency)
        {
            _triggerName = triggerName;
            CheckFrequency = checkFrequency;
        }
        #endregion

        #region Overrides
        protected override void OnInit()
        {
            Core.Log.LibVerbose("{0}: OnInit(), setting frequency {1}", GetType().Name, CheckFrequency);
            _tokenSource = new CancellationTokenSource();
            _timer = new Timer(obj =>
            {
                var tSource = (CancellationTokenSource)obj;
                if (tSource.Token.IsCancellationRequested) return;
                Core.Log.LibVerbose("{0}: Trigger call", GetType().Name);
                if (OnEventTriggerCheck == null) return;

                var shouldUpdate = OnEventTriggerCheck(Core.EnvironmentName, Core.ApplicationName, Core.MachineName, _triggerName);
                if (shouldUpdate)
                    Trigger();

            }, _tokenSource, CheckFrequency, CheckFrequency);
        }
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
