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
    /// <summary>
	/// Periodic Update Trigger
	/// </summary>
	public class PeriodicTrigger : TriggerBase
    {
        /// <summary>
        /// Periodic Time to use by the trigger
        /// </summary>
        public TimeSpan Time { get; private set; }

        #region Private Fields
        CancellationTokenSource tokenSource = null;
        Timer timer = null;
        #endregion

        #region .ctor
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
        /// <summary>
        /// On trigger init
        /// </summary>
        protected override void OnInit()
        {
            Core.Log.LibVerbose("{0}: OnInit() for Every {1}", this.GetType().Name, Time);
            tokenSource = new CancellationTokenSource();
            timer = new Timer(obj =>
            {
                var tSource = (CancellationTokenSource)obj;
                if (!tSource.Token.IsCancellationRequested)
                {
                    Core.Log.LibVerbose("{0}: Trigger call", this.GetType().Name);
                    Trigger();
                }
            }, tokenSource, Time, Time);
        }
        /// <summary>
        /// On init finalize
        /// </summary>
        protected override void OnFinalize()
        {
            Core.Log.LibVerbose("{0}: OnFinalize()", this.GetType().Name);
            if (tokenSource != null)
                tokenSource.Cancel();
            if (timer != null)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                timer.Dispose();
                timer = null;
            }
        }
        #endregion
    }
}
