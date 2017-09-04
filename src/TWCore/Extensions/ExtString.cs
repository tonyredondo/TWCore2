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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using TWCore.Text;

namespace TWCore
{
    /// <summary>
    /// Extensions for strings
    /// </summary>
    public static partial class Extensions
    {
        static char[] space = new char[] { ' ' };
        static string spaceString = " ";
        static Lazy<LevenshteinStringDistance> levenshteinStringDistance = new Lazy<LevenshteinStringDistance>();
        static Lazy<DamerauLevenshteinStringDistance> damerauLevenshteinStringDistance = new Lazy<DamerauLevenshteinStringDistance>();
        static Regex shrinkRegex = new Regex(@"[ ]{2,}", RegexOptions.Compiled);
        static Regex _invalidXMLChars = new Regex(@"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]", RegexOptions.Compiled);

        #region Is? conditionals
        /// <summary>
        /// A nicer way of calling <see cref="System.String.IsNullOrEmpty(string)"/>
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>true if the value parameter is null or an empty string (""); otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);
        /// <summary>
        /// A nicer way of calling the inverse of <see cref="System.String.IsNullOrEmpty(string)"/>
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>true if the value parameter is not null or an empty string (""); otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNullOrEmpty(this string value) => !value.IsNullOrEmpty();
        /// <summary>
        /// A nicer way of calling <see cref="System.String.IsNullOrWhiteSpace(string)"/>
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>true if the value parameter is null or an empty string ("") or a whitespace; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrWhitespace(this string value) => string.IsNullOrWhiteSpace(value);
        /// <summary>
        /// A nicer way of calling the inverse of <see cref="System.String.IsNullOrEmpty(string)"/>
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>true if the value parameter is not null or an empty string ("") or whitespace; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNullOrWhitespace(this string value) => !value.IsNullOrWhitespace();
        #endregion

