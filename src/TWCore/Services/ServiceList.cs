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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TWCore.Services
{
    /// <inheritdoc cref="IService" />
    /// <summary>
    /// Collection of IService to support multiple services on the same container
    /// </summary>
    public class ServiceList : Collection<IService>, IService
    {
        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Collection of IService to support multiple services on the same container
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceList() { }
        /// <inheritdoc />
        /// <summary>
        /// Collection of IService to support multiple services on the same container
        /// </summary>
        /// <param name="col">Collection of services</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceList(IList<IService> col) : base(col) { }
        /// <inheritdoc />
        /// <summary>
        /// Collection of IService to support multiple services on the same container
        /// </summary>
        /// <param name="services">Collection of services</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceList(params IService[] services) : base(services) { }
        #endregion

        #region IService Implementation
        /// <inheritdoc />
        /// <summary>
        /// Get if the service support pause and continue
        /// </summary>
        public bool CanPauseAndContinue => this.All(i => i.CanPauseAndContinue);
        /// <inheritdoc />
        /// <summary>
        /// On Continue from pause method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnContinue() => this.Each(i => i.OnContinue());
        /// <inheritdoc />
        /// <summary>
        /// On Pause method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnPause() => this.Reverse().Each(i => i.OnPause());
        /// <inheritdoc />
        /// <summary>
        /// On shutdown requested method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnShutdown() => this.Reverse().Each(i => i.OnShutdown());
        /// <inheritdoc />
        /// <summary>
        /// On Service Start method
        /// </summary>
        /// <param name="args">Start arguments</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnStart(string[] args) => this.Each(i => i.OnStart(args));
        /// <inheritdoc />
        /// <summary>
        /// On Service Stops method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnStop() => this.Reverse().Each(i => i.OnStop());
        #endregion
    }
}
