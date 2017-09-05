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

using System.Threading;

// ReSharper disable CheckNamespace

namespace TWCore.Services.Messaging
{
    /// <inheritdoc />
    /// <summary>
    /// Business base class
    /// </summary>
    /// <typeparam name="T">Message object type to process</typeparam>
    public abstract class BusinessBase<T> : IBusiness<T>
    {
        protected CancellationToken TimeoutCancellationToken;

        #region IBusiness Methods
        /// <summary>
        /// Initialize business instance
        /// </summary>
        public void Init()
        {
            OnInit();
        }
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        public object Process(object message, CancellationToken cancellationToken)
        {
            return Process((T)message, cancellationToken);
        }
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        public object Process(T message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcess(message);
        }
        /// <summary>
        /// Dispose all resources
        /// </summary>
        public void Dispose()
        {
            OnDispose();
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// On Init business
        /// </summary>
        protected virtual void OnInit() { }
        /// <summary>
        /// On Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        protected abstract object OnProcess(T message);
        /// <summary>
        /// On Dispose business
        /// </summary>
        protected virtual void OnDispose() { }
        #endregion
    }

    /// <inheritdoc />
    /// <summary>
    /// Business base class
    /// </summary>
    public abstract class BusinessBase : IBusiness
    {
        protected CancellationToken TimeoutCancellationToken;

        #region IBusiness Methods
        /// <summary>
        /// Initialize business instance
        /// </summary>
        public void Init()
        {
            OnInit();
        }
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        public object Process(object message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcess(message);
        }
        /// <summary>
        /// Dispose all resources
        /// </summary>
        public void Dispose()
        {
            OnDispose();
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// On Init business
        /// </summary>
        protected virtual void OnInit() { }
        /// <summary>
        /// On Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        protected abstract object OnProcess(object message);
        /// <summary>
        /// On Dispose business
        /// </summary>
        protected virtual void OnDispose() { }
        #endregion
    }
}
