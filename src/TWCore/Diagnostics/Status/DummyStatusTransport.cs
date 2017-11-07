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

using System.Runtime.CompilerServices;

namespace TWCore.Diagnostics.Status
{
    /// <inheritdoc />
    /// <summary>
    /// Dummy status transport
    /// </summary>
    public class DummyStatusTransport : IStatusTransport
    {
        /// <summary>
        /// Handles when a fetch status event has been received.
        /// </summary>
        public event FetchStatusDelegate OnFetchStatus;

        /// <summary>
        /// Get the current status
        /// </summary>
        /// <returns>Status Item Collection</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatusItemCollection GetStatus()
            => OnFetchStatus?.Invoke();

        /// <inheritdoc />
        /// <summary>
        /// Dispose instance
        /// </summary>
        public void Dispose()
        {
        }
    }
}
