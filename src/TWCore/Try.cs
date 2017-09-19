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
using System.Runtime.CompilerServices;

namespace TWCore
{
    /// <summary>
    /// Manage actions and functions executions inside a try/catch sentence
    /// </summary>
    [IgnoreStackFrameLog]
    public static class Try
    {
        /// <summary>
        /// Evaluates a function inside a try/catch sentence and returns the value.
        /// </summary>
        /// <typeparam name="T">Function type</typeparam>
        /// <param name="tryFunction">Function to be executed inside the try/catch.</param>
        /// <param name="onException">Function to be executed in case an Exception catch</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Function result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Do<T>(Func<T> tryFunction, Func<Exception, T> onException, T defaultValue = default(T))
        {
            var res = defaultValue;
            try
            {
                res = tryFunction();
            }
            catch (Exception e)
            {
                if (onException != null)
                    res = onException(e);
                else
                    Core.Log.Write(e);
            }
            return res;
        }
        /// <summary>
        /// Evaluates a function inside a try/catch sentence and returns the value.
        /// </summary>
        /// <typeparam name="T">Function type</typeparam>
        /// <param name="tryFunction">Function to be executed inside the try/catch.</param>
        /// <param name="onException">Action to be executed in case an Exception catch</param>
        /// <param name="throwsAfter">Indicates if after the catch handleling the exceptions need to be throw to an upper try/catch</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Function result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Do<T>(Func<T> tryFunction, Action<Exception> onException = null, bool throwsAfter = false, T defaultValue = default(T))
        {
            var res = defaultValue;
            try
            {
                res = tryFunction();
            }
            catch (Exception e)
            {
                if (onException != null)
                    onException(e);
                else
                    Core.Log.Write(e);
                if (throwsAfter)
                    throw;
            }
            return res;
        }
        /// <summary>
        /// Evaluates a function inside a try/catch sentence and returns the value.
        /// </summary>
        /// <typeparam name="T">Function type</typeparam>
        /// <param name="tryFunction">Function to be executed inside the try/catch.</param>
        /// <param name="throwsAfter">Indicates if after the catch handleling the exceptions need to be throw to an upper try/catch</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Function result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Do<T>(Func<T> tryFunction, bool throwsAfter, T defaultValue = default(T))
        {
            var res = defaultValue;
            try
            {
                res = tryFunction();
            }
            catch (Exception e)
            {
                Core.Log.Write(e);
                if (throwsAfter)
                    throw;
            }
            return res;
        }
        /// <summary>
        /// Evaluates a function inside a try/catch sentence and returns the value.
        /// </summary>
        /// <typeparam name="T">Function type</typeparam>
        /// <param name="tryFunction">Function to be executed inside the try/catch.</param>
        /// <param name="exceptionMessage">Message on exception</param>
        /// <returns>Function result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Do<T>(Func<T> tryFunction, string exceptionMessage)
        {
            T res;
            try
            {
                res = tryFunction();
            }
            catch (Exception e)
            {
                throw new Exception(exceptionMessage, e);
            }
            return res;
        }
        /// <summary>
        /// Execute an action inside a try/catch sentence and returns if it was sucessfully executed or not.
        /// </summary>
        /// <param name="action">Action to be executed inside the try/catch sentence.</param>
        /// <param name="onException">Action to be executed in case an Exception catch</param>
        /// <param name="throwsAfter">Indicates if after the catch handleling the exceptions need to be throw to an upper try/catch</param>
        /// <returns>true if the action was executed sucessfully, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Do(Action action, Action<Exception> onException = null, bool throwsAfter = false)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception e)
            {
                if (onException != null)
                    onException(e);
                else
                    Core.Log.Write(e);
                if (throwsAfter)
                    throw;
            }
            return false;
        }
        /// <summary>
        /// Execute an action inside a try/catch sentence and returns if it was sucessfully executed or not.
        /// </summary>
        /// <param name="action">Action to be executed inside the try/catch sentence.</param>
        /// <param name="throwsAfter">Indicates if after the catch handleling the exceptions need to be throw to an upper try/catch</param>
        /// <returns>true if the action was executed sucessfully, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Do(Action action, bool throwsAfter)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception e)
            {
                Core.Log.Write(e);
                if (throwsAfter)
                    throw;
            }
            return false;
        }
        /// <summary>
        /// Execute an action inside a try/catch sentence and returns if it was sucessfully executed or not.
        /// </summary>
        /// <param name="action">Action to be executed inside the try/catch sentence.</param>
        /// <param name="exceptionMessage">Exception message</param>
        /// <returns>true if the action was executed sucessfully, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Do(Action action, string exceptionMessage)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception e)
            {
                throw new Exception(exceptionMessage, e);
            }
        }
    }
}
