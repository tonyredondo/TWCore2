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
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Net;
// ReSharper disable MethodSupportsCancellation

// ReSharper disable CheckNamespace

namespace TWCore
{
    /// <summary>
    /// General Extensions
    /// </summary>
    public static partial class Extensions
    {
        #region Attributes
        /// <summary>
        /// Gets an attribute from an object
        /// </summary>
        /// <typeparam name="T">Attribute type</typeparam>
        /// <param name="obj">Object to get the attribute from</param>
        /// <returns>Attribute</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetAttribute<T>(this object obj) where T : class
        {
            var objType = obj as Type ?? obj.GetType();
            object[] attrs;
            if (objType.IsEnum)
                attrs = objType.GetRuntimeField(obj.ToString()).GetCustomAttributes(true);
            else if (obj is PropertyInfo pinfo)
                attrs = pinfo.GetCustomAttributes(true);
            else if (obj is FieldInfo finfo)
                attrs = finfo.GetCustomAttributes(true);
            else if (obj is MethodInfo minfo)
                attrs = minfo.GetCustomAttributes(true);
            else
                attrs = objType.GetCustomAttributes(true);
            foreach (var item in attrs)
                if (item is T titem)
                    return titem;
            return null;
        }
        #endregion

        #region To/From Dictionary
        /// <summary>
        /// Gets a dictionary form an object converting all the properties to key/value pairs.
        /// </summary>
        /// <param name="source">Source object</param>
        /// <returns>IDictionary instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDictionary<string, object> ToDictionary(this object source)
        {
            if (source is null) return null;
            var sType = source.GetType();
            if (sType.IsValueType) return null;
            var dct = new Dictionary<string, object>();
            var props = sType.GetRuntimeProperties();
            foreach (var prop in props)
            {
                if (!prop.CanRead) continue;
                var value = prop.GetValue(source, null);
                dct.Add(prop.Name, value);
            }
            return dct;
        }

        /// <summary>
        /// Sets the properties values of an object from a dictionary with key/value pairs.
        /// </summary>
        /// <param name="source">Source dictionary with values</param>
        /// <param name="target">Target object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FromDictionary(this object target, IDictionary<string, object> source)
        {
            if (source is null || target is null) return;
            var sType = target.GetType();
            if (sType.IsValueType) return;
            foreach (var item in source)
            {
                var prop = sType.GetRuntimeProperty(item.Key);
                if (prop != null && prop.CanWrite)
                    prop.SetValue(target, item.Value, null);
            }
        }
        #endregion

