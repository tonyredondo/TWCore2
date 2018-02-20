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
using TWCore.Triggers;
// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

namespace TWCore.Services
{
    /// <inheritdoc />
    /// <summary>
    /// Scheduler service
    /// </summary>
    public abstract class SchedulerService : ISchedulerService
    {
        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Get if the service support pause and continue
        /// </summary>
        public bool CanPauseAndContinue => true;
        /// <inheritdoc />
        /// <summary>
        /// Triggered actions list
        /// </summary>
        public List<TriggeredAction> TriggeredActions { get; private set; }
        #endregion

        #region Public Methods
        /// <inheritdoc />
        /// <summary>
        /// On Continue from pause method
        /// </summary>
        public void OnContinue() => OnStart(null);

        /// <inheritdoc />
        /// <summary>
        /// On Pause method
        /// </summary>
        public void OnPause() => OnStop();

        /// <inheritdoc />
        /// <summary>
        /// On shutdown requested method
        /// </summary>
        public void OnShutdown() => OnStop();

        /// <inheritdoc />
        /// <summary>
        /// On Service Start method
        /// </summary>
        /// <param name="args">Start arguments</param>
        public void OnStart(string[] args)
        {
            try
            {
                OnInit(args);
                var triggeredActions = GetTriggeredActions();
                if (triggeredActions == null)
                    throw new NullReferenceException("Check your GetTriggeredActions method implementation, the return value can't be null.");
                TriggeredActions = new List<TriggeredAction>(triggeredActions);
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
        public void OnStop()
        {
            try
            {
                OnFinalizing();
                foreach (var act in TriggeredActions)
                    act.Dispose();
                TriggeredActions.Clear();
                OnDispose();
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// On Service Init
        /// </summary>
        /// <param name="args">Service arguments</param>
        protected abstract void OnInit(string[] args);
        /// <summary>
        /// On Service Stop
        /// </summary>
        protected virtual void OnFinalizing() { }
        /// <summary>
        /// On Service Dispose
        /// </summary>
        protected virtual void OnDispose() { }
        /// <summary>
        /// Get triggered actions
        /// </summary>
        /// <returns>IEnumerable of TriggeredAction</returns>
        protected abstract IEnumerable<TriggeredAction> GetTriggeredActions();
        #endregion
    }
}