        /// <summary>
        /// Get if the string is all in UpperCase
        /// </summary>
        /// <param name="value">String to check</param>
        /// <returns>true if the string is in uppercase; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUpperCase(this string value)
        {
            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                    if (char.IsLetter(value[i]) && !char.IsUpper(value[i]))
                        return false;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Get if the string is all in LoweCase
        /// </summary>
        /// <param name="value">String to check</param>
        /// <returns>true if the string is in lowercase; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLowerCase(this string value)
        {
            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                    if (char.IsLetter(value[i]) && !char.IsLower(value[i]))
                        return false;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Capitalize the string
        /// </summary>
        /// <param name="value">String to capitalize</param>
        /// <returns>Capitalized string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Capitalize(this string value)
        {
            if (value != null)
            {
                if (value.Length > 1)
                    return char.ToUpperInvariant(value[0]) + value.Substring(1).ToLowerInvariant();
                else if (value.Length == 1)
                    return char.IsLower(value[0]) ? value.ToUpperInvariant() : value;
            }
            return value;
        }
        /// <summary>
        /// Capitalize each word of the string
        /// </summary>
        /// <param name="value">String to capitalize</param>
        /// <returns>Capitalized string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CapitalizeEachWords(this string value)
            => value?.ToLowerInvariant().Split(space).Select(i => i.Capitalize()).Join(spaceString);
        /// <summary>
        /// Apply format to a string
        /// </summary>
        /// <param name="format">Format pattern</param>
        /// <param name="args">Arguments</param>
        /// <returns>String with the format result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ApplyFormat(this string format, params object[] args)
            => string.Format(format, args);
        /// <summary>
        /// Compare two strings and gives the equality in percent.
        /// Using Levenshtein 
        /// </summary>
        /// <param name="a"> First string to compare</param>
        /// <param name="b"> Second string to compare</param>
        /// <param name="comparer"> IEqualityComparer to use, to support Case Sensitive or not, if null it uses Case Sensitive</param>
        /// <returns>Percent of equality of the two strings</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double EqualPercentLevenshtein(this string a, string b, IEqualityComparer<char> comparer = null)
            => StringCompare.EqualPercent(a, b, levenshteinStringDistance.Value, comparer);
        /// <summary>
        /// Compare two strings and gives the equality in percent.
        /// Using Damerau-Levenshtein 
        /// </summary>
        /// <param name="a"> First string to compare</param>
        /// <param name="b"> Second string to compare</param>
        /// <param name="comparer"> IEqualityComparer to use, to support Case Sensitive or not, if null it uses Case Sensitive</param>
        /// <returns>Percent of equality of the two strings</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double EqualPercentDamerauLevenshtein(this string a, string b, IEqualityComparer<char> comparer = null)
            => StringCompare.EqualPercent(a, b, damerauLevenshteinStringDistance.Value, comparer);
        /// <summary>
        /// Reverse a string
        /// </summary>
        /// <param name="value">String to reverse</param>
        /// <returns>Reversed string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Reverse(this string value)
        {
            if (value == null) return null;
            char[] charArray = value.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
        /// <summary>
        /// Reverse a string using a split separation
        /// </summary>
        /// <param name="value">String to split and then reverse</param>
        /// <param name="separator">Separator for the split</param>
        /// <returns>String reversed by the split</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReverseBySplit(this string value, string separator)
        {
            var valRevArray = value.Split(separator);
            Array.Reverse(valRevArray);
            return string.Join(separator, valRevArray);
        }
        /// <summary>
        /// Remove accent mark from a string
        /// </summary>
        /// <param name="value">String to remove the accent marks</param>
        /// <returns>String without accent marks</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string RemoveAccentMark(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Replace("á", "a");
                value = value.Replace("é", "e");
                value = value.Replace("í", "i");
                value = value.Replace("ó", "o");
                value = value.Replace("ú", "u");

                value = value.Replace("à", "a");
                value = value.Replace("è", "e");
                value = value.Replace("ì", "i");
                value = value.Replace("ò", "o");
                value = value.Replace("ù", "u");

                value = value.Replace("ä", "a");
                value = value.Replace("ë", "e");
                value = value.Replace("ï", "i");
                value = value.Replace("ö", "o");
                value = value.Replace("ü", "u");

                value = value.Replace("Á", "A");
                value = value.Replace("É", "E");
                value = value.Replace("Í", "I");
                value = value.Replace("Ó", "O");
                value = value.Replace("Ú", "U");

                value = value.Replace("À", "A");
                value = value.Replace("È", "E");
                value = value.Replace("Ì", "I");
                value = value.Replace("Ò", "O");
                value = value.Replace("Ù", "U");

                value = value.Replace("Ä", "A");
                value = value.Replace("Ë", "E");
                value = value.Replace("Ï", "I");
                value = value.Replace("Ö", "O");
                value = value.Replace("Ü", "U");
            }
            return value;
        }
        /// <summary>
        /// Remove accent mark and no chars and no numbers
        /// </summary>
        /// <param name="text">Original string</param>
        /// <returns>String without accent mark and no chars and no numbers</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string RemoveAccentMarkAndNoCharsAndNoNumbers(this string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();
            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory == UnicodeCategory.LowercaseLetter ||
                    unicodeCategory == UnicodeCategory.UppercaseLetter ||
                    unicodeCategory == UnicodeCategory.SpaceSeparator ||
                    unicodeCategory == UnicodeCategory.LetterNumber ||
                    unicodeCategory == UnicodeCategory.DecimalDigitNumber ||
                    unicodeCategory == UnicodeCategory.OtherNumber)
                {
                    if (c == 160) continue;
                    stringBuilder.Append(c);
                }
            }
            var value = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
            value = value.ShrinkSpaces();
            return value.ToString();
        }
        /// <summary>
        /// Remove invalid XML unicode chars
        /// </summary>
        /// <param name="text">Original string</param>
        /// <returns>String without all invalid xml chars</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string RemoveInvalidXMLChars(this string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return _invalidXMLChars.Replace(text, "");
        }
        /// <summary>
        /// Remove spaces from a string
        /// </summary>
        /// <param name="value">String to remove spaces</param>
        /// <returns>String without spaces</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string RemoveSpaces(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                StringBuilder sb = new StringBuilder();
                for (var x = 0; x < value.Length; x++)
                    if (value[x] != ' ') sb.Append(value[x]);
                return sb.ToString();
            }
            return value;
        }
        /// <summary>
        /// Remove more than one consecutive spaces from a string
        /// </summary>
        /// <param name="value">String to remove spaces</param>
        /// <returns>String without more than one consecutive space</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ShrinkSpaces(this string value)
        {
            if (!string.IsNullOrEmpty(value))
                return shrinkRegex.Replace(value, @" ");
            return value;
        }
        /// <summary>
        /// Split a string in to an array.
        /// </summary>
        /// <param name="item">String to split</param>
        /// <param name="separator">String separator</param>
        /// <param name="splitOptions">Split options</param>
        /// <returns>Array of strings</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] Split(this string item, string separator, StringSplitOptions splitOptions = StringSplitOptions.None)
            => item?.Split(new string[] { separator }, splitOptions);

        #region Substring
        /// <summary>
        /// Gets a substring using a index to start and end.
        /// </summary>
        /// <param name="item">Source string value</param>
        /// <param name="startIndex">Start index</param>
        /// <param name="endIndex">Final index</param>
        /// <returns>Substring value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringIndex(this string item, int startIndex, int endIndex = 0)
        {
            if (endIndex < 1)
                endIndex = item.Length - endIndex;
            return item.Substring(startIndex, endIndex - startIndex);
        }
        /// <summary>
        /// Gets a substring from the first appearance of a char
        /// </summary>
        /// <param name="item">Source string value</param>
        /// <param name="charStart">Char to find</param>
        /// <returns>Substring value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringFromFirst(this string item, char charStart)
            => item.Substring(item.IndexOf(charStart));
        /// <summary>
        /// Gets a substring from the first appearance of a string
        /// </summary>
        /// <param name="item">Source string value</param>
        /// <param name="stringStart">String to find</param>
        /// <returns>Substring value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringFromFirst(this string item, string stringStart)
            => item.Substring(item.IndexOf(stringStart, StringComparison.Ordinal));
        /// <summary>
        /// Gets a substring from the last appearance of a char
        /// </summary>
        /// <param name="item">Source string value</param>
        /// <param name="charStart">Char to find</param>
        /// <returns>Substring value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringFromLast(this string item, char charStart)
            => item.Substring(item.LastIndexOf(charStart));
        /// <summary>
        /// Gets a substring from the first appearance of a string
        /// </summary>
        /// <param name="item">Source string value</param>
        /// <param name="stringStart">String to find</param>
        /// <returns>Substring value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringFromLast(this string item, string stringStart)
            => item.Substring(item.LastIndexOf(stringStart, StringComparison.Ordinal));
        /// <summary>
        /// Gets a substring from the start of the string until the first appearance of a char
        /// </summary>
        /// <param name="item">Source string value</param>
        /// <param name="charStart">Char to find</param>
        /// <returns>Substring value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringToFirst(this string item, char charStart)
            => item.Substring(0, item.IndexOf(charStart));
        /// <summary>
        /// Gets a substring from the start of the string until the first appearance of a char
        /// </summary>
        /// <param name="item">Source string value</param>
        /// <param name="stringStart">String to find</param>
        /// <returns>Substring value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringToFirst(this string item, string stringStart)
            => item.Substring(0, item.IndexOf(stringStart, StringComparison.Ordinal));
        /// <summary>
        /// Gets a substring from the start of the string until the last appearance of a char
        /// </summary>
        /// <param name="item">Source string value</param>
        /// <param name="charStart">Char to find</param>
        /// <returns>Substring value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringToLast(this string item, char charStart)
            => item.Substring(0, item.LastIndexOf(charStart));
        /// <summary>
        /// Gets a substring from the start of the string until the last appearance of a string
        /// </summary>
        /// <param name="item">Source string value</param>
        /// <param name="stringStart">String to find</param>
        /// <returns>Substring value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringToLast(this string item, string stringStart)
            => item.Substring(0, item.LastIndexOf(stringStart, StringComparison.Ordinal));
        #endregion

        #region Hexadecimal
        /// <summary>
        /// Gets the Hexadecimal string from a byte array.
        /// </summary>
        /// <param name="obj">String array</param>
        /// <returns>Hexadecimal string value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToHexString(this byte[] obj)
        {
            var sb = new StringBuilder(obj.Length * 2);
            foreach (byte b in obj)
                sb.AppendFormat("{0:x2}", b);
            return sb.ToString();
        }
        /// <summary>
        /// Gets the Hexadecimal string from a byte array.
        /// </summary>
        /// <param name="obj">String array</param>
        /// <returns>Hexadecimal string value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToHexString(this SubArray<byte> obj)
        {
            var sb = new StringBuilder(obj.Count * 2);
			obj.ForEach((ref byte b, ref StringBuilder sbuilder) => sbuilder.AppendFormat("{0:x2}", b), ref sb);
            return sb.ToString();
        }
        /// <summary>
        /// Gets a byte array from a Hexadecimal string
        /// </summary>
        /// <param name="hex">Hexadecimal string</param>
        /// <returns>Byte array value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] FromHexStringToByteArray(this string hex)
            => Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
        #endregion

        /// <summary>
        /// Separates a PascalCase string
        /// </summary>
        /// <example>
        /// "ThisIsPascalCase".SeparatePascalCase(); // returns "This Is Pascal Case"
        /// </example>
        /// <param name="value">The value to split</param>
        /// <returns>The original string separated on each uppercase character.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SeparatePascalCase(this string value)
            => Regex.Replace(value, "([A-Z])", " $1").Trim();
        /// <summary>
        /// Returns a string array containing the trimmed substrings in this <paramref name="value"/>
        /// that are delimited by the provided <paramref name="separators"/>.
        /// </summary>
        /// <param name="value">Value to split and trim</param>
        /// <param name="separators">Separators for the split</param>
        /// <returns>IEnumerable with split and trim result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] SplitAndTrim(this string value, params char[] separators)
        {
            if (string.IsNullOrEmpty(value))
                return new string[0];
            if (separators?.Length == 1 && separators[0] == space[0])
                return value.Split(space, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
            else
                return value.Trim().Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
        }
        /// <summary>
        /// Returns a string array containing the trimmed substrings in this <paramref name="value"/>
        /// that are delimited by the provided <paramref name="separators"/>.
        /// </summary>
        /// <param name="value">Value to split and trim</param>
        /// <param name="separators">Separators for the split</param>
        /// <returns>IEnumerable with split and trim result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] SplitAndTrim(this string value, string separators)
        {
            if (string.IsNullOrEmpty(value))
                return new string[0];
            if (separators?.Length == 1 && separators == spaceString)
                return value.Split(space, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
            else
                return value.Trim().Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
        }
        /// <summary>
        /// Truncates a string to a specific length
        /// </summary>
        /// <param name="value">String to truncate</param>
        /// <param name="maxLength">Maximum length of the string</param>
        /// <returns>Truncated string value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TruncateTo(this string value, int maxLength)
        {
            if (value?.Length > maxLength)
                return value.Substring(0, maxLength);
            return value;
        }
        /// <summary>
        /// Check if a string is an integer
        /// </summary>
        /// <param name="value">String to check</param>
        /// <returns>true if the value of the string is a number, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNumeric(this string value)
        {
            return !string.IsNullOrWhiteSpace(value) && int.TryParse(value, out int _);
        }
        /// <summary>
        /// Parse a string to another type.
        /// </summary>
        /// <param name="value">String source value</param>
        /// <param name="defaultValue">Default value in case the parse fails.</param>
        /// <param name="dataFormat">Indicate wich format have to be use in the parse method</param>
        /// <returns>Result of the parser</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ParseTo<T>(this string value, T defaultValue, string dataFormat = null)
            => StringParser<T>.Parse(value, defaultValue, dataFormat);
        /// <summary>
        /// Parse a string to another type.
        /// </summary>
        /// <param name="value">String source value</param>
        /// <param name="objectType">Object type</param>
        /// <param name="defaultValue">Default value in case the parse fails.</param>
        /// <param name="dataFormat">Indicate wich format have to be use in the parse method</param>
        /// <returns>Result of the parser</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object ParseTo(this string value, Type objectType, object defaultValue, string dataFormat = null)
            => StringParser.Parse(value, objectType, defaultValue, dataFormat);

        /// <summary>
        /// Removes all the path invalid chars from the string
        /// </summary>
        /// <param name="value">String value with a path</param>
        /// <returns>Path value with only safe chars</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string RemovePathInvalidChars(this string value)
        {
            var invalidChars = Path.GetInvalidPathChars();
            var sb = new StringBuilder();
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (!invalidChars.Contains(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }
        /// <summary>
        /// Removes all the file name invalid chars from the string
        /// </summary>
        /// <param name="value">String value with a filename</param>
        /// <returns>File name value with only safe chars</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string RemoveFileNameInvalidChars(this string value)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder();
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (!invalidChars.Contains(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }
        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <returns>A byte array containing the contents of the file.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ReadBytesFromFile(this string path)
            => File.ReadAllBytes(path);
        /// <summary>
        /// Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <returns>A string containing all lines of the file.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadTextFromFile(this string path, Encoding encoding = null)
            => File.ReadAllText(path, encoding);
        /// <summary>
        /// Faster Index Of method
        /// </summary>
        /// <param name="source">Source string</param>
        /// <param name="pattern">Pattern string</param>
        /// <param name="startIndex">Start index</param>
        /// <returns>Index of</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FastIndexOf(this string source, string pattern, int startIndex = 0)
        {
            if (pattern == null) throw new ArgumentNullException();
            switch (pattern.Length)
            {
                case 0:
                    return 0;
                case 1:
                    return source.IndexOf(pattern[0], startIndex);
            }

            bool found;
            int limit = (source.Length - pattern.Length + 1) - startIndex;
            if (limit < 1) return -1;
            // Store the first 2 characters of "pattern"
            var c0 = pattern[0];
            var c1 = pattern[1];
            // Find the first occurrence of the first character
            var first = source.IndexOf(c0, startIndex, limit);
            while (first != -1)
            {
                // Check if the following character is the same like
                // the 2nd character of "pattern"
                if (source[first + 1] != c1)
                {
                    first = source.IndexOf(c0, ++first, limit - (first - startIndex));
                    continue;
                }
                // Check the rest of "pattern" (starting with the 3rd character)
                found = true;
                for (var j = 2; j < pattern.Length; j++)
                    if (source[first + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                // If the whole word was found, return its index, otherwise try again
                if (found)
                    return first;

                first = source.IndexOf(c0, ++first, limit - (first - startIndex));
            }
            return -1;
        }
		/// <summary>
		/// Gets the jenkins hash.
		/// </summary>
		/// <returns>The jenkins hash.</returns>
		/// <param name="value">String Value</param>
		public static uint GetJenkinsHash(this string value)
		{
			if (string.IsNullOrWhiteSpace(value)) return 0;
			var length = value.Length;
			var i = 0;
			uint hash = 0;
			while(i != length)
			{
				hash += value[i++];
				hash += hash << 10;
				hash ^= hash >> 6;
			}
			hash += hash << 3;
			hash ^= hash >> 11;
			hash += hash << 15;
			return hash;
		}
    }
}
