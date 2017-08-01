﻿/*
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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace TWCore.Net.HttpServer
{
    /// <summary>
    /// Parses a Http form multipart data
    /// </summary>
    public class FormMultipartParser
    {
        #region Statics Regex
        static Regex nameRegex = new Regex(@"(?<=name\=\"")(.*?)(?=\"")", RegexOptions.Compiled);
        static Regex contentTypeRegex = new Regex(@"(?<=Content\-Type:)(.*?)(?=\r\n\r\n)", RegexOptions.Compiled);
        static Regex filenameRegex = new Regex(@"(?<=filename\=\"")(.*?)(?=\"")", RegexOptions.Compiled);
        #endregion

        #region Properties
        /// <summary>
        /// File part name
        /// </summary>
        public string FilePartName { get; private set; }
        /// <summary>
        /// true if the content was successfully parsed; otherwise, false.
        /// </summary>
        public bool Success { get; private set; }
        /// <summary>
        /// Content type
        /// </summary>
        public string ContentType { get; private set; }
        /// <summary>
        /// File name
        /// </summary>
        public string Filename { get; private set; }
        /// <summary>
        /// File content byte array
        /// </summary>
        public byte[] FileContents { get; private set; }
        /// <summary>
        /// Form parameters
        /// </summary>
        public IDictionary<string, string> Parameters { get; private set; } = new Dictionary<string, string>();
        #endregion

        #region .ctor
        /// <summary>
        /// Parses a Http form multipart data
        /// </summary>
        /// <param name="bytes">Byte array</param>
        /// <param name="filePartName">File part name</param>
        /// <param name="encoding">Encoding used to convert the byte array to a string</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FormMultipartParser(byte[] bytes, string filePartName, Encoding encoding = null)
        {
            FilePartName = filePartName;
            Parse(bytes, encoding ?? Encoding.UTF8);
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Parse(byte[] bytes, Encoding encoding)
        {
            Success = false;
            string content = encoding.GetString(bytes);
            int delimiterEndIndex = content.IndexOf("\r\n", StringComparison.OrdinalIgnoreCase);

            if (delimiterEndIndex > -1)
            {
				string delimiter = content.Substring(0, content.IndexOf("\r\n", StringComparison.OrdinalIgnoreCase));
                string[] sections = content.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string s in sections)
                {
                    if (s.Contains("Content-Disposition"))
                    {
                        // If we find "Content-Disposition", this is a valid multi-part section
                        // Now, look for the "name" parameter
                        var nameMatch = nameRegex.Match(s);
                        string name = nameMatch.Value.Trim().ToLower();

                        if (name == FilePartName)
                        {
                            // Look for Content-Type
                            var contentTypeMatch = contentTypeRegex.Match(content);

                            // Look for filename
                            var filenameMatch = filenameRegex.Match(content);

                            // Did we find the required values?
                            if (contentTypeMatch.Success && filenameMatch.Success)
                            {
                                // Set properties
                                this.ContentType = contentTypeMatch.Value.Trim();
                                this.Filename = filenameMatch.Value.Trim();

                                // Get the start & end indexes of the file contents
                                int startIndex = contentTypeMatch.Index + contentTypeMatch.Length + "\r\n\r\n".Length;

                                byte[] delimiterBytes = encoding.GetBytes("\r\n" + delimiter);
                                int endIndex = IndexOf(bytes, delimiterBytes, startIndex);

                                int contentLength = endIndex - startIndex;

                                // Extract the file contents from the byte array
                                byte[] fileData = new byte[contentLength];

                                Buffer.BlockCopy(bytes, startIndex, fileData, 0, contentLength);

                                this.FileContents = fileData;
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(name))
                        {
                            // Get the start & end indexes of the file contents
                            int startIndex = nameMatch.Index + nameMatch.Length + "\r\n\r\n".Length;
                            Parameters.Add(name, s.Substring(startIndex).TrimEnd(new char[] { '\r', '\n' }).Trim());
                        }
                    }
                }

                // If some data has been successfully received, set success to true
                Success |= FileContents != null || Parameters.Count != 0;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IndexOf(byte[] searchWithin, byte[] serachFor, int startIndex)
        {
            int index = 0;
            int startPos = Array.IndexOf(searchWithin, serachFor[0], startIndex);

            if (startPos != -1)
            {
                while ((startPos + index) < searchWithin.Length)
                {
                    if (searchWithin[startPos + index] == serachFor[index])
                    {
                        index++;
                        if (index == serachFor.Length)
                            return startPos;
                    }
                    else
                    {
                        startPos = Array.IndexOf<byte>(searchWithin, serachFor[0], startPos + index);
                        if (startPos == -1)
                            return -1;
                        index = 0;
                    }
                }
            }
            return -1;
        }
        #endregion
    }
}
