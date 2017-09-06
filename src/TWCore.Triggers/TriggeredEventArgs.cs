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
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Triggers
{
    /// <summary>
    /// Triggered instance event args
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    public class TriggeredEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Update trigger instance
        /// </summary>
        public TriggerBase Trigger { get; private set; }
        /// <summary>
        /// Current Value
        /// </summary>
        public T Value { get; private set; }
        /// <summary>
        /// Elapsed Time
        /// </summary>
        public double Elapsed { get; private set; }

        #region .ctor
        /// <summary>
        /// Triggered instance event args
        /// </summary>
        /// <param name="trigger">Trigger instance</param>
        /// <param name="value">Current Value</param>
        /// <param name="elapsed">Time elapsed</param>
        public TriggeredEventArgs(TriggerBase trigger, T value, double elapsed = 0)
        {
            Trigger = trigger;
            Value = value;
            Elapsed = elapsed;
        }
        #endregion
    }
}
