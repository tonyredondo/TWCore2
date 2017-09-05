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
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TWCore
{
    /// <summary>
    /// Helper to call delegates using objects with a weak reference
    /// </summary>
    public class WeakDelegate
    {
        #region Properties
        /// <summary>
        /// Weak object reference
        /// </summary>
        public WeakReference<object> WeakObject { get; }
        /// <summary>
        /// Method info to execute
        /// </summary>
        public MethodInfo Method { get; }
        #endregion

        #region .ctor
        /// <summary>
        /// Helper to call delegates of object with a weak reference
        /// </summary>
        /// <param name="target">Target object</param>
        /// <param name="method">Method info</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WeakDelegate(object target, MethodInfo method)
        {
            WeakObject = new WeakReference<object>(target);
            Method = method;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Tries to invoke a method info to a weak object if is available
        /// </summary>
        /// <param name="parameters">Invoke parameters</param>
        /// <returns>Invoke return value, if the object has been collected by the GC then null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object TryInvoke(params object[] parameters)
        {
            return WeakObject.TryGetTarget(out var target) ? Method.Invoke(target, parameters) : null;
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Creates an Weak Action delegate from a normal Action
        /// </summary>
        /// <param name="action">Action delegate</param>
        /// <returns>Weak action delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action Create(Action action)
        {
            var weakDelegate = new WeakDelegate(action.Target, action.GetMethodInfo());
            return () => weakDelegate.TryInvoke();
        }
        /// <summary>
        /// Creates an Weak Action delegate from a normal Action
        /// </summary>
        /// <param name="action">Action delegate</param>
        /// <returns>Weak action delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<T> Create<T>(Action<T> action)
        {
            var weakDelegate = new WeakDelegate(action.Target, action.GetMethodInfo());
            return t => weakDelegate.TryInvoke(t);
        }
        /// <summary>
        /// Creates an Weak Action delegate from a normal Action
        /// </summary>
        /// <param name="action">Action delegate</param>
        /// <returns>Weak action delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<T1, T2> Create<T1, T2>(Action<T1, T2> action)
        {
            var weakDelegate = new WeakDelegate(action.Target, action.GetMethodInfo());
            return (t1, t2) => weakDelegate.TryInvoke(t1, t2);
        }
        /// <summary>
        /// Creates an Weak Action delegate from a normal Action
        /// </summary>
        /// <param name="action">Action delegate</param>
        /// <returns>Weak action delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<T1, T2, T3> Create<T1, T2, T3>(Action<T1, T2, T3> action)
        {
            var weakDelegate = new WeakDelegate(action.Target, action.GetMethodInfo());
            return (t1, t2, t3) => weakDelegate.TryInvoke(t1, t2, t3);
        }
        /// <summary>
        /// Creates an Weak Action delegate from a normal Action
        /// </summary>
        /// <param name="action">Action delegate</param>
        /// <returns>Weak action delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<T1, T2, T3, T4> Create<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
        {
            var weakDelegate = new WeakDelegate(action.Target, action.GetMethodInfo());
            return (t1, t2, t3, t4) => weakDelegate.TryInvoke(t1, t2, t3, t4);
        }


        /// <summary>
        /// Creates an Weak Func delegate from a normal Func
        /// </summary>
        /// <param name="func">Func delegate</param>
        /// <returns>Weak Func delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<TResult> Create<TResult>(Func<TResult> func)
        {
            var weakDelegate = new WeakDelegate(func.Target, func.GetMethodInfo());
            return () => (TResult)weakDelegate.TryInvoke();
        }
        /// <summary>
        /// Creates an Weak Func delegate from a normal Func
        /// </summary>
        /// <param name="func">Func delegate</param>
        /// <returns>Weak Func delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T, TResult> Create<T, TResult>(Func<T, TResult> func)
        {
            var weakDelegate = new WeakDelegate(func.Target, func.GetMethodInfo());
            return (t) => (TResult)weakDelegate.TryInvoke(t);
        }
        /// <summary>
        /// Creates an Weak Func delegate from a normal Func
        /// </summary>
        /// <param name="func">Func delegate</param>
        /// <returns>Weak Func delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, TResult> Create<T1, T2, TResult>(Func<T1, T2, TResult> func)
        {
            var weakDelegate = new WeakDelegate(func.Target, func.GetMethodInfo());
            return (t1, t2) => (TResult)weakDelegate.TryInvoke(t1, t2);
        }
        /// <summary>
        /// Creates an Weak Func delegate from a normal Func
        /// </summary>
        /// <param name="func">Func delegate</param>
        /// <returns>Weak Func delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, T3, TResult> Create<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func)
        {
            var weakDelegate = new WeakDelegate(func.Target, func.GetMethodInfo());
            return (t1, t2, t3) => (TResult)weakDelegate.TryInvoke(t1, t2, t3);
        }
        /// <summary>
        /// Creates an Weak Func delegate from a normal Func
        /// </summary>
        /// <param name="func">Func delegate</param>
        /// <returns>Weak Func delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, T3, T4, TResult> Create<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func)
        {
            var weakDelegate = new WeakDelegate(func.Target, func.GetMethodInfo());
            return (t1, t2, t3, t4) => (TResult)weakDelegate.TryInvoke(t1, t2, t3, t4);
        }
        #endregion
    }
}
