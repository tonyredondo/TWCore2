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
using System.Reflection;
using System.Runtime.CompilerServices;
using TWCore.Reflection;

// ReSharper disable UnusedMember.Global

namespace TWCore
{
    /// <summary>
    /// Helper to call delegates using objects with a weak reference
    /// </summary>
    public class WeakDelegate
    {
        private readonly bool _isStatic;
        
        #region Delegates
        public delegate bool WeakAction();
        public delegate bool WeakAction<in T>(T arg);
        public delegate bool WeakAction<in T1, in T2>(T1 arg1, T2 arg2);
        public delegate bool WeakAction<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);
        public delegate bool WeakAction<in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
        public delegate (bool Ran, TResult Result) WeakFunc<TResult>();
        public delegate (bool Ran, TResult Result) WeakFunc<in T, TResult>(T arg);
        public delegate (bool Ran, TResult Result) WeakFunc<in T1, in T2, TResult>(T1 arg1, T2 arg2);
        public delegate (bool Ran, TResult Result) WeakFunc<in T1, in T2, in T3, TResult>(T1 arg1, T2 arg2, T3 arg3);
        public delegate (bool Ran, TResult Result) WeakFunc<in T1, in T2, in T3, in T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
        #endregion
        
        #region Properties
        /// <summary>
        /// Weak object reference
        /// </summary>
        public WeakReference<object> WeakObject { get; }
        /// <summary>
        /// Method to execute
        /// </summary>
        public MethodAccessorDelegate Method { get; }
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
            if (method == null) return;
            _isStatic = method.IsStatic;
            if (!_isStatic && target != null)
                WeakObject = target is WeakReference<object> wTarget ? wTarget : new WeakReference<object>(target);
            Method = method.GetMethodAccessor();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Tries to invoke a method info to a weak object if is available
        /// </summary>
        /// <param name="parameters">Invoke parameters</param>
        /// <returns>Invoke return value, if the object has been collected by the GC then null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (bool Ran, object Result) TryInvoke(params object[] parameters)
        {
            if (Method == null) return (false, null);
            if (_isStatic || WeakObject == null)
                return (true, Method.Invoke(null, parameters));
            return WeakObject.TryGetTarget(out var target) ? 
                (true, Method.Invoke(target, parameters)) : 
                (false, null);
        }

        #region GetActions
        /// <summary>
        /// Gets a WeakAction delegate from the instance
        /// </summary>
        /// <returns>WeakAction instance</returns>
        public WeakAction GetAction()
            => () => TryInvoke().Ran;
        /// <summary>
        /// Gets a WeakAction delegate from the instance
        /// </summary>
        /// <returns>WeakAction instance</returns>
        public WeakAction<T> GetAction<T>()
            => (arg) => TryInvoke(arg).Ran;
        /// <summary>
        /// Gets a WeakAction delegate from the instance
        /// </summary>
        /// <returns>WeakAction instance</returns>
        public WeakAction<T1, T2> GetAction<T1, T2>()
            => (arg1, arg2) => TryInvoke(arg1, arg2).Ran;
        /// <summary>
        /// Gets a WeakAction delegate from the instance
        /// </summary>
        /// <returns>WeakAction instance</returns>
        public WeakAction<T1, T2, T3> GetAction<T1, T2, T3>()
            => (arg1, arg2, arg3) => TryInvoke(arg1, arg2, arg3).Ran;
        /// <summary>
        /// Gets a WeakAction delegate from the instance
        /// </summary>
        /// <returns>WeakAction instance</returns>
        public WeakAction<T1, T2, T3, T4> GetAction<T1, T2, T3, T4>()
            => (arg1, arg2, arg3, arg4) => TryInvoke(arg1, arg2, arg3, arg4).Ran;
        #endregion
        
