using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Threading;

namespace TWCore
{
    /// <summary>
    /// Instance Lock helps to create locks object based on instance values
    /// </summary>
    /// <typeparam name="T">Type of the instance locks</typeparam>
    public class InstanceLockerAsync<T>
    {
        private readonly Lazy<AsyncLock>[] _lockers;

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Instance Lock helps to create locks object based on instance values
        /// </summary>
        public InstanceLockerAsync() : this(1024)
        {
        }
        /// <summary>
        /// Instance Lock helps to create locks object based on instance values
        /// </summary>
        /// <param name="concurrencyLevel">Number of locks for all instance values</param>
        public InstanceLockerAsync(int concurrencyLevel)
        {
            _lockers = new Lazy<AsyncLock>[concurrencyLevel];
            for (var i = 0; i < concurrencyLevel; i++)
                _lockers[i] = new Lazy<AsyncLock>();
        }
        #endregion

        /// <summary>
        /// Get a Lock for use with lock(){} block
        /// </summary>
        /// <param name="key">Key to make a lock</param>
        /// <returns>Object for lock use</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncLock GetLockAsync(T key)
        {
            var idx = Math.Abs(key?.GetHashCode() ?? 0) % _lockers.Length;
            return _lockers[idx].Value;
        }

        /// <summary>
        /// Run a short lock inline using a lambda
        /// </summary>
        /// <param name="key">Key to make a lock</param>
        /// <param name="body">Function to be executed in lock</param>
        /// <returns>Return value of the function</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TResult> RunWithLockAsync<TResult>(T key, Func<TResult> body)
        {
            TResult res;
            var idx = Math.Abs(key?.GetHashCode() ?? 0) % _lockers.Length;
            using(await _lockers[idx].Value.LockAsync().ConfigureAwait(false))
                res = body();
            return res;
        }

        /// <summary>
        /// Run a short lock inline using a lambda
        /// </summary>
        /// <param name="key">Key to make a lock</param>
        /// <param name="body">Action to be executed in lock</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task RunWithLockAsync(T key, Action body)
        {
            var idx = Math.Abs(key?.GetHashCode() ?? 0) % _lockers.Length;
            using(await _lockers[idx].Value.LockAsync().ConfigureAwait(false))
                body();
        }
    }
}