﻿/*
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
using System.Threading.Tasks;

namespace TWCore.Services
{
    /// <inheritdoc />
    /// <summary>
    /// Async Simple Service Base
    /// </summary>
    public abstract class SimpleServiceAsync : IService
    {
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
        /// <summary>
        /// Gets the service start arguments
        /// </summary>
        protected string[] StartArguments { get; private set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Async Simple Service Base
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected SimpleServiceAsync()
        {
            Core.RunOnInit(() => Core.Status.AttachObject(this));
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// On Action async method.
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Action task</returns>
        protected abstract Task OnActionAsync(CancellationToken token);
        #endregion

        #region Private Methods
        private async Task ActionTask(CancellationToken token)
        {
            try
            {
                await OnActionAsync(token).ConfigureAwait(false);
                if (EndAfterTaskFinish)
                    ServiceContainer.ServiceExit();
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                ServiceContainer.ServiceExit();
            }
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
                StartArguments = args;
                _tokenSource = new CancellationTokenSource();
                _token = _tokenSource.Token;
                _task = ActionTask(_token);
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
                _tokenSource.Cancel();
                if (!_task.IsCompleted || _task.Status == TaskStatus.RanToCompletion || _task.Status == TaskStatus.Running)
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
