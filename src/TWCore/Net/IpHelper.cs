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
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming

namespace TWCore.Net
{
    /// <summary>
    /// Ip Helper class to do calculation on IpAddresses
    /// </summary>
    public static class IpHelper
    {
        /// <summary>
        /// Gets if a hostname or address is a localhost address
        /// </summary>
        /// <param name="hostNameOrAddress">Hostname or IpAddress</param>
        /// <returns>True if the hostname or address is in localhost, otherwise; false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLocalhost(string hostNameOrAddress)
        {
            if (string.IsNullOrEmpty(hostNameOrAddress))
                return false;

            if (hostNameOrAddress.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                return true;
            if (hostNameOrAddress.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase))
                return true;
            if (hostNameOrAddress.Equals("::", StringComparison.OrdinalIgnoreCase))
                return true;

            try
            {
                var hostIPs = Dns.GetHostAddressesAsync(hostNameOrAddress).WaitAsync();
                var localIPs = Dns.GetHostAddressesAsync(Dns.GetHostName()).WaitAsync();
                return hostIPs.Any(hostIp => IPAddress.IsLoopback(hostIp) || localIPs.Contains(hostIp));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Ip Address string to UInt
        /// </summary>
        /// <param name="ipAddress">IpAddress</param>
        /// <returns>UInt value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint IpToInt(string ipAddress)
        {
            var uip = ipAddress.Split('.').Select(i => {
                uint.TryParse(i, out uint puip);
                return puip;
            }).ToArray();
            return uip.Length == 4 ? IpToInt(uip[0], uip[1], uip[2], uip[3]) : 0;
        }
        /// <summary>
        /// Ip Address to UInt
        /// </summary>
        /// <param name="first">First element of the IpAddress</param>
        /// <param name="second">Second element of the IpAddress</param>
        /// <param name="third">Third element of the IpAddress</param>
        /// <param name="fourth">Fourth element of the IpAddress</param>
        /// <returns>UInt value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint IpToInt(uint first, uint second, uint third, uint fourth) => (first << 24) | (second << 16) | (third << 8) | (fourth);
        /// <summary>
        /// Parse IpAddress to Int value
        /// </summary>
        /// <param name="ipaddr">Ip Address</param>
        /// <returns>Int Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ParseIpAddress(string ipaddr)
        {
            if (ipaddr == null || ipaddr.Length < 7 || ipaddr.Length > 15)
                return 0;
            var ipTokens = ipaddr.Split('.');
            if (ipTokens.Length != 4)
                return 0;
            var ipInt = 0;
            for (var i = 0; i < 4; i++)
            {
                var ipNum = ipTokens[i];
                try
                {
                    var ipVal = int.Parse(ipNum);
                    if (ipVal < 0 || ipVal > 255)
                        return 0;
                    ipInt = (ipInt << 8) + ipVal;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            return ipInt;
        }

        /// <summary>
        /// Get current public ip address
        /// </summary>
        /// <returns>Public Ip address</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<string> GetPublicIPAsync()
        {
            Core.Log.LibVerbose("Getting the public ip address...");
            string address;
            var request = WebRequest.Create("http://checkip.dyndns.org/");
            using (var response = await request.GetResponseAsync().ConfigureAwait(false))
            using (var stream = new StreamReader(response.GetResponseStream()))
                address = stream.ReadToEnd();
            //Search for the ip in the html
            var first = address.IndexOf("Address: ", StringComparison.Ordinal) + 9;
            var last = address.LastIndexOf("</body>", StringComparison.Ordinal);
            address = address.SubstringIndex(first, last);
            Core.Log.LibVerbose("The Ip address is: {0}", address);
            return address;
        }

        /// <summary>
        /// Get the IpAddress from the HostName
        /// </summary>
        /// <param name="hostname">Hostname</param>
        /// <returns>First IpAddress resolution.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<IPAddress> GetIpFromHostAsync(string hostname)
        {
            if (!IPAddress.TryParse(hostname, out var ipAddress))
                ipAddress = (await Dns.GetHostAddressesAsync(hostname).ConfigureAwait(false))?.FirstOrDefault();
            return ipAddress;
        }
    }
}
