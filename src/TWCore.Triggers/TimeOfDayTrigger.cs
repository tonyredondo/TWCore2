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
    /// Time of Day Update Trigger
    /// </summary>
	public class TimeOfDayTrigger : TriggerBase
    {
        /// <summary>
		/// Time of day for the Trigger
		/// </summary>
        public TimeSpan Time { get; }

        #region Private Fields
        private CancellationTokenSource _tokenSource;
        private Timer _timer;
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Time of Day Update Trigger
        /// </summary>
        /// <param name="time">Time of the day to fire the trigger</param>
		public TimeOfDayTrigger(TimeSpan time)
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
            TimeSpan startTime;
            var nowTime = Core.Now.TimeOfDay;
            startTime = nowTime < Time ? 
                Time.Subtract(nowTime) : 
                Time.Add(TimeSpan.FromDays(1)).Subtract(nowTime);

            Core.Log.LibVerbose("{0}: OnInit() for TimeOfDay: {1}. Next update on: {2}", GetType().Name, Time, startTime);

            _tokenSource = new CancellationTokenSource();
            _timer = new Timer(obj =>
            {
                var tSource = (CancellationTokenSource)obj;
                if (!tSource.Token.IsCancellationRequested)
                {
                    Core.Log.LibVerbose("{0}: Trigger call", GetType().Name);
                    Trigger();
                }
            }, _tokenSource, startTime, TimeSpan.FromDays(1));
        }
        /// <inheritdoc />
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
