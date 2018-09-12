﻿/*
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
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TWCore.Diagnostics.Status
{
    /// <summary>
    /// Represent a status item value item
    /// </summary>
    [DataContract]
    public class StatusItemValueItem
    {
        /// <summary>
        /// Item Id
        /// </summary>
        [XmlAttribute, DataMember]
        public string Id { get; set; }
        /// <summary>
        /// Name
        /// </summary>
        [XmlAttribute, DataMember]
        public string Name { get; set; }
        /// <summary>
        /// Raw Value
        /// </summary>
        [XmlElement, DataMember]
        public object RawValue { get; set; }
        /// <summary>
        /// Value
        /// </summary>
        [XmlAttribute, DataMember]
        public string Value { get; set; }
        /// <summary>
        /// Value Type
        /// </summary>
        [XmlAttribute, DataMember]
        public StatusItemValueType Type { get; set; }
        /// <summary>
        /// Value status
        /// </summary>
        [XmlAttribute, DataMember]
        public StatusItemValueStatus Status { get; set; }
        /// <summary>
        /// Enable to plot
        /// </summary>
        [XmlAttribute, DataMember]
        public bool Plot { get; set; }

        #region .ctor
        /// <summary>
        /// Represent a status item value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatusItemValueItem() { }
        /// <summary>
        /// Represent a status item value
        /// </summary>
        /// <param name="name">Name of the value</param>
        /// <param name="value">Value</param>
        /// <param name="status">Value status</param>
        /// <param name="plot">Enable to plot</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatusItemValueItem(string name, object value, StatusItemValueStatus status, bool plot)
        {
            Name = name;
            if (value is null)
            {
                Value = null;
                RawValue = null;
                Type = StatusItemValueType.Text;
                plot = false;
            }
            else
            {
                switch (value)
                {
                    case int intValue:
                        Type = StatusItemValueType.Number;
                        RawValue = value;
                        Value = intValue.ToString("0.####", CultureInfo.InvariantCulture);
                        break;
                    case decimal decValue:
                        Type = StatusItemValueType.Number;
                        RawValue = value;
                        Value = decValue.ToString("0.####", CultureInfo.InvariantCulture);
                        break;
                    case double doubleValue:
                        Type = StatusItemValueType.Number;
                        RawValue = value;
                        Value = doubleValue.ToString("0.####", CultureInfo.InvariantCulture);
                        break;
                    case float floatValue:
                        Type = StatusItemValueType.Number;
                        RawValue = value;
                        Value = floatValue.ToString("0.####", CultureInfo.InvariantCulture);
                        break;
                    case long longValue:
                        Type = StatusItemValueType.Number;
                        RawValue = value;
                        Value = longValue.ToString("0.####", CultureInfo.InvariantCulture);
                        break;
                    case short shortValue:
                        Type = StatusItemValueType.Number;
                        RawValue = value;
                        Value = shortValue.ToString("0.####", CultureInfo.InvariantCulture);
                        break;
                    case byte byteValue:
                        Type = StatusItemValueType.Number;
                        RawValue = value;
                        Value = byteValue.ToString("0.####", CultureInfo.InvariantCulture);
                        break;
                    case uint intValue:
                        Type = StatusItemValueType.Number;
                        RawValue = value;
                        Value = intValue.ToString("0.####", CultureInfo.InvariantCulture);
                        break;
                    case ulong longValue:
                        Type = StatusItemValueType.Number;
                        RawValue = value;
                        Value = longValue.ToString("0.####", CultureInfo.InvariantCulture);
                        break;
                    case ushort shortValue:
                        Type = StatusItemValueType.Number;
                        RawValue = value;
                        Value = shortValue.ToString("0.####", CultureInfo.InvariantCulture);
                        break;
                    case sbyte byteValue:
                        Type = StatusItemValueType.Number;
                        RawValue = value;
                        Value = byteValue.ToString("0.####", CultureInfo.InvariantCulture);
                        break;
                    case DateTime date:
                        Type = StatusItemValueType.Date;
                        RawValue = date.ToString("o");
                        Value = date.TimeOfDay.TotalSeconds > 0 ? date.ToString("s") : date.ToString("yyyy-MM-dd");
                        break;
                    case TimeSpan time:
                        Type = StatusItemValueType.Time;
                        RawValue = time.TotalMilliseconds;
                        Value = time.ToString();
                        break;
                    default:
                        Type = StatusItemValueType.Text;
                        RawValue = value.ToString();
                        Value = value.ToString();
                        plot = false;
                        break;
                }
            }
            Status = status;
            Plot = plot;
        }
        /// <inheritdoc />
        /// <summary>
        /// Represent a status item value
        /// </summary>
        /// <param name="name">Name of the value</param>
        /// <param name="value">Value</param>
        /// <param name="plotEnabled">Enable to plot</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatusItemValueItem(string name, object value, bool plotEnabled = false) : this(name, value, StatusItemValueStatus.Unknown, plotEnabled)
        {
        }
        #endregion

    }
}
