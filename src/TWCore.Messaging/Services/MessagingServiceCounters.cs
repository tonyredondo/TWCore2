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

namespace TWCore.Services
{
    /// <summary>
    /// Messaging Service counters
    /// </summary>
    public class MessagingServiceCounters
    {
        readonly object locker = new object();
        Timer timer;

        readonly Queue<double> processTimes = new Queue<double>();
        readonly Queue<double> lastMinuteProcessTimes = new Queue<double>();

        #region Properties
        /// <summary>
        /// Process average time
        /// </summary>
        public double ProcessAverageTime
        {
            get
            {
                lock (locker)
                    return processTimes.Count > 0 ? processTimes.Average() : 0;
            }
        }
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
        public double LastMinuteProcessAverageTime
        {
            get
            {
                lock (locker)
                    return lastMinuteProcessTimes.Count > 0 ? lastMinuteProcessTimes.Average() : 0;
            }
        }
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
            timer = new Timer(state =>
            {
                lock (locker)
                {
                    lastMinuteProcessTimes.Clear();
                    PeakLastMinuteProcessAverageTime = 0;
                    PeakLastMinuteProcessAverageTimeLastDate = DateTime.MinValue;

                    LastMinuteMessagesBeingProcessed = CurrentMessagesBeingProcessed;
                    PeakLastMinuteMessagesBeingProcessed = CurrentMessagesBeingProcessed;
                    PeakLastMinuteMessagesBeingProcessedLastDate = LastProcessingDateTime;
                }
            }, this, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            Core.Status.Attach(collection =>
            {
                collection.Add("Process average time in ms", ProcessAverageTime);
                collection.Add("Peak value of process average time in ms", PeakProcessAverageTime);
                collection.Add("Date and time of the peak value of process average time", PeakProcessAverageTimeLastDate);

                collection.Add("Process average time on the last minute in ms", LastMinuteProcessAverageTime);
                collection.Add("Peak value of Process average time on the last minute in ms", PeakLastMinuteProcessAverageTime);
                collection.Add("Date and time of the peak value of Process average time on the last minute", PeakLastMinuteProcessAverageTimeLastDate);

                collection.Add("Number of current active processing threads", CurrentMessagesBeingProcessed);
                collection.Add("Peak value of number of active processing threads", PeakCurrentMessagesBeingProcessed);
                collection.Add("Date and time of the peak value of number of active processing threads", PeakCurrentMessagesBeingProcessedLastDate);
                collection.Add("Number of active processing threads on the last minute", LastMinuteMessagesBeingProcessed);
                collection.Add("Peak value of the number of active processing threads on the last minute", PeakLastMinuteMessagesBeingProcessed);
                collection.Add("Date and time of the peak value of number of active processing threads on the last minute", PeakLastMinuteMessagesBeingProcessedLastDate);
                collection.Add("Date and time of the last received message", LastMessageDateTime);
                collection.Add("Date and time of the last process of a message", LastProcessingDateTime);

                collection.Add("Number of received messages", TotalMessagesReceived, StatusItemValueStatus.Green);
                collection.Add("Number of processed messages", TotalMessagesProccesed, StatusItemValueStatus.Green);
                collection.Add("Number of exceptions", TotalExceptions, StatusItemValueStatus.Red);
            });
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Increments the total exceptions number
        /// </summary>
        public void IncrementTotalExceptions()
        {
            lock (locker)
                TotalExceptions++;
        }
        /// <summary>
        /// Increments the total exceptions number
        /// </summary>
        public void IncrementTotalMessagesProccesed()
        {
            lock (locker)
                TotalMessagesProccesed++;
        }
        /// <summary>
        /// Increments the messages being processed
        /// </summary>
        public void IncrementCurrentMessagesBeingProcessed()
        {
            lock (locker)
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
            lock (locker)
            {
                processTimes.Enqueue(time);
                if (processTimes.Count > 1000)
                    processTimes.Dequeue();

                lastMinuteProcessTimes.Enqueue(time);
                if (lastMinuteProcessTimes.Count > 1000)
                    lastMinuteProcessTimes.Dequeue();

                var avgPT = ProcessAverageTime;
                if (avgPT > PeakProcessAverageTime)
                {
                    PeakProcessAverageTime = avgPT;
                    PeakProcessAverageTimeLastDate = Core.Now;
                }

                var avgLMPT = LastMinuteProcessAverageTime;
                if (avgLMPT > PeakLastMinuteProcessAverageTime)
                {
                    PeakLastMinuteProcessAverageTime = avgLMPT;
                    PeakProcessAverageTimeLastDate = Core.Now;
                }
            }
        }
        /// <summary>
        /// Decrement the current processing threads
        /// </summary>
        public void DecrementCurrentMessagesBeingProcessed()
        {
            lock (locker)
                CurrentMessagesBeingProcessed--;
        }
        #endregion
    }
}
