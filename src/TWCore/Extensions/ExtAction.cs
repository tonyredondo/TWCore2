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
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Threading;

// ReSharper disable CheckNamespace

namespace TWCore
{
    /// <summary>
    /// Extensions for Action classes
    /// </summary>
    public static partial class Extensions
    {
        #region Delay
        /// <summary>
        /// Wraps an action with a delay time.
        /// </summary>
        /// <param name="action">Original action</param>
        /// <param name="milliseconds">Delay time in milliseconds</param>
        /// <returns>Delayed action instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action CreateDelay(this Action action, int milliseconds)
        {
            return () => Task.Delay(milliseconds).ContinueWith(t => action(), CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
                TaskScheduler.Default);
        }
        /// <summary>
        /// Wraps an action with a delay time.
        /// </summary>
        /// <param name="action">Original action</param>
        /// <param name="milliseconds">Delay time in milliseconds</param>
        /// <returns>Delayed action instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<T> CreateDelay<T>(this Action<T> action, int milliseconds)
        {
            return obj => Task.Delay(milliseconds).ContinueWith((t, s) => action((T)s), obj, CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
                TaskScheduler.Default);
        }
        #endregion

        #region Buffered
        /// <summary>
        ///  Creates a buffered action wrapper from a action base.
        /// </summary>
        /// <param name="action">Original action</param>
        /// <param name="milliseconds">Delay time to start the action</param>
        /// <returns>Buffered action instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action CreateBufferedAction(this Action action, int milliseconds)
        {
            Timer timer = null;
            return () =>
            {
                if (timer != null)
                    timer.Change(milliseconds, Timeout.Infinite);
                else
                {
                    timer = new Timer(obj =>
                    {
                        action();
                        timer = null;
                    }, null, milliseconds, Timeout.Infinite);
                }
            };
        }
        /// <summary>
        ///  Creates a buffered action wrapper from a action base.
        /// </summary>
        /// <param name="action">Original action</param>
        /// <param name="milliseconds">Delay time to start the action</param>
        /// <returns>Buffered action instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<T> CreateBufferedAction<T>(this Action<T> action, int milliseconds)
        {
            Timer timer = null;
            return obj =>
            {
                if (timer != null)
                    timer.Change(milliseconds, Timeout.Infinite);
                else
                {
                    timer = new Timer(ot =>
                    {
                        action((T)ot);
                        timer = null;
                    }, obj, milliseconds, Timeout.Infinite);
                }
            };
        }
        #endregion

        #region Throttled
        /// <summary>
        /// Creates a Throttled action wrapper form an action base.
        /// </summary>
        /// <param name="action">Original action</param>
        /// <param name="milliseconds">Delay time to start the action</param>
        /// <returns>Throttled action instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action CreateThrottledAction(this Action action, int milliseconds)
        {
            var date = DateTime.MinValue;
            return () =>
            {
                if (DateTime.UtcNow.Subtract(date).TotalMilliseconds < milliseconds) return;
                action();
                date = DateTime.UtcNow;
            };
        }
        /// <summary>
        /// Creates a Throttled action wrapper form an action base.
        /// </summary>
        /// <param name="action">Original action</param>
        /// <param name="milliseconds">Delay time to start the action</param>
        /// <returns>Throttled action instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<T> CreateThrottledAction<T>(this Action<T> action, int milliseconds)
        {
            var date = DateTime.MinValue;
            return obj =>
            {
                if (DateTime.UtcNow.Subtract(date).TotalMilliseconds < milliseconds) return;
                action(obj);
                date = DateTime.UtcNow;
            };
        }
        #endregion