        #region GetFunc
        /// <summary>
        /// Gets a WeakFunc delegate from the instance
        /// </summary>
        /// <returns>WeakFunc instance</returns>
        public WeakFunc<TResult> GetFunc<TResult>() 
            => () =>
            {
                var response = TryInvoke();
                return (response.Ran, (TResult) response.Result);
            };
        /// <summary>
        /// Gets a WeakFunc delegate from the instance
        /// </summary>
        /// <returns>WeakFunc instance</returns>
        public WeakFunc<T1, TResult> GetFunc<T1, TResult>()
            => (arg) =>
            {
                var response = TryInvoke(arg);
                return (response.Ran, (TResult) response.Result);
            };
        /// <summary>
        /// Gets a WeakFunc delegate from the instance
        /// </summary>
        /// <returns>WeakFunc instance</returns>
        public WeakFunc<T1, T2, TResult> GetFunc<T1, T2, TResult>()
            => (arg1, arg2) =>
            {
                var response = TryInvoke(arg1, arg2);
                return (response.Ran, (TResult) response.Result);
            };
        /// <summary>
        /// Gets a WeakFunc delegate from the instance
        /// </summary>
        /// <returns>WeakFunc instance</returns>
        public WeakFunc<T1, T2, T3, TResult> GetFunc<T1, T2, T3, TResult>()
            => (arg1, arg2, arg3) =>
            {
                var response = TryInvoke(arg1, arg2, arg3);
                return (response.Ran, (TResult) response.Result);
            };
        /// <summary>
        /// Gets a WeakFunc delegate from the instance
        /// </summary>
        /// <returns>WeakFunc instance</returns>
        public WeakFunc<T1, T2, T3, T4, TResult> GetFunc<T1, T2, T3, T4, TResult>()
            => (arg1, arg2, arg3, arg4) =>
            {
                var response = TryInvoke(arg1, arg2, arg3);
                return (response.Ran, (TResult) response.Result);
            };
        #endregion
        
        #endregion

        #region Static Methods
        /// <summary>
        /// Creates an Weak Action delegate from a normal Action
        /// </summary>
        /// <param name="action">Action delegate</param>
        /// <returns>Weak action delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakAction Create(Action action)
            => new WeakDelegate(action.Target, action.GetMethodInfo()).GetAction();
        /// <summary>
        /// Creates an Weak Action delegate from a normal Action
        /// </summary>
        /// <param name="action">Action delegate</param>
        /// <returns>Weak action delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakAction<T> Create<T>(Action<T> action)
            => new WeakDelegate(action.Target, action.GetMethodInfo()).GetAction<T>();
        /// <summary>
        /// Creates an Weak Action delegate from a normal Action
        /// </summary>
        /// <param name="action">Action delegate</param>
        /// <returns>Weak action delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakAction<T1, T2> Create<T1, T2>(Action<T1, T2> action)
            => new WeakDelegate(action.Target, action.GetMethodInfo()).GetAction<T1, T2>();
        /// <summary>
        /// Creates an Weak Action delegate from a normal Action
        /// </summary>
        /// <param name="action">Action delegate</param>
        /// <returns>Weak action delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakAction<T1, T2, T3> Create<T1, T2, T3>(Action<T1, T2, T3> action)
            => new WeakDelegate(action.Target, action.GetMethodInfo()).GetAction<T1, T2, T3>();
        /// <summary>
        /// Creates an Weak Action delegate from a normal Action
        /// </summary>
        /// <param name="action">Action delegate</param>
        /// <returns>Weak action delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakAction<T1, T2, T3, T4> Create<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
            => new WeakDelegate(action.Target, action.GetMethodInfo()).GetAction<T1, T2, T3, T4>();

        /// <summary>
        /// Creates an Weak Func delegate from a normal Func
        /// </summary>
        /// <param name="func">Func delegate</param>
        /// <returns>Weak Func delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakFunc<TResult> Create<TResult>(Func<TResult> func)
            => new WeakDelegate(func.Target, func.GetMethodInfo()).GetFunc<TResult>();
        /// <summary>
        /// Creates an Weak Func delegate from a normal Func
        /// </summary>
        /// <param name="func">Func delegate</param>
        /// <returns>Weak Func delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakFunc<T, TResult> Create<T, TResult>(Func<T, TResult> func)
            => new WeakDelegate(func.Target, func.GetMethodInfo()).GetFunc<T, TResult>();
        /// <summary>
        /// Creates an Weak Func delegate from a normal Func
        /// </summary>
        /// <param name="func">Func delegate</param>
        /// <returns>Weak Func delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakFunc<T1, T2, TResult> Create<T1, T2, TResult>(Func<T1, T2, TResult> func)
            => new WeakDelegate(func.Target, func.GetMethodInfo()).GetFunc<T1, T2, TResult>();
        /// <summary>
        /// Creates an Weak Func delegate from a normal Func
        /// </summary>
        /// <param name="func">Func delegate</param>
        /// <returns>Weak Func delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakFunc<T1, T2, T3, TResult> Create<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func)
            => new WeakDelegate(func.Target, func.GetMethodInfo()).GetFunc<T1, T2, T3, TResult>();
        /// <summary>
        /// Creates an Weak Func delegate from a normal Func
        /// </summary>
        /// <param name="func">Func delegate</param>
        /// <returns>Weak Func delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakFunc<T1, T2, T3, T4, TResult> Create<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func)
            => new WeakDelegate(func.Target, func.GetMethodInfo()).GetFunc<T1, T2, T3, T4, TResult>();
        #endregion
    }
}
