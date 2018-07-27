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
        public static IDictionary<string, object> ToDictionary(this object source)
        {
            if (source == null) return null;
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
        public static void FromDictionary(this object target, IDictionary<string, object> source)
        {
            if (source == null || target == null) return;
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
        public static Dictionary<string, string> ToStringDictionary(this object source, Func<object, Type, string> objectToStringFunction)
        {
            if (source == null) return null;
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
        public static void FromStringDictionary(this object target, Dictionary<string, string> source, Func<string, Type, object> stringToObjectFunction)
        {
            if (source == null || target == null) return;
            var sType = target.GetType();
            if (sType.IsValueType) return;
            foreach (var item in source)
            {
                var prop = sType.GetRuntimeProperty(item.Key);
                if (prop == null || !prop.CanWrite) continue;
                var nValue = stringToObjectFunction(item.Value, prop.PropertyType);
                prop.SetValue(target, nValue, null);
            }
        }
        /// <summary>
        /// Sets the object properties with values from the Dictionary
        /// </summary>
        /// <param name="source">Source Dictionary</param>
        /// <param name="target">Object Target</param>
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
        public static T WaitAndResults<T>(this Task<T> task)
        {
            if (task == null)
                return default(T);
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
        public static T WaitAndResults<T>(this Task<T> task, int millisecondsTimeout)
        {
            if (task == null)
                return default(T);
            try
            {
                return task.Wait(millisecondsTimeout) ? task.Result : default(T);
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
        /// <param name="timeout">TimeSpan for waiting the task to complete</param>
        /// <returns>Response of the task completation</returns>
        public static T WaitAndResults<T>(this Task<T> task, TimeSpan timeout)
        {
            if (task == null)
                return default(T);
            try
            {
                return task.Wait(timeout) ? task.Result : default(T);
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
        /// <param name="cancellationToken">Cancellation token on the Wait for the task to complete</param>
        /// <returns>Response of the task completation</returns>
        public static T WaitAndResults<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            if (task == null)
                return default(T);
            try
            {
                task.Wait(cancellationToken);
                return task.Result;
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
        /// Wait for task avoiding deadlocks
        /// </summary>
        /// <typeparam name="T">Task type</typeparam>
        /// <param name="task">Rask</param>
        /// <returns>Task complete</returns>
        public static T WaitAsync<T>(this Task<T> task)
        {
            var wait = new ManualResetEventSlim(false);
            var continuation = task.ContinueWith((oldTask, state) =>
            {
                var ex = oldTask.Exception;
                var result = (ex != null) ? (object) ExceptionDispatchInfo.Capture(ex.InnerExceptions.Count == 1 ? ex.InnerExceptions[0] : ex) : oldTask.Result;
                ((ManualResetEventSlim)state).Set();
                return result;
            }, wait, CancellationToken.None, TaskContinuationOptions.RunContinuationsAsynchronously, TaskScheduler.Default);
            wait.Wait();
            if (continuation.Result is ExceptionDispatchInfo edi)
                edi.Throw();
            return (T)continuation.Result;
        }
        /// <summary>
        /// Wait for task avoiding deadlocks
        /// </summary>
        /// <param name="task">Rask</param>
        /// <returns>Task complete</returns>
        public static void WaitAsync(this Task task)
        {
            var wait = new ManualResetEventSlim(false);
            var continuation = task.ContinueWith((oldTask, state) =>
            {
                ExceptionDispatchInfo tEx = null;
                var ex = oldTask.Exception;
                if (ex != null)
                    tEx = ExceptionDispatchInfo.Capture(ex.InnerExceptions.Count == 1 ? ex.InnerExceptions[0] : ex);
                ((ManualResetEventSlim)state).Set();
                return tEx;
            }, wait, CancellationToken.None, TaskContinuationOptions.RunContinuationsAsynchronously, TaskScheduler.Default);
            wait.Wait();
            continuation.Result?.Throw();
        }
        /// <summary>
        /// Handles a cancellation Token for a task without support
        /// </summary>
        /// <param name="asyncTask">Async task without cancellation token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task with cancellation token support</returns>
        public static Task<TResult> HandleCancellationAsync<TResult>(this Task<TResult> asyncTask, CancellationToken cancellationToken)
        {
            if (asyncTask.IsCompleted) return asyncTask;
            if (cancellationToken.IsCancellationRequested) return Task.FromCanceled<TResult>(cancellationToken);
            return InnerHandle();

            async Task<TResult> InnerHandle()
            {
                var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
                using (cancellationToken.Register(() => tcs.TrySetCanceled(), false))
                {
                    var cancellationTask = tcs.Task;
                    var readyTask = await Task.WhenAny(asyncTask, cancellationTask).ConfigureAwait(false);
                    if (readyTask == cancellationTask)
                        await asyncTask.ContinueWith(tsk => Core.Log.Write(tsk.Exception), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously)
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
        public static Task<TResult> HandleCancellationAsync<TResult>(this Task<TResult> asyncTask, CancellationToken cancellationToken, CancellationToken cancellationToken2)
        {
            if (asyncTask.IsCompleted) return asyncTask;
            if (cancellationToken.IsCancellationRequested) return Task.FromCanceled<TResult>(cancellationToken);
            if (cancellationToken2.IsCancellationRequested) return Task.FromCanceled<TResult>(cancellationToken2);
            return InnerHandle();

            async Task<TResult> InnerHandle()
            {
                var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
                using (var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationToken2))
                using (linkedCancellation.Token.Register(() => tcs.TrySetCanceled(), false))
                {
                    var cancellationTask = tcs.Task;
                    var readyTask = await Task.WhenAny(asyncTask, cancellationTask).ConfigureAwait(false);
                    if (readyTask == cancellationTask)
                        await asyncTask.ContinueWith(tsk => Core.Log.Write(tsk.Exception), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously)
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
        public static Task<TResult> HandleCancellationAsync<TResult>(this Task<TResult> asyncTask, CancellationToken cancellationToken, CancellationToken cancellationToken2, CancellationToken cancellationToken3)
        {
            if (asyncTask.IsCompleted) return asyncTask;
            if (cancellationToken.IsCancellationRequested) return Task.FromCanceled<TResult>(cancellationToken);
            if (cancellationToken2.IsCancellationRequested) return Task.FromCanceled<TResult>(cancellationToken2);
            if (cancellationToken3.IsCancellationRequested) return Task.FromCanceled<TResult>(cancellationToken3);
            return InnerHandle();

            async Task<TResult> InnerHandle()
            {
                var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
                using (var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationToken2, cancellationToken3))
                using (linkedCancellation.Token.Register(() => tcs.TrySetCanceled(), false))
                {
                    var cancellationTask = tcs.Task;
                    var readyTask = await Task.WhenAny(asyncTask, cancellationTask).ConfigureAwait(false);
                    if (readyTask == cancellationTask)
                        await asyncTask.ContinueWith(tsk => Core.Log.Write(tsk.Exception), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously)
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
        public static Task HandleCancellationAsync(this Task asyncTask, CancellationToken cancellationToken)
        {
            if (asyncTask.IsCompleted) return Task.CompletedTask;
            if (cancellationToken.IsCancellationRequested) return Task.FromCanceled(cancellationToken);
            return InnerHandle();

            async Task InnerHandle()
            {
                var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                using (cancellationToken.Register(() => tcs.TrySetCanceled(), false))
                {
                    var cancellationTask = tcs.Task;
                    var readyTask = await Task.WhenAny(asyncTask, cancellationTask).ConfigureAwait(false);
                    if (readyTask == cancellationTask)
                        await asyncTask.ContinueWith(tsk => Core.Log.Write(tsk.Exception), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously)
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
        public static Task HandleCancellationAsync(this Task asyncTask, CancellationToken cancellationToken, CancellationToken cancellationToken2)
        {
            if (asyncTask.IsCompleted) return Task.CompletedTask;
            if (cancellationToken.IsCancellationRequested) return Task.FromCanceled(cancellationToken);
            if (cancellationToken2.IsCancellationRequested) return Task.FromCanceled(cancellationToken2);
            return InnerHandle();

            async Task InnerHandle()
            {
                var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                using (var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationToken2))
                using (linkedCancellation.Token.Register(() => tcs.TrySetCanceled(), false))
                {
                    var cancellationTask = tcs.Task;
                    var readyTask = await Task.WhenAny(asyncTask, cancellationTask).ConfigureAwait(false);
                    if (readyTask == cancellationTask)
                        await asyncTask.ContinueWith(tsk => Core.Log.Write(tsk.Exception), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously)
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
        public static Task HandleCancellationAsync(this Task asyncTask, CancellationToken cancellationToken, CancellationToken cancellationToken2, CancellationToken cancellationToken3)
        {
            if (asyncTask.IsCompleted) return Task.CompletedTask;
            if (cancellationToken.IsCancellationRequested) return Task.FromCanceled(cancellationToken);
            if (cancellationToken2.IsCancellationRequested) return Task.FromCanceled(cancellationToken2);
            if (cancellationToken3.IsCancellationRequested) return Task.FromCanceled(cancellationToken3);
            return InnerHandle();

            async Task InnerHandle()
            {
                var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                using (var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationToken2, cancellationToken3))
                using (linkedCancellation.Token.Register(() => tcs.TrySetCanceled(), false))
                {
                    var cancellationTask = tcs.Task;
                    var readyTask = await Task.WhenAny(asyncTask, cancellationTask).ConfigureAwait(false);
                    if (readyTask == cancellationTask)
                        await asyncTask.ContinueWith(tsk => Core.Log.Write(tsk.Exception), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously)
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
        public static Task WhenCanceledAsync(this CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return Task.CompletedTask;
            var tcs = new TaskCompletionSource<object>();
            var registration = cancellationToken.Register(state => ((TaskCompletionSource<object>)state).TrySetResult(null), tcs, false);
            return tcs.Task.ContinueWith((_, obj) => ((CancellationTokenRegistration)obj).Dispose(), registration);
        }
        /// <summary>
        /// Create a Task to await the cancellation of the token
        /// </summary>
        /// <param name="cancellationToken">Cancellation token instance</param>
	    /// <param name="cancellationToken2">Cancellation token 2 instance</param>
        /// <returns>Task to await the cancellation</returns>
        public static Task WhenCanceledAsync(this CancellationToken cancellationToken, CancellationToken cancellationToken2)
        {
            if (cancellationToken.IsCancellationRequested || cancellationToken2.IsCancellationRequested) return Task.CompletedTask;
            return InnerHandle();

            async Task InnerHandle()
            {
                var tcs = new TaskCompletionSource<object>();
                using (var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationToken2))
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
        public static Task WhenCanceledAsync(this CancellationToken cancellationToken, CancellationToken cancellationToken2, CancellationToken cancellationToken3)
        {
            if (cancellationToken.IsCancellationRequested || cancellationToken2.IsCancellationRequested || cancellationToken3.IsCancellationRequested)
                return Task.CompletedTask;
            return InnerHandle();

            async Task InnerHandle()
            {
                var tcs = new TaskCompletionSource<object>();
                using (var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationToken2, cancellationToken3))
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
        #endregion

        #region ValueTask extensions
        /// <summary>
        /// Waits for task finalization and returns the result value
        /// </summary>
        /// <typeparam name="T">Type of task response</typeparam>
        /// <param name="task">Task source object</param>
        /// <returns>Response of the task completation</returns>
        public static T WaitAndResults<T>(this ValueTask<T> task)
        {
            if (task == null)
                return default(T);
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
        public static void WaitAsync(this ValueTask task)
        {
            if (task.IsCompleted)
                return ;
            task.AsTask().WaitAsync();
        }
        #endregion


        #region Others
        /// <summary>
        /// Gets the Timespan format of the DateTime object.
        /// </summary>
        /// <param name="time">DateTime to format</param>
        /// <returns>Formatted Datetime string</returns>
        public static string GetTimeSpanFormat(this DateTime time)
        {
            var day = time.Day;
            var month = time.Month;
            var year = time.Year;
            var hour = time.Hour;
            var minute = time.Minute;
            var second = time.Second;
            var millisecond = time.Millisecond;
            var dateData = new char[21];
            dateData[0] = (char)(day / 10 + '0');
            dateData[1] = (char)(day % 10 + '0');
            dateData[2] = '/';
            dateData[3] = (char)(month / 10 + '0');
            dateData[4] = (char)(month % 10 + '0');
            dateData[5] = '/';
            dateData[6] = (char)(year / 10 % 10 + '0');
            dateData[7] = (char)(year % 10 + '0');
            dateData[8] = ' ';
            dateData[9] = (char)(hour / 10 + '0');
            dateData[10] = (char)(hour % 10 + '0');
            dateData[11] = ':';
            dateData[12] = (char)(minute / 10 + '0');
            dateData[13] = (char)(minute % 10 + '0');
            dateData[14] = ':';
            dateData[15] = (char)(second / 10 + '0');
            dateData[16] = (char)(second % 10 + '0');
            dateData[17] = '.';
            dateData[18] = (char)(millisecond / 100 + '0');
            dateData[19] = (char)(millisecond / 10 % 10 + '0');
            dateData[20] = (char)(millisecond % 10 + '0');
            return new string(dateData);
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
            if (handle == null)
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
            if (waitHandle == null)
                throw new ArgumentNullException("waitHandle");

            var tcs = new TaskCompletionSource<object>();
            var rwh = ThreadPool.UnsafeRegisterWaitForSingleObject(waitHandle, FinalizeTaskCompletionObject, tcs, -1, true);
            var t = tcs.Task;
            t.ContinueWith((antecedent) => rwh.Unregister(null));
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
            if (waitHandle == null)
                throw new ArgumentNullException("waitHandle");

            var tcs = new TaskCompletionSource<object>();
            var rwh = ThreadPool.UnsafeRegisterWaitForSingleObject(waitHandle, FinalizeTaskCompletionObject, tcs, millisecondsTimeout, true);
            var t = tcs.Task;
            t.ContinueWith((antecedent) => rwh.Unregister(null));
            return t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FinalizeTaskCompletionObject(object state, bool isTimeout)
            => ((TaskCompletionSource<object>)state).TrySetResult(null);
        #endregion
    }
}
