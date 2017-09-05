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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TWCore.Services
{
    /// <inheritdoc />
    /// <summary>
    /// Simple service using a task
    /// </summary>
    public class TaskService : IService
    {
        private readonly Func<CancellationToken, Task> _creationFunction;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        private Task _task;

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Get if the service support pause and continue
        /// </summary>
        public bool CanPauseAndContinue { get; } = false;
        /// <summary>
        /// Get if the service should end after task finish
        /// </summary>
        public bool EndAfterTaskFinish { get; set; } = false;
        #endregion

        #region .ctor
        /// <summary>
        /// Simple service using a task
        /// </summary>
        /// <param name="taskCreationFunction">Task creation function with a generated cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TaskService(Func<CancellationToken, Task> taskCreationFunction)
        {
            _creationFunction = taskCreationFunction ?? throw new ArgumentNullException(nameof(taskCreationFunction), "The task creation function for the simple service can't be null.");
            Core.RunOnInit(() => Core.Status.AttachObject(this));
        }
        /// <summary>
        /// Simple service using a task
        /// </summary>
        /// <param name="actionFunction">Task creation function with a generated cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TaskService(Action<CancellationToken> actionFunction)
        {
            if (actionFunction == null) throw new ArgumentNullException(nameof(actionFunction), "The task creation function for the simple service can't be null.");
            _creationFunction = token => new Task(() => actionFunction(token), token);
            Core.RunOnInit(() => Core.Status.AttachObject(this));
        }
        /// <summary>
        /// Simple service using a task
        /// </summary>
        /// <param name="actionFunction">Task creation function with a generated cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TaskService(Action actionFunction)
        {
            if (actionFunction == null) throw new ArgumentNullException(nameof(actionFunction), "The task creation function for the simple service can't be null.");
            _creationFunction = token => new Task(actionFunction, token);
            Core.RunOnInit(() => Core.Status.AttachObject(this));
        }
        #endregion

        #region IService Methods
        /// <inheritdoc />
        /// <summary>
        /// On Service Start method
        /// </summary>
        /// <param name="args">Start arguments</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnStart(string[] args)
        {
            try
            {
                Core.Log.InfoBasic("Starting service");
                _tokenSource = new CancellationTokenSource();
                _token = _tokenSource.Token;
                _task = _creationFunction(_token);
                if (_task == null)
                    throw new NullReferenceException("The returned task from the Task creation function can't be null");
                _task.ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        Core.Log.Write(t.Exception);
                        ServiceContainer.ServiceExit();
                    }
                    if (EndAfterTaskFinish)
                        ServiceContainer.ServiceExit();
                }, TaskContinuationOptions.ExecuteSynchronously);
                if (!_task.IsCompleted && _task.Status != TaskStatus.Running && _task.Status != TaskStatus.WaitingForActivation && _task.Status != TaskStatus.WaitingForChildrenToComplete && _task.Status != TaskStatus.WaitingToRun)
                    _task.Start();
                Core.Log.InfoBasic("Service started");
                if (EndAfterTaskFinish)
                    _task.WaitAsync();
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                throw;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// On Service Stops method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnStop()
        {
            try
            {
                Core.Log.InfoBasic("Stopping service");
                if (!_task.IsCompleted || _task.Status == TaskStatus.RanToCompletion || _task.Status == TaskStatus.Running)
                    _tokenSource.Cancel();
                _task.Wait(10000);
                Core.Log.InfoBasic("Service stopped");
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                //throw;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// On shutdown requested method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnShutdown()
        {
            OnStop();
        }
        /// <inheritdoc />
        /// <summary>
        /// On Continue from pause method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnContinue()
        {
        }
        /// <inheritdoc />
        /// <summary>
        /// On Pause method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnPause()
        {
        }
        #endregion
    }
}
