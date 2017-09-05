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

namespace TWCore.Triggers
{
    /// <inheritdoc />
    /// <summary>
    /// Periodic Update Trigger
    /// </summary>
	public class PeriodicTrigger : TriggerBase
    {
        /// <summary>
        /// Periodic Time to use by the trigger
        /// </summary>
        public TimeSpan Time { get; }

        #region Private Fields
        private CancellationTokenSource _tokenSource;
        private Timer _timer;
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Periodic Update Trigger
        /// </summary>
        /// <param name="time">Period value for the trigger</param>
        public PeriodicTrigger(TimeSpan time)
        {
            Time = time;

            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(Time), Time);
            });
        }
        #endregion

        #region Overrides
        /// <inheritdoc />
        /// <summary>
        /// On trigger init
        /// </summary>
        protected override void OnInit()
        {
            Core.Log.LibVerbose("{0}: OnInit() for Every {1}", GetType().Name, Time);
            _tokenSource = new CancellationTokenSource();
            _timer = new Timer(obj =>
            {
                var tSource = (CancellationTokenSource)obj;
                if (tSource.Token.IsCancellationRequested) return;
                Core.Log.LibVerbose("{0}: Trigger call", GetType().Name);
                Trigger();
            }, _tokenSource, Time, Time);
        }
        /// <inheritdoc />
        /// <summary>
        /// On init finalize
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
