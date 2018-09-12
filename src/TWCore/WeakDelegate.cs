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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using TWCore.Reflection;

// ReSharper disable UnusedMember.Global

namespace TWCore
{
    #region Delegates
    /// <summary>
    /// Weak action delegate
    /// </summary>
    public delegate bool WeakAction();
    /// <summary>
    /// Weak action delegate
    /// </summary>
    /// <param name="arg">Argument 1</param>
    /// <typeparam name="T">Type of argument 1</typeparam>
    public delegate bool WeakAction<in T>(T arg);
    /// <summary>
    /// Weak action delegate
    /// </summary>
    /// <param name="arg1">Argument 1</param>
    /// <param name="arg2">Argument 2</param>
    /// <typeparam name="T1">Type of argument 1</typeparam>
    /// <typeparam name="T2">Type of argument 2</typeparam>
    public delegate bool WeakAction<in T1, in T2>(T1 arg1, T2 arg2);
    /// <summary>
    /// Weak action delegate
    /// </summary>
    /// <param name="arg1">Argument 1</param>
    /// <param name="arg2">Argument 2</param>
    /// <param name="arg3">Argument 3</param>
    /// <typeparam name="T1">Type of argument 1</typeparam>
    /// <typeparam name="T2">Type of argument 2</typeparam>
    /// <typeparam name="T3">Type of argument 3</typeparam>
    public delegate bool WeakAction<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);
    /// <summary>
    /// Weak action delegate
    /// </summary>
    /// <param name="arg1">Argument 1</param>
    /// <param name="arg2">Argument 2</param>
    /// <param name="arg3">Argument 3</param>
    /// <param name="arg4">Argument 3</param>
    /// <typeparam name="T1">Type of argument 1</typeparam>
    /// <typeparam name="T2">Type of argument 2</typeparam>
    /// <typeparam name="T3">Type of argument 3</typeparam>
    /// <typeparam name="T4">Type of argument 4</typeparam>
    public delegate bool WeakAction<in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    /// <summary>
    /// Weak Func delegate
    /// </summary>
    /// <typeparam name="TResult">Type of result</typeparam>
    public delegate (bool Ran, TResult Result) WeakFunc<TResult>();
    /// <summary>
    /// Weak Func delegate
    /// </summary>
    /// <param name="arg">Argument 1</param>
    /// <typeparam name="T">Type of argument 1</typeparam>
    /// <typeparam name="TResult">Type of result</typeparam>
    public delegate (bool Ran, TResult Result) WeakFunc<in T, TResult>(T arg);
    /// <summary>
    /// Weak Func delegate
    /// </summary>
    /// <param name="arg1">Argument 1</param>
    /// <param name="arg2">Argument 2</param>
    /// <typeparam name="T1">Type of argument 1</typeparam>
    /// <typeparam name="T2">Type of argument 2</typeparam>
    /// <typeparam name="TResult">Type of result</typeparam>
    public delegate (bool Ran, TResult Result) WeakFunc<in T1, in T2, TResult>(T1 arg1, T2 arg2);
    /// <summary>
    /// Weak Func delegate
    /// </summary>
    /// <param name="arg1">Argument 1</param>
    /// <param name="arg2">Argument 2</param>
    /// <param name="arg3">Argument 3</param>
    /// <typeparam name="T1">Type of argument 1</typeparam>
    /// <typeparam name="T2">Type of argument 2</typeparam>
    /// <typeparam name="T3">Type of argument 3</typeparam>
    /// <typeparam name="TResult">Type of result</typeparam>
    public delegate (bool Ran, TResult Result) WeakFunc<in T1, in T2, in T3, TResult>(T1 arg1, T2 arg2, T3 arg3);
    /// <summary>
    /// Weak Func delegate
    /// </summary>
    /// <param name="arg1">Argument 1</param>
    /// <param name="arg2">Argument 2</param>
    /// <param name="arg3">Argument 3</param>
    /// <param name="arg4">Argument 4</param>
    /// <typeparam name="T1">Type of argument 1</typeparam>
    /// <typeparam name="T2">Type of argument 2</typeparam>
    /// <typeparam name="T3">Type of argument 3</typeparam>
    /// <typeparam name="T4">Type of argument 4</typeparam>
    /// <typeparam name="TResult">Type of result</typeparam>
    public delegate (bool Ran, TResult Result) WeakFunc<in T1, in T2, in T3, in T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    #endregion

    /// <summary>
    /// Delegates using objects with a weak reference
    /// </summary>
    public class WeakDelegate
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly bool _isStatic;

        #region Properties
        /// <summary>
        /// Weak object reference
        /// </summary>
        public WeakReference WeakObject { get; }
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
            _isStatic = method?.IsStatic ?? false;
            Method = method?.GetMethodAccessor();
            if (target is WeakReference weakTarget)
                WeakObject = weakTarget;
            else if (method != null && !_isStatic)
                WeakObject = new WeakReference(target);
            else
                WeakObject = null;
        }
        /// <summary>
        /// Create a WeakDelegate from a Delegate
        /// </summary>
        /// <param name="delegate">Delegate</param>
        public WeakDelegate(Delegate @delegate) : this(@delegate.Target, @delegate.Method) { }
        #endregion

        #region Public Methods
        /// <summary>
        /// Tries to invoke a method info to a weak object if is available
        /// </summary>
        /// <param name="parameters">Invoke parameters</param>
        /// <param name="result">Invoke return value.</param>
        /// <returns>True if the delegate was invoked; false if the object was collected by the GC</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryInvoke(object[] parameters, out object result)
        {
            result = null;
            if (Method is null)
                return false;
            if (_isStatic)
            {
                result = Method.Invoke(null, parameters);
                return true;
            }
            var target = WeakObject.Target;
            if (WeakObject.IsAlive)
            {
                result = Method.Invoke(target, parameters);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Tries to invoke a method info to a weak object if is available
        /// </summary>
        /// <param name="parameters">Invoke parameters</param>
        /// <returns>True if the delegate was invoked; false if the object was collected by the GC</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryInvokeAction(params object[] parameters)
        {
            if (Method is null)
                return false;
            if (_isStatic)
            {
                Method.Invoke(null, parameters);
                return true;
            }
            var target = WeakObject.Target;
            if (WeakObject.IsAlive)
            {
                Method.Invoke(target, parameters);
                return true;
            }
            return false;
        }

        #region GetActions
        /// <summary>
        /// Gets a WeakAction delegate from the instance
        /// </summary>
        /// <returns>WeakAction instance</returns>
        public WeakAction GetAction()
        {
            return () => TryInvokeAction();
        }
        /// <summary>
        /// Gets a WeakAction delegate from the instance
        /// </summary>
        /// <returns>WeakAction instance</returns>
        public WeakAction<T> GetAction<T>()
        {
            return (arg) => TryInvokeAction(arg);
        }
        /// <summary>
        /// Gets a WeakAction delegate from the instance
        /// </summary>
        /// <returns>WeakAction instance</returns>
        public WeakAction<T1, T2> GetAction<T1, T2>()
        {
            return (arg1, arg2) => TryInvokeAction(arg1, arg2);
        }
        /// <summary>
        /// Gets a WeakAction delegate from the instance
        /// </summary>
        /// <returns>WeakAction instance</returns>
        public WeakAction<T1, T2, T3> GetAction<T1, T2, T3>()
        {
            return (arg1, arg2, arg3) => TryInvokeAction(arg1, arg2, arg3);
        }
        /// <summary>
        /// Gets a WeakAction delegate from the instance
        /// </summary>
        /// <returns>WeakAction instance</returns>
        public WeakAction<T1, T2, T3, T4> GetAction<T1, T2, T3, T4>()
        {
            return (arg1, arg2, arg3, arg4) => TryInvokeAction(arg1, arg2, arg3, arg4);
        }
        #endregion

        #region GetFunc
        /// <summary>
        /// Gets a WeakFunc delegate from the instance
        /// </summary>
        /// <returns>WeakFunc instance</returns>
        public WeakFunc<TResult> GetFunc<TResult>()
        {
            return () =>
            {
                if (TryInvoke(null, out var result))
                    return (true, (TResult)result);
                return (false, default(TResult));
            };
        }
        /// <summary>
        /// Gets a WeakFunc delegate from the instance
        /// </summary>
        /// <returns>WeakFunc instance</returns>
        public WeakFunc<T1, TResult> GetFunc<T1, TResult>()
        {
            return (arg) =>
            {
                if (TryInvoke(new object[] { arg }, out var result))
                    return (true, (TResult)result);
                return (false, default(TResult));
            };
        }
        /// <summary>
        /// Gets a WeakFunc delegate from the instance
        /// </summary>
        /// <returns>WeakFunc instance</returns>
        public WeakFunc<T1, T2, TResult> GetFunc<T1, T2, TResult>()
        {
            return (arg1, arg2) =>
            {
                if (TryInvoke(new object[] { arg1, arg2 }, out var result))
                    return (true, (TResult)result);
                return (false, default(TResult));
            };
        }
        /// <summary>
        /// Gets a WeakFunc delegate from the instance
        /// </summary>
        /// <returns>WeakFunc instance</returns>
        public WeakFunc<T1, T2, T3, TResult> GetFunc<T1, T2, T3, TResult>()
        {
            return (arg1, arg2, arg3) =>
            {
                if (TryInvoke(new object[] { arg1, arg2, arg3 }, out var result))
                    return (true, (TResult)result);
                return (false, default(TResult));
            };
        }
        /// <summary>
        /// Gets a WeakFunc delegate from the instance
        /// </summary>
        /// <returns>WeakFunc instance</returns>
        public WeakFunc<T1, T2, T3, T4, TResult> GetFunc<T1, T2, T3, T4, TResult>()
        {
            return (arg1, arg2, arg3, arg4) =>
            {
                if (TryInvoke(new object[] { arg1, arg2, arg3, arg4 }, out var result))
                    return (true, (TResult)result);
                return (false, default(TResult));
            };
        }
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
        {
            var weakDelegate = new WeakDelegate(action.Target, action.Method);
            return () => weakDelegate.TryInvokeAction();
        }
        /// <summary>
        /// Creates an Weak Action delegate from a normal Action
        /// </summary>
        /// <param name="action">Action delegate</param>
        /// <returns>Weak action delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakAction<T> Create<T>(Action<T> action)
        {
            var weakDelegate = new WeakDelegate(action.Target, action.Method);
            return (arg) => weakDelegate.TryInvokeAction(arg);
        }
        /// <summary>
        /// Creates an Weak Action delegate from a normal Action
        /// </summary>
        /// <param name="action">Action delegate</param>
        /// <returns>Weak action delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakAction<T1, T2> Create<T1, T2>(Action<T1, T2> action)
        {
            var weakDelegate = new WeakDelegate(action.Target, action.Method);
            return (arg1, arg2) => weakDelegate.TryInvokeAction(arg1, arg2);
        }
        /// <summary>
        /// Creates an Weak Action delegate from a normal Action
        /// </summary>
        /// <param name="action">Action delegate</param>
        /// <returns>Weak action delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakAction<T1, T2, T3> Create<T1, T2, T3>(Action<T1, T2, T3> action)
        {
            var weakDelegate = new WeakDelegate(action.Target, action.Method);
            return (arg1, arg2, arg3) => weakDelegate.TryInvokeAction(arg1, arg2, arg3);
        }
        /// <summary>
        /// Creates an Weak Action delegate from a normal Action
        /// </summary>
        /// <param name="action">Action delegate</param>
        /// <returns>Weak action delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakAction<T1, T2, T3, T4> Create<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
        {
            var weakDelegate = new WeakDelegate(action.Target, action.Method);
            return (arg1, arg2, arg3, arg4) => weakDelegate.TryInvokeAction(arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// Creates an Weak Func delegate from a normal Func
        /// </summary>
        /// <param name="func">Func delegate</param>
        /// <returns>Weak Func delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakFunc<TResult> Create<TResult>(Func<TResult> func)
        {
            var weakDelegate = new WeakDelegate(func.Target, func.Method);
            return () =>
            {
                if (weakDelegate.TryInvoke(null, out var result))
                    return (true, (TResult)result);
                return (false, default(TResult));
            };
        }
        /// <summary>
        /// Creates an Weak Func delegate from a normal Func
        /// </summary>
        /// <param name="func">Func delegate</param>
        /// <returns>Weak Func delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakFunc<T, TResult> Create<T, TResult>(Func<T, TResult> func)
        {
            var weakDelegate = new WeakDelegate(func.Target, func.Method);
            return (arg) =>
            {
                if (weakDelegate.TryInvoke(new object[] { arg }, out var result))
                    return (true, (TResult)result);
                return (false, default(TResult));
            };
        }
        /// <summary>
        /// Creates an Weak Func delegate from a normal Func
        /// </summary>
        /// <param name="func">Func delegate</param>
        /// <returns>Weak Func delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakFunc<T1, T2, TResult> Create<T1, T2, TResult>(Func<T1, T2, TResult> func)
        {
            var weakDelegate = new WeakDelegate(func.Target, func.Method);
            return (arg1, arg2) =>
            {
                if (weakDelegate.TryInvoke(new object[] { arg1, arg2 }, out var result))
                    return (true, (TResult)result);
                return (false, default(TResult));
            };
        }
        /// <summary>
        /// Creates an Weak Func delegate from a normal Func
        /// </summary>
        /// <param name="func">Func delegate</param>
        /// <returns>Weak Func delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakFunc<T1, T2, T3, TResult> Create<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func)
        {
            var weakDelegate = new WeakDelegate(func.Target, func.Method);
            return (arg1, arg2, arg3) =>
            {
                if (weakDelegate.TryInvoke(new object[] { arg1, arg2, arg3 }, out var result))
                    return (true, (TResult)result);
                return (false, default(TResult));
            };
        }
        /// <summary>
        /// Creates an Weak Func delegate from a normal Func
        /// </summary>
        /// <param name="func">Func delegate</param>
        /// <returns>Weak Func delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakFunc<T1, T2, T3, T4, TResult> Create<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func)
        {
            var weakDelegate = new WeakDelegate(func.Target, func.Method);
            return (arg1, arg2, arg3, arg4) =>
            {
                if (weakDelegate.TryInvoke(new object[] { arg1, arg2, arg3, arg4 }, out var result))
                    return (true, (TResult)result);
                return (false, default(TResult));
            };
        }
        #endregion
    }

    /// <summary>
    /// Weak Delegate Extensions
    /// </summary>
    public static class WeakDelegateExtensions
    {
        #region Static Create Delegate Methods
        /// <summary>
        /// Creates an Weak delegate from a normal Action
        /// </summary>
        /// <param name="action">Action delegate</param>
        /// <returns>WeakDelegate instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakDelegate GetWeak(this Action action)
            => new WeakDelegate(action.Target, action.Method);
        /// <summary>
        /// Creates an Weak delegate from a normal Action
        /// </summary>
        /// <param name="action">Action delegate</param>
        /// <returns>WeakDelegate instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakDelegate GetWeak<T>(this Action<T> action)
            => new WeakDelegate(action.Target, action.Method);
        /// <summary>
        /// Creates an Weak delegate from a normal Action
        /// </summary>
        /// <param name="action">Action delegate</param>
        /// <returns>WeakDelegate instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakDelegate GetWeak<T1, T2>(this Action<T1, T2> action)
            => new WeakDelegate(action.Target, action.Method);
        /// <summary>
        /// Creates an Weak delegate from a normal Action
        /// </summary>
        /// <param name="action">Action delegate</param>
        /// <returns>WeakDelegate instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakDelegate GetWeak<T1, T2, T3>(this Action<T1, T2, T3> action)
            => new WeakDelegate(action.Target, action.Method);
        /// <summary>
        /// Creates an Weak delegate from a normal Func
        /// </summary>
        /// <param name="func">Func delegate</param>
        /// <returns>WeakDelegate instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakDelegate GetWeak<TResult>(this Func<TResult> func)
            => new WeakDelegate(func.Target, func.Method);
        /// <summary>
        /// Creates an Weak delegate from a normal Func
        /// </summary>
        /// <param name="func">Func delegate</param>
        /// <returns>WeakDelegate instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakDelegate GetWeak<T1, TResult>(this Func<T1, TResult> func)
            => new WeakDelegate(func.Target, func.Method);
        /// <summary>
        /// Creates an Weak delegate from a normal Func
        /// </summary>
        /// <param name="func">Func delegate</param>
        /// <returns>WeakDelegate instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakDelegate GetWeak<T1, T2, TResult>(this Func<T1, T2, TResult> func)
            => new WeakDelegate(func.Target, func.Method);
        /// <summary>
        /// Creates an Weak delegate from a normal Func
        /// </summary>
        /// <param name="func">Func delegate</param>
        /// <returns>WeakDelegate instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakDelegate GetWeak<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> func)
            => new WeakDelegate(func.Target, func.Method);
        /// <summary>
        /// Creates an Weak delegate from a normal Func
        /// </summary>
        /// <param name="func">Func delegate</param>
        /// <returns>WeakDelegate instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeakDelegate GetWeak<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> func)
            => new WeakDelegate(func.Target, func.Method);
        #endregion
    }
}
