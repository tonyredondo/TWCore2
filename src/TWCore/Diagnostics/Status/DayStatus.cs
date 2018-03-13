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

using System.Linq;
// ReSharper disable NotAccessedField.Local
// ReSharper disable MemberCanBePrivate.Local

namespace TWCore.Diagnostics.Status
{
    /// <inheritdoc />
    /// <summary>
    /// Statuses grouped by Day/Hour
    /// </summary>
    public class DayStatus : IStatusItemProvider
    {
        private readonly NonBlocking.ConcurrentDictionary<string, IStatusItemProvider> _data = new NonBlocking.ConcurrentDictionary<string, IStatusItemProvider>();

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Name of the Status Item
        /// </summary>
        public string Name { get; set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Statuses grouped by Day/Hour
        /// </summary>
        public DayStatus(string name)
        {
            Name = name;
            Core.Status.Attach(GetStatusItem);
        }
        #endregion

        #region Public Methods
        /// <inheritdoc />
        /// <summary>
        /// Get's the Status Item instance
        /// </summary>
        /// <returns>StatusItem Instance</returns>
        public StatusItem GetStatusItem()
        {
            var baseItem = new StatusItem { Name = Name };
            var values = _data.Values.ToArray().Select(v => v.GetStatusItem());
            baseItem.Children.AddRange(values);
            return baseItem;
        }

        /// <summary>
        /// Register a call on the status with a name
        /// </summary>
        /// <param name="name">Name of the status category</param>
        public void Register(string name)
        {
            var status = _data.GetOrAdd(name, mName => new DayStatusCalls(mName));
            if (status is DayStatusCalls sCalls)
                sCalls.Register();
        }
        /// <summary>
        /// Register a value on the status with a name
        /// </summary>
        /// <param name="name">Name of the status category</param>
        /// <param name="value">Value to register</param>
        public void Register<T>(string name, T value)
        {
            var status = _data.GetOrAdd(name, mName => new DayStatusValue<T>(mName));
            if (status is DayStatusValue<T> sValues)
                sValues.Register(value);
        }
        #endregion
    }
}
