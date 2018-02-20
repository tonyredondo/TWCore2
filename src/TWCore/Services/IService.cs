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

namespace TWCore.Services
{
    /// <summary>
    /// Define a Service
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Get if the service support pause and continue
        /// </summary>
        bool CanPauseAndContinue { get; }
        /// <summary>
        /// On Service Start method
        /// </summary>
        /// <param name="args">Start arguments</param>
        void OnStart(string[] args);
        /// <summary>
        /// On Service Stops method
        /// </summary>
        void OnStop();
        /// <summary>
        /// On Continue from pause method
        /// </summary>
        void OnContinue();
        /// <summary>
        /// On Pause method
        /// </summary>
        void OnPause();
        /// <summary>
        /// On shutdown requested method
        /// </summary>
        void OnShutdown();
    }
}
