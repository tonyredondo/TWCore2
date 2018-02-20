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

using System;
using System.IO;
using System.Runtime.CompilerServices;
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Global

namespace TWCore
{
    /// <summary>
    /// Simple contract methods
    /// </summary>
    [IgnoreStackFrameLog]
    public static class Ensure
    {
        #region Range Int
        /// <summary>
        /// Ensure that the value is lower than the comparer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LowerThan(int value, int comparer, string userMessage = null)
        {
            if (value >= comparer)
                throw new ArgumentOutOfRangeException(userMessage);
        }
        /// <summary>
        /// Ensure that the value is lower or equal than the comprarer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LowerEqualThan(int value, int comparer, string userMessage = null)
        {
            if (value > comparer)
                throw new ArgumentOutOfRangeException(userMessage);
        }
        /// <summary>
        /// Ensure that the value is greater than the comparer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GreaterThan(int value, int comparer, string userMessage = null)
        {
            if (value <= comparer)
                throw new ArgumentOutOfRangeException(userMessage);
        }
        /// <summary>
        /// Ensure that the value is greater or equal than the comprarer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GreaterEqualThan(int value, int comparer, string userMessage = null)
        {
            if (value < comparer)
                throw new ArgumentOutOfRangeException(userMessage);
        }
        #endregion

        #region Range DateTime
        /// <summary>
        /// Ensure that value is lower than the comparer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LowerThan(DateTime value, DateTime comparer, string userMessage = null)
        {
            if (value >= comparer)
                throw new ArgumentOutOfRangeException(userMessage);
        }
        /// <summary>
        /// Ensure that the value is lower or equal to the compararer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LowerEqualThan(DateTime value, DateTime comparer, string userMessage = null)
        {
            if (value > comparer)
                throw new ArgumentOutOfRangeException(userMessage);
        }
        /// <summary>
        /// Ensure that the value is greater than the comparer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GreaterThan(DateTime value, DateTime comparer, string userMessage = null)
        {
            if (value <= comparer)
                throw new ArgumentOutOfRangeException(userMessage);
        }
        /// <summary>
        /// Ensure that the value is greater or equal than the comprar
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GreaterEqualThan(DateTime value, DateTime comparer, string userMessage = null)
        {
            if (value < comparer)
                throw new ArgumentOutOfRangeException(userMessage);
        }
        #endregion

        #region Argument
        /// <summary>
        /// Ensure that the argument is not null.
        /// </summary>
        /// <param name="argument">Argument</param>
        /// <param name="userMessage">User message</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ArgumentNotNull(object argument, string userMessage = null)
        {
            if (argument == null)
                throw new ArgumentException(userMessage);
        }
        /// <summary>
        /// Ensure that the argument is not null.
        /// </summary>
        /// <param name="argument">Argument</param>
        /// <param name="userMessage">User message</param>
        /// <param name="argumentName">Argument name</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ArgumentNotNull(object argument, string userMessage, string argumentName)
        {
            if (argument == null)
                throw new ArgumentException(userMessage, argumentName);
        }
		#endregion

		#region Reference null
		/// <summary>
		/// Ensure that the argument is not null.
		/// </summary>
		/// <param name="obj">Object Argument</param>
		/// <param name="userMessage">User message</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReferenceNotNull(object obj, string userMessage = null)
        {
            if (obj == null)
                throw new NullReferenceException(userMessage);
        }
        #endregion

        #region Requires
        /// <summary>
        /// Ensure that a condition is true, if is false throw an error with a message
        /// </summary>
        /// <param name="condition">Condition to evaluate</param>
        /// <param name="userMessage">Custom message in case of error</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Requires(bool condition, string userMessage = null)
        {
            if (!condition)
                throw new ArgumentException(userMessage);
        }
        /// <summary>
        /// Ensure that a condition is true, if is false throw an error with a message
        /// </summary>
        /// <typeparam name="T">Type of exception to throw</typeparam>
        /// <param name="condition">Condition to evaluate</param>
        /// <param name="userMessage">Custom message in case of error</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Requires<T>(bool condition, string userMessage = null) where T : Exception
        {
            if (!condition)
                throw (string.IsNullOrEmpty(userMessage)) ? Activator.CreateInstance<T>() : (T)Activator.CreateInstance(typeof(T), userMessage);
        }
		#endregion

		#region File and Folder
		/// <summary>
		/// Ensure that the file exist.
		/// </summary>
		/// <param name="filePath">File path</param>
		/// <param name="userMessage">User message</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExistFile(string filePath, string userMessage = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(userMessage ?? "File doesn't exist: " + filePath, filePath);
        }
		/// <summary>
		/// Ensure that the directory exist.
		/// </summary>
		/// <param name="directoryPath">Directory Path</param>
		/// <param name="userMessage">User message</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExistDirectory(string directoryPath, string userMessage = null)
        {
            if (!Directory.Exists(directoryPath))
                throw new FileNotFoundException(userMessage ?? "Directory doesn't exist: " + directoryPath, directoryPath);
        }
        #endregion
    }
}
