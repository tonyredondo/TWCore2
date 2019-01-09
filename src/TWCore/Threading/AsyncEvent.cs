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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TWCore.Threading
{
    /// <summary>
    /// Async event handler class
    /// </summary>
    /// <typeparam name="TEventArgs">Event args instance</typeparam>
    public sealed class AsyncEvent<TEventArgs>
    {
	    private readonly object _locker;
        private List<Func<object, TEventArgs, Task>> _funcsInvocationList;
        private List<Action<object, TEventArgs>> _actionsInvocationList;
		private bool _dirty;
		private Func<object, TEventArgs, Task>[] _callFuncArray;
		private Action<object, TEventArgs>[] _callActionArray;

        #region .ctor
        /// <summary>
        /// .ctor
        /// </summary>
        private AsyncEvent()
        {
            _dirty = true;
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
			if (_dirty)
			{
				lock (_locker)
				{
					if (_dirty)
					{
						_callFuncArray = _funcsInvocationList?.ToArray();
						_callActionArray = _actionsInvocationList?.ToArray();
						_dirty = false;
					}
				}
			}

			if (!(_callFuncArray is null))
			{
				var length = _callFuncArray.Length;
				for (var i = 0; i < length; i++)
					await _callFuncArray[i](sender, eventArgs).ConfigureAwait(false);
			}
			if (!(_callActionArray is null))
			{
				var length = _callActionArray.Length;
				for (var i = 0; i < length; i++)
					_callActionArray[i](sender, eventArgs);
			}
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
            if (callback is null) throw new NullReferenceException("callback is null");
            if (e is null) e = new AsyncEvent<TEventArgs>();
			lock (e._locker)
			{
				if (e._funcsInvocationList is null)
					e._funcsInvocationList = new List<Func<object, TEventArgs, Task>>();
				e._funcsInvocationList.Add(callback);
				e._dirty = true;
			}
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
            if (callback is null) throw new NullReferenceException("callback is null");
            if (e is null) return null;
			lock (e._locker)
			{
				if (e._funcsInvocationList is null)
					e._funcsInvocationList = new List<Func<object, TEventArgs, Task>>();
				e._funcsInvocationList.Remove(callback);
				e._dirty = true;
			}
            return e;
        }
        /// <summary>
        /// Add operator overwrite
        /// </summary>
        /// <param name="e">AsyncEvent instance</param>
        /// <param name="callback">Callback delegate to add</param>
        /// <returns>AsyncEvent instance</returns>
        public static AsyncEvent<TEventArgs> operator +(AsyncEvent<TEventArgs> e, Action<object, TEventArgs> callback)
        {
            if (callback is null) throw new NullReferenceException("callback is null");
            if (e is null) e = new AsyncEvent<TEventArgs>();
            lock (e._locker)
            {
	            if (e._actionsInvocationList is null)
		            e._actionsInvocationList = new List<Action<object, TEventArgs>>();
                e._actionsInvocationList.Add(callback);
                e._dirty = true;
            }
            return e;
        }
        /// <summary>
        /// Substract operator overwrite
        /// </summary>
        /// <param name="e">AsyncEvent instance</param>
        /// <param name="callback">Callback delegate to substract</param>
        /// <returns>AsyncEvent instance</returns>
        public static AsyncEvent<TEventArgs> operator -(AsyncEvent<TEventArgs> e, Action<object, TEventArgs> callback)
        {
            if (callback is null) throw new NullReferenceException("callback is null");
            if (e is null) return null;
            lock (e._locker)
            {
	            if (e._actionsInvocationList is null)
		            e._actionsInvocationList = new List<Action<object, TEventArgs>>();
                e._actionsInvocationList.Remove(callback);
                e._dirty = true;
            }
            return e;
        }
        #endregion
    }
}