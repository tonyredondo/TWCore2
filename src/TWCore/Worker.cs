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
using System.Threading.Channels;
using System.Threading.Tasks;
using TWCore.Threading;

// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace TWCore
{
    /// <inheritdoc />
    /// <summary>
    /// Action to process a Queue of elements in a new thread.
    /// </summary>
    /// <typeparam name="T">Queue item type</typeparam>
    public class Worker<T> : IDisposable
    {
        #region Private fields
        private readonly Func<T, Task> _func;
        private readonly Func<bool> _precondition;
        //private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
        private readonly AsyncManualResetEvent _resetEvent = new AsyncManualResetEvent();
        private ConcurrentQueue<T> _queue;
        private Task _processThread;
        private CancellationTokenSource _tokenSource;
        private volatile bool _startActive;
        private volatile WorkerStatus _status = WorkerStatus.Stopped;
        private long _queueCount;
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
        /// Current queued items.
        /// </summary>
        public IEnumerable<T> QueueItems => _queue?.ToArray();
        /// <summary>
        /// Get the Worker status.
        /// </summary>
        public WorkerStatus Status => _status;
        /// <summary>
        /// Enable the wait timeout
        /// </summary>
        /// <value><c>true</c> if enable wait timeout; otherwise, <c>false</c>.</value>
        public bool EnableWaitTimeout { get; set; } = true;
        /// <summary>
        /// Cancellation Token
        /// </summary>
        public CancellationToken CancellationToken => _tokenSource.Token;
        /// <summary>
        /// Gets the number of elements in the queue
        /// </summary>
        public int Count => (int)_queueCount;
        /// <summary>
        /// Gets or Sets if the worker must ignore Exceptions
        /// </summary>
        public bool IgnoreExceptions { get; set; }
        /// <summary>
        /// Get if the worker is using its own thread.
        /// </summary>
        public bool UseOwnThread { get; }
        #endregion

        #region .ctors
        /// <summary>
        /// Action to process a Queue of elements in a new thread.
        /// </summary>
        /// <param name="action">Action to process each element of the queue</param>
        /// <param name="startActive">Start active flag, default value is true</param>
        /// <param name="ignoreExceptions">Sets if the worker must ignore Exceptions</param>
        /// <param name="useOwnThread">Sets if the worker uses its own thread</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Worker(Action<T> action, bool startActive = true, bool ignoreExceptions = false, bool useOwnThread = true)
        {
            _queue = new ConcurrentQueue<T>();
            _precondition = null;
            _func = arg => { action(arg); return Task.CompletedTask; };
            Ensure.ArgumentNotNull(action, "The action can't be null.");
            _tokenSource = new CancellationTokenSource();
            _startActive = startActive;
            IgnoreExceptions = ignoreExceptions;
            UseOwnThread = useOwnThread;
            Init();
        }
        /// <summary>
        /// Action to process a Queue of elements in a new thread.
        /// </summary>
        /// <param name="precondition">Precondition to accomplish before dequeuing an element from the queue</param>
        /// <param name="action">Action to process each element of the queue</param>
        /// <param name="startActive">Start active flag, default value is true</param>
        /// <param name="ignoreExceptions">Sets if the worker must ignore Exceptions</param>
        /// <param name="useOwnThread">Sets if the worker uses its own thread</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Worker(Func<bool> precondition, Action<T> action, bool startActive = true, bool ignoreExceptions = false, bool useOwnThread = true)
        {
            _queue = new ConcurrentQueue<T>();
            _precondition = precondition;
            _func = arg => { action(arg); return Task.CompletedTask; };
            Ensure.ArgumentNotNull(action, "The action can't be null.");
            _tokenSource = new CancellationTokenSource();
            _startActive = startActive;
            IgnoreExceptions = ignoreExceptions;
            UseOwnThread = useOwnThread;
            Init();
        }
        /// <summary>
        /// Action to process a Queue of elements in a new thread.
        /// </summary>
        /// <param name="function">Func to process each element of the queue</param>
        /// <param name="startActive">Start active flag, default value is true</param>
        /// <param name="ignoreExceptions">Sets if the worker must ignore Exceptions</param>
        /// <param name="useOwnThread">Sets if the worker uses its own thread</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Worker(Func<T, Task> function, bool startActive = true, bool ignoreExceptions = false, bool useOwnThread = false)
        {
            _queue = new ConcurrentQueue<T>();
            _precondition = null;
            _func = function;
            Ensure.ArgumentNotNull(_func, "The func can't be null.");
            _tokenSource = new CancellationTokenSource();
            _startActive = startActive;
            IgnoreExceptions = ignoreExceptions;
            UseOwnThread = useOwnThread;
            Init();
        }
        /// <summary>
        /// Action to process a Queue of elements in a new thread.
        /// </summary>
        /// <param name="precondition">Precondition to accomplish before dequeuing an element from the queue</param>
        /// <param name="function">Func to process each element of the queue</param>
        /// <param name="startActive">Start active flag, default value is true</param>
        /// <param name="ignoreExceptions">Sets if the worker must ignore Exceptions</param>
        /// <param name="useOwnThread">Sets if the worker uses its own thread</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Worker(Func<bool> precondition, Func<T, Task> function, bool startActive = true, bool ignoreExceptions = false, bool useOwnThread = false)
        {
            _queue = new ConcurrentQueue<T>();
            _precondition = precondition;
            _func = function;
            Ensure.ArgumentNotNull(_func, "The func can't be null.");
            _tokenSource = new CancellationTokenSource();
            _startActive = startActive;
            IgnoreExceptions = ignoreExceptions;
            UseOwnThread = useOwnThread;
            Init();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Init()
        {
            _tokenSource = new CancellationTokenSource();
            _processThread = _precondition is null ?
                (UseOwnThread ? Task.Factory.StartNew(OneLoopDequeueThread, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default) : OneLoopDequeueThread()) :
                (UseOwnThread ? Task.Factory.StartNew(OneLoopDequeueThreadWithPrecondition, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default) : OneLoopDequeueThreadWithPrecondition());
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
            if (Status == WorkerStatus.Disposed || Status == WorkerStatus.Stopping) return false;
            _queue.Enqueue(item);
            Interlocked.Increment(ref _queueCount);
            if (_startActive && Status == WorkerStatus.Stopped)
                Start();
            else
                _resetEvent.Set();
                //_semaphore.Release();
            return true;
        }
        /// <summary>
        /// Clears the queue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var prevStatus = _status;
            Stop();
            Interlocked.Exchange(ref _queueCount, 0);
            lock (this)
                _queue = new ConcurrentQueue<T>();
            if (prevStatus == WorkerStatus.Started)
                Start();
        }

        #endregion

        #region Thread
        /// <summary>
        /// Starts the processing thread
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start()
        {
            if (Status == WorkerStatus.Stopped)
                _status = WorkerStatus.Started;
            _resetEvent.Set();
            //_semaphore.Release();
        }
        /// <summary>
        /// Stops the processing thread
        /// </summary>
        /// <param name="afterNumItemsInQueue">Number of element to process before stopping the worker.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task StopAsync(int afterNumItemsInQueue)
        {
            if (_status != WorkerStatus.Started) return;
            _status = WorkerStatus.Stopping;
            if (afterNumItemsInQueue > 0)
            {
                var numItems = _queue.Count;
                var remain = numItems > afterNumItemsInQueue ? numItems - afterNumItemsInQueue : 0;
                if (EnableWaitTimeout)
                    await TaskHelper.SleepUntil(() => _queue.Count <= remain, Core.GlobalSettings.WorkerWaitTimeout).ConfigureAwait(false);
                else
                    await TaskHelper.SleepUntil(() => _queue.Count <= remain).ConfigureAwait(false);
            }
            _status = WorkerStatus.Stopped;
        }
        /// <summary>
        /// Stops the processing thread
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop()
        {
            if (_status != WorkerStatus.Started) return;
            _status = WorkerStatus.Stopped;
        }

        [IgnoreStackFrameLog]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task OneLoopDequeueThreadWithPrecondition()
        {
            var token = _tokenSource.Token;
            var workDone = false;
            var useOwnThread = UseOwnThread;
            while (!token.IsCancellationRequested)
            {
                if (Interlocked.Read(ref _queueCount) == 0)
                {
                    //var smTask = _semaphore.WaitAsync(token);
                    var smTask = _resetEvent.WaitAsync(token);
                    if (useOwnThread)
                        await smTask;
                    else
                        await smTask.ConfigureAwait(false);
                }

                while (!token.IsCancellationRequested && (_status == WorkerStatus.Started || _status == WorkerStatus.Stopping) && _queue.TryDequeue(out var item))
                {
                    Interlocked.Decrement(ref _queueCount);
                    if (_precondition?.Invoke() == false)
                    {
                        if (useOwnThread)
                            await TaskHelper.SleepUntil(_precondition, token);
                        else
                            await TaskHelper.SleepUntil(_precondition, token).ConfigureAwait(false);
                    }
                    if (token.IsCancellationRequested)
                        break;
                    try
                    {
                        if (useOwnThread)
                            await _func(item);
                        else
                            await _func(item).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        if (IgnoreExceptions) continue;
                        OnException?.Invoke(this, (ex, item));
                    }
                    finally
                    {
                        workDone = true;
                    }
                }
                _resetEvent.Reset();
                if (workDone && OnWorkDone != null)
                {
                    try
                    {
                        workDone = false;
                        OnWorkDone.Invoke(this, new EventArgs());
                    }
                    catch
                    {
                        //
                    }
                }
                if (token.IsCancellationRequested) break;
            }
        }
        [IgnoreStackFrameLog]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task OneLoopDequeueThread()
        {
            var token = _tokenSource.Token;
            var workDone = false;
            var useOwnThread = UseOwnThread;
            while (!token.IsCancellationRequested)
            {
                if (Interlocked.Read(ref _queueCount) == 0)
                {
                    //var smTask = _semaphore.WaitAsync(token);
                    var smTask = _resetEvent.WaitAsync(token);
                    if (useOwnThread)
                        await smTask;
                    else
                        await smTask.ConfigureAwait(false);
                }

                while (!token.IsCancellationRequested && (_status == WorkerStatus.Started || _status == WorkerStatus.Stopping) && _queue.TryDequeue(out var item))
                {
                    Interlocked.Decrement(ref _queueCount);
                    try
                    {
                        if (useOwnThread)
                            await _func(item);
                        else
                            await _func(item).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        if (IgnoreExceptions) continue;
                        OnException?.Invoke(this, (ex, item));
                    }
                    finally
                    {
                        workDone = true;
                    }
                }
                _resetEvent.Reset();

                if (workDone && OnWorkDone != null)
                {
                    try
                    {
                        workDone = false;
                        OnWorkDone.Invoke(this, new EventArgs());
                    }
                    catch
                    {
                        //
                    }
                }
                if (token.IsCancellationRequested) break;
            }
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            StopAsync(int.MaxValue).WaitAsync();
            //_semaphore.Release();
            _resetEvent.Set();
            _tokenSource.Cancel();
            try
            {
                _processThread?.Wait(2500);
                _processThread = null;
            }
            catch
            {
                // ignored
            }
            Interlocked.Exchange(ref _queueCount, 0);
            Core.Status.DeAttachObject(this);
        }
    }


    /// <summary>
    /// WorkerStatus enum
    /// </summary>
    public enum WorkerStatus
    {
        /// <summary>
        /// Worker stopped
        /// </summary>
        Stopped,
        /// <summary>
        /// Worker started
        /// </summary>
        Started,
        /// <summary>
        /// Worker stopping
        /// </summary>
        Stopping,
        /// <summary>
        /// Worker disposed
        /// </summary>
        Disposed
    }

    //public class WorkerNew<T> : IDisposable
    //{
    //    private CancellationTokenSource _tokenSource;
    //    private readonly Action<T> _action;
    //    private readonly Func<T, Task> _func;
    //    private readonly Func<bool> _precondition;
    //    private readonly bool _startActive;
    //    private readonly bool _ignoreExceptions;
    //    private readonly bool _useOwnThread;
    //    private ChannelReader<T> _reader;
    //    private ChannelWriter<T> _writer;
    //    private volatile WorkerStatus _status = WorkerStatus.Stopped;


    //    #region .ctor
    //    /// <summary>
    //    /// Action to process a Queue of elements in a new thread.
    //    /// </summary>
    //    /// <param name="action">Action to process each element of the queue</param>
    //    /// <param name="startActive">Start active flag, default value is true</param>
    //    /// <param name="ignoreExceptions">Sets if the worker must ignore Exceptions</param>
    //    /// <param name="useOwnThread">Sets if the worker uses its own thread</param>
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public WorkerNew(Action<T> action, bool startActive = true, bool ignoreExceptions = false, bool useOwnThread = true)
    //    {
    //        Ensure.ArgumentNotNull(action, "The action can't be null.");
    //        _precondition = null;
    //        _action = action;
    //        _func = null;
    //        _startActive = startActive;
    //        _ignoreExceptions = ignoreExceptions;
    //        _useOwnThread = useOwnThread;
    //        Init();
    //    }
    //    /// <summary>
    //    /// Action to process a Queue of elements in a new thread.
    //    /// </summary>
    //    /// <param name="precondition">Precondition to accomplish before dequeuing an element from the queue</param>
    //    /// <param name="action">Action to process each element of the queue</param>
    //    /// <param name="startActive">Start active flag, default value is true</param>
    //    /// <param name="ignoreExceptions">Sets if the worker must ignore Exceptions</param>
    //    /// <param name="useOwnThread">Sets if the worker uses its own thread</param>
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public WorkerNew(Func<bool> precondition, Action<T> action, bool startActive = true, bool ignoreExceptions = false, bool useOwnThread = true)
    //    {
    //        Ensure.ArgumentNotNull(action, "The action can't be null.");
    //        _precondition = precondition;
    //        _action = action;
    //        _func = null;
    //        _startActive = startActive;
    //        _ignoreExceptions = ignoreExceptions;
    //        _useOwnThread = useOwnThread;
    //        Init();
    //    }
    //    /// <summary>
    //    /// Action to process a Queue of elements in a new thread.
    //    /// </summary>
    //    /// <param name="function">Func to process each element of the queue</param>
    //    /// <param name="startActive">Start active flag, default value is true</param>
    //    /// <param name="ignoreExceptions">Sets if the worker must ignore Exceptions</param>
    //    /// <param name="useOwnThread">Sets if the worker uses its own thread</param>
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public WorkerNew(Func<T, Task> function, bool startActive = true, bool ignoreExceptions = false, bool useOwnThread = false)
    //    {
    //        Ensure.ArgumentNotNull(_func, "The func can't be null.");
    //        _precondition = null;
    //        _action = null;
    //        _func = function;
    //        _startActive = startActive;
    //        _ignoreExceptions = ignoreExceptions;
    //        _useOwnThread = useOwnThread;
    //        Init();
    //    }
    //    /// <summary>
    //    /// Action to process a Queue of elements in a new thread.
    //    /// </summary>
    //    /// <param name="precondition">Precondition to accomplish before dequeuing an element from the queue</param>
    //    /// <param name="function">Func to process each element of the queue</param>
    //    /// <param name="startActive">Start active flag, default value is true</param>
    //    /// <param name="ignoreExceptions">Sets if the worker must ignore Exceptions</param>
    //    /// <param name="useOwnThread">Sets if the worker uses its own thread</param>
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public WorkerNew(Func<bool> precondition, Func<T, Task> function, bool startActive = true, bool ignoreExceptions = false, bool useOwnThread = false)
    //    {
    //        Ensure.ArgumentNotNull(_func, "The func can't be null.");
    //        _precondition = precondition;
    //        _action = null;
    //        _func  = function;
    //        _startActive = startActive;
    //        _ignoreExceptions = ignoreExceptions;
    //        _useOwnThread = useOwnThread;
    //        Init();
    //    }
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    private void Init()
    //    {
    //        _tokenSource = new CancellationTokenSource();
    //        CreateChannels();
    //    }
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    private void CreateChannels()
    //    {
    //        var channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions
    //        {
    //            AllowSynchronousContinuations = false,
    //            SingleReader = true,
    //            SingleWriter = false
    //        });
    //        Interlocked.Exchange(ref _writer, channel.Writer);
    //        Interlocked.Exchange(ref _reader, channel.Reader);
    //    }
    //    #endregion

    //    #region Queue Methods
    //    /// <summary>
    //    /// Enqueue a new element on the queue
    //    /// </summary>
    //    /// <param name="item">Element to enqueue</param>
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public bool Enqueue(T item)
    //    {
    //        if (_status == WorkerStatus.Disposed || _status == WorkerStatus.Stopping) return false;
    //        var res = _writer.TryWrite(item);
    //        if (res && _startActive && _status == WorkerStatus.Stopped)
    //            Start();
    //        return res;
    //    }
    //    /// <summary>
    //    /// Clears the queue
    //    /// </summary>
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void Clear()
    //    {
    //        var prevStatus = _status;
    //        Stop();
    //        CreateChannels();
    //        if (prevStatus == WorkerStatus.Started)
    //            Start();
    //    }
    //    #endregion


    //    /// <inheritdoc />
    //    /// <summary>
    //    /// Dispose all resources
    //    /// </summary>
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void Dispose()
    //    {
    //    }
    //}
}