        #region String Dictionary Extensions
        /// <summary>
        /// Convert object properties to a Dictionary
        /// </summary>
        /// <param name="source">Object source</param>
        /// <param name="objectToStringFunction">Function to convert an object property to string</param>
        /// <returns>Dictionary</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<string, string> ToStringDictionary(this object source, Func<object, Type, string> objectToStringFunction)
        {
            if (source is null) return null;
            var sType = source.GetType();
            if (sType.IsValueType) return null;
            var dct = new Dictionary<string, string>();
            var props = sType.GetRuntimeProperties();
            foreach (var prop in props)
            {
                if (!prop.CanRead) continue;
                var value = prop.GetValue(source, null);
                var nValue = objectToStringFunction(value, prop.PropertyType);
                dct[prop.Name] = nValue;
            }
            return dct;
        }
        /// <summary>
        /// Sets the object properties with values from the Dictionary
        /// </summary>
        /// <param name="source">Source Dictionary</param>
        /// <param name="target">Object Target</param>
        /// <param name="stringToObjectFunction">Function to convert a string value to an object value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FromStringDictionary(this object target, Dictionary<string, string> source, Func<string, Type, object> stringToObjectFunction)
        {
            if (source is null || target is null) return;
            var sType = target.GetType();
            if (sType.IsValueType) return;
            foreach (var item in source)
            {
                var prop = sType.GetRuntimeProperty(item.Key);
                if (prop is null || !prop.CanWrite) continue;
                var nValue = stringToObjectFunction(item.Value, prop.PropertyType);
                prop.SetValue(target, nValue, null);
            }
        }
        /// <summary>
        /// Sets the object properties with values from the Dictionary
        /// </summary>
        /// <param name="source">Source Dictionary</param>
        /// <param name="target">Object Target</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FromStringDictionary(this object target, Dictionary<string, string> source)
        {
            FromStringDictionary(target, source, (strValue, type) =>
            {
                return strValue.ParseTo(type, type.IsValueType ? Activator.CreateInstance(type) : null);
            });
        }
        #endregion

        
        #region Task extensions
        /// <summary>
        /// Waits for task finalization and returns the result value
        /// </summary>
        /// <typeparam name="T">Type of task response</typeparam>
        /// <param name="task">Task source object</param>
        /// <returns>Response of the task completation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WaitAndResults<T>(this Task<T> task)
        {
            if (task is null)
                return default;
            SynchronizationContext currentSyncContext = null;
            try
            {
                if (task.IsCompleted)
                    return task.Result;
                currentSyncContext = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(null);
                return task.Result;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count == 1)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerExceptions[0]).Throw();
                }
                throw;
            }
            finally
            {
                if (currentSyncContext != null)
                    SynchronizationContext.SetSynchronizationContext(currentSyncContext);
            }
        }
        /// <summary>
        /// Waits for task finalization and returns the result value
        /// </summary>
        /// <typeparam name="T">Type of task response</typeparam>
        /// <param name="task">Task source object</param>
        /// <param name="millisecondsTimeout">Milliseconds for waiting the task to complete</param>
        /// <returns>Response of the task completation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WaitAndResults<T>(this Task<T> task, int millisecondsTimeout)
        {
            if (task is null)
                return default;
            SynchronizationContext currentSyncContext = null;
            try
            {
                if (task.IsCompleted)
                    return task.Result;
                currentSyncContext = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(null);
                return task.Wait(millisecondsTimeout) ? task.Result : default;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count == 1)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerExceptions[0]).Throw();
                }
                throw;
            }
            finally
            {
                if (currentSyncContext != null)
                    SynchronizationContext.SetSynchronizationContext(currentSyncContext);
            }
        }
        /// <summary>
        /// Waits for task finalization and returns the result value
        /// </summary>
        /// <typeparam name="T">Type of task response</typeparam>
        /// <param name="task">Task source object</param>
        /// <param name="timeout">TimeSpan for waiting the task to complete</param>
        /// <returns>Response of the task completation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WaitAndResults<T>(this Task<T> task, TimeSpan timeout)
        {
            if (task is null)
                return default;
            SynchronizationContext currentSyncContext = null;
            try
            {
                if (task.IsCompleted)
                    return task.Result;
                currentSyncContext = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(null);
                return task.Wait(timeout) ? task.Result : default;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count == 1)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerExceptions[0]).Throw();
                }
                throw;
            }
            finally
            {
                if (currentSyncContext != null)
                    SynchronizationContext.SetSynchronizationContext(currentSyncContext);
            }
        }
        /// <summary>
        /// Waits for task finalization and returns the result value
        /// </summary>
        /// <typeparam name="T">Type of task response</typeparam>
        /// <param name="task">Task source object</param>
        /// <param name="cancellationToken">Cancellation token on the Wait for the task to complete</param>
        /// <returns>Response of the task completation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WaitAndResults<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            if (task is null)
                return default;
            SynchronizationContext currentSyncContext = null;
            try
            {
                if (task.IsCompleted)
                    return task.Result;

                currentSyncContext = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(null);
                task.Wait(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return default;
                return task.Result;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count == 1)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerExceptions[0]).Throw();
                }
                throw;
            }
            finally
            {
                if (currentSyncContext != null)
                    SynchronizationContext.SetSynchronizationContext(currentSyncContext);
            }
        }
        /// <summary>
        /// Wait for task avoiding deadlocks
        /// </summary>
        /// <typeparam name="T">Task type</typeparam>
        /// <param name="task">Rask</param>
        /// <returns>Task complete</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WaitAsync<T>(this Task<T> task)
        {
            if (task.IsCompleted)
                return task.Result;

            var currentSyncContext = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(null);
                return task.Result;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count == 1)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerExceptions[0]).Throw();
                }
                throw;
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(currentSyncContext);
            }
        }
        /// <summary>
        /// Wait for task avoiding deadlocks
        /// </summary>
        /// <param name="task">Rask</param>
        /// <returns>Task complete</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WaitAsync(this Task task)
        {
            if (task.IsCompleted)
                return;

            var currentSyncContext = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(null);
                task.Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count == 1)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerExceptions[0]).Throw();
                }
                throw;
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(currentSyncContext);
            }
        }
        /// <summary>
        /// Handles a cancellation Token for a task without support
        /// </summary>
        /// <param name="asyncTask">Async task without cancellation token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task with cancellation token support</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<TResult> HandleCancellationAsync<TResult>(this Task<TResult> asyncTask, CancellationToken cancellationToken)
        {
            if (asyncTask.IsCompleted) return asyncTask;
            if (cancellationToken.IsCancellationRequested) return Task.FromCanceled<TResult>(cancellationToken);
            return InnerHandle(asyncTask, cancellationToken);

            async Task<TResult> InnerHandle(Task<TResult> aTask, CancellationToken cToken)
            {
                var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
                using (cToken.Register(() => tcs.TrySetCanceled(), false))
                {
                    var cancellationTask = tcs.Task;
                    var readyTask = await Task.WhenAny(aTask, cancellationTask).ConfigureAwait(false);
                    if (readyTask == cancellationTask)
                        await aTask.ContinueWith(tsk => Core.Log.Write(tsk.Exception), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously)
                            .ConfigureAwait(false);
                    return await readyTask.ConfigureAwait(false);
                }
            }
        }
        /// <summary>
        /// Handles a cancellation Token for a task without support
        /// </summary>
        /// <param name="asyncTask">Async task without cancellation token</param>
        /// <param name="cancellationToken">Cancellation token</param>
	    /// <param name="cancellationToken2">Cancellation token 2</param>
        /// <returns>Task with cancellation token support</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<TResult> HandleCancellationAsync<TResult>(this Task<TResult> asyncTask, CancellationToken cancellationToken, CancellationToken cancellationToken2)
        {
            if (asyncTask.IsCompleted) return asyncTask;
            if (cancellationToken.IsCancellationRequested) return Task.FromCanceled<TResult>(cancellationToken);
            if (cancellationToken2.IsCancellationRequested) return Task.FromCanceled<TResult>(cancellationToken2);
            return InnerHandle(asyncTask, cancellationToken, cancellationToken2);

            async Task<TResult> InnerHandle(Task<TResult> aTask, CancellationToken cToken, CancellationToken cToken2)
            {
                var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
                using (var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cToken, cToken2))
                using (linkedCancellation.Token.Register(() => tcs.TrySetCanceled(), false))
                {
                    var cancellationTask = tcs.Task;
                    var readyTask = await Task.WhenAny(aTask, cancellationTask).ConfigureAwait(false);
                    if (readyTask == cancellationTask)
                        await aTask.ContinueWith(tsk => Core.Log.Write(tsk.Exception), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously)
                            .ConfigureAwait(false);
                    return await readyTask.ConfigureAwait(false);
                }
            }
        }
        /// <summary>
        /// Handles a cancellation Token for a task without support
        /// </summary>
        /// <param name="asyncTask">Async task without cancellation token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="cancellationToken2">Cancellation token 2</param>
	    /// <param name="cancellationToken3">Cancellation token 3</param>
        /// <returns>Task with cancellation token support</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<TResult> HandleCancellationAsync<TResult>(this Task<TResult> asyncTask, CancellationToken cancellationToken, CancellationToken cancellationToken2, CancellationToken cancellationToken3)
        {
            if (asyncTask.IsCompleted) return asyncTask;
            if (cancellationToken.IsCancellationRequested) return Task.FromCanceled<TResult>(cancellationToken);
            if (cancellationToken2.IsCancellationRequested) return Task.FromCanceled<TResult>(cancellationToken2);
            if (cancellationToken3.IsCancellationRequested) return Task.FromCanceled<TResult>(cancellationToken3);
            return InnerHandle(asyncTask, cancellationToken, cancellationToken2, cancellationToken3);

            async Task<TResult> InnerHandle(Task<TResult> aTask, CancellationToken cToken, CancellationToken cToken2, CancellationToken cToken3)
            {
                var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
                using (var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cToken, cToken2, cToken3))
                using (linkedCancellation.Token.Register(() => tcs.TrySetCanceled(), false))
                {
                    var cancellationTask = tcs.Task;
                    var readyTask = await Task.WhenAny(aTask, cancellationTask).ConfigureAwait(false);
                    if (readyTask == cancellationTask)
                        await aTask.ContinueWith(tsk => Core.Log.Write(tsk.Exception), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously)
                            .ConfigureAwait(false);
                    return await readyTask.ConfigureAwait(false);
                }
            }
        }
        /// <summary>
        /// Handles a cancellation Token for a task without support
        /// </summary>
        /// <param name="asyncTask">Async task without cancellation token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task with cancellation token support</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task HandleCancellationAsync(this Task asyncTask, CancellationToken cancellationToken)
        {
            if (asyncTask.IsCompleted) return Task.CompletedTask;
            if (cancellationToken.IsCancellationRequested) return Task.FromCanceled(cancellationToken);
            return InnerHandle(asyncTask, cancellationToken);

            async Task InnerHandle(Task aTask, CancellationToken cToken)
            {
                var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                using (cToken.Register(() => tcs.TrySetCanceled(), false))
                {
                    var cancellationTask = tcs.Task;
                    var readyTask = await Task.WhenAny(aTask, cancellationTask).ConfigureAwait(false);
                    if (readyTask == cancellationTask)
                        await aTask.ContinueWith(tsk => Core.Log.Write(tsk.Exception), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously)
                            .ConfigureAwait(false);
                    await readyTask.ConfigureAwait(false);
                }
            }
        }
        /// <summary>
        /// Handles a cancellation Token for a task without support
        /// </summary>
        /// <param name="asyncTask">Async task without cancellation token</param>
        /// <param name="cancellationToken">Cancellation token</param>
	    /// <param name="cancellationToken2">Cancellation token 2</param>
        /// <returns>Task with cancellation token support</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task HandleCancellationAsync(this Task asyncTask, CancellationToken cancellationToken, CancellationToken cancellationToken2)
        {
            if (asyncTask.IsCompleted) return Task.CompletedTask;
            if (cancellationToken.IsCancellationRequested) return Task.FromCanceled(cancellationToken);
            if (cancellationToken2.IsCancellationRequested) return Task.FromCanceled(cancellationToken2);
            return InnerHandle(asyncTask, cancellationToken, cancellationToken2);

            async Task InnerHandle(Task aTask, CancellationToken cToken, CancellationToken cToken2)
            {
                var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                using (var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cToken, cToken2))
                using (linkedCancellation.Token.Register(() => tcs.TrySetCanceled(), false))
                {
                    var cancellationTask = tcs.Task;
                    var readyTask = await Task.WhenAny(aTask, cancellationTask).ConfigureAwait(false);
                    if (readyTask == cancellationTask)
                        await aTask.ContinueWith(tsk => Core.Log.Write(tsk.Exception), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously)
                            .ConfigureAwait(false);
                    await readyTask.ConfigureAwait(false);
                }
            }
        }
        /// <summary>
        /// Handles a cancellation Token for a task without support
        /// </summary>
        /// <param name="asyncTask">Async task without cancellation token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="cancellationToken2">Cancellation token 2</param>
	    /// <param name="cancellationToken3">Cancellation token 3</param>
        /// <returns>Task with cancellation token support</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task HandleCancellationAsync(this Task asyncTask, CancellationToken cancellationToken, CancellationToken cancellationToken2, CancellationToken cancellationToken3)
        {
            if (asyncTask.IsCompleted) return Task.CompletedTask;
            if (cancellationToken.IsCancellationRequested) return Task.FromCanceled(cancellationToken);
            if (cancellationToken2.IsCancellationRequested) return Task.FromCanceled(cancellationToken2);
            if (cancellationToken3.IsCancellationRequested) return Task.FromCanceled(cancellationToken3);
            return InnerHandle(asyncTask, cancellationToken, cancellationToken2, cancellationToken3);

            async Task InnerHandle(Task aTask, CancellationToken cToken, CancellationToken cToken2, CancellationToken cToken3)
            {
                var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                using (var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cToken, cToken2, cToken3))
                using (linkedCancellation.Token.Register(() => tcs.TrySetCanceled(), false))
                {
                    var cancellationTask = tcs.Task;
                    var readyTask = await Task.WhenAny(aTask, cancellationTask).ConfigureAwait(false);
                    if (readyTask == cancellationTask)
                        await aTask.ContinueWith(tsk => Core.Log.Write(tsk.Exception), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously)
                            .ConfigureAwait(false);
                    await readyTask.ConfigureAwait(false);
                }
            }
        }
        /// <summary>
        /// Create a Task to await the cancellation of the token
        /// </summary>
        /// <param name="cancellationToken">Cancellation token instance</param>
        /// <returns>Task to await the cancellation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task WhenCanceledAsync(this CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;
            var tcs = new TaskCompletionSource<object>();
            using (cancellationToken.Register(state => ((TaskCompletionSource<object>)state).TrySetResult(null), tcs, false))
                await tcs.Task.ConfigureAwait(false);
        }
        /// <summary>
        /// Create a Task to await the cancellation of the token
        /// </summary>
        /// <param name="cancellationToken">Cancellation token instance</param>
	    /// <param name="cancellationToken2">Cancellation token 2 instance</param>
        /// <returns>Task to await the cancellation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task WhenCanceledAsync(this CancellationToken cancellationToken, CancellationToken cancellationToken2)
        {
            if (cancellationToken.IsCancellationRequested || cancellationToken2.IsCancellationRequested) return Task.CompletedTask;
            return InnerHandle(cancellationToken, cancellationToken2);

            async Task InnerHandle(CancellationToken cToken, CancellationToken cToken2)
            {
                var tcs = new TaskCompletionSource<object>();
                using (var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cToken, cToken2))
                using (linkedToken.Token.Register(() => tcs.TrySetResult(null), false))
                    await tcs.Task.ConfigureAwait(false);
            }
        }
        /// <summary>
        /// Create a Task to await the cancellation of the token
        /// </summary>
        /// <param name="cancellationToken">Cancellation token instance</param>
        /// <param name="cancellationToken2">Cancellation token 2 instance</param>
	    /// <param name="cancellationToken3">Cancellation token 3 instance</param>
        /// <returns>Task to await the cancellation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task WhenCanceledAsync(this CancellationToken cancellationToken, CancellationToken cancellationToken2, CancellationToken cancellationToken3)
        {
            if (cancellationToken.IsCancellationRequested || cancellationToken2.IsCancellationRequested || cancellationToken3.IsCancellationRequested)
                return Task.CompletedTask;
            return InnerHandle(cancellationToken, cancellationToken2, cancellationToken3);

            async Task InnerHandle(CancellationToken cToken, CancellationToken cToken2, CancellationToken cToken3)
            {
                var tcs = new TaskCompletionSource<object>();
                using (var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cToken, cToken2, cToken3))
                using (linkedToken.Token.Register(() => tcs.TrySetResult(null), false))
                    await tcs.Task.ConfigureAwait(false);
            }
        }

        private delegate object InvokeDelegate(Delegate @delegate, params object[] args);
        /// <summary>
        /// Invoke a delegate as an Async Task
        /// </summary>
        /// <param name="delegate">Delegate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="args">Delegate arguments</param>
        /// <returns>Task with cancellation token support</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<object> DynamicInvokeAsync(this Delegate @delegate, CancellationToken cancellationToken, params object[] args)
        {
            var nDelegate = new InvokeDelegate((del, mArgs) => del.DynamicInvoke(mArgs));
            return Task.Factory.FromAsync(nDelegate.BeginInvoke(@delegate, args, null, null), nDelegate.EndInvoke).HandleCancellationAsync(cancellationToken);
        }
        /// <summary>
        /// Invoke a delegate as an Async Task
        /// </summary>
        /// <param name="delegate">Delegate</param>
        /// <param name="args">Delegate arguments</param>
        /// <returns>Task with cancellation token support</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<object> DynamicInvokeAsync(this Delegate @delegate, params object[] args)
        {
            var nDelegate = new InvokeDelegate((mDel, mArgs) => mDel.DynamicInvoke(mArgs));
            return Task.Factory.FromAsync(nDelegate.BeginInvoke(@delegate, args, null, null), nDelegate.EndInvoke);
        }
        /// <summary>
        /// Configure an awaiter for all IEnumerable Tasks and return a Task with all results
        /// </summary>
        /// <typeparam name="T">Type of task</typeparam>
        /// <param name="tasks">IEnumerable instance</param>
        /// <param name="continueOnCapturedContext">Continue on captured context</param>
        /// <returns>Task with all results</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable<T[]> ConfigureAwait<T>(this IEnumerable<Task<T>> tasks, bool continueOnCapturedContext)
            => Task.WhenAll(tasks).ConfigureAwait(continueOnCapturedContext);
        /// <summary>
        /// Configure an awaiter for all IEnumerable Tasks and return a Task
        /// </summary>
        /// <param name="tasks">IEnumerable instance</param>
        /// <param name="continueOnCapturedContext">Continue on captured context</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable ConfigureAwait(this IEnumerable<Task> tasks, bool continueOnCapturedContext)
            => Task.WhenAll(tasks).ConfigureAwait(continueOnCapturedContext);
        /// <summary>
        /// Projects an item into a new form.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
        /// <param name="source">A task to invoke a transform function on.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>A task with the result of invoking the transform function on each element of source.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async static Task<TResult> Select<T, TResult>(this Task<T> source, Func<T, TResult> selector)
        {
            var x = await source.ConfigureAwait(false);
            return selector(x);
        }
        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
        /// <param name="source">A sequence of values to invoke a transform function on.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>An IEnumerable whose elements are the result of invoking the transform function on each element of source.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async static Task<IEnumerable<TResult>> Select<T, TResult>(this Task<IEnumerable<T>> source, Func<T, TResult> selector)
        {
            var res = await source.ConfigureAwait(false);
            return res.Select(selector);
        }
        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
        /// <param name="source">A sequence of values to invoke a transform function on.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>An IEnumerable whose elements are the result of invoking the transform function on each element of source.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async static Task<IEnumerable<TResult>> Select<T, TResult>(this IEnumerable<Task<T>> source, Func<T, TResult> selector)
        {
            var res = await Task.WhenAll(source).ConfigureAwait(false);
            return res.Select(selector);
        }
        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">An System.Collections.Generic.IEnumerable`1 to filter.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>An IEnumerable that contains elements from the input sequence that satisfy the condition.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async static Task<IEnumerable<T>> Where<T>(this Task<IEnumerable<T>> source, Func<T, bool> predicate)
        {
            var res = await source.ConfigureAwait(false);
            return res.Where(predicate);
        }
        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">An System.Collections.Generic.IEnumerable`1 to filter.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>An IEnumerable that contains elements from the input sequence that satisfy the condition.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async static Task<IEnumerable<T>> Where<T>(this IEnumerable<Task<T>> source, Func<T, bool> predicate)
        {
            var res = await Task.WhenAll(source).ConfigureAwait(false);
            return res.Where(predicate);
        }
        #endregion

        #region ValueTask extensions
        /// <summary>
        /// Waits for task finalization and returns the result value
        /// </summary>
        /// <typeparam name="T">Type of task response</typeparam>
        /// <param name="task">Task source object</param>
        /// <returns>Response of the task completation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WaitAndResults<T>(this ValueTask<T> task)
        {
            try
            {
                return task.GetAwaiter().GetResult();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count == 1)
                {
                    Core.Log.Write(ex.InnerExceptions[0]);
                    ExceptionDispatchInfo.Capture(ex.InnerExceptions[0]).Throw();
                }
                Core.Log.Write(ex);
                throw;
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                throw;
            }
        }
        /// <summary>
        /// Waits for task finalization and returns the result value
        /// </summary>
        /// <typeparam name="T">Type of task response</typeparam>
        /// <param name="task">Task source object</param>
        /// <param name="millisecondsTimeout">Milliseconds for waiting the task to complete</param>
        /// <returns>Response of the task completation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WaitAndResults<T>(this ValueTask<T> task, int millisecondsTimeout)
        {
            if (task.IsCompleted)
                return task.Result;
            return task.AsTask().WaitAndResults(millisecondsTimeout);
        }
        /// <summary>
        /// Waits for task finalization and returns the result value
        /// </summary>
        /// <typeparam name="T">Type of task response</typeparam>
        /// <param name="task">Task source object</param>
        /// <param name="timeout">TimeSpan for waiting the task to complete</param>
        /// <returns>Response of the task completation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WaitAndResults<T>(this ValueTask<T> task, TimeSpan timeout)
        {
            if (task.IsCompleted)
                return task.Result;
            return task.AsTask().WaitAndResults(timeout);
        }
        /// <summary>
        /// Waits for task finalization and returns the result value
        /// </summary>
        /// <typeparam name="T">Type of task response</typeparam>
        /// <param name="task">Task source object</param>
        /// <param name="cancellationToken">Cancellation token on the Wait for the task to complete</param>
        /// <returns>Response of the task completation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WaitAndResults<T>(this ValueTask<T> task, CancellationToken cancellationToken)
        {
            if (task.IsCompleted)
                return task.Result;
            return task.AsTask().WaitAndResults(cancellationToken);
        }
        /// <summary>
        /// Wait for task avoiding deadlocks
        /// </summary>
        /// <typeparam name="T">Task type</typeparam>
        /// <param name="task">Rask</param>
        /// <returns>Task complete</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WaitAsync<T>(this ValueTask<T> task)
        {
            if (task.IsCompleted)
                return task.Result;
            return task.AsTask().WaitAsync();
        }
        /// <summary>
        /// Wait for task avoiding deadlocks
        /// </summary>
        /// <param name="task">Rask</param>
        /// <returns>Task complete</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WaitAsync(this ValueTask task)
        {
            if (task.IsCompleted)
                return;
            task.AsTask().WaitAsync();
        }
        #endregion


        #region Others
        /// <summary>
        /// Gets the Timespan format of the DateTime object.
        /// </summary>
        /// <param name="time">DateTime to format</param>
        /// <returns>Formatted Datetime string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetTimeSpanFormat(this DateTime time)
        {
            var day = time.Day;
            var month = time.Month;
            var year = time.Year;
            var hour = time.Hour;
            var minute = time.Minute;
            var second = time.Second;
            var millisecond = time.Millisecond;
            Span<char> dateData = stackalloc char[23];
            dateData[0] = (char)(year / 1000 % 10 + 48);
            dateData[1] = (char)(year / 100 % 10 + 48);
            dateData[2] = (char)(year / 10 % 10 + 48);
            dateData[3] = (char)(year % 10 + 48);
            dateData[4] = '-';
            dateData[5] = (char)(month / 10 + 48);
            dateData[6] = (char)(month % 10 + 48);
            dateData[7] = '-';
            dateData[8] = (char)(day / 10 + 48);
            dateData[9] = (char)(day % 10 + 48);
            dateData[10] = ' ';
            dateData[11] = (char)(hour / 10 + 48);
            dateData[12] = (char)(hour % 10 + 48);
            dateData[13] = ':';
            dateData[14] = (char)(minute / 10 + 48);
            dateData[15] = (char)(minute % 10 + 48);
            dateData[16] = ':';
            dateData[17] = (char)(second / 10 + 48);
            dateData[18] = (char)(second % 10 + 48);
            dateData[19] = '.';
            dateData[20] = (char)(millisecond / 100 + 48);
            dateData[21] = (char)(millisecond / 10 % 10 + 48);
            dateData[22] = (char)(millisecond % 10 + 48);
            return dateData.ToString();
        }
        #endregion

        #region WaitHandles Extensions
        /// <summary>
        /// WaitOne with cancellationToken
        /// </summary>
        /// <returns><c>true</c>, if one was waited, <c>false</c> otherwise.</returns>
        /// <param name="handle">Handle.</param>
        /// <param name="millisecondsTimeout">Milliseconds timeout.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WaitOne(this WaitHandle handle, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var n = WaitHandle.WaitAny(new[] { handle, cancellationToken.WaitHandle }, millisecondsTimeout);
            switch (n)
            {
                case WaitHandle.WaitTimeout:
                    return false;
                case 0:
                    return true;
                default:
                    cancellationToken.ThrowIfCancellationRequested();
                    return false;
            }
        }
        /// <summary>
        /// WaitOne with cancellationToken
        /// </summary>
        /// <returns><c>true</c>, if one was waited, <c>false</c> otherwise.</returns>
        /// <param name="handle">Handle.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WaitOne(this WaitHandle handle, TimeSpan timeout, CancellationToken cancellationToken)
            => handle.WaitOne((int)timeout.TotalMilliseconds, cancellationToken);
        /// <summary>
        /// WaitOne with cancellationToken
        /// </summary>
        /// <returns><c>true</c>, if one was waited, <c>false</c> otherwise.</returns>
        /// <param name="handle">Handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WaitOne(this WaitHandle handle, CancellationToken cancellationToken)
            => handle.WaitOne(Timeout.Infinite, cancellationToken);

        /// <summary>
        /// WaitOne Async
        /// </summary>
        /// <returns>The async Task with the result.</returns>
        /// <param name="handle">Handle.</param>
        /// <param name="millisecondsTimeout">Milliseconds timeout.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> WaitOneAsync(this WaitHandle handle, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (handle is null)
                throw new ArgumentNullException("waitHandle");
            var tcs = new TaskCompletionSource<bool>();
            var registeredHandle = ThreadPool.UnsafeRegisterWaitForSingleObject(handle, FinalizeTaskCompletionBoolWithCancellation, tcs, millisecondsTimeout, true);
            var tokenRegistration = cancellationToken.Register(TaskCompletionBoolCancellation, tcs);
            var t = tcs.Task;
            t.ContinueWith((antecedent) =>
            {
                registeredHandle.Unregister(null);
                tokenRegistration.Dispose();
            });
            return t;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TaskCompletionBoolCancellation(object state)
            => ((TaskCompletionSource<bool>)state).TrySetCanceled();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FinalizeTaskCompletionBoolWithCancellation(object state, bool isTimeout)
            => ((TaskCompletionSource<bool>)state).TrySetResult(!isTimeout);
        /// <summary>
        /// WaitOne Async
        /// </summary>
        /// <returns>The async Task with the result.</returns>
        /// <param name="handle">Handle.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> WaitOneAsync(this WaitHandle handle, TimeSpan timeout, CancellationToken cancellationToken)
            => handle.WaitOneAsync((int)timeout.TotalMilliseconds, cancellationToken);
        /// <summary>
        /// WaitOne Async
        /// </summary>
        /// <returns>The async Task with the result.</returns>
        /// <param name="handle">Handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> WaitOneAsync(this WaitHandle handle, CancellationToken cancellationToken)
            => handle.WaitOneAsync(Timeout.Infinite, cancellationToken);
        /// <summary>
        /// WaitOne Async
        /// </summary>
        /// <returns>The async Task with the result.</returns>
        /// <param name="waitHandle">Handle.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task WaitOneAsync(this WaitHandle waitHandle)
        {
            if (waitHandle is null)
                throw new ArgumentNullException("waitHandle");

            var tcs = new TaskCompletionSource<object>();
            var rwh = ThreadPool.UnsafeRegisterWaitForSingleObject(waitHandle, FinalizeTaskCompletionObject, tcs, -1, true);
            var t = tcs.Task;
            t.ContinueWith((antecedent, state) => ((RegisteredWaitHandle)state).Unregister(null), rwh);
            return t;
        }
        /// <summary>
        /// WaitOne Async
        /// </summary>
        /// <returns>The async Task with the result.</returns>
        /// <param name="waitHandle">Handle.</param>
        /// <param name="millisecondsTimeout">Milliseconds timeout.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task WaitOneAsync(this WaitHandle waitHandle, int millisecondsTimeout)
        {
            if (waitHandle is null)
                throw new ArgumentNullException("waitHandle");

            var tcs = new TaskCompletionSource<object>();
            var rwh = ThreadPool.UnsafeRegisterWaitForSingleObject(waitHandle, FinalizeTaskCompletionObject, tcs, millisecondsTimeout, true);
            var t = tcs.Task;
            t.ContinueWith((antecedent, state) => ((RegisteredWaitHandle)state).Unregister(null), rwh);
            return t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FinalizeTaskCompletionObject(object state, bool isTimeout)
            => ((TaskCompletionSource<object>)state).TrySetResult(null);
        #endregion

        #region DateTime
        /// <summary>
        /// Truncates a DateTime using the TimeSpan value
        /// </summary>
        /// <returns>The result DateTime</returns>
        /// <param name="dateTime">DateTime value</param>
        /// <param name="timeSpan">TimeSpan value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime TruncateTo(this DateTime dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero)
                return dateTime;
            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
                return dateTime;
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }
        #endregion

        #region TimeSpan
        /// <summary>
        /// Truncates a TimeSpan using the TimeSpan value
        /// </summary>
        /// <returns>The result TimeSpan</returns>
        /// <param name="timeSpan">TimeSpan value</param>
        /// <param name="spanValue">TimeSpan value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan TruncateTo(this TimeSpan timeSpan, TimeSpan spanValue)
        {
            if (spanValue == TimeSpan.Zero)
                return timeSpan;
            if (timeSpan == TimeSpan.MinValue || timeSpan == TimeSpan.MaxValue)
                return timeSpan;
            return timeSpan.Add(TimeSpan.FromTicks(-(timeSpan.Ticks % spanValue.Ticks)));
        }
        #endregion
    }
}
