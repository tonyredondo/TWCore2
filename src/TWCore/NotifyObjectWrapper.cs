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

using System.Reflection;
using System.Runtime.CompilerServices;

namespace TWCore
{
    /// <inheritdoc />
    /// <summary>
    /// Wraps an object to add support to the INotifyPropertyChanged interface
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    public abstract class NotifyObjectWrapper<T> : NotifyObject
    {
        /// <summary>
        /// Wrapped object base
        /// </summary>
        protected readonly T Item;

        #region .ctor
        /// <summary>
        /// Wraps an object to add support to the INotifyPropertyChanged interface
        /// </summary>
        /// <param name="item">Object to wrap</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected NotifyObjectWrapper(T item)
        {
            Item = item;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the value for a property and check if the value has changed to trigger the PropertyChanged event
        /// </summary>
        /// <param name="value">The new value in the property set</param>
        /// <param name="propertyName">Property name, the default value is the caller property name</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void SetValue(object value, [CallerMemberName] string propertyName = null)
        {
            var property = Item.GetType().GetRuntimeProperty(propertyName);
            if (property == null || !property.CanWrite || !property.CanRead) return;
            var propValue = property.GetValue(Item);
            if (Equals(propValue, value)) return;
            property.SetValue(Item, value);
            OnPropertyChanged(propertyName);
        }
        #endregion
    }
}
