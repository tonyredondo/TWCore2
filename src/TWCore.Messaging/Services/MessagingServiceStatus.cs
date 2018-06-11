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
// ReSharper disable NotAccessedField.Local
// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter

namespace TWCore.Services
{
    /// <summary>
    /// Messaging Service status
    /// </summary>
    [StatusName("Messaging Service Status")]
    public class MessagingServiceStatus
    {
        private double? _processTimes;
        private double? _lastMinuteProcessTimes;
        private readonly DayStatus _dayStatus = new DayStatus("Day Statistics");
        private Timer _timer;
        private long _currentMessagesBeingProcessed;
        private long _peakCurrentMessagesBeingProcessed;
        private DateTime _peakCurrentMessagesBeingProcessedLastDate;
        private long _lastMinuteMessagesBeingProcessed;
        private long _peakLastMinuteMessagesBeingProcessed;
        private DateTime _peakLastMinuteMessagesBeingProcessedLastDate;
        private DateTime _lastMessageDateTime;
        private DateTime _lastProcessingDateTime;
        private long _totalMessagesReceived;
        private long _totalMessagesProccesed;
        private long _totalExceptions;
        
        #region Properties
        /// <summary>
        /// Process average time
        /// </summary>
        public double ProcessAverageTime => _processTimes ?? 0;
        /// <summary>
        /// Process average time on the last minute
        /// </summary>
        public double LastMinuteProcessAverageTime => _lastMinuteProcessTimes ?? 0;

        /// <summary>
        /// Number of current active processing threads
        /// </summary>
        public long CurrentMessagesBeingProcessed => _currentMessagesBeingProcessed;
        /// <summary>
        /// Peak value of number of active processing threads
        /// </summary>
        public long PeakCurrentMessagesBeingProcessed => _peakCurrentMessagesBeingProcessed;
        /// <summary>
        /// Date and time of the peak value of number of active processing threads
        /// </summary>
        public DateTime PeakCurrentMessagesBeingProcessedLastDate => _peakCurrentMessagesBeingProcessedLastDate;

        /// <summary>
        /// Number of active processing threads on the last minute
        /// </summary>
        public long LastMinuteMessagesBeingProcessed => _lastMinuteMessagesBeingProcessed;
        /// <summary>
        /// Peak value of the number of active processing threads on the last minute
        /// </summary>
        public long PeakLastMinuteMessagesBeingProcessed => _peakLastMinuteMessagesBeingProcessed;
        /// <summary>
        /// Date and time of the peak value of number of active processing threads on the last minute
        /// </summary>
        public DateTime PeakLastMinuteMessagesBeingProcessedLastDate => _peakLastMinuteMessagesBeingProcessedLastDate;

        /// <summary>
        /// Date and time of the last received message
        /// </summary>
        public DateTime LastMessageDateTime => _lastMessageDateTime;
        /// <summary>
        /// Date and time of the last process of a message
        /// </summary>
        public DateTime LastProcessingDateTime => _lastProcessingDateTime;

        /// <summary>
        /// Number of received messages
        /// </summary>
        public long TotalMessagesReceived => _totalMessagesReceived;
        /// <summary>
        /// Number of processed messages
        /// </summary>
        public long TotalMessagesProccesed => _totalMessagesProccesed;
        /// <summary>
        /// Number of exceptions
        /// </summary>
        public long TotalExceptions => _totalExceptions;
        #endregion

