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
    /// Date Update Trigger
    /// </summary>
    public class DateTimeTrigger : TriggerBase
    {
        /// <summary>
        /// Trigger DateTime
        /// </summary>
        public DateTime DateTime { get; private set; }

        #region Private Fields
        CancellationTokenSource tokenSource;
        Timer timer;
        #endregion

        #region .ctor
        /// <summary>
        /// Date Upate Trigger
        /// </summary>
        /// <param name="dateTime">DateTime value of the trigger</param>
        public DateTimeTrigger(DateTime dateTime)
        {
            DateTime = dateTime;

            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(DateTime), DateTime);
            });
        }
        #endregion

        #region Overrides
        /// <summary>
        /// On trigger init
        /// </summary>
        protected override void OnInit()
        {
            Core.Log.LibVerbose("{0}: OnInit() for DateTime {1}", GetType().Name, DateTime);
            tokenSource = new CancellationTokenSource();
            timer = new Timer(obj =>
            {
                var tSource = (CancellationTokenSource)obj;
                if (tSource.Token.IsCancellationRequested) return;
                Core.Log.LibVerbose("{0}: Trigger call", GetType().Name);
                Trigger();
            }, tokenSource, Core.Now.Subtract(DateTime), Timeout.InfiniteTimeSpan);
        }
        /// <summary>
        /// On trigger finalize
        /// </summary>
        protected override void OnFinalize()
        {
            Core.Log.LibVerbose("{0}: OnFinalize()", GetType().Name);
            tokenSource?.Cancel();
            if (timer == null) return;
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            timer.Dispose();
            timer = null;
        }
        #endregion
    }
}
