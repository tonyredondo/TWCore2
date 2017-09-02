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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TWCore
{
    /// <summary>
    /// Action to process a Queue of elements in a new thread.
    /// </summary>
    /// <typeparam name="T">Queue item type</typeparam>
    public class Worker<T> : IDisposable
    {
        #region Private fields
        ConcurrentQueue<T> _queue;
        Action<T> _action;
        Func<bool> _precondition;
        Task _processThread;
        CancellationTokenSource tokenSource = null;
        volatile bool startActive = false;
        volatile WorkerStatus status = WorkerStatus.Stopped;
        ManualResetEventSlim processHandler = new ManualResetEventSlim(false);
        #endregion

        #region Events
        /// <summary>
        /// Events that triggers when the action couldn't process an element from the Queue.
        /// </summary>
        public event EventHandler<(Exception, T)> OnException;
        /// <summary>
        /// Event that triggers when the worker has finished to do all elements in the queue.
        /// </summary>
        public event EventHandler OnWorkDone;
        #endregion

        #region Properties
        /// <summary>
        /// List of all exceptions occurred when the Queue process.
        /// </summary>
        public List<(Exception, T)> Exceptions { get; private set; }
        /// <summary>
        /// Current queued items.
        /// </summary>
        public IEnumerable<T> QueueItems => _queue?.ToArray();
        /// <summary>
        /// Get the Worker status.
        /// </summary>
        public WorkerStatus Status => status;
		/// <summary>
		/// Enable the wait timeout
		/// </summary>
		/// <value><c>true</c> if enable wait timeout; otherwise, <c>false</c>.</value>
		public bool EnableWaitTimeout { get; set; } = true;
        #endregion

        #region .ctors
        /// <summary>
        /// Action to process a Queue of elements in a new thread.
        /// </summary>
        /// <param name="action">Action to process each element of the queue</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Worker(Action<T> action, bool startActive = true)
        {
            _queue = new ConcurrentQueue<T>();
            _precondition = null;
            _action = action;
            Ensure.ArgumentNotNull(_action, "The action can't be null.");
            Exceptions = new List<(Exception, T)>();
            tokenSource = new CancellationTokenSource();
            this.startActive = startActive;
            Init();
        }
        /// <summary>
        /// Action to process a Queue of elements in a new thread.
        /// </summary>
        /// <param name="precondition">Precondition to accomplish before dequeuing an element from the queue</param>
        /// <param name="action">Action to process each element of the queue</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Worker(Func<bool> precondition, Action<T> action, bool startActive = true)
        {
            _queue = new ConcurrentQueue<T>();
            _precondition = precondition;
            _action = action;
            Ensure.ArgumentNotNull(_action, "The action can't be null.");
            Exceptions = new List<(Exception, T)>();
            tokenSource = new CancellationTokenSource();
            this.startActive = startActive;
            Init();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Init()
        {
            processHandler.Reset();
            tokenSource = new CancellationTokenSource();
            _processThread = Task.Factory.StartNew(obj =>
            {
                OneLoopDequeueThread((CancellationToken)obj);
            }, tokenSource.Token, tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(Exceptions) + " Count", Exceptions?.Count);
                collection.Add("Queue Count", _queue?.Count);
                collection.Add("Status", status);
                collection.Add("Started Active", this.startActive);
            });
        }
        #endregion

        #region Queue Methods
        /// <summary>
        /// Enqueue a new element on the queue
        /// </summary>
        /// <param name="item">Element to enqueue</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Enqueue(T item)
        {
            if (Status != WorkerStatus.Disposed && Status != WorkerStatus.Stopping) 
            {
                _queue.Enqueue(item);
                if (startActive && Status == WorkerStatus.Stopped)
                    Start();
                else
                    processHandler.Set();
				return true;
            }
			return false;
        }
        /// <summary>
        /// Clears the queue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var prevStatus = status;
            Stop();
            lock (this)
                _queue = new ConcurrentQueue<T>();
            if (prevStatus == WorkerStatus.Started)
                Start();
        }
        /// <summary>
        /// Gets the number of elements in the queue
        /// </summary>
        public int Count => _queue.Count;
        #endregion

        #region Thread
        /// <summary>
        /// Starts the processing thread
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start()
        {
            if (Status == WorkerStatus.Stopped)
                status = WorkerStatus.Started;
            processHandler.Set();
        }
        /// <summary>
        /// Stops the processing thread
        /// </summary>
        /// <param name="afterNumItemsInQueue">Number of element to process before stopping the worker.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop(int afterNumItemsInQueue)
        {
            if (status == WorkerStatus.Started)
            {
                status = WorkerStatus.Stopping;
                if (afterNumItemsInQueue > 0)
                {
                    var numItems = _queue.Count;
                    var remain = numItems > afterNumItemsInQueue ? numItems - afterNumItemsInQueue : 0;
					if (EnableWaitTimeout)
                    	Factory.Thread.SleepUntil(() => _queue.Count <= remain, Core.GlobalSettings.WorkerWaitTimeout);
					else
						Factory.Thread.SleepUntil(() => _queue.Count <= remain);
                }
                status = WorkerStatus.Stopped;
                processHandler.Reset();
            }
        }
        /// <summary>
        /// Stops the processing thread
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop()
        {
            if (status == WorkerStatus.Started)
            {
                status = WorkerStatus.Stopped;
                processHandler.Reset();
            }
        }

        readonly object locker = new object();
        [IgnoreStackFrameLog]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OneLoopDequeueThread(CancellationToken token)
        {
            bool workDone = false;
            while (!token.IsCancellationRequested)
            {
                processHandler.Wait(token);

                while (!token.IsCancellationRequested && (status == WorkerStatus.Started || status == WorkerStatus.Stopping) && _queue.TryDequeue(out var item))
                {
                    if (_precondition?.Invoke() == false)
                        Factory.Thread.SleepUntil(_precondition, token);
                    if (token.IsCancellationRequested)
                        break;
                    try
                    {
                        _action(item);
                        workDone = true;
                    }
                    catch (Exception ex)
                    {
                        OnException?.Invoke(this, (ex, item));
                        lock (locker)
                            Exceptions.Add((ex, item));
                    }
                }
                if (workDone && OnWorkDone != null)
                    Try.Do(() => OnWorkDone.Invoke(this, new EventArgs()));
                if (!token.IsCancellationRequested) 
                {
                    try
                    {
                        processHandler.Reset();
                    }
                    catch { }
                }
            }
        }
        #endregion

        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Stop(int.MaxValue);
            processHandler.Set();
            tokenSource.Cancel();
            _processThread?.Wait(5000);
            _processThread = null;
            processHandler.Reset();
            Core.Status.DeAttachObject(this);
        }
    }


    /// <summary>
    /// WorkerStatus enum
    /// </summary>
    public enum WorkerStatus 
    {
        Stopped,
        Started,
        Stopping,
        Disposed
    }
}
