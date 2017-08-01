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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace TWCore.Net.HttpServer
{
    /// <summary>
    /// Http form url encoded content parser
    /// </summary>
    public class FormUrlEncodedContentParser
    {
		Dictionary<string, string> Parameters = new Dictionary<string, string>();

        #region Properties
        /// <summary>
        /// true if the form content was parsed successfully; otherwise, false.
        /// </summary>
        public bool Success { get; private set; }
        /// <summary>
        /// Gets the parameters from the form
        /// </summary>
        /// <param name="key">Key or name of the parameter</param>
        /// <returns>Parameter value</returns>
        public string this[string key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Parameters.TryGetValue(key, out string value);
                return value;
            }
        }
        #endregion

        #region .ctor
        /// <summary>
        /// Http form url encoded content parser
        /// </summary>
        /// <param name="bytes">Byte array to parse</param>
        /// <param name="encoding">Encoding used to convert the byte array to a string</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FormUrlEncodedContentParser(byte[] bytes = null, Encoding encoding = null)
        {
            if (bytes != null)
                Parse(bytes, encoding ?? Encoding.UTF8);
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Parse(byte[] bytes, Encoding encoding)
        {
            Success = false;
            string content = encoding.GetString(bytes);
            Parameters = content.SplitAndTrim('&').Select(i => i.SplitAndTrim('=').ToArray()).ToDictionary(i => i[0], i => i[1]);
            Success = Parameters.Count > 0;
        }
        #endregion
    }
}
