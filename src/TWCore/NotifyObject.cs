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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace TWCore
{
    /// <inheritdoc />
    /// <summary>
    /// Property notify changed object base
    /// </summary>
    [DataContract]
    public abstract class NotifyObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Sets the value for a property and check if the value has changed to trigger the PropertyChanged event
        /// </summary>
        /// <typeparam name="TK">Value type</typeparam>
        /// <param name="valueField">Value field object by ref, where the data of the property is stored.</param>
        /// <param name="value">The new value in the property set</param>
        /// <param name="propertyName">Property name, the default value is the caller property name</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void SetValue<TK>(ref TK valueField, TK value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(valueField, value))
            {
                valueField = value;
                OnPropertyChanged(propertyName);
            }
        }
        /// <summary>
        /// Triggers the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">Property name that has been changed</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
