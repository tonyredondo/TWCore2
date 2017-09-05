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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Net;
using TWCore.Serialization;

namespace TWCore.Geo
{
    [DataContract]
    public class CountryIpIntervalCollection : RangeTree<uint, CountryIpInterval>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CountryIpIntervalCollection() : base(new DefaultRangeProviderComparer<CountryIpInterval, uint>())
        {
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CountryIpIntervalCollection(IEnumerable<CountryIpInterval> enumerable) : base(enumerable, new DefaultRangeProviderComparer<CountryIpInterval, uint>())
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CountryIpIntervalCollection ImportFromCsv(string filename)
        {
            var lstInterval = new CountryIpIntervalCollection();
            using (var stream = new StreamReader(File.Open(filename, FileMode.Open, FileAccess.Read)))
            {
                while (!stream.EndOfStream)
                {
                    var line = stream.ReadLine();
                    var items = line.Split(',');
                    var ip1 = items[0].Replace("\"", string.Empty);
                    var ip2 = items[1].Replace("\"", string.Empty);
                    var iso = items[2].Replace("\"", string.Empty);
                    var ipInterval = new CountryIpInterval(ip1, ip2, iso);
                    lstInterval.Add(ipInterval);
                }
            }
            lstInterval.Rebuild();
            return lstInterval;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<CountryIpIntervalCollection> ImportFromCsvAsync(string filename)
        {
            var lstInterval = new CountryIpIntervalCollection();
            using (var stream = new StreamReader(File.Open(filename, FileMode.Open, FileAccess.Read)))
            {
                while (!stream.EndOfStream)
                {
                    var line = await stream.ReadLineAsync().ConfigureAwait(false);
                    var items = line.Split(',');
                    var ip1 = items[0].Replace("\"", string.Empty);
                    var ip2 = items[1].Replace("\"", string.Empty);
                    var iso = items[2].Replace("\"", string.Empty);
                    var ipInterval = new CountryIpInterval(ip1, ip2, iso);
                    lstInterval.Add(ipInterval);
                }
            }
            lstInterval.Rebuild();
            return lstInterval;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CountryIpInterval GetInterval(string address)
        {
            if (string.IsNullOrEmpty(address))
                return null;
            var number = IpHelper.IpToInt(address);
            return Query(number).FirstOrDefault();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetCountry(string address)
        {
            var interval = GetInterval(address);
            return interval?.Country;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Save(string fileName)
        {
            if (Items != null)
                SerializerManager.DefaultBinarySerializer.SerializeToFile(Items.ToList(), fileName);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CountryIpIntervalCollection Load(string fileName)
        {
            var serializer = SerializerManager.GetByFileName(fileName);
            var list = serializer.DeserializeFromFile<List<CountryIpInterval>>(fileName);
            return new CountryIpIntervalCollection(list);
        }

    }
}