        #region Throttled Task
        /// <summary>
        /// Creates a Throttled task wrapper form an action base.
        /// </summary>
        /// <param name="task">Original task</param>
        /// <param name="milliseconds">Delay time to start the action</param>
        /// <returns>Throttled task instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<Task> CreateThrottledTaskAsync(this Task task, int milliseconds)
        {
            var date = DateTime.MinValue;
            return () =>
            {
                if (DateTime.UtcNow.Subtract(date).TotalMilliseconds < milliseconds) return Task.CompletedTask;
                return task.ContinueWith(_ => date = DateTime.UtcNow, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            };
        }
        /// <summary>
        /// Creates a Throttled task wrapper form an Task base.
        /// </summary>
        /// <param name="task">Original task</param>
        /// <param name="milliseconds">Delay time to start the action</param>
        /// <returns>Throttled task instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T, Task> CreateThrottledAction<T>(this Func<T, Task> task, int milliseconds)
        {
            var date = DateTime.MinValue;
            return obj =>
            {
                if (DateTime.UtcNow.Subtract(date).TotalMilliseconds < milliseconds) return Task.CompletedTask;
                return task(obj).ContinueWith(_ => date = DateTime.UtcNow, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            };
        }
        #endregion

        #region InvokeAsync
        /// <summary>
        /// Invoke an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task InvokeAsync(this Action action)
        {
            return Task.Run(() =>
            {
                try
                {
                    action();
                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    return Task.FromException(ex);
                }
            });
        }
        /// <summary>
        /// Invoke an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <param name="value">Argument value</param>
        /// <returns>Task of the invocation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task InvokeAsync<T>(this Action<T> action, T value)
            => InvokeAsync(() => action(value));
        /// <summary>
        /// Invoke an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <param name="value1">Argument 1 value</param>
        /// <param name="value2">Argument 2 value</param>
        /// <returns>Task of the invocation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task InvokeAsync<T1, T2>(this Action<T1, T2> action, T1 value1, T2 value2)
            => InvokeAsync(() => action(value1, value2));
        /// <summary>
        /// Invoke an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <param name="value1">Argument 1 value</param>
        /// <param name="value2">Argument 2 value</param>
        /// <param name="value3">Argument 3 value</param>
        /// <returns>Task of the invocation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task InvokeAsync<T1, T2, T3>(this Action<T1, T2, T3> action, T1 value1, T2 value2, T3 value3)
            => InvokeAsync(() => action(value1, value2, value3));
        /// <summary>
        /// Invoke an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <param name="value1">Argument 1 value</param>
        /// <param name="value2">Argument 2 value</param>
        /// <param name="value3">Argument 3 value</param>
        /// <param name="value4">Argument 4 value</param>
        /// <returns>Task of the invocation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task InvokeAsync<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action, T1 value1, T2 value2, T3 value3, T4 value4)
            => InvokeAsync(() => action(value1, value2, value3, value4));
        /// <summary>
        /// Invoke an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <param name="value1">Argument 1 value</param>
        /// <param name="value2">Argument 2 value</param>
        /// <param name="value3">Argument 3 value</param>
        /// <param name="value4">Argument 4 value</param>
        /// <param name="value5">Argument 5 value</param>
        /// <returns>Task of the invocation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task InvokeAsync<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> action, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
            => InvokeAsync(() => action(value1, value2, value3, value4, value5));
        /// <summary>
        /// Invoke a Func in Async Task
        /// </summary>
        /// <param name="func">Func to invoke</param>
        /// <returns>Task of the invocation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<TResult> InvokeAsync<TResult>(this Func<TResult> func)
        {
            var tcs = new TaskCompletionSource<TResult>();
            Task.Factory.StartNew(InternalInvokeAsync<TResult>, new object[] { tcs, func }, CancellationToken.None);
            return tcs.Task;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalInvokeAsync<TResult>(object state)
        {
            var objArray = (object[])state;
            var mTcs = (TaskCompletionSource<TResult>)objArray[0];
            var mFunc = (Func<TResult>)objArray[1];

            try
            {
                mTcs.TrySetResult(mFunc());
            }
            catch (Exception ex)
            {
                mTcs.TrySetException(ex);
            }
        }
        /// <summary>
        /// Invoke a Func in Async Task
        /// </summary>
        /// <param name="func">Func to invoke</param>
        /// <param name="value">Func argument</param>
        /// <returns>Task of the invocation</returns>
        public static Task<TResult> InvokeAsync<T1, TResult>(this Func<T1, TResult> func, T1 value)
            => InvokeAsync(() => func(value));
        /// <summary>
        /// Invoke a Func in Async Task
        /// </summary>
        /// <param name="func">Func to invoke</param>
        /// <param name="value1">Func argument 1</param>
        /// <param name="value2">Func argument 2</param>
        /// <returns>Task of the invocation</returns>
        public static Task<TResult> InvokeAsync<T1, T2, TResult>(this Func<T1, T2, TResult> func, T1 value1, T2 value2)
            => InvokeAsync(() => func(value1, value2));
        /// <summary>
        /// Invoke a Func in Async Task
        /// </summary>
        /// <param name="func">Func to invoke</param>
        /// <param name="value1">Func argument 1</param>
        /// <param name="value2">Func argument 2</param>
        /// <param name="value3">Func argument 3</param>
        /// <returns>Task of the invocation</returns>
        public static Task<TResult> InvokeAsync<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> func, T1 value1, T2 value2, T3 value3)
            => InvokeAsync(() => func(value1, value2, value3));
        /// <summary>
        /// Invoke a Func in Async Task
        /// </summary>
        /// <param name="func">Func to invoke</param>
        /// <param name="value1">Func argument 1</param>
        /// <param name="value2">Func argument 2</param>
        /// <param name="value3">Func argument 3</param>
        /// <param name="value4">Func argument 4</param>
        /// <returns>Task of the invocation</returns>
        public static Task<TResult> InvokeAsync<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> func, T1 value1, T2 value2, T3 value3, T4 value4)
            => InvokeAsync(() => func(value1, value2, value3, value4));
        /// <summary>
        /// Invoke a Func in Async Task
        /// </summary>
        /// <param name="func">Func to invoke</param>
        /// <param name="value1">Func argument 1</param>
        /// <param name="value2">Func argument 2</param>
        /// <param name="value3">Func argument 3</param>
        /// <param name="value4">Func argument 4</param>
        /// <param name="value5">Func argument 5</param>
        /// <returns>Task of the invocation</returns>
        public static Task<TResult> InvokeAsync<T1, T2, T3, T4, T5, TResult>(this Func<T1, T2, T3, T4, T5, TResult> func, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
            => InvokeAsync(() => func(value1, value2, value3, value4, value5));
        /// <summary>
        /// Invoke a Delegate in Async Task
        /// </summary>
        public static Task<object> InvokeAsync(this Delegate @delegate, params object[] args)
        {
            var tcs = new TaskCompletionSource<object>();
            Task.Run(() =>
            {
                try
                {
                    tcs.TrySetResult(@delegate.DynamicInvoke(args));
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
            return tcs.Task;
        }
        /// <summary>
        /// Invoke a EventHandler in Async Task
        /// </summary>
        /// <param name="event">EventHandler instance</param>
        /// <param name="sender">Sender instance</param>
        /// <param name="e">Event Args</param>
        public static Task InvokeAsync<T>(this EventHandler<T> @event, object sender, T e)
        {
            if (@event == null) return Task.CompletedTask;
            var tcs = new TaskCompletionSource<bool>();
            Task.Run(() =>
            {
                try
                {
                    @event(sender, e);
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
            return tcs.Task;
        }
        /// <summary>
        /// Invoke a EventHandler in Async Task
        /// </summary>
        /// <param name="event">EventHandler instance</param>
        /// <param name="sender">Sender instance</param>
        /// <param name="e">Event Args</param>
        public static Task InvokeAsync(this EventHandler @event, object sender, EventArgs e)
        {
            if (@event == null) return Task.CompletedTask;
            var tcs = new TaskCompletionSource<bool>();
            Task.Run(() =>
            {
                try
                {
                    @event(sender, e);
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
            return tcs.Task;
        }
        #endregion

        #region CreateAsync
        /// <summary>
        /// Create an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<Task> CreateAsync(this Action action) => action.InvokeAsync;
        /// <summary>
        /// Create an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T, Task> CreateAsync<T>(this Action<T> action) => action.InvokeAsync;
        /// <summary>
        /// Create an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, Task> CreateAsync<T1, T2>(this Action<T1, T2> action) => action.InvokeAsync;
        /// <summary>
        /// Create an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, T3, Task> CreateAsync<T1, T2, T3>(this Action<T1, T2, T3> action) => action.InvokeAsync;
        /// <summary>
        /// Create an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, T3, T4, Task> CreateAsync<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action) => action.InvokeAsync;
        /// <summary>
        /// Create an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, T3, T4, T5, Task> CreateAsync<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> action) => action.InvokeAsync;
        /// <summary>
        /// Create a Func in Async Task
        /// </summary>
        /// <returns>Task of the invocation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<Task<TResult>> CreateAsync<TResult>(this Func<TResult> func) => func.InvokeAsync;
        /// <summary>
        /// Create a Func in Async Task
        /// </summary>
        /// <returns>Task of the invocation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, Task<TResult>> CreateAsync<T1, TResult>(this Func<T1, TResult> func) => func.InvokeAsync;
        /// <summary>
        /// Create an Func in Async Task
        /// </summary>
        /// <returns>Task of the invocation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, Task<TResult>> CreateAsync<T1, T2, TResult>(this Func<T1, T2, TResult> func) => func.InvokeAsync;
        /// <summary>
        /// Create a Func in Async Task
        /// </summary>
        /// <returns>Task of the invocation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, T3, Task<TResult>> CreateAsync<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> func) => func.InvokeAsync;
        /// <summary>
        /// Create a Func in Async Task
        /// </summary>
        /// <returns>Task of the invocation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, T3, T4, Task<TResult>> CreateAsync<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> func) => func.InvokeAsync;
        /// <summary>
        /// Create a Func in Async Task
        /// </summary>
        /// <returns>Task of the invocation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, T3, T4, T5, Task<TResult>> CreateAsync<T1, T2, T3, T4, T5, TResult>(this Func<T1, T2, T3, T4, T5, TResult> func) => func.InvokeAsync;
        /// <summary>
        /// Create a Func in Async Task
        /// </summary>
        /// <returns>Task of the invocation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<object[], Task<object>> CreateAsync(this Delegate @delegate) => @delegate.InvokeAsync;
        #endregion

        #region Try
        /// <summary>
        /// Creates an action wrapper around the action to handles the execution inside a try/catch sentence
        /// </summary>
        /// <param name="action">Action to be executed inside a try/catch sentence</param>
        /// <param name="onException">Action to be executed when an exception has been catched</param>
        /// <returns>A new action wrapper</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action CreateTry(this Action action, Action<Exception> onException = null) => () => Try.Do(action, onException);
        /// <summary>
        /// Creates an action wrapper around the action to handles the execution inside a try/catch sentence
        /// </summary>
        /// <param name="action">Action to be executed inside a try/catch sentence</param>
        /// <param name="onException">Action to be executed when an exception has been catched</param>
        /// <returns>A new action wrapper</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<T> CreateTry<T>(this Action<T> action, Action<Exception> onException = null) => obj => Try.Do(o => action(o), obj, onException);
        /// <summary>
        /// Invoke the action inside a try/catch sentence
        /// </summary>
        /// <param name="action">Action to be executed inside a try/catch sentence</param>
        /// <param name="onException">Action to be executed when an exception has been catched</param>
        /// <returns>true if the execution finished sucessfully, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryInvoke(this Action action, Action<Exception> onException = null) => Try.Do(action, onException);
        /// <summary>
        /// Invoke the action inside a try/catch sentence
        /// </summary>
        /// <param name="action">Action to be executed inside a try/catch sentence</param>
        /// <param name="obj">Parameter of the action</param>
        /// <param name="onException">Action to be executed when an exception has been catched</param>
        /// <returns>true if the execution finished sucessfully, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryInvoke<T>(this Action<T> action, T obj, Action<Exception> onException = null) => Try.Do(o => action(o), obj, onException);
        #endregion

        #region Watch
        /// <summary>
        /// Creates an action wrapper around the action to messure the execution time for that action.
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <param name="onMethodEnds">Action to be executed at the end of the invoked method with the execution time data</param>
        /// <param name="asyncRun">Execute the onMethodsEnd action in a separated thread to avoid performance impact.</param>
        /// <returns>A new action wrapper</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action CreateWatch(this Action action, Action<Stopwatch> onMethodEnds, bool asyncRun = false) => () => Watch.Invoke(action, onMethodEnds, asyncRun);
        /// <summary>
        /// Creates an action wrapper around the action to messure the execution time for that action.
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <param name="statsMessage">Stats message to write in the log instance</param>
        /// <param name="asyncRun">Execute the onMethodsEnd action in a separated thread to avoid performance impact.</param>
        /// <returns>A new action wrapper</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action CreateWatch(this Action action, string statsMessage, bool asyncRun = false) => () => Watch.Invoke(action, statsMessage, asyncRun);
        /// <summary>
        /// Invokes the action and calls the onMethodEnds function at the end of the function with the execution time.
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <param name="onMethodEnds">Action to be executed at the end of the invoked method with the execution time data</param>
        /// <param name="asyncRun">Execute the onMethodsEnd action in a separated thread to avoid performance impact.</param>
        /// <returns>Function return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Invoke(this Action action, Action<Stopwatch> onMethodEnds, bool asyncRun = false) => Watch.Invoke(action, onMethodEnds, asyncRun);
        /// <summary>
        /// Invokes the action and save the stats message on the log instance.
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <param name="statsMessage">Stats message to write in the log instance</param>
        /// <param name="asyncRun">Execute the onMethodsEnd action in a separated thread to avoid performance impact.</param>
        /// <returns>Function return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Invoke(this Action action, string statsMessage, bool asyncRun = false) => Watch.Invoke(action, statsMessage, asyncRun);
        #endregion

        #region Lock
        /// <summary>
        /// Creates an action wrapper around the action to create a lock over the execution.
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <param name="lock">Object reference to create the lock sentence</param>
        /// <returns>A new action wrapper</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action CreateLock(this Action action, object @lock) => () => InvokeWithLock(action, @lock);
        /// <summary>
        /// Invokes the action with a lock over an object reference
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <param name="lock">Object reference to create the lock sentence</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InvokeWithLock(this Action action, object @lock)
        {
            lock (@lock)
                action();
        }

        /// <summary>
        /// Creates an func wrapper around the action to create a lock over the execution.
        /// </summary>
        /// <param name="function">Function to invoke</param>
        /// <param name="lock">Object reference to create the lock sentence</param>
        /// <returns>A new action wrapper</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T> CreateLock<T>(this Func<T> function, object @lock) => () => InvokeWithLock(function, @lock);
        /// <summary>
        /// Invokes the func with a lock over an object reference
        /// </summary>
        /// <param name="function">Function to invoke</param>
        /// <param name="lock">Object reference to create the lock sentence</param>
        /// <returns>Function invoke return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T InvokeWithLock<T>(this Func<T> function, object @lock)
        {
            lock (@lock)
                return function();
        }
        /// <summary>
        /// Creates an func wrapper around the action to create a lock over the execution.
        /// </summary>
        /// <param name="function">Function to invoke</param>
        /// <param name="lock">Object reference to create the lock sentence</param>
        /// <returns>A new action wrapper</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T, TR> CreateLock<T, TR>(this Func<T, TR> function, object @lock) => t => InvokeWithLock(function, t, @lock);
        /// <summary>
        /// Invokes the func with a lock over an object reference
        /// </summary>
        /// <param name="function">Function to invoke</param>
        /// <param name="state">State object parameter for function</param>
        /// <param name="lock">Object reference to create the lock sentence</param>
        /// <returns>Function invoke return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TR InvokeWithLock<T, TR>(this Func<T, TR> function, T state, object @lock)
        {
            lock (@lock)
                return function(state);
        }
        #endregion

        #region Retry
        /// <summary>
        /// Tries to run the function maximum times until no error and it returns true with a time interval
        /// </summary>
        /// <param name="function">Function to try</param>
        /// <param name="retryInterval">Time between retries</param>
        /// <param name="retryCount">Number max of retries</param>
        /// <returns>Result of the function</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> InvokeWithRetry(this Func<Task<bool>> function, TimeSpan? retryInterval, int retryCount = 3)
            => InvokeWithRetry(function, ret => !ret, (int?)retryInterval?.TotalMilliseconds ?? 1000, retryCount);
        /// <summary>
        /// Tries to run the function maximum times until no error and it returns true with a time interval
        /// </summary>
        /// <param name="function">Function to try</param>
        /// <param name="retryInterval">Time between retries</param>
        /// <param name="retryCount">Number max of retries</param>
        /// <returns>Result of the function</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> InvokeWithRetry(this Func<Task<bool>> function, int retryInterval = 1000, int retryCount = 3)
            => InvokeWithRetry(function, ret => !ret, retryInterval, retryCount);
        /// <summary>
        /// Tries to run the function maximum times until no error and it returns true with a time interval
        /// </summary>
        /// <param name="function">Function to try</param>
        /// <param name="shouldRetry">Predicate to check if the function needs to be executed again</param>
        /// <param name="retryInterval">Time between retries</param>
        /// <param name="retryCount">Number max of retries</param>
        /// <returns>Result of the function</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<TResult> InvokeWithRetry<TResult>(this Func<Task<TResult>> function, Predicate<TResult> shouldRetry, TimeSpan? retryInterval, int retryCount = 3)
            => InvokeWithRetry(function, shouldRetry, (int?)retryInterval?.TotalMilliseconds ?? 1000, retryCount);
        /// <summary>
        /// Tries to run the function maximum times until no error and it returns true with a time interval
        /// </summary>
        /// <param name="function">Function to try</param>
        /// <param name="shouldRetry">Predicate to check if the function needs to be executed again</param>
        /// <param name="retryInterval">Time between retries</param>
        /// <param name="retryCount">Number max of retries</param>
        /// <returns>Result of the function</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<TResult> InvokeWithRetry<TResult>(this Func<Task<TResult>> function, Predicate<TResult> shouldRetry, int retryInterval = 1000, int retryCount = 3)
        {
            var exceptions = new Queue<Exception>();
            for (var retry = 0; retry < retryCount; retry++)
            {
                var hasException = false;
                try
                {
                    var result = await function().ConfigureAwait(false);
                    if (!shouldRetry(result))
                        return result;
                }
                catch (Exception ex)
                {
                    hasException = true;
                    exceptions.Enqueue(ex);
                    if (exceptions.Count > 10)
                        exceptions.Dequeue();
                }
                if (retry < retryCount)
                {
                    if (hasException)
                        Core.Log.Warning("Error: {0}, Retrying in {1}ms...", exceptions.Last().Message, retryInterval);
                    else
                        Core.Log.Warning("Retrying in {0}ms...", retryInterval);
                    await Task.Delay(retryInterval).ConfigureAwait(false);
                }
            }
            throw new AggregateException("The function couldn't be executed successfully", exceptions);
        }
        /// <summary>
        /// Creates a Func With tries to run the function maximum times until no error and it returns true with a time interval
        /// </summary>
        /// <param name="function">Function to try</param>
        /// <param name="retryInterval">Time between retries</param>
        /// <param name="retryCount">Number max of retries</param>
        /// <returns>Result of the function</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<Task<bool>> CreateWithRetry(this Func<Task<bool>> function, TimeSpan? retryInterval, int retryCount = 3)
            => () => function.InvokeWithRetry(retryInterval, retryCount);
        /// <summary>
        /// Creates a Func With tries to run the function maximum times until no error and it returns true with a time interval
        /// </summary>
        /// <param name="function">Function to try</param>
        /// <param name="retryInterval">Time between retries</param>
        /// <param name="retryCount">Number max of retries</param>
        /// <returns>Result of the function</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<Task<bool>> CreateWithRetry(this Func<Task<bool>> function, int retryInterval = 1000, int retryCount = 3)
            => () => function.InvokeWithRetry(retryInterval, retryCount);
        /// <summary>
        /// Creates a Func With tries to run the function maximum times until no error and it returns true with a time interval
        /// </summary>
        /// <param name="function">Function to try</param>
        /// <param name="shouldRetry">Predicate to check if the function needs to be executed again</param>
        /// <param name="retryInterval">Time between retries</param>
        /// <param name="retryCount">Number max of retries</param>
        /// <returns>Result of the function</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<Task<TResult>> CreateWithRetry<TResult>(this Func<Task<TResult>> function, Predicate<TResult> shouldRetry, TimeSpan? retryInterval, int retryCount = 3)
            => () => function.InvokeWithRetry(shouldRetry, retryInterval, retryCount);
        /// <summary>
        /// Creates a Func With tries to run the function maximum times until no error and it returns true with a time interval
        /// </summary>
        /// <param name="function">Function to try</param>
        /// <param name="shouldRetry">Predicate to check if the function needs to be executed again</param>
        /// <param name="retryInterval">Time between retries</param>
        /// <param name="retryCount">Number max of retries</param>
        /// <returns>Result of the function</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<Task<TResult>> CreateWithRetry<TResult>(this Func<Task<TResult>> function, Predicate<TResult> shouldRetry, int retryInterval = 1000, int retryCount = 3)
            => () => function.InvokeWithRetry(shouldRetry, retryInterval, retryCount);
        #endregion

        #region Retry Action
        /// <summary>
        /// Tries to run the action maximum times until no error with a time interval
        /// </summary>
        /// <param name="action">Action to try</param>
        /// <param name="retryInterval">Time between retries</param>
        /// <param name="retryCount">Number max of retries</param>
        /// <returns>Retry Task instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task InvokeWithRetry(this Action action, TimeSpan? retryInterval, int retryCount = 3)
            => InvokeWithRetry(action, (int?)retryInterval?.TotalMilliseconds ?? 1000, retryCount);
        /// <summary>
        /// Tries to run the action maximum times until no error with a time interval
        /// </summary>
        /// <param name="action">Action to try</param>
        /// <param name="retryInterval">Time between retries</param>
        /// <param name="retryCount">Number max of retries</param>
        /// <returns>Retry Task instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task InvokeWithRetry(this Action action, int retryInterval = 1000, int retryCount = 3)
        {
            var exceptions = new Queue<Exception>();
            for (var retry = 0; retry < retryCount; retry++)
            {
                var hasException = false;
                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    hasException = true;
                    exceptions.Enqueue(ex);
                    if (exceptions.Count > 10)
                        exceptions.Dequeue();
                }
                if (retry < retryCount)
                {
                    if (hasException)
                        Core.Log.Warning("Error: {0}, Retrying in {1}ms...", exceptions.Last().Message, retryInterval);
                    else
                        Core.Log.Warning("Retrying in {0}ms...", retryInterval);
                    await Task.Delay(retryInterval).ConfigureAwait(false);
                }
            }
            throw new AggregateException("The function couldn't be executed successfully", exceptions);
        }
        /// <summary>
        /// Creates a Func With tries to run the function maximum times until no error with a time interval
        /// </summary>
        /// <param name="action">Action to try</param>
        /// <param name="retryInterval">Time between retries</param>
        /// <param name="retryCount">Number max of retries</param>
        /// <returns>Retry Task instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<Task> CreateWithRetry(this Action action, TimeSpan? retryInterval, int retryCount = 3)
            => () => action.InvokeWithRetry(retryInterval, retryCount);
        /// <summary>
        /// Creates a Func With tries to run the function maximum times until no error with a time interval
        /// </summary>
        /// <param name="action">Action to try</param>
        /// <param name="retryInterval">Time between retries</param>
        /// <param name="retryCount">Number max of retries</param>
        /// <returns>Retry Task instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<Task> CreateWithRetry(this Action function, int retryInterval = 1000, int retryCount = 3)
            => () => function.InvokeWithRetry(retryInterval, retryCount);
        #endregion

        #region WaitForAction
        /// <summary>
        /// Wait for action with timeout
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="milliseconds">Timeout in milliseconds</param>
        /// <returns>True if run successfully</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CallAndWaitFor(this Action action, int milliseconds)
        {
            var done = false;
            var task = Task.Run(() =>
            {
                try
                {
                    action();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            });
            if (task.Wait(milliseconds))
                done = task.Result;
            return done;
        }
        /// <summary>
        /// Wait for action with timeout
        /// </summary>
        /// <param name="task">Task to execute</param>
        /// <param name="milliseconds">Timeout in milliseconds</param>
        /// <returns>True if run successfully</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> CallAndWaitFor(this Task task, int milliseconds)
        {
            if (task.IsCompleted) return TaskHelper.CompleteTrue;
            return Task.WhenAny(Task.Delay(milliseconds), task).ContinueWith((resTask, state) => resTask == (Task)state, task,
                CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }
        /// <summary>
        /// Wait for action with timeout
        /// </summary>
        /// <param name="task">Task to execute</param>
        /// <param name="milliseconds">Timeout in milliseconds</param>
        /// <param name="cancellationToken">Cancellation token instance</param>
        /// <returns>True if run successfully</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> CallAndWaitFor(this Task task, int milliseconds, CancellationToken cancellationToken)
        {
            if (task.IsCompleted) return TaskHelper.CompleteTrue;
            if (cancellationToken.IsCancellationRequested) return TaskHelper.CompleteFalse;
            return Task.WhenAny(Task.Delay(milliseconds, cancellationToken), task).ContinueWith((resTask, state) => resTask == (Task)state, task,
                CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }
        #endregion
    }
}
