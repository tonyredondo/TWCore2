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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

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
        public static Action CreateDelay(this Action action, int milliseconds)
        {
            return new Action(() => Task.Delay(milliseconds).ContinueWith(t => action(), CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
                TaskScheduler.Default));
        }
        /// <summary>
        /// Wraps an action with a delay time.
        /// </summary>
        /// <param name="action">Original action</param>
        /// <param name="milliseconds">Delay time in milliseconds</param>
        /// <returns>Delayed action instance</returns>
        public static Action<T> CreateDelay<T>(this Action<T> action, int milliseconds)
        {
            return new Action<T>(obj => Task.Delay(milliseconds).ContinueWith((t, s) => action((T)s), obj, CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
                TaskScheduler.Default));
        }
        #endregion

        #region Buffered
        /// <summary>
        ///  Creates a buffered action wrapper from a action base.
        /// </summary>
        /// <param name="action">Original action</param>
        /// <param name="milliseconds">Delay time to start the action</param>
        /// <returns>Buffered action instance</returns>
        public static Action CreateBufferedAction(this Action action, int milliseconds)
        {
            Timer timer = null;
            return new Action(() =>
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
            });
        }
        /// <summary>
        ///  Creates a buffered action wrapper from a action base.
        /// </summary>
        /// <param name="action">Original action</param>
        /// <param name="milliseconds">Delay time to start the action</param>
        /// <returns>Buffered action instance</returns>
        public static Action<T> CreateBufferedAction<T>(this Action<T> action, int milliseconds)
        {
            Timer timer = null;
            return new Action<T>((obj) =>
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
            });
        }
        #endregion

        #region Throttled
        /// <summary>
        /// Creates a Throttled action wrapper form an action base.
        /// </summary>
        /// <param name="action">Original action</param>
        /// <param name="milliseconds">Delay time to start the action</param>
        /// <returns>Throttled action instance</returns>
        public static Action CreateThrottledAction(this Action action, int milliseconds)
        {
            var Date = DateTime.MinValue;
            return new Action(() =>
            {
                if (DateTime.UtcNow.Subtract(Date).TotalMilliseconds > milliseconds)
                {
                    action();
                    Date = DateTime.UtcNow;
                }
            });
        }
        /// <summary>
        /// Creates a Throttled action wrapper form an action base.
        /// </summary>
        /// <param name="action">Original action</param>
        /// <param name="milliseconds">Delay time to start the action</param>
        /// <returns>Throttled action instance</returns>
        public static Action<T> CreateThrottledAction<T>(this Action<T> action, int milliseconds)
        {
            var Date = DateTime.MinValue;
            return new Action<T>((obj) =>
            {
                if (DateTime.UtcNow.Subtract(Date).TotalMilliseconds > milliseconds)
                {
                    action(obj);
                    Date = DateTime.UtcNow;
                }
            });
        }
        #endregion

        #region InvokeAsync
        /// <summary>
        /// Invoke an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        public static Task InvokeAsync(this Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            Task.Run(() =>
            {
                try
                {
                    action();
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
        /// Invoke an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        public static Task InvokeAsync<T>(this Action<T> action, T value)
            => InvokeAsync(() => action(value));
        /// <summary>
        /// Invoke an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        public static Task InvokeAsync<T1, T2>(this Action<T1, T2> action, T1 value1, T2 value2)
            => InvokeAsync(() => action(value1, value2));
        /// <summary>
        /// Invoke an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        public static Task InvokeAsync<T1, T2, T3>(this Action<T1, T2, T3> action, T1 value1, T2 value2, T3 value3)
            => InvokeAsync(() => action(value1, value2, value3));
        /// <summary>
        /// Invoke an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        public static Task InvokeAsync<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action, T1 value1, T2 value2, T3 value3, T4 value4)
            => InvokeAsync(() => action(value1, value2, value3, value4));
        /// <summary>
        /// Invoke an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        public static Task InvokeAsync<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> action, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
            => InvokeAsync(() => action(value1, value2, value3, value4, value5));
        /// <summary>
        /// Invoke a Func in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        public static Task<TResult> InvokeAsync<TResult>(this Func<TResult> func)
        {
            var tcs = new TaskCompletionSource<TResult>();
            Task.Run(() =>
            {
                try
                {
                    tcs.TrySetResult(func());
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
            return tcs.Task;
        }
        /// <summary>
        /// Invoke a Func in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        public static Task<TResult> InvokeAsync<T1, TResult>(this Func<T1, TResult> func, T1 value)
            => InvokeAsync(() => func(value));
        /// <summary>
        /// Invoke a Func in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        public static Task<TResult> InvokeAsync<T1, T2, TResult>(this Func<T1, T2, TResult> func, T1 value1, T2 value2)
            => InvokeAsync(() => func(value1, value2));
        /// <summary>
        /// Invoke a Func in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        public static Task<TResult> InvokeAsync<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> func, T1 value1, T2 value2, T3 value3)
            => InvokeAsync(() => func(value1, value2, value3));
        /// <summary>
        /// Invoke a Func in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        public static Task<TResult> InvokeAsync<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> func, T1 value1, T2 value2, T3 value3, T4 value4)
            => InvokeAsync(() => func(value1, value2, value3, value4));
        /// <summary>
        /// Invoke a Func in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
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
        #endregion

        #region CreateAsync
        /// <summary>
        /// Create an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        public static Func<Task> CreateAsync(this Action action) => () => action.InvokeAsync();
        /// <summary>
        /// Create an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        public static Func<T, Task> CreateAsync<T>(this Action<T> action) => (v) => action.InvokeAsync(v);
        /// <summary>
        /// Create an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        public static Func<T1, T2, Task> CreateAsync<T1, T2>(this Action<T1, T2> action) => (v1, v2) => action.InvokeAsync(v1, v2);
        /// <summary>
        /// Create an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        public static Func<T1, T2, T3, Task> CreateAsync<T1, T2, T3>(this Action<T1, T2, T3> action) => (v1, v2, v3) => action.InvokeAsync(v1, v2, v3);
        /// <summary>
        /// Create an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        public static Func<T1, T2, T3, T4, Task> CreateAsync<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action) => (v1, v2, v3, v4) => action.InvokeAsync(v1, v2, v3, v4);
        /// <summary>
        /// Create an Action in Async Task
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Task of the invocation</returns>
        public static Func<T1, T2, T3, T4, T5, Task> CreateAsync<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> action) => (v1, v2, v3, v4, v5) => action.InvokeAsync(v1, v2, v3, v4, v5);
        /// <summary>
        /// Create a Func in Async Task
        /// </summary>
        /// <returns>Task of the invocation</returns>
        public static Func<Task<TResult>> CreateAsync<TResult>(this Func<TResult> func) => () => func.InvokeAsync();
        /// <summary>
        /// Create a Func in Async Task
        /// </summary>
        /// <returns>Task of the invocation</returns>
        public static Func<T1, Task<TResult>> CreateAsync<T1, TResult>(this Func<T1, TResult> func) => (v) => func.InvokeAsync(v);
        /// <summary>
        /// Create an Func in Async Task
        /// </summary>
        /// <returns>Task of the invocation</returns>
        public static Func<T1, T2, Task<TResult>> CreateAsync<T1, T2, TResult>(this Func<T1, T2, TResult> func) => (v1, v2) => func.InvokeAsync(v1, v2);
        /// <summary>
        /// Create a Func in Async Task
        /// </summary>
        /// <returns>Task of the invocation</returns>
        public static Func<T1, T2, T3, Task<TResult>> CreateAsync<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> func) => (v1, v2, v3) => func.InvokeAsync(v1, v2, v3);
        /// <summary>
        /// Create a Func in Async Task
        /// </summary>
        /// <returns>Task of the invocation</returns>
        public static Func<T1, T2, T3, T4, Task<TResult>> CreateAsync<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> func) => (v1, v2, v3, v4) => func.InvokeAsync(v1, v2, v3, v4);
        /// <summary>
        /// Create a Func in Async Task
        /// </summary>
        /// <returns>Task of the invocation</returns>
        public static Func<T1, T2, T3, T4, T5, Task<TResult>> CreateAsync<T1, T2, T3, T4, T5, TResult>(this Func<T1, T2, T3, T4, T5, TResult> func) => (v1, v2, v3, v4, v5) => func.InvokeAsync(v1, v2, v3, v4, v5);
        /// <summary>
        /// Create a Func in Async Task
        /// </summary>
        /// <returns>Task of the invocation</returns>
        public static Func<object[], Task<object>> CreateAsync(this Delegate @delegate) => (args) => @delegate.InvokeAsync(args);
        #endregion

        #region Try
        /// <summary>
        /// Creates an action wrapper around the action to handles the execution inside a try/catch sentence
        /// </summary>
        /// <param name="action">Action to be executed inside a try/catch sentence</param>
        /// <param name="onException">Action to be executed when an exception has been catched</param>
        /// <returns>A new action wrapper</returns>
        public static Action CreateTry(this Action action, Action<Exception> onException = null) => new Action(() => Try.Do(action, onException));
        /// <summary>
        /// Creates an action wrapper around the action to handles the execution inside a try/catch sentence
        /// </summary>
        /// <param name="action">Action to be executed inside a try/catch sentence</param>
        /// <param name="onException">Action to be executed when an exception has been catched</param>
        /// <returns>A new action wrapper</returns>
        public static Action<T> CreateTry<T>(this Action<T> action, Action<Exception> onException = null) => new Action<T>((obj) => Try.Do(() => action(obj), onException));
        /// <summary>
        /// Invoke the action inside a try/catch sentence
        /// </summary>
        /// <param name="action">Action to be executed inside a try/catch sentence</param>
        /// <param name="onException">Action to be executed when an exception has been catched</param>
        /// <returns>true if the execution finished sucessfully, otherwise false.</returns>
        public static bool TryInvoke(this Action action, Action<Exception> onException = null) => Try.Do(action, onException);
        /// <summary>
        /// Invoke the action inside a try/catch sentence
        /// </summary>
        /// <param name="action">Action to be executed inside a try/catch sentence</param>
        /// <param name="obj">Parameter of the action</param>
        /// <param name="onException">Action to be executed when an exception has been catched</param>
        /// <returns>true if the execution finished sucessfully, otherwise false.</returns>
        public static bool TryInvoke<T>(this Action<T> action, T obj, Action<Exception> onException = null) => Try.Do(() => action(obj), onException);
        #endregion

        #region Watch
        /// <summary>
        /// Creates an action wrapper around the action to messure the execution time for that action.
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <param name="onMethodEnds">Action to be executed at the end of the invoked method with the execution time data</param>
        /// <param name="asyncRun">Execute the onMethodsEnd action in a separated thread to avoid performance impact.</param>
        /// <returns>A new action wrapper</returns>
        public static Action CreateWatch(this Action action, Action<Stopwatch> onMethodEnds, bool asyncRun = false) => new Action(() => Watch.Invoke(action, onMethodEnds, asyncRun));
        /// <summary>
        /// Creates an action wrapper around the action to messure the execution time for that action.
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <param name="statsMessage">Stats message to write in the log instance</param>
        /// <param name="asyncRun">Execute the onMethodsEnd action in a separated thread to avoid performance impact.</param>
        /// <returns>A new action wrapper</returns>
        public static Action CreateWatch(this Action action, string statsMessage, bool asyncRun = false) => new Action(() => Watch.Invoke(action, statsMessage, asyncRun));
        /// <summary>
        /// Invokes the action and calls the onMethodEnds function at the end of the function with the execution time.
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <param name="onMethodEnds">Action to be executed at the end of the invoked method with the execution time data</param>
        /// <param name="asyncRun">Execute the onMethodsEnd action in a separated thread to avoid performance impact.</param>
        /// <returns>Function return value</returns>
        public static void Invoke(this Action action, Action<Stopwatch> onMethodEnds, bool asyncRun = false) => Watch.Invoke(action, onMethodEnds, asyncRun);
        /// <summary>
        /// Invokes the action and save the stats message on the log instance.
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <param name="statsMessage">Stats message to write in the log instance</param>
        /// <param name="asyncRun">Execute the onMethodsEnd action in a separated thread to avoid performance impact.</param>
        /// <returns>Function return value</returns>
        public static void Invoke(this Action action, string statsMessage, bool asyncRun = false) => Watch.Invoke(action, statsMessage, asyncRun);
        #endregion

        #region Lock
        /// <summary>
        /// Creates an action wrapper around the action to create a lock over the execution.
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <param name="lock">Object reference to create the lock sentence</param>
        /// <returns>A new action wrapper</returns>
        public static Action CreateLock(this Action action, object @lock) => new Action(() => InvokeWithLock(action, @lock));
        /// <summary>
        /// Invokes the action with a lock over an object reference
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <param name="lock">Object reference to create the lock sentence</param>
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
        public static Func<T> CreateLock<T>(this Func<T> function, object @lock) => new Func<T>(() => InvokeWithLock(function, @lock));
        /// <summary>
        /// Invokes the func with a lock over an object reference
        /// </summary>
        /// <param name="function">Function to invoke</param>
        /// <param name="lock">Object reference to create the lock sentence</param>
        /// <returns>Function invoke return value</returns>
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
        public static Func<T, R> CreateLock<T, R>(this Func<T, R> function, object @lock) => new Func<T, R>(t => InvokeWithLock(function, t, @lock));
        /// <summary>
        /// Invokes the func with a lock over an object reference
        /// </summary>
        /// <param name="function">Function to invoke</param>
        /// <param name="state">State object parameter for function</param>
        /// <param name="lock">Object reference to create the lock sentence</param>
        /// <returns>Function invoke return value</returns>
        public static R InvokeWithLock<T, R>(this Func<T, R> function, T state, object @lock)
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
        public static bool InvokeWithRetry(this Func<bool> function, TimeSpan? retryInterval, int retryCount = 3)
            => InvokeWithRetry(function, ret => !ret , (int?)retryInterval?.TotalMilliseconds ?? 1000, retryCount);
        /// <summary>
        /// Tries to run the function maximum times until no error and it returns true with a time interval
        /// </summary>
        /// <param name="function">Function to try</param>
        /// <param name="retryInterval">Time between retries</param>
        /// <param name="retryCount">Number max of retries</param>
        /// <returns>Result of the function</returns>
        public static bool InvokeWithRetry(this Func<bool> function, int retryInterval = 1000, int retryCount = 3)
            => InvokeWithRetry(function, ret => !ret , retryInterval, retryCount);
        /// <summary>
        /// Tries to run the function maximum times until no error and it returns true with a time interval
        /// </summary>
        /// <param name="function">Function to try</param>
        /// <param name="shouldRetry">Predicate to check if the function needs to be executed again</param>
        /// <param name="retryInterval">Time between retries</param>
        /// <param name="retryCount">Number max of retries</param>
        /// <returns>Result of the function</returns>
        public static TResult InvokeWithRetry<TResult>(this Func<TResult> function, Predicate<TResult> shouldRetry, TimeSpan? retryInterval, int retryCount = 3)
            => InvokeWithRetry(function, shouldRetry, (int?)retryInterval?.TotalMilliseconds ?? 1000, retryCount);
        /// <summary>
        /// Tries to run the function maximum times until no error and it returns true with a time interval
        /// </summary>
        /// <param name="function">Function to try</param>
        /// <param name="shouldRetry">Predicate to check if the function needs to be executed again</param>
        /// <param name="retryInterval">Time between retries</param>
        /// <param name="retryCount">Number max of retries</param>
        /// <returns>Result of the function</returns>
        public static TResult InvokeWithRetry<TResult>(this Func<TResult> function, Predicate<TResult> shouldRetry, int retryInterval = 1000, int retryCount = 3)
        {
            var exceptions = new List<Exception>();
            for (int retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    var result = function();
                    if (!shouldRetry(result))
                        return result;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
                Thread.Sleep(retryInterval);
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
        public static Func<bool> CreateWithRetry(this Func<bool> function, TimeSpan? retryInterval, int retryCount = 3)
            =>  () => function.InvokeWithRetry(retryInterval, retryCount);
        /// <summary>
        /// Creates a Func With tries to run the function maximum times until no error and it returns true with a time interval
        /// </summary>
        /// <param name="function">Function to try</param>
        /// <param name="retryInterval">Time between retries</param>
        /// <param name="retryCount">Number max of retries</param>
        /// <returns>Result of the function</returns>
        public static Func<bool> CreateWithRetry(this Func<bool> function, int retryInterval = 1000, int retryCount = 3)
            =>  () => function.InvokeWithRetry(retryInterval, retryCount);
        /// <summary>
        /// Creates a Func With tries to run the function maximum times until no error and it returns true with a time interval
        /// </summary>
        /// <param name="function">Function to try</param>
        /// <param name="shouldRetry">Predicate to check if the function needs to be executed again</param>
        /// <param name="retryInterval">Time between retries</param>
        /// <param name="retryCount">Number max of retries</param>
        /// <returns>Result of the function</returns>
        public static Func<TResult> CreateWithRetry<TResult>(this Func<TResult> function, Predicate<TResult> shouldRetry, TimeSpan? retryInterval, int retryCount = 3)
            =>  () => function.InvokeWithRetry(shouldRetry, retryInterval, retryCount);
        /// <summary>
        /// Creates a Func With tries to run the function maximum times until no error and it returns true with a time interval
        /// </summary>
        /// <param name="function">Function to try</param>
        /// <param name="shouldRetry">Predicate to check if the function needs to be executed again</param>
        /// <param name="retryInterval">Time between retries</param>
        /// <param name="retryCount">Number max of retries</param>
        /// <returns>Result of the function</returns>
        public static Func<TResult> CreateWithRetry<TResult>(this Func<TResult> function, Predicate<TResult> shouldRetry, int retryInterval = 1000, int retryCount = 3)
            =>  () => function.InvokeWithRetry(shouldRetry, retryInterval, retryCount);
        #endregion

        #region WaitForAction
        /// <summary>
        /// Wait for action with timeout
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="milliseconds">Timeout in milliseconds</param>
        /// <returns>True if run successfully</returns>
        public static bool WaitForAction(this Action action, int milliseconds)
        {
            bool done = false;
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
        #endregion
    }
}
