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

using System.Text.RegularExpressions;

namespace TWCore.Text
{
    /// <summary>
    /// Regex helper const definitions
    /// </summary>
    public static class RegexHelper
    {
        /// <summary>
        /// A regular expression for validating slugs.
        /// Does not allow leading or trailing hypens or whitespace
        /// </summary>
        public static readonly Regex SlugRegex = new Regex(@"(^[a-z0-9])([a-z0-9-]+)*([a-z0-9])$", RegexOptions.Compiled);
        /// <summary>
        /// A regular expression for validating slugs with segments
        /// Does not allow leading or trailing hypens or whitespace
        /// </summary>
        public static readonly Regex SlugWithSegmentsRegex = new Regex(@"^(?!-)[a-z0-9-]+(?<!-)(/(?!-)[a-z0-9-]+(?<!-))*$", RegexOptions.Compiled);
        /// <summary>
        /// A regular expression for validating IPAddresses. Taken from http://net.tutsplus.com/tutorials/other/8-regular-expressions-you-should-know/
        /// </summary>
        public static readonly Regex IpAddressRegex = new Regex(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", RegexOptions.Compiled);
        /// <summary>
        /// A regular expression for validating Email Addresses. Taken from http://net.tutsplus.com/tutorials/other/8-regular-expressions-you-should-know/
        /// </summary>
        public static readonly Regex EmailRegex = new Regex(@"^([\w_\.-]+)@([\w\.-]+)\.([w\.]{2,8})$", RegexOptions.Compiled);
        /// <summary>
        /// A regular expression for validating absolute Urls. Taken from http://net.tutsplus.com/tutorials/other/8-regular-expressions-you-should-know/
        /// </summary>
        public static readonly Regex UrlRegex = new Regex(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$", RegexOptions.Compiled);
        /// <summary>
        /// A regular expression for validating that string is a positive number GREATER THAN zero.
        /// </summary>
        public static readonly Regex PositiveNumberRegex = new Regex(@"^[1-9]+[0-9]*$", RegexOptions.Compiled);
        /// <summary>
        /// A regular expression for validating that string is a number.
        /// </summary>
        public static readonly Regex NumericRegex = new Regex("^[0-9]+$", RegexOptions.Compiled);
        /// <summary>
        /// A regular expression for validating that string has two or more spaces.
        /// </summary>
        public static readonly Regex TwoOrMoreSpaces = new Regex(@"[ ]{2,}", RegexOptions.Compiled);
        /// <summary>
        /// The Microsoft standard regular expression for validate an email address.
        /// </summary>
        public static readonly Regex MsEmailRegex = new Regex(@"^(?("")("".+?(?<!\\)""@)|(([0-9a-z_]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z_])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        /// <summary>
        /// A regular expression for validating a postal code.
        /// </summary>
        public static readonly Regex PostalCode = new Regex("^[0-9a-zA-Z -.]+$", RegexOptions.Compiled);
        /// <summary>
        /// A regular expression for validating a phone number.
        /// </summary>
        public static readonly Regex Phone = new Regex("^[0-9 -.]+$", RegexOptions.Compiled);
    }
}
