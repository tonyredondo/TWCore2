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

namespace TWCore.Triggers
{
    /// <summary>
    /// Day Plan Update Trigger
    /// </summary>
    public class DayPlanTrigger : TriggerBase
    {
        TimeOfDayTrigger[] Triggers = null;

        #region .ctor
        /// <summary>
        /// Day Plan Update Trigger
        /// </summary>
        /// <param name="times">Times of the day when the trigger will execute.</param>
        public DayPlanTrigger(params TimeSpan[] times)
        {
            if (times != null)
            {
                Triggers = new TimeOfDayTrigger[times.Length];
                for (var i = 0; i < times.Length; i++)
                {
                    Triggers[i] = new TimeOfDayTrigger(times[i]);
                    Triggers[i].OnTriggered += ChildTrigger;
                }
            }
        }
        /// <summary>
        /// Day Plan Update Trigger
        /// </summary>
        /// /// <param name="timesInString">Times of the day when the trigger will execute.</param>
        public DayPlanTrigger(params string[] timesInString)
        {
            if (timesInString != null)
            {
                Triggers = new TimeOfDayTrigger[timesInString.Length];
                for (var i = 0; i < timesInString.Length; i++)
                {
                    var time = timesInString[i].ParseTo<TimeSpan?>(null);
                    if (time.HasValue)
                    {
                        Triggers[i] = new TimeOfDayTrigger(time.Value);
                        Triggers[i].OnTriggered += ChildTrigger;
                    }
                    else
                        throw new FormatException("The Time: {0} for the DayPlanTrigger can't be parsed as a TimeSpan value".ApplyFormat(timesInString[i]));
                }
            }
        }
        #endregion

        #region Overrides
        /// <summary>
        /// On trigger init
        /// </summary>
        protected override void OnInit()
        {
            if (Triggers != null)
            {
                Core.Log.LibVerbose("{0}: OnInit()", GetType().Name);
                foreach (var trigger in Triggers)
                    trigger.Init();
            }
        }
        /// <summary>
        /// On trigger finalize
        /// </summary>
        protected override void OnFinalize()
        {
            if (Triggers != null)
            {
                Core.Log.LibVerbose("{0}: OnFinalize()", GetType().Name);
                foreach (var trigger in Triggers)
                    trigger.Dispose();
            }
        }
        #endregion

        #region Private Methods
        void ChildTrigger(TriggerBase child)
        {
            Core.Log.LibVerbose("{0}: Plan Trigger call", GetType().Name);
            Trigger();
        }
        #endregion
    }
}
