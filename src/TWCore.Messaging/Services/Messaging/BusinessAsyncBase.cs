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

using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable CheckNamespace
// ReSharper disable NotAccessedField.Global

namespace TWCore.Services.Messaging
{
    /// <inheritdoc />
    /// <summary>
    /// Business Async base class
    /// </summary>
    public abstract class BusinessAsyncBase : IBusinessAsync
    {
        /// <summary>
        /// Timeout cancellation token
        /// </summary>
        protected CancellationToken TimeoutCancellationToken;

        #region IBusiness Methods
        /// <inheritdoc />
        /// <summary>
        /// Initialize business instance
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            OnInit();
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(object message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            OnDispose();
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// On Init business
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnInit() { }
        /// <summary>
        /// On Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(object message);
        /// <summary>
        /// On Dispose business
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnDispose() { }
        #endregion
    }

    /// <inheritdoc />
    /// <summary>
    /// Business Async base class
    /// </summary>
    /// <typeparam name="T">Message object type to process</typeparam>
    public abstract class BusinessAsyncBase<T> : IBusinessAsync<T>
    {
        /// <summary>
        /// Timeout cancellation token
        /// </summary>
        protected CancellationToken TimeoutCancellationToken;

        #region IBusiness Methods
        /// <inheritdoc />
        /// <summary>
        /// Initialize business instance
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            OnInit();
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(object message, CancellationToken cancellationToken)
        {
            return ProcessAsync((T)message, cancellationToken);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            OnDispose();
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// On Init business
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnInit() { }
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T message);
        /// <summary>
        /// On Dispose business
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnDispose() { }
        #endregion
    }

    /// <inheritdoc />
    /// <summary>
    /// Business Async base class
    /// </summary>
    /// <typeparam name="T1">Message object type 1 to process</typeparam>
    /// <typeparam name="T2">Message object type 2 to process</typeparam>
    public abstract class BusinessAsyncBase<T1, T2> : IBusinessAsync<T1, T2>
    {
        /// <summary>
        /// Timeout cancellation token
        /// </summary>
        protected CancellationToken TimeoutCancellationToken;

        #region IBusiness Methods
        /// <inheritdoc />
        /// <summary>
        /// Initialize business instance
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            OnInit();
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(object message, CancellationToken cancellationToken)
        {
            if (message is T1 mt1)
                return ProcessAsync(mt1, cancellationToken);
            return ProcessAsync((T2)message, cancellationToken);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T1 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T2 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            OnDispose();
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// On Init business
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnInit() { }
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T1 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T2 message);
        /// <summary>
        /// On Dispose business
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnDispose() { }
        #endregion
    }

    /// <inheritdoc />
    /// <summary>
    /// Business Async base class
    /// </summary>
    /// <typeparam name="T1">Message object type 1 to process</typeparam>
    /// <typeparam name="T2">Message object type 2 to process</typeparam>
    /// <typeparam name="T3">Message object type 3 to process</typeparam>
    public abstract class BusinessAsyncBase<T1, T2, T3> : IBusinessAsync<T1, T2, T3>
    {
        /// <summary>
        /// Timeout cancellation token
        /// </summary>
        protected CancellationToken TimeoutCancellationToken;

        #region IBusiness Methods
        /// <inheritdoc />
        /// <summary>
        /// Initialize business instance
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            OnInit();
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(object message, CancellationToken cancellationToken)
        {
            switch (message)
            {
                case T1 mt1:
                    return ProcessAsync(mt1, cancellationToken);
                case T2 mt2:
                    return ProcessAsync(mt2, cancellationToken);
            }
            return ProcessAsync((T3)message, cancellationToken);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T1 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T2 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T3 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            OnDispose();
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// On Init business
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnInit() { }
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T1 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T2 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T3 message);
        /// <summary>
        /// On Dispose business
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnDispose() { }
        #endregion
    }

    /// <inheritdoc />
    /// <summary>
    /// Business Async base class
    /// </summary>
    /// <typeparam name="T1">Message object type 1 to process</typeparam>
    /// <typeparam name="T2">Message object type 2 to process</typeparam>
    /// <typeparam name="T3">Message object type 3 to process</typeparam>
    /// <typeparam name="T4">Message object type 4 to process</typeparam>
    public abstract class BusinessAsyncBase<T1, T2, T3, T4> : IBusinessAsync<T1, T2, T3, T4>
    {
        /// <summary>
        /// Timeout cancellation token
        /// </summary>
        protected CancellationToken TimeoutCancellationToken;

