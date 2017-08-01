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
    /// <summary>
    /// Simple service using a task
    /// </summary>
    public class TaskService : IService
    {
        CancellationTokenSource tokenSource;
        CancellationToken token;
        Task task;
        Func<CancellationToken, Task> creationFunction;

        #region Properties
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
            creationFunction = taskCreationFunction ?? throw new ArgumentNullException(nameof(taskCreationFunction), "The task creation function for the simple service can't be null.");
            Core.RunOnInit(() => Core.Status.AttachObject(this));
        }
        /// <summary>
        /// Simple service using a task
        /// </summary>
        /// <param name="actionFunction">Task creation function with a generated cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TaskService(Action<CancellationToken> actionFunction)
        {
            creationFunction = token => new Task(() => actionFunction(token), token) ?? throw new ArgumentNullException(nameof(actionFunction), "The task creation function for the simple service can't be null.");
            Core.RunOnInit(() => Core.Status.AttachObject(this));
        }
        /// <summary>
        /// Simple service using a task
        /// </summary>
        /// <param name="actionFunction">Task creation function with a generated cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TaskService(Action actionFunction)
        {
            creationFunction = token => new Task(actionFunction, token) ?? throw new ArgumentNullException(nameof(actionFunction), "The task creation function for the simple service can't be null.");
            Core.RunOnInit(() => Core.Status.AttachObject(this));
        }
        #endregion

        #region IService Methods
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
                tokenSource = new CancellationTokenSource();
                token = tokenSource.Token;
                task = creationFunction(token);
                if (task == null)
                    throw new NullReferenceException("The returned task from the Task creation function can't be null");
                task.ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        Core.Log.Write(t.Exception);
                        ServiceContainer.ServiceExit();
                    }
                    if (EndAfterTaskFinish)
                        ServiceContainer.ServiceExit();
                }, TaskContinuationOptions.ExecuteSynchronously);
                if (!task.IsCompleted && task.Status != TaskStatus.Running && task.Status != TaskStatus.WaitingForActivation && task.Status != TaskStatus.WaitingForChildrenToComplete && task.Status != TaskStatus.WaitingToRun)
                    task.Start();
                Core.Log.InfoBasic("Service started");
                if (EndAfterTaskFinish)
                    task.WaitAsync();
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                throw;
            }
        }
        /// <summary>
        /// On Service Stops method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnStop()
        {
            try
            {
                Core.Log.InfoBasic("Stopping service");
                if (!task.IsCompleted || task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Running)
                    tokenSource.Cancel();
                task.Wait(10000);
                Core.Log.InfoBasic("Service stopped");
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                //throw;
            }
        }
        /// <summary>
        /// On shutdown requested method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnShutdown()
        {
            OnStop();
        }
        /// <summary>
        /// On Continue from pause method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnContinue()
        {
        }
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