        #region .ctor
        /// <summary>
        /// Messaging service status
        /// </summary>
        public MessagingServiceStatus(IMessagingServiceAsync messagingService)
        {
            _timer = new Timer(state =>
            {
                _lastMinuteProcessTimes = null;
                _lastMinuteMessagesBeingProcessed = _currentMessagesBeingProcessed;
                _peakLastMinuteMessagesBeingProcessed = _currentMessagesBeingProcessed;
                _peakLastMinuteMessagesBeingProcessedLastDate = _lastProcessingDateTime;
            }, this, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            Core.Status.Attach(collection =>
            {
                collection.Add("Message process time average",
                    new StatusItemValueItem("Historic Time (ms)", ProcessAverageTime, true),
                    new StatusItemValueItem("Last Minute Time (ms)", LastMinuteProcessAverageTime, true));

                collection.Add("Current active processing threads",
                    new StatusItemValueItem("Quantity", CurrentMessagesBeingProcessed, true),
                    new StatusItemValueItem("Peak Quantity", PeakCurrentMessagesBeingProcessed, true),
                    new StatusItemValueItem("Peak DateTime", PeakCurrentMessagesBeingProcessedLastDate),
                    new StatusItemValueItem("Last Message Date", LastMessageDateTime),
                    new StatusItemValueItem("Last Processing Date", LastProcessingDateTime));

                collection.Add("Last minute active processed threads",
                    new StatusItemValueItem("Quantity", LastMinuteMessagesBeingProcessed, true),
                    new StatusItemValueItem("Peak Quantity", PeakLastMinuteMessagesBeingProcessed, true),
                    new StatusItemValueItem("Peak DateTime", PeakLastMinuteMessagesBeingProcessedLastDate));

                collection.Add("Totals",
                    new StatusItemValueItem("Message Received", TotalMessagesReceived, true),
                    new StatusItemValueItem("Message Processed", TotalMessagesProccesed, true),
                    new StatusItemValueItem("Exceptions", TotalExceptions, true));
                
                Core.Status.AttachChild(messagingService.QueueServer, this);
                Core.Status.AttachChild(messagingService.Processor, this);
                Core.Status.AttachChild(_dayStatus, this);
            }, this);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Increments the total exceptions number
        /// </summary>
        public void IncrementTotalExceptions()
        {
            Interlocked.Increment(ref _totalExceptions);
            _dayStatus.Register("Exceptions");
        }
        /// <summary>
        /// Increments the total exceptions number
        /// </summary>
        public void IncrementTotalMessagesProccesed()
        {
            Interlocked.Increment(ref _totalMessagesProccesed);
        }
        /// <summary>
        /// Increments the messages being processed
        /// </summary>
        public void IncrementCurrentMessagesBeingProcessed()
        {
            Interlocked.Increment(ref _currentMessagesBeingProcessed);
            Interlocked.Increment(ref _totalMessagesReceived);
            Interlocked.Increment(ref _lastMinuteMessagesBeingProcessed);
            _lastMessageDateTime = Core.Now;
            var cmbp = Interlocked.Read(ref _currentMessagesBeingProcessed);
            if (cmbp >= Interlocked.Read(ref _peakCurrentMessagesBeingProcessed))
            {
                Interlocked.Exchange(ref _peakCurrentMessagesBeingProcessed, cmbp);
                _peakCurrentMessagesBeingProcessedLastDate = Core.Now;
            }
            var lmbp = Interlocked.Read(ref _lastMinuteMessagesBeingProcessed);
            if (lmbp >= Interlocked.Read(ref _peakLastMinuteMessagesBeingProcessed))
            {
                Interlocked.Exchange(ref _peakLastMinuteMessagesBeingProcessed, lmbp);
                _peakLastMinuteMessagesBeingProcessedLastDate = Core.Now;
            }
        }
        /// <summary>
        /// Reports the processing time
        /// </summary>
        public void ReportProcessingTime(double time)
        {
            _dayStatus.Register("Successfully Processed Messages", time);
            _lastProcessingDateTime = Core.Now;
            var pTime = _processTimes;
            var lmpTime = _lastMinuteProcessTimes;
            pTime = pTime * 0.8 + time * 0.2 ?? time;
            lmpTime = lmpTime * 0.8 + time * 0.2 ?? time;
            _processTimes = pTime;
            _lastMinuteProcessTimes = lmpTime;
        }
        /// <summary>
        /// Decrement the current processing threads
        /// </summary>
        public void DecrementCurrentMessagesBeingProcessed()
        {
            Interlocked.Decrement(ref _currentMessagesBeingProcessed);
        }
        #endregion
    }
}
