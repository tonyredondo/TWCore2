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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace TWCore
{
    /// <summary>
    /// Worker Task Scheduler
    /// </summary>
    [IgnoreStackFrameLog]
    public class WorkerTaskScheduler : TaskScheduler, IDisposable
    {
        private readonly BlockingCollection<Task> _tasks = new BlockingCollection<Task>();
        private readonly int _concurrency;
        private readonly Thread[] _threads;
        private readonly TaskFactory _factory;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private readonly ManualResetEventSlim _finalizerEvent = new ManualResetEventSlim();

        #region .ctor
        /// <summary>
        /// Worker Task Scheduler
        /// </summary>
        /// <param name="concurrency">Concurrency level</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WorkerTaskScheduler(int concurrency)
        {
            if (concurrency < 1)
                throw new ArgumentException("The concurrency must be greater than 0", nameof(concurrency));
            _concurrency = concurrency;
            _threads = new Thread[concurrency];
            for (var i = 0; i < concurrency; i++)
                _threads[i] = CreateProcessThread();
            _factory = new TaskFactory(CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskContinuationOptions.None, this);
        }
        #endregion

        #region Protected methods
        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _tasks;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void QueueTask(Task task)
        {
            _tasks.Add(task);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Run Task in the Thread Scheduler
        /// </summary>
        /// <param name="func">Func to execute inside the scheduler</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [IgnoreStackFrameLog]
        public Task Run(Func<Task> func)
        {
            if (_tokenSource.IsCancellationRequested) return Task.FromCanceled(_tokenSource.Token);
            return _factory.StartNew(func).Unwrap();
        }
        /// <summary>
        /// Run Task in the Thread Scheduler
        /// </summary>
        /// <param name="func">Func to execute inside the scheduler</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [IgnoreStackFrameLog]
        public Task<T> Run<T>(Func<Task<T>> func)
        {
            if (_tokenSource.IsCancellationRequested) return Task.FromCanceled<T>(_tokenSource.Token);
            return _factory.StartNew(func).Unwrap();
        }
        /// <summary>
        /// Run Task in the Thread Scheduler
        /// </summary>
        /// <param name="func">Func to execute inside the scheduler</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [IgnoreStackFrameLog]
        public Task Run(Action func)
        {
            if (_tokenSource.IsCancellationRequested) return Task.FromCanceled(_tokenSource.Token);
            return _factory.StartNew(func);
        }
        /// <summary>
        /// Run Task in the Thread Scheduler
        /// </summary>
        /// <param name="func">Func to execute inside the scheduler</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [IgnoreStackFrameLog]
        public Task<T> Run<T>(Func<T> func)
        {
            if (_tokenSource.IsCancellationRequested) return Task.FromCanceled<T>(_tokenSource.Token);
            return _factory.StartNew(func);
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [IgnoreStackFrameLog]
        private Thread CreateProcessThread()
        {
            var thread = new Thread(state => ThreadExecutor(state))
            {
                IsBackground = true,
                Name = "WorkerTaskSchedulerThread"
            };
            thread.Start(this);
            return thread;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [IgnoreStackFrameLog]
        private static void ThreadExecutor(object state)
        {
            var scheduler = (WorkerTaskScheduler)state;
            var token = scheduler._tokenSource.Token;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (scheduler._tasks.TryTake(out var task, Timeout.Infinite, token))
                    {
                        try
                        {
                            scheduler.TryExecuteTask(task);
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Write(ex);
                        }
                    }
                }
                catch(OperationCanceledException)
                {
                    //
                }
            }
            scheduler._finalizerEvent.Set();
        }

        #region IDisposable Support
        private bool disposedValue = false; // Para detectar llamadas redundantes

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: eliminse el estado administrado (objetos administrados).
                    _tokenSource.Cancel();
                    _finalizerEvent.Wait();
                }
                disposedValue = true;
            }
        }

        // Este código se agrega para implementar correctamente el patrón descartable.
        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
        #endregion
    }
}
