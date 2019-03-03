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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using TWCore.Threading;

// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace TWCore
{
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


    /// <inheritdoc />
    /// <summary>
    /// Action to process a Queue of elements in a new thread.
    /// </summary>
    /// <typeparam name="T">Queue item type</typeparam>
    public class Worker<T> : IDisposable
    {
        private CancellationTokenSource _tokenSource;
        private readonly Action<T> _action;
        private readonly Func<T, Task> _func;
        private readonly Func<bool> _precondition;
        private readonly bool _startActive;
        private readonly bool _useOwnThread;
        private bool _ignoreExceptions;
        private ChannelReader<T> _reader;
        private ChannelWriter<T> _writer;
        private volatile WorkerStatus _status = WorkerStatus.Stopped;
        private Task _processTask;
        private Thread _processThread;
        private ManualResetEventSlim _processThreadResetEvent;
        private long _queueCount;
        private int _workerStartStatus;

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
        /// Get the Worker status.
        /// </summary>
        public WorkerStatus Status => _status;
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
        public bool IgnoreExceptions
        {
            get => _ignoreExceptions;
            set => _ignoreExceptions = value;
        }
        /// <summary>
        /// Get if the worker is using its own thread.
        /// </summary>
        public bool UseOwnThread => _useOwnThread;
        #endregion

        #region .ctor
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
            Ensure.ArgumentNotNull(action, "The action can't be null.");
            _precondition = null;
            _action = action;
            _func = null;
            _startActive = startActive;
            _ignoreExceptions = ignoreExceptions;
            _useOwnThread = useOwnThread;
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
            Ensure.ArgumentNotNull(action, "The action can't be null.");
            _precondition = precondition;
            _action = action;
            _func = null;
            _startActive = startActive;
            _ignoreExceptions = ignoreExceptions;
            _useOwnThread = useOwnThread;
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
            Ensure.ArgumentNotNull(function, "The func can't be null.");
            _precondition = null;
            _action = null;
            _func = function;
            _startActive = startActive;
            _ignoreExceptions = ignoreExceptions;
            _useOwnThread = useOwnThread;
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
            Ensure.ArgumentNotNull(function, "The func can't be null.");
            _precondition = precondition;
            _action = null;
            _func  = function;
            _startActive = startActive;
            _ignoreExceptions = ignoreExceptions;
            _useOwnThread = useOwnThread;
            Init();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Init()
        {
            _tokenSource = new CancellationTokenSource();
            CreateChannels();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CreateChannels()
        {
            var channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = false,
                SingleReader = true,
                SingleWriter = false
            });
            Interlocked.Exchange(ref _writer, channel.Writer);
            Interlocked.Exchange(ref _reader, channel.Reader);
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
            if (_status == WorkerStatus.Disposed || _status == WorkerStatus.Stopping) return false;
            var res = _writer.TryWrite(item);
            if (res)
            {
                Interlocked.Increment(ref _queueCount);
                if (_startActive && _status == WorkerStatus.Stopped)
                    Start();
            }
            return res;
        }
        /// <summary>
        /// Clears the queue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var prevStatus = _status;
            Stop(true);
            Interlocked.Exchange(ref _queueCount, 0);
            if (prevStatus == WorkerStatus.Started)
                Start();
        }
        #endregion

        #region Start / Stop Methods
        /// <summary>
        /// Starts the processing thread
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start()
        {
            if (Interlocked.CompareExchange(ref _workerStartStatus, 1, 0) == 1) return;
            if (_status != WorkerStatus.Stopped) return;
            if (_useOwnThread)
            {
                _processThread = new Thread(ProcessThread);
                _processThread.IsBackground = true;
                _processThread.Start();
            }
            else
            {
                _processTask = ProcessAsync();
            }
        }
        /// <summary>
        /// Stops the processing thread
        /// </summary>
        /// <param name="forceInmediate">Force inmediate end.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task StopAsync(bool forceInmediate = false)
        {
            if (Interlocked.CompareExchange(ref _workerStartStatus, 0, 1) == 0) return;
            if (_status != WorkerStatus.Started) return;
            _status = WorkerStatus.Stopping;
            _writer.TryComplete();
            if (forceInmediate)
                _tokenSource.Cancel();
            if (_useOwnThread)
            {
                await TaskHelper.SleepUntil(() => _processThreadResetEvent.IsSet, Core.GlobalSettings.WorkerWaitTimeout).ConfigureAwait(false);
                _processThreadResetEvent.Reset();
            }
            else
                await _processTask.ConfigureAwait(false);
            CreateChannels();
            Interlocked.Exchange(ref _queueCount, 0);
        }
        /// <summary>
        /// Stops the processing thread
        /// </summary>
        /// <param name="forceInmediate">Force inmediate end.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop(bool forceInmediate = false)
        {
            if (Interlocked.CompareExchange(ref _workerStartStatus, 0, 1) == 0) return;
            if (_status != WorkerStatus.Started) return;
            _status = WorkerStatus.Stopping;
            _writer.TryComplete();
            if (forceInmediate)
                _tokenSource.Cancel();
            if (_useOwnThread)
            {
                _processThreadResetEvent.Wait(Core.GlobalSettings.WorkerWaitTimeout);
                _processThreadResetEvent.Reset();
            }
            else
                _processTask.WaitAsync();
            CreateChannels();
            Interlocked.Exchange(ref _queueCount, 0);
        }
        #endregion

        #region Private Worker Methods
        [IgnoreStackFrameLog]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task ProcessAsync()
        {
            _status = WorkerStatus.Started;
            var isFunc = _func != null;
            var usePrecondition = _precondition != null;
            var token = _tokenSource.Token;
            var maxInCancellation = 50;
            try
            {
                while (await _reader.WaitToReadAsync(token))
                {
                    if (usePrecondition)
                    {
                        await TaskHelper.SleepUntil(_precondition, token).ConfigureAwait(false);
                        if (token.IsCancellationRequested) return;
                    }
                    while (_reader.TryRead(out var item))
                    {
                        Interlocked.Decrement(ref _queueCount);
                        try
                        {
                            if (isFunc)
                                await _func(item).ConfigureAwait(false);
                            else
                                _action(item);
                        }
                        catch (Exception ex)
                        {
                            if (!_ignoreExceptions && OnException != null)
                                OnException(this, (ex, item));
                        }
                        if (token.IsCancellationRequested)
                        {
                            if (maxInCancellation-- == 0)
                                break;
                        }
                    }
                    OnWorkDone?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (TaskCanceledException) { }
            _status = WorkerStatus.Stopped;
        }
        [IgnoreStackFrameLog]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessThread()
        {
            Thread.CurrentThread.Name = "Worker Thread";
            _status = WorkerStatus.Started;
            _processThreadResetEvent = new ManualResetEventSlim();
            var isFunc = _func != null;
            var usePrecondition = _precondition != null;
            var token = _tokenSource.Token;
            var maxInCancellation = 50;
            try
            {
                while (_reader.WaitToReadAsync(_tokenSource.Token).WaitAndResults())
                {
                    if (usePrecondition)
                    {
                        while (!token.IsCancellationRequested && !_precondition())
                            Thread.Sleep(250);
                        if (token.IsCancellationRequested) return;
                    }

                    while (_reader.TryRead(out var item))
                    {
                        Interlocked.Decrement(ref _queueCount);
                        try
                        {
                            if (isFunc)
                                _func(item).WaitAsync();
                            else
                                _action(item);
                        }
                        catch (Exception ex)
                        {
                            if (!_ignoreExceptions && OnException != null)
                                OnException(this, (ex, item));
                        }
                        if (token.IsCancellationRequested)
                        {
                            if (maxInCancellation-- == 0)
                                break;
                        }
                    }
                    OnWorkDone?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (TaskCanceledException) { }
            _status = WorkerStatus.Stopped;
            _processThreadResetEvent.Set();
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_status != WorkerStatus.Started) return;
            _status = WorkerStatus.Stopping;
            _writer.TryComplete();
            _tokenSource.Cancel();
            if (_useOwnThread)
                _processThreadResetEvent.Wait(Core.GlobalSettings.WorkerWaitTimeout);
            else
                _processTask.WaitAsync();
            _status = WorkerStatus.Disposed;
            Interlocked.Exchange(ref _queueCount, 0);
        }
    }
}
