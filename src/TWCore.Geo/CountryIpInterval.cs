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

using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Collections;
using TWCore.Net;
using TWCore.Serialization;
// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TWCore.Geo
{
    /// <inheritdoc />
    /// <summary>
    /// Country ip interval
    /// </summary>
    [DataContract]
    public class CountryIpInterval : IRangeProvider<uint>
    {
        private Range<uint> _range;

        #region Properties
        [XmlAttribute, DataMember]
        public uint From
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _range.From; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { _range.From = value; }
        }
        [XmlAttribute, DataMember]
        public uint To
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _range.To; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { _range.To = value; }
        }
        [XmlAttribute, DataMember]
        public string Country { get; set; }
        [XmlIgnore, NonSerialize]
        public uint Length => _range.To - _range.From;
        [XmlIgnore, NonSerialize]
        public Range<uint> Range => _range;
        #endregion

        #region .ctor
        /// <summary>
        /// Country ip interval
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CountryIpInterval() { }
        /// <summary>
        /// Country ip interval
        /// </summary>
        /// <param name="from">From IpAddress</param>
        /// <param name="to">To IpAddress</param>
        /// <param name="country">Contry IATA</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CountryIpInterval(string from, string to, string country)
        {
            From = IpHelper.IpToInt(from);
            To = IpHelper.IpToInt(to);
            Country = country;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Check if a IpAddress is in the interval.
        /// </summary>
        /// <param name="address">string IpAddress</param>
        /// <returns>true if address is in the interval; otherwise, false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsIn(string address)
        {
            var number = IpHelper.IpToInt(address);
            return From <= number && number <= To;
        }
        #endregion
    }
}
