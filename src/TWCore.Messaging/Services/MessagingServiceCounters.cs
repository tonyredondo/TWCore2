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
using System.Linq;
using System.Threading;
using TWCore.Diagnostics.Status;
// ReSharper disable NotAccessedField.Local
// ReSharper disable CheckNamespace

namespace TWCore.Services
{
    /// <summary>
    /// Messaging Service counters
    /// </summary>
    public class MessagingServiceCounters
    {
        private readonly object _locker = new object();
        private double? _processTimes;
        private double? _lastMinuteProcessTimes;
        private readonly DayStatus _dayStatus = new DayStatus("Day Statistics");
        private Timer _timer;

        #region Properties
        /// <summary>
        /// Process average time
        /// </summary>
        public double ProcessAverageTime => _processTimes ?? 0;
        /// <summary>
        /// Peak value of process average time
        /// </summary>
        public double PeakProcessAverageTime { get; private set; }
        /// <summary>
        /// Date and time of the peak value of process average time
        /// </summary>
        public DateTime PeakProcessAverageTimeLastDate { get; private set; }

        /// <summary>
        /// Process average time on the last minute
        /// </summary>
        public double LastMinuteProcessAverageTime => _lastMinuteProcessTimes ?? 0;
        /// <summary>
        /// Peak value of process average time on the last minute
        /// </summary>
        public double PeakLastMinuteProcessAverageTime { get; private set; }
        /// <summary>
        /// Date and time of the peak value of process average time on the last minute
        /// </summary>
        public DateTime PeakLastMinuteProcessAverageTimeLastDate { get; private set; }

        /// <summary>
        /// Number of current active processing threads
        /// </summary>
        public long CurrentMessagesBeingProcessed { get; private set; }
        /// <summary>
        /// Peak value of number of active processing threads
        /// </summary>
        public long PeakCurrentMessagesBeingProcessed { get; private set; }
        /// <summary>
        /// Date and time of the peak value of number of active processing threads
        /// </summary>
        public DateTime PeakCurrentMessagesBeingProcessedLastDate { get; private set; }

        /// <summary>
        /// Number of active processing threads on the last minute
        /// </summary>
        public long LastMinuteMessagesBeingProcessed { get; private set; }
        /// <summary>
        /// Peak value of the number of active processing threads on the last minute
        /// </summary>
        public long PeakLastMinuteMessagesBeingProcessed { get; private set; }
        /// <summary>
        /// Date and time of the peak value of number of active processing threads on the last minute
        /// </summary>
        public DateTime PeakLastMinuteMessagesBeingProcessedLastDate { get; private set; }

        /// <summary>
        /// Date and time of the last received message
        /// </summary>
        public DateTime LastMessageDateTime { get; private set; }
        /// <summary>
        /// Date and time of the last process of a message
        /// </summary>
        public DateTime LastProcessingDateTime { get; private set; }

