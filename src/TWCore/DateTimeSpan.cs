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
using System.Runtime.CompilerServices;
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable UnusedMember.Global
// ReSharper disable InheritdocConsiderUsage

namespace TWCore
{
    /// <summary>
    /// DateTimeSpan Structure for comparing a get differences of dates
    /// </summary>
    public readonly struct DateTimeSpan : IEquatable<DateTimeSpan>
    {
        #region Private fields
        private readonly int _years;
        private readonly int _months;
        private readonly int _days;
        private readonly int _hours;
        private readonly int _minutes;
        private readonly int _seconds;
        private readonly int _milliseconds;
        private readonly int _hashCode;
        private enum Phase { Years, Months, Days, Done }
        #endregion

        #region .ctor
        /// <summary>
        /// DateTimeSpan Structure for comparing a get differences of dates
        /// </summary>
        /// <param name="years">Years</param>
        /// <param name="months">Months</param>
        /// <param name="days">Days</param>
        /// <param name="hours">Hours</param>
        /// <param name="minutes">Minutes</param>
        /// <param name="seconds">Seconds</param>
        /// <param name="milliseconds">Milliseconds</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeSpan(int years, int months, int days, int hours, int minutes, int seconds, int milliseconds)
        {
            _years = years;
            _months = months;
            _days = days;
            _hours = hours;
            _minutes = minutes;
            _seconds = seconds;
            _milliseconds = milliseconds;
            var hash = 13;
            hash = (hash * 7) + years.GetHashCode();
            hash = (hash * 7) + months.GetHashCode();
            hash = (hash * 7) + days.GetHashCode();
            hash = (hash * 7) + hours.GetHashCode();
            hash = (hash * 7) + minutes.GetHashCode();
            hash = (hash * 7) + seconds.GetHashCode();
            hash = (hash * 7) + milliseconds.GetHashCode();
            _hashCode = hash;
        }
        #endregion

        #region Equality
        /// <summary>
        /// Get Hash Code
        /// </summary>
        /// <returns>Hash code for the instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => _hashCode;
        /// <inheritdoc />
        /// <summary>
        /// Check if this DateTimeSpan is equal to other
        /// </summary>
        /// <param name="other">The other instance of DateTimeSpan</param>
        /// <returns>True if is equal</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(DateTimeSpan other)
        {
            if (_years != other._years) return false;
            if (_months != other._months) return false;
            if (_days != other._days) return false;
            if (_hours != other._hours) return false;
            if (_minutes != other._minutes) return false;
            if (_seconds != other._seconds) return false;
            if (_milliseconds != other._milliseconds) return false;
            return true;
        }
        /// <summary>
        /// Check if this DateTimeSpan is equal to other object
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True if is equal</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj is null || GetType() != obj.GetType())
                return false;
            return Equals((DateTimeSpan)obj);
        }
        /// <summary>
        /// Check if this DateTimeSpan is equal to other DateTimeSpan
        /// </summary>
        /// <param name="c1">First DateTimeSpan</param>
        /// <param name="c2">Second DateTimeSpan</param>
        /// <returns>true if both DateTimeSpan are equals, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(DateTimeSpan c1, DateTimeSpan c2)
        {
            return c1.Equals(c2);
        }
        /// <summary>
        /// Check if this DateTimeSpan is different to other DateTimeSpan
        /// </summary>
        /// <param name="c1">First DateTimeSpan</param>
        /// <param name="c2">Second DateTimeSpan</param>
        /// <returns>true if both DateTimeSpan are differents, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(DateTimeSpan c1, DateTimeSpan c2)
        {
            return !c1.Equals(c2);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Year component of the structure
        /// </summary>
        public int Years => _years;
        /// <summary>
        /// Month component of the structure
        /// </summary>
        public int Months => _months;
        /// <summary>
        /// Days component of the structure
        /// </summary>
        public int Days => _days;
        /// <summary>
        /// Hours component of the structure
        /// </summary>
        public int Hours => _hours;
        /// <summary>
        /// Minutes component of the structure
        /// </summary>
        public int Minutes => _minutes;
        /// <summary>
        /// Seconds component of the structure
        /// </summary>
        public int Seconds => _seconds;
        /// <summary>
        /// Milliseconds component of the structure
        /// </summary>
        public int Milliseconds => _milliseconds;
        #endregion

        #region Methods
        /// <summary>
        /// Get the diference between two DateTimes with all the date components
        /// </summary>
        /// <param name="date1">DateTime</param>
        /// <param name="date2">DateTime</param>
        /// <returns>DateTimeSpan structure with the difference between the two dates</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTimeSpan CompareDates(DateTime date1, DateTime date2)
        {
            if (date2 < date1)
            {
                var sub = date1;
                date1 = date2;
                date2 = sub;
            }

            var current = date1;
            var years = 0;
            var months = 0;
            var days = 0;
            var phase = Phase.Years;
            var span = new DateTimeSpan();

            while (phase != Phase.Done)
            {
                switch (phase)
                {
                    case Phase.Years:
                        if (current.AddYears(years + 1) > date2)
                        {
                            phase = Phase.Months;
                            current = current.AddYears(years);
                        }
                        else
                            years++;
                        break;
                    case Phase.Months:
                        if (current.AddMonths(months + 1) > date2)
                        {
                            phase = Phase.Days;
                            current = current.AddMonths(months);
                        }
                        else
                            months++;
                        break;
                    case Phase.Days:
                        if (current.AddDays(days + 1) > date2)
                        {
                            current = current.AddDays(days);
                            var timespan = date2 - current;
                            span = new DateTimeSpan(years, months, days, timespan.Hours, timespan.Minutes, timespan.Seconds, timespan.Milliseconds);
                            phase = Phase.Done;
                        }
                        else
                            days++;
                        break;
                    case Phase.Done:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return span;
        }
        #endregion
    }
}