        #region IBusiness Methods
        /// <inheritdoc />
        /// <summary>
        /// Initialize business instance
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            OnInit();
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(object message, CancellationToken cancellationToken)
        {
            switch (message)
            {
                case T1 mt1:
                    return ProcessAsync(mt1, cancellationToken);
                case T2 mt2:
                    return ProcessAsync(mt2, cancellationToken);
                case T3 mt3:
                    return ProcessAsync(mt3, cancellationToken);
            }
            return ProcessAsync((T4)message, cancellationToken);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T1 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T2 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T3 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T4 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            OnDispose();
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// On Init business
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnInit() { }
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T1 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T2 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T3 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T4 message);
        /// <summary>
        /// On Dispose business
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnDispose() { }
        #endregion
    }

    /// <inheritdoc />
    /// <summary>
    /// Business Async base class
    /// </summary>
    /// <typeparam name="T1">Message object type 1 to process</typeparam>
    /// <typeparam name="T2">Message object type 2 to process</typeparam>
    /// <typeparam name="T3">Message object type 3 to process</typeparam>
    /// <typeparam name="T4">Message object type 4 to process</typeparam>
    /// <typeparam name="T5">Message object type 5 to process</typeparam>
    public abstract class BusinessAsyncBase<T1, T2, T3, T4, T5> : IBusinessAsync<T1, T2, T3, T4, T5>
    {
        /// <summary>
        /// Timeout cancellation token
        /// </summary>
        protected CancellationToken TimeoutCancellationToken;

        #region IBusiness Methods
        /// <inheritdoc />
        /// <summary>
        /// Initialize business instance
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            OnInit();
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(object message, CancellationToken cancellationToken)
        {
            switch (message)
            {
                case T1 mt1:
                    return ProcessAsync(mt1, cancellationToken);
                case T2 mt2:
                    return ProcessAsync(mt2, cancellationToken);
                case T3 mt3:
                    return ProcessAsync(mt3, cancellationToken);
                case T4 mt4:
                    return ProcessAsync(mt4, cancellationToken);
            }
            return ProcessAsync((T5)message, cancellationToken);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T1 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T2 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T3 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T4 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T5 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            OnDispose();
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// On Init business
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnInit() { }
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T1 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T2 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T3 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T4 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T5 message);
        /// <summary>
        /// On Dispose business
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnDispose() { }
        #endregion
    }
    
    /// <inheritdoc />
    /// <summary>
    /// Business Async base class
    /// </summary>
    /// <typeparam name="T1">Message object type 1 to process</typeparam>
    /// <typeparam name="T2">Message object type 2 to process</typeparam>
    /// <typeparam name="T3">Message object type 3 to process</typeparam>
    /// <typeparam name="T4">Message object type 4 to process</typeparam>
    /// <typeparam name="T5">Message object type 5 to process</typeparam>
    /// <typeparam name="T6">Message object type 6 to process</typeparam>
    public abstract class BusinessAsyncBase<T1, T2, T3, T4, T5, T6> : IBusinessAsync<T1, T2, T3, T4, T5, T6>
    {
        /// <summary>
        /// Timeout cancellation token
        /// </summary>
        protected CancellationToken TimeoutCancellationToken;

        #region IBusiness Methods
        /// <inheritdoc />
        /// <summary>
        /// Initialize business instance
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            OnInit();
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(object message, CancellationToken cancellationToken)
        {
            switch (message)
            {
                case T1 mt1:
                    return ProcessAsync(mt1, cancellationToken);
                case T2 mt2:
                    return ProcessAsync(mt2, cancellationToken);
                case T3 mt3:
                    return ProcessAsync(mt3, cancellationToken);
                case T4 mt4:
                    return ProcessAsync(mt4, cancellationToken);
                case T5 mt5:
                    return ProcessAsync(mt5, cancellationToken);
            }
            return ProcessAsync((T6)message, cancellationToken);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T1 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T2 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T3 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T4 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T5 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T6 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            OnDispose();
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// On Init business
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnInit() { }
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T1 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T2 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T3 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T4 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T5 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T6 message);
        /// <summary>
        /// On Dispose business
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnDispose() { }
        #endregion
    }
    