        /// <summary>
        /// Number of received messages
        /// </summary>
        public long TotalMessagesReceived { get; private set; }
        /// <summary>
        /// Number of processed messages
        /// </summary>
        public long TotalMessagesProccesed { get; private set; }
        /// <summary>
        /// Number of exceptions
        /// </summary>
        public long TotalExceptions { get; private set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Message queue server counters
        /// </summary>
        public MessagingServiceCounters()
        {
            _timer = new Timer(state =>
            {
                lock (_locker)
                {
                    _lastMinuteProcessTimes = null;
                    PeakLastMinuteProcessAverageTime = 0;
                    PeakLastMinuteProcessAverageTimeLastDate = DateTime.MinValue;

                    LastMinuteMessagesBeingProcessed = CurrentMessagesBeingProcessed;
                    PeakLastMinuteMessagesBeingProcessed = CurrentMessagesBeingProcessed;
                    PeakLastMinuteMessagesBeingProcessedLastDate = LastProcessingDateTime;
                }
            }, this, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            Core.Status.Attach(collection =>
            {
                collection.Add("Message process time average",
                    new StatusItemValueItem("Time (ms)", ProcessAverageTime, true),
                    new StatusItemValueItem("Peak Time (ms)", PeakProcessAverageTime, true),
                    new StatusItemValueItem("Peak DateTime", PeakProcessAverageTimeLastDate));

                collection.Add("Message process time average in last minute ",
                    new StatusItemValueItem("Time (ms)", LastMinuteProcessAverageTime, true),
                    new StatusItemValueItem("Peak Time (ms)", PeakLastMinuteProcessAverageTime, true),
                    new StatusItemValueItem("Peak DateTime", PeakLastMinuteProcessAverageTimeLastDate));

                collection.Add("Current active processing threads",
                    new StatusItemValueItem("Quantity", CurrentMessagesBeingProcessed, true),
                    new StatusItemValueItem("Peak Quantity", PeakCurrentMessagesBeingProcessed, true),
                    new StatusItemValueItem("Peak DateTime", PeakCurrentMessagesBeingProcessedLastDate));

                collection.Add("Last minute active processed threads",
                    new StatusItemValueItem("Quantity", LastMinuteMessagesBeingProcessed, true),
                    new StatusItemValueItem("Peak Quantity", PeakLastMinuteMessagesBeingProcessed, true),
                    new StatusItemValueItem("Peak DateTime", PeakLastMinuteMessagesBeingProcessedLastDate));

                collection.Add("Last DateTime",
                    new StatusItemValueItem("Message Received", LastMessageDateTime),
                    new StatusItemValueItem("Message Processed", LastProcessingDateTime));

                collection.Add("Totals",
                    new StatusItemValueItem("Message Received", TotalMessagesReceived, true),
                    new StatusItemValueItem("Message Processed", TotalMessagesProccesed, true),
                    new StatusItemValueItem("Exceptions", TotalExceptions, true));
            });
            Core.Status.AttachChild(_dayStatus, this);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Increments the total exceptions number
        /// </summary>
        public void IncrementTotalExceptions()
        {
            lock (_locker)
                TotalExceptions++;
            _dayStatus.Register("Exceptions");
        }
        /// <summary>
        /// Increments the total exceptions number
        /// </summary>
        public void IncrementTotalMessagesProccesed()
        {
            lock (_locker)
                TotalMessagesProccesed++;
        }
        /// <summary>
        /// Increments the messages being processed
        /// </summary>
        public void IncrementCurrentMessagesBeingProcessed()
        {
            lock (_locker)
            {
                CurrentMessagesBeingProcessed++;
                TotalMessagesReceived++;
                LastMinuteMessagesBeingProcessed++;
                LastMessageDateTime = Core.Now;
                if (CurrentMessagesBeingProcessed >= PeakCurrentMessagesBeingProcessed)
                {
                    PeakCurrentMessagesBeingProcessed = CurrentMessagesBeingProcessed;
                    PeakCurrentMessagesBeingProcessedLastDate = LastMessageDateTime;
                }
                if (LastMinuteMessagesBeingProcessed >= PeakLastMinuteMessagesBeingProcessed)
                {
                    PeakLastMinuteMessagesBeingProcessed = LastMinuteMessagesBeingProcessed;
                    PeakLastMinuteMessagesBeingProcessedLastDate = LastMessageDateTime;
                }
            }
        }
        /// <summary>
        /// Reports the processing time
        /// </summary>
        public void ReportProcessingTime(double time)
        {
            lock (_locker)
            {
                _dayStatus.Register("Processed Messages", time);
                _processTimes = _processTimes.HasValue ? (_processTimes.Value * 0.8) + (time * 0.2) : time;
                _lastMinuteProcessTimes = _lastMinuteProcessTimes.HasValue ? (_lastMinuteProcessTimes.Value * 0.8) + (time * 0.2) : time;

                var avgPt = ProcessAverageTime;
                if (avgPt > PeakProcessAverageTime)
                {
                    PeakProcessAverageTime = avgPt;
                    PeakProcessAverageTimeLastDate = Core.Now;
                }

                var avgLmpt = LastMinuteProcessAverageTime;
                if (avgLmpt > PeakLastMinuteProcessAverageTime)
                {
                    PeakLastMinuteProcessAverageTime = avgLmpt;
                    PeakLastMinuteProcessAverageTimeLastDate = Core.Now;
                }
            }
        }
        /// <summary>
        /// Decrement the current processing threads
        /// </summary>
        public void DecrementCurrentMessagesBeingProcessed()
        {
            lock (_locker)
                CurrentMessagesBeingProcessed--;
        }
        #endregion
    }
}
