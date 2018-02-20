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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace TWCore
{
    /// <inheritdoc />
    /// <summary>
    /// Worker where the Queue elements are actions to be executed in order
    /// </summary>
    public class ActionWorker : Worker<ActionWorker.WorkerItem>
    {
        #region .ctors
        /// <inheritdoc />
        /// <summary>
        /// Worker where all elements are actions to be executed in order
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ActionWorker() : base(DoActionAsync) { }
        /// <inheritdoc />
        /// <summary>
        /// Worker where all elements are actions to be executed in order
        /// </summary>
        /// <param name="precondition">Precondition to accomplish before dequeuing an element from the queue</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ActionWorker(Func<bool> precondition) : base(precondition, DoActionAsync) { }
        #endregion

        #region Static Methods
        /// <summary>
        /// Perform worker item action
        /// </summary>
        /// <param name="item">WorkerItem instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static async Task DoActionAsync(WorkerItem item)
        {
            try
            {
                await item.Function(item.State).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                if (item.OnExceptionCallback != null)
                    item.OnExceptionCallback(ex);
                else
                    Core.Log.Write(ex);
                throw;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Enqueue a new Action on the queue
        /// </summary>
        /// <param name="action">Action to be executed</param>
        /// <param name="onExceptionCallback">Action executed in case of an Exception</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(Action action, Action<Exception> onExceptionCallback = null) => Enqueue(new WorkerItem(o => { action(); return Task.CompletedTask; }, null, onExceptionCallback));
        /// <summary>
        /// Enqueue a new Action on the queue
        /// </summary>
        /// <param name="action">Action to be executed</param>
        /// <param name="state">State object to pass to the action</param>
        /// <param name="onExceptionCallback">Action executed in case of an Exception</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue<T>(Action<T> action, T state, Action<Exception> onExceptionCallback = null) => Enqueue(new WorkerItem(o => { action((T)o); return Task.CompletedTask; }, state, onExceptionCallback));

        /// <summary>
        /// Enqueue a new Action on the queue
        /// </summary>
        /// <param name="function">Function to be executed</param>
        /// <param name="onExceptionCallback">Action executed in case of an Exception</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(Func<Task> function, Action<Exception> onExceptionCallback = null) => Enqueue(new WorkerItem(o => function(), null, onExceptionCallback));
        /// <summary>
        /// Enqueue a new Action on the queue
        /// </summary>
        /// <param name="function">Function to be executed</param>
        /// <param name="state">State object to pass to the action</param>
        /// <param name="onExceptionCallback">Action executed in case of an Exception</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue<T>(Func<T, Task> function, T state, Action<Exception> onExceptionCallback = null) => Enqueue(new WorkerItem(o => function((T)o), state, onExceptionCallback));
        #endregion

        #region Nested classes
        /// <summary>
        /// Action Worker queue item where is stored the action to be executed and the action when an exception occurs
        /// </summary>
        public struct WorkerItem
        {
            /// <summary>
            /// Function to be executed
            /// </summary>
            public Func<object, Task> Function { get; }
            /// <summary>
            /// Saves the state to be used by the action
            /// </summary>
            public object State { get; }
            /// <summary>
            /// Action executed in case of an Exception
            /// </summary>
            public Action<Exception> OnExceptionCallback { get; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal WorkerItem(Func<object, Task> function, object state, Action<Exception> onExceptionCallback)
            {
                Function = function;
                State = state;
                OnExceptionCallback = onExceptionCallback;
            }
        }
        #endregion
    }

    /// <inheritdoc />
    /// <summary>
    /// Worker where the Queue elements are actions to be executed in order
    /// </summary>
    public class ActionWorker<T> : Worker<ActionWorker<T>.WorkerItem>
    {
        #region .ctors
        /// <inheritdoc />
        /// <summary>
        /// Worker where all elements are actions to be executed in order
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ActionWorker() : base(DoActionAsync) { }
        /// <inheritdoc />
        /// <summary>
        /// Worker where all elements are actions to be executed in order
        /// </summary>
        /// <param name="precondition">Precondition to accomplish before dequeuing an element from the queue</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ActionWorker(Func<bool> precondition) : base(precondition, DoActionAsync) { }
        #endregion

        #region Static Methods
        /// <summary>
        /// Perform worker item action
        /// </summary>
        /// <param name="item">WorkerItem instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static async Task DoActionAsync(WorkerItem item)
        {
            try
            {
                await item.Function(item.State).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (item.OnExceptionCallback != null)
                    item.OnExceptionCallback(ex);
                else
                    Core.Log.Write(ex);
                throw;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Enqueue a new Action on the queue
        /// </summary>
        /// <param name="action">Action to be executed</param>
        /// <param name="state">State object to pass to the action</param>
        /// <param name="onExceptionCallback">Action executed in case of an Exception</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(Action<T> action, T state, Action<Exception> onExceptionCallback = null) => Enqueue(new WorkerItem(arg => { action(arg); return Task.CompletedTask; }, state, onExceptionCallback));
        /// <summary>
        /// Enqueue a new Function on the queue
        /// </summary>
        /// <param name="function">Function to be executed</param>
        /// <param name="state">State object to pass to the action</param>
        /// <param name="onExceptionCallback">Action executed in case of an Exception</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(Func<T, Task> function, T state, Action<Exception> onExceptionCallback = null) => Enqueue(new WorkerItem(function, state, onExceptionCallback));

        #endregion

        #region Nested classes
        /// <summary>
        /// Action Worker queue item where is stored the action to be executed and the action when an exception occurs
        /// </summary>
        public struct WorkerItem
        {
            /// <summary>
            /// Function to be executed
            /// </summary>
            public Func<T, Task> Function { get; }
            /// <summary>
            /// Saves the state to be used by the action
            /// </summary>
            public T State { get; }
            /// <summary>
            /// Action executed in case of an Exception
            /// </summary>
            public Action<Exception> OnExceptionCallback { get; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal WorkerItem(Func<T, Task> function, T state, Action<Exception> onExceptionCallback)
            {
                Function = function;
                State = state;
                OnExceptionCallback = onExceptionCallback;
            }
        }
        #endregion
    }
}