    /// <inheritdoc />
    /// <summary>
    /// Business Async base class
    /// </summary>
    /// <typeparam name="T1">Message object type 1 to process</typeparam>
    /// <typeparam name="T2">Message object type 2 to process</typeparam>
    /// <typeparam name="T3">Message object type 3 to process</typeparam>
    /// <typeparam name="T4">Message object type 4 to process</typeparam>
    /// <typeparam name="T5">Message object type 5 to process</typeparam>
    /// <typeparam name="T6">Message object type 6 to process</typeparam>
    /// <typeparam name="T7">Message object type 7 to process</typeparam>
    public abstract class BusinessAsyncBase<T1, T2, T3, T4, T5, T6, T7> : IBusinessAsync<T1, T2, T3, T4, T5, T6, T7>
    {
        /// <summary>
        /// Timeout cancellation token
        /// </summary>
        protected CancellationToken TimeoutCancellationToken;

        #region IBusiness Methods
        /// <inheritdoc />
        /// <summary>
        /// Initialize business instance
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            OnInit();
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(object message, CancellationToken cancellationToken)
        {
            switch (message)
            {
                case T1 mt1:
                    return ProcessAsync(mt1, cancellationToken);
                case T2 mt2:
                    return ProcessAsync(mt2, cancellationToken);
                case T3 mt3:
                    return ProcessAsync(mt3, cancellationToken);
                case T4 mt4:
                    return ProcessAsync(mt4, cancellationToken);
                case T5 mt5:
                    return ProcessAsync(mt5, cancellationToken);
                case T6 mt6:
                    return ProcessAsync(mt6, cancellationToken);
            }
            return ProcessAsync((T7)message, cancellationToken);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T1 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T2 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T3 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T4 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T5 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T6 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T7 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            OnDispose();
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// On Init business
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnInit() { }
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T1 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T2 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T3 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T4 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T5 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T6 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T7 message);
        /// <summary>
        /// On Dispose business
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnDispose() { }
        #endregion
    }
    
    /// <inheritdoc />
    /// <summary>
    /// Business Async base class
    /// </summary>
    /// <typeparam name="T1">Message object type 1 to process</typeparam>
    /// <typeparam name="T2">Message object type 2 to process</typeparam>
    /// <typeparam name="T3">Message object type 3 to process</typeparam>
    /// <typeparam name="T4">Message object type 4 to process</typeparam>
    /// <typeparam name="T5">Message object type 5 to process</typeparam>
    /// <typeparam name="T6">Message object type 6 to process</typeparam>
    /// <typeparam name="T7">Message object type 7 to process</typeparam>
    /// <typeparam name="T8">Message object type 8 to process</typeparam>
    public abstract class BusinessAsyncBase<T1, T2, T3, T4, T5, T6, T7, T8> : IBusinessAsync<T1, T2, T3, T4, T5, T6, T7, T8>
    {
        /// <summary>
        /// Timeout cancellation token
        /// </summary>
        protected CancellationToken TimeoutCancellationToken;

        #region IBusiness Methods
        /// <inheritdoc />
        /// <summary>
        /// Initialize business instance
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            OnInit();
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(object message, CancellationToken cancellationToken)
        {
            switch (message)
            {
                case T1 mt1:
                    return ProcessAsync(mt1, cancellationToken);
                case T2 mt2:
                    return ProcessAsync(mt2, cancellationToken);
                case T3 mt3:
                    return ProcessAsync(mt3, cancellationToken);
                case T4 mt4:
                    return ProcessAsync(mt4, cancellationToken);
                case T5 mt5:
                    return ProcessAsync(mt5, cancellationToken);
                case T6 mt6:
                    return ProcessAsync(mt6, cancellationToken);
                case T7 mt7:
                    return ProcessAsync(mt7, cancellationToken);
            }
            return ProcessAsync((T8)message, cancellationToken);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T1 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T2 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T3 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T4 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T5 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T6 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T7 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<object> ProcessAsync(T8 message, CancellationToken cancellationToken)
        {
            TimeoutCancellationToken = cancellationToken;
            return OnProcessAsync(message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            OnDispose();
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// On Init business
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnInit() { }
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T1 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T2 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T3 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T4 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T5 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T6 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T7 message);
        /// <summary>
        /// On Process Async message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns>Process result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<object> OnProcessAsync(T8 message);
        /// <summary>
        /// On Dispose business
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnDispose() { }
        #endregion
    }
}
