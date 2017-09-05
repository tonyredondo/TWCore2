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

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TWCore.Services
{
    /// <summary>
    /// ParameterHandler Info Class
    /// </summary>
    public class ParameterHandlerInfo
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly ServiceContainer _container;

        #region Properties
        /// <summary>
        /// Arguments array
        /// </summary>
        public string[] Arguments { get; private set; }
        /// <summary>
        /// Value for parameter
        /// </summary>
        public string Value { get; private set; }
        /// <summary>
        /// Gets or Sets if after the handle the service should end the execution
        /// </summary>
        public bool ShouldEndExecution { get; set; } = true;
        /// <summary>
        /// Service Name
        /// </summary>
        public string ServiceName => _container.ServiceName;
        /// <summary>
        /// Service
        /// </summary>
        public IService Service
        {
            get => _container.Service;
            set => _container.Service = value;
        }
        #endregion

        #region .ctor
        /// <summary>
        /// ParameterHandler Info Class
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ParameterHandlerInfo(ServiceContainer container, string[] args, string value)
        {
            Arguments = args;
            Value = value;
            _container = container;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Show Basic Header
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ShowHeader() => _container.ShowHeader();
        /// <summary>
        /// Show Full Header
        /// </summary>
        /// <param name="showSettings">Show loaded settings</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ShowFullHeader(bool showSettings = true) => _container.ShowFullHeader(showSettings);
        /// <summary>
        /// Show Help
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ShowHelp() => _container.ShowHelp();
        /// <summary>
        /// Show Settings
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ShowSettings() => _container.ShowSettings();
        /// <summary>
        /// Show Assemblies Version
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ShowVersion() => _container.ShowVersion();
        /// <summary>
        /// Call the Init Action of the Container
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitContainer() => _container.InitAction?.Invoke();
        #endregion
    }
}
