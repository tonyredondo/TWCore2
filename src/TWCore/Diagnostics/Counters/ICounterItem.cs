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

namespace TWCore.Diagnostics.Counters
{
    /// <summary>
    /// Counter item interface
    /// </summary>
    public interface ICounterItem
    {
        /// <summary>
        /// Gets or sets the counter environment
        /// </summary>
        string Environment { get; }
        /// <summary>
        /// Gets or sets the counter application name
        /// </summary>
        string Application { get; }
        /// <summary>
        /// Gets or sets the counter category
        /// </summary>
        string Category { get; set; }
        /// <summary>
        /// Gets or sets the counter name
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Gets or sets the counter type
        /// </summary>
        CounterType Type { get; set; }
        /// <summary>
        /// Gets or sets the counter level
        /// </summary>
        CounterLevel Level { get; set; }
        /// <summary>
        /// Gets the counter kind
        /// </summary>
        CounterKind Kind { get; set; }
        /// <summary>
        /// Gets the counter unit
        /// </summary>
        CounterUnit Unit { get; set; }
        /// <summary>
        /// Type of value
        /// </summary>
        Type TypeOfValue { get; }
        /// <summary>
        /// Values Count
        /// </summary>
        int Count { get; }
    }
}