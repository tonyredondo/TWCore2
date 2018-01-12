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
using System.Threading.Tasks;

namespace TWCore.Threading
{
    /// <summary>
    /// Async event handler class
    /// </summary>
    /// <typeparam name="TEventArgs">Event args instance</typeparam>
    public class AsyncEvent<TEventArgs> where TEventArgs : EventArgs
    {
        private readonly List<Func<object, TEventArgs, Task>> _invocationList;
        private readonly object _locker;

        #region .ctor
        /// <summary>
        /// .ctor
        /// </summary>
        private AsyncEvent()
        {
            _invocationList = new List<Func<object, TEventArgs, Task>>();
            _locker = new object();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Invoke async
        /// </summary>
        /// <param name="sender">Sender instance</param>
        /// <param name="eventArgs">Event args instance</param>
        /// <returns>Task instance</returns>
        public async Task InvokeAsync(object sender, TEventArgs eventArgs)
        {
            List<Func<object, TEventArgs, Task>> tmpInvocationList;
            lock (_locker)
                tmpInvocationList = new List<Func<object, TEventArgs, Task>>(_invocationList);

            foreach (var callback in tmpInvocationList)
                await callback(sender, eventArgs);
        }
        #endregion

        #region Public static methods
        /// <summary>
        /// Add operator overwrite
        /// </summary>
        /// <param name="e">AsyncEvent instance</param>
        /// <param name="callback">Callback delegate to add</param>
        /// <returns>AsyncEvent instance</returns>
        public static AsyncEvent<TEventArgs> operator +(AsyncEvent<TEventArgs> e, Func<object, TEventArgs, Task> callback)
        {
            if (callback == null) throw new NullReferenceException("callback is null");
            if (e == null) e = new AsyncEvent<TEventArgs>();
            lock (e._locker)
                e._invocationList.Add(callback);
            return e;
        }
        /// <summary>
        /// Substract operator overwrite
        /// </summary>
        /// <param name="e">AsyncEvent instance</param>
        /// <param name="callback">Callback delegate to substract</param>
        /// <returns>AsyncEvent instance</returns>
        public static AsyncEvent<TEventArgs> operator -(AsyncEvent<TEventArgs> e, Func<object, TEventArgs, Task> callback)
        {
            if (callback == null) throw new NullReferenceException("callback is null");
            if (e == null) return null;
            lock (e._locker)
                e._invocationList.Remove(callback);
            return e;
        }
        #endregion
    }
}