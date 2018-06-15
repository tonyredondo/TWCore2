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

namespace TWCore.Triggers
{
    /// <inheritdoc />
    /// <summary>
    /// Date Update Trigger
    /// </summary>
    [StatusName("DateTime Trigger")]
    public class DateTimeTrigger : TriggerBase
    {
        private volatile bool _processing;
        /// <summary>
        /// Trigger DateTime
        /// </summary>
        public DateTime DateTime { get; }

        #region Private Fields
        private CancellationTokenSource _tokenSource;
        private Timer _timer;
        #endregion

        #region .ctor
        /// <inheritdoc />
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
        /// <inheritdoc />
        /// <summary>
        /// On trigger init
        /// </summary>
        protected override void OnInit()
        {
            Core.Log.LibVerbose("{0}: OnInit() for DateTime {1}", GetType().Name, DateTime);
            _tokenSource = new CancellationTokenSource();
            _timer = new Timer(obj =>
            {
                if (_processing) return;
                _processing = true;
                var tSource = (CancellationTokenSource)obj;
                if (tSource.Token.IsCancellationRequested) return;
                Core.Log.LibVerbose("{0}: Trigger call", GetType().Name);
                try
                {
                    Trigger();
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
                _processing = false;
            }, _tokenSource, Core.Now.Subtract(DateTime), Timeout.InfiniteTimeSpan);
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
