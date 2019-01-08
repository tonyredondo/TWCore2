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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable TailRecursiveCall
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMethodReturnValue.Global

// ReSharper disable CheckNamespace

namespace TWCore
{
    /// <summary>
    /// Action for object ref delegate
    /// </summary>
    /// <param name="obj">Object reference</param>
    /// <typeparam name="T">Object type</typeparam>
    public delegate void ActionRef<T>(ref T obj);
    /// <summary>
    /// Action for object ref delegate
    /// </summary>
    /// <param name="obj">Object reference</param>
    /// <param name="arg1">Argument 1</param>
    /// <typeparam name="T">Object type</typeparam>
    /// <typeparam name="T1">Object argument 1</typeparam>
    public delegate void ActionRef<T, T1>(ref T obj, in T1 arg1);
    /// <summary>
    /// Action for object ref delegate
    /// </summary>
    /// <param name="obj">Object reference</param>
    /// <param name="arg1">Argument 1</param>
    /// <param name="arg2">Argument 2</param>
    /// <typeparam name="T">Object type</typeparam>
    /// <typeparam name="T1">Object argument 1</typeparam>
    /// <typeparam name="T2">Object argument 2</typeparam>
    public delegate void ActionRef<T, T1, T2>(ref T obj, in T1 arg1, in T2 arg2);

    /// <summary>
    /// Extension for IEnumerables interface
    /// </summary>
    public static partial class Extensions
    {
        #region Each Loops

        #region IEnumerable
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable EachObject(this IEnumerable enumerable, Action<object> action)
        {
            if (enumerable is null || action is null) return enumerable;
            var eachObject = enumerable as IList<object> ?? enumerable.Cast<object>().ToList();
            var length = eachObject.Count;
            for (var i = 0; i < length; i++) action(eachObject[i]);
            return eachObject;
        }
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <param name="token">Cancellation token in case the current execution thread is cancelled</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable EachObject(this IEnumerable enumerable, Action<object> action, CancellationToken token)
        {
            if (enumerable is null || action is null || token.IsCancellationRequested) return enumerable;
            var eachObject = enumerable as IList<object> ?? enumerable.Cast<object>().ToList();
            var length = eachObject.Count;
            for (var i = 0; i < length; i++)
            {
                if (token.IsCancellationRequested)
                    break;
                action(eachObject[i]);
            }
            return eachObject;
        }
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable EachObject(this IEnumerable enumerable, Action<object, int> action)
        {
            if (enumerable is null || action is null) return enumerable;
            var eachObject = enumerable as IList<object> ?? enumerable.Cast<object>().ToList();
            var length = eachObject.Count;
            for (var i = 0; i < length; i++) action(eachObject[i], i);
            return eachObject;
        }
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <param name="token">Cancellation token in case the current execution thread is cancelled</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable EachObject(this IEnumerable enumerable, Action<object, int> action, CancellationToken token)
        {
            if (enumerable is null || action is null || token.IsCancellationRequested) return enumerable;
            var eachObject = enumerable as IList<object> ?? enumerable.Cast<object>().ToList();
            var length = eachObject.Count;
            for (var i = 0; i < length; i++)
            {
                if (token.IsCancellationRequested)
                    break;
                action(eachObject[i], i);
            }
            return eachObject;
        }
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <param name="state">Object to pass to the Action on each element of the IEnumerable</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable EachObject<TArg>(this IEnumerable enumerable, Action<object, int, TArg> action, in TArg state)
        {
            if (enumerable is null || action is null) return enumerable;
            var eachObject = enumerable as IList<object> ?? enumerable.Cast<object>().ToList();
            var length = eachObject.Count;
            for (var i = 0; i < length; i++) action(eachObject[i], i, state);
            return eachObject;
        }
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <param name="state">Object to pass to the Action on each element of the IEnumerable</param>
        /// <param name="token">Cancellation token in case the current execution thread is cancelled</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable EachObject<TArg>(this IEnumerable enumerable, Action<object, int, TArg> action, in TArg state, CancellationToken token)
        {
            if (enumerable is null || action is null || token.IsCancellationRequested) return enumerable;
            var eachObject = enumerable as IList<object> ?? enumerable.Cast<object>().ToList();
            var length = eachObject.Count;
            for (var i = 0; i < length; i++)
            {
                if (token.IsCancellationRequested)
                    break;
                action(eachObject[i], i, state);
            }
            return eachObject;
        }
        #endregion

        #region IEnumerable<T>		
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> Each<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable is null || action is null) return enumerable;
            var objs = enumerable as IList<T> ?? enumerable.ToList();
            var length = objs.Count;
            for (var i = 0; i < length; i++)
                action(objs[i]);
            return objs;
        }
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <param name="token">Cancellation token in case the current execution thread is cancelled</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> Each<T>(this IEnumerable<T> enumerable, Action<T> action, CancellationToken token)
        {
            if (enumerable is null || action is null || token.IsCancellationRequested) return enumerable;
            var objs = enumerable as IList<T> ?? enumerable.ToList();
            var length = objs.Count;
            for (var i = 0; i < length; i++)
            {
                if (token.IsCancellationRequested)
                    break;
                action(objs[i]);
            }
            return objs;
        }
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> Each<T>(this IEnumerable<T> enumerable, Action<T, int> action)
        {
            if (enumerable is null || action is null) return enumerable;
            var objs = enumerable as IList<T> ?? enumerable.ToList();
            var length = objs.Count;
            for (var i = 0; i < length; i++)
                action(objs[i], i);
            return objs;
        }
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <param name="token">Cancellation token in case the current execution thread is cancelled</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> Each<T>(this IEnumerable<T> enumerable, Action<T, int> action, CancellationToken token)
        {
            if (enumerable is null || action is null || token.IsCancellationRequested) return enumerable;
            var objs = enumerable as IList<T> ?? enumerable.ToList();
            var length = objs.Count;
            for (var i = 0; i < length; i++)
            {
                if (token.IsCancellationRequested)
                    break;
                action(objs[i], i);
            }
            return objs;
        }
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <param name="state">Object to pass to the Action on each element of the IEnumerable</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> Each<T, TArg>(this IEnumerable<T> enumerable, Action<T, int, TArg> action, in TArg state)
        {
            if (enumerable is null || action is null) return enumerable;
            var innerObjs = enumerable as IList<T> ?? enumerable.ToList();
            var length = innerObjs.Count;
            for (var i = 0; i < length; i++)
                action(innerObjs[i], i, state);
            return innerObjs;
        }
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <param name="state">Object to pass to the Action on each element of the IEnumerable</param>
        /// <param name="token">Cancellation token in case the current execution thread is cancelled</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> Each<T, TArg>(this IEnumerable<T> enumerable, Action<T, int, TArg> action, in TArg state, CancellationToken token)
        {
            if (enumerable is null || action is null || token.IsCancellationRequested) return enumerable;
            var innerObjs = enumerable as IList<T> ?? enumerable.ToList();
            var length = innerObjs.Count;
            for (var i = 0; i < length; i++)
            {
                if (token.IsCancellationRequested)
                    break;
                action(innerObjs[i], i, state);
            }
            return innerObjs;
        }
        #endregion

        #region IEnumerable<T> by Reference
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> Each<T>(this IEnumerable<T> enumerable, ActionRef<T> action)
        {
            if (enumerable is null || action is null) return enumerable;
            var objs = enumerable as T[] ?? enumerable.ToArray();
            for (var i = 0; i < objs.Length; i++)
                action(ref objs[i]);
            return objs;
        }
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <param name="token">Cancellation token in case the current execution thread is cancelled</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> Each<T>(this IEnumerable<T> enumerable, ActionRef<T> action, CancellationToken token)
        {
            if (enumerable is null || action is null || token.IsCancellationRequested) return enumerable;
            var objs = enumerable as T[] ?? enumerable.ToArray();
            for (var i = 0; i < objs.Length; i++)
            {
                if (token.IsCancellationRequested)
                    break;
                action(ref objs[i]);
            }
            return objs;
        }
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> Each<T>(this IEnumerable<T> enumerable, ActionRef<T, int> action)
        {
            if (enumerable is null || action is null) return enumerable;
            var objs = enumerable as T[] ?? enumerable.ToArray();
            for (var i = 0; i < objs.Length; i++)
                action(ref objs[i], i);
            return objs;
        }
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <param name="token">Cancellation token in case the current execution thread is cancelled</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> Each<T>(this IEnumerable<T> enumerable, ActionRef<T, int> action, CancellationToken token)
        {
            if (enumerable is null || action is null || token.IsCancellationRequested) return enumerable;
            var objs = enumerable as T[] ?? enumerable.ToArray();
            for (var i = 0; i < objs.Length; i++)
            {
                if (token.IsCancellationRequested)
                    break;
                action(ref objs[i], i);
            }
            return objs;
        }
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <param name="state">Object to pass to the Action on each element of the IEnumerable</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> Each<T, TArg>(this IEnumerable<T> enumerable, ActionRef<T, int, TArg> action, in TArg state)
        {
            if (enumerable is null || action is null) return enumerable;
            var innerObjs = enumerable as T[] ?? enumerable.ToArray();
            for (var i = 0; i < innerObjs.Length; i++)
                action(ref innerObjs[i], i, state);
            return innerObjs;
        }
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <param name="state">Object to pass to the Action on each element of the IEnumerable</param>
        /// <param name="token">Cancellation token in case the current execution thread is cancelled</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> Each<T, TArg>(this IEnumerable<T> enumerable, ActionRef<T, int, TArg> action, in TArg state, CancellationToken token)
        {
            if (enumerable is null || action is null || token.IsCancellationRequested) return enumerable;
            var innerObjs = enumerable as T[] ?? enumerable.ToArray();
            for (var i = 0; i < innerObjs.Length; i++)
            {
                if (token.IsCancellationRequested)
                    break;
                action(ref innerObjs[i], i, state);
            }
            return innerObjs;
        }
        #endregion

        #region Other IEnumerable Methods
        /// <summary>
        /// Enumerate the Linq expression to a IList
        /// </summary>
        /// <param name="linqExpression">Linq expression</param>
        /// <returns>IList with the result of the enumeration</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable Enumerate(this IEnumerable linqExpression)
        {
            if (linqExpression is null || linqExpression is IList || linqExpression is string || linqExpression is IDictionary) return linqExpression;
            var lqType = linqExpression.GetType();
            if (lqType.ReflectedType != typeof(Enumerable) && lqType.FullName.IndexOf("System.Linq", StringComparison.Ordinal) == -1) return linqExpression;
            var ienumerable = lqType.AllInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            if (ienumerable is null) return linqExpression;
            var type = typeof(List<>).MakeGenericType(ienumerable.GenericTypeArguments[0]);
            return (IList) Activator.CreateInstance(type, linqExpression);
        }
        /// <summary>
        /// Enumerate the Linq expression to a IList
        /// </summary>
        /// <param name="linqExpression">Linq expression</param>
        /// <returns>IList with the result of the enumeration</returns>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> Enumerate<T>(this IEnumerable<T> linqExpression)
        {
            if (linqExpression is null || linqExpression is IList || linqExpression is string || linqExpression is IDictionary) return linqExpression;
            var lqType = linqExpression.GetType();
            if (lqType.ReflectedType != typeof(Enumerable) && lqType.FullName.IndexOf("System.Linq", StringComparison.Ordinal) == -1) return linqExpression;
            var ienumerable = lqType.AllInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            if (ienumerable is null) return linqExpression;
            var type = typeof(List<>).MakeGenericType(ienumerable.GenericTypeArguments[0]);
            return (IList<T>)Activator.CreateInstance(type, linqExpression);
        }
        #endregion

        #endregion

        #region Sets Management
        /// <summary>
        /// Gets a hashset instance with the items of the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <returns>Hashset instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
            => new HashSet<T>(enumerable);
        /// <summary>
        /// Remove all null items in the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <returns>IEnumerable without null items</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable RemoveNulls(this IEnumerable enumerable)
        {
            return enumerable != null ? RemoveNullsInner(enumerable) : null;
            IEnumerable RemoveNullsInner(IEnumerable _enum)
            {
                foreach (var item in _enum)
                    if (item != null)
                        yield return item;
            }
        }
        /// <summary>
        /// Remove all null items in the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <returns>IEnumerable without null items</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T> enumerable) where T : class
        {
            if (enumerable is null) return null;
            if (enumerable is IList<T> enumList)
            {
                var noNullValues = new List<T>();
                for (var i = 0; i < enumList.Count; i++)
                    if (enumList[i] != null)
                        noNullValues.Add(enumList[i]);
                return noNullValues;
            }
            return RemoveNullsInner(enumerable);

            IEnumerable<T> RemoveNullsInner(IEnumerable<T> _enum)
            {
                foreach (var item in _enum)
                    if (item != null)
                        yield return item;
            }
        }
        /// <summary>
        /// Remove all default items in the IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <returns>IEnumerable without the default value items</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> RemoveDefaults<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable is null) return null;
            var comparer = EqualityComparer<T>.Default;
            T defaultValue = default;
            if (enumerable is IList<T> enumList)
            {
                var noDefaults = new List<T>();
                for (var i = 0; i < enumList.Count; i++)
                    if (!comparer.Equals(enumList[i], defaultValue))
                        noDefaults.Add(enumList[i]);
                return noDefaults;
            }
            return RemoveDefaultsInner(enumerable);

            IEnumerable<T> RemoveDefaultsInner(IEnumerable<T> _enum)
            {
                foreach (var item in _enum)
                    if (!comparer.Equals(item, defaultValue))
                        yield return item;
            }
        }
        /// <summary>
        /// Add all items and returns a new collection with all elements.
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="value">Values to add to the IEnumerable</param>
        /// <returns>IEnumerable with the values concatenated</returns>
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, params T[] value) => enumerable.Concat((IEnumerable<T>)value);
        /// <summary>
        /// Split the IEnumerable in two sets using a predicate.
        /// </summary>
        /// <typeparam name="T">Type of the IEnumerable</typeparam>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="separatorPredicate">Predicate to separete the IEnumerable in two sets</param>
        /// <returns>Tuple of two IEnumerable from the split of the IEnumerable using the predicate</returns>
        public static (T[], T[]) Split<T>(this IEnumerable<T> enumerable,
            Predicate<T> separatorPredicate)
        {
            if (enumerable is null) return (Array.Empty<T>(), Array.Empty<T>());
            var pool = ReferencePool<List<T>>.Shared;
            var firstList = pool.New();
            var secondList = pool.New();
            foreach (var item in enumerable)
            {
                if (separatorPredicate(item))
                    firstList.Add(item);
                else
                    secondList.Add(item);
            }
            var value = (firstList.ToArray(), secondList.ToArray());
            firstList.Clear();
            secondList.Clear();
            pool.Store(firstList);
            pool.Store(secondList);
            return value;
        }

        #region SymmetricExceptWith
        /// <summary>
        /// Returns a new IEnumerable containing only elements that are present either in that object or in the specified collection, but not both.
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="value">IEnumerable with elements to perform the SymmetricExcept</param>
        /// <returns>IEnumerable instance</returns>
        public static IEnumerable<T> SymmetricExceptWith<T>(this IEnumerable<T> enumerable, IEnumerable<T> value)
        {
            var hSet = new HashSet<T>(enumerable);
            hSet.SymmetricExceptWith(value);
            return hSet;
        }
        /// <summary>
        /// Returns a new IEnumerable containing only elements that are present either in that object or in the specified collection, but not both.
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="value">IEnumerable with elements to perform the SymmetricExcept</param>
        /// <param name="keySelector">Key selector to do the SymetricExcept</param>
        /// <returns>IEnumerable instance</returns>
        public static IEnumerable<T> SymmetricExceptWithKey<T, TKey>(this IEnumerable<T> enumerable, IEnumerable<T> value, Func<T, TKey> keySelector)
        {
            var hSet = new HashSet<T>(enumerable, KeySelectorEqualityComparer.Create(keySelector));
            hSet.SymmetricExceptWith(value);
            return hSet;
        }
        /// <summary>
        /// Returns a new IEnumerable containing only elements that are present either in that object or in the specified collection, but not both.
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="value">IEnumerable with elements to perform the SymmetricExcept</param>
        /// <param name="keySelector">Key selector to do the SymetricExcept</param>
        /// <param name="keyComparer">IEqualityComparer for the Key selector object</param>
        /// <returns>IEnumerable instance</returns>
        public static IEnumerable<T> SymmetricExceptWithKey<T, TKey>(this IEnumerable<T> enumerable, IEnumerable<T> value, Func<T, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
        {
            var hSet = new HashSet<T>(enumerable, KeySelectorEqualityComparer.Create(keySelector, keyComparer));
            hSet.SymmetricExceptWith(value);
            return hSet;
        }
        #endregion

        #region RemoveAll
        /// <summary>
        /// Returns a new IEnumerable containing only elements that are not present in the specified collection (Removes elements).
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="value">IEnumerable with elements to perform the Except</param>
        /// <returns>IEnumerable without the value items</returns>
        public static IEnumerable<T> RemoveAll<T>(this IEnumerable<T> enumerable, IEnumerable<T> value)
        {
            var hSet = new HashSet<T>(enumerable);
            hSet.ExceptWith(value);
            return hSet;
        }
        /// <summary>
        /// Returns a new IEnumerable containing only elements that are not present in the specified collection (Removes elements).
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="value">IEnumerable with elements to perform the Except</param>
	    /// <param name="keySelector">Key selector to do the SymetricExcept</param>
        /// <returns>IEnumerable without the value items</returns>
        public static IEnumerable<T> RemoveAllWithKey<T, TKey>(this IEnumerable<T> enumerable, IEnumerable<T> value, Func<T, TKey> keySelector)
        {
            var hSet = new HashSet<T>(enumerable, KeySelectorEqualityComparer.Create(keySelector));
            hSet.ExceptWith(value);
            return hSet;
        }
        /// <summary>
        /// Returns a new IEnumerable containing only elements that are not present in the specified collection (Removes elements).
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="value">IEnumerable with elements to perform the Except</param>
	    /// <param name="keySelector">Key selector to do the SymetricExcept</param>
	    /// <param name="keyComparer">IEqualityComparer for the Key selector object</param>
        /// <returns>IEnumerable without the value items</returns>
        public static IEnumerable<T> RemoveAllWithKey<T, TKey>(this IEnumerable<T> enumerable, IEnumerable<T> value, Func<T, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
        {
            var hSet = new HashSet<T>(enumerable, KeySelectorEqualityComparer.Create(keySelector, keyComparer));
            hSet.ExceptWith(value);
            return hSet;
        }
        #endregion

        /// <summary>
        /// Determines whether this object and the specified collection contain the same elements.
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="value">The collection to compare to the current object</param>
        /// <returns>true if the value enumerable contains the same elements; otherwise, false.</returns>
        public static bool SetEquals<T>(this IEnumerable<T> enumerable, IEnumerable<T> value)
        {
            if (enumerable is null && value is null)
                return true;
            if (enumerable is null || value is null)
                return false;
            var hSet = new HashSet<T>(enumerable);
            return hSet.SetEquals(value);
        }
        /// <summary>
        /// Gets an IEnumerable with all elements distinct by a key
        /// </summary>
        /// <typeparam name="T">IEnumerable element type</typeparam>
        /// <typeparam name="TK">Key object type</typeparam>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="keySelector">Key selector function to perform the distinct</param>
        /// <param name="comparer">Equality comparer object based on the key</param>
        /// <returns>IEnumerable with all elements dictinct by the key</returns>
        public static IEnumerable<T> DistinctBy<T, TK>(this IEnumerable<T> enumerable, Func<T, TK> keySelector, IEqualityComparer<TK> comparer = null)
        {
            return enumerable != null ? DistinctByInner(enumerable, keySelector, comparer) : null;

            IEnumerable<T> DistinctByInner(IEnumerable<T> mEnumerable, Func<T, TK> mKeySelector, IEqualityComparer<TK> mComparer = null)
            {
                var hSet = mComparer is null ? new HashSet<TK>() : new HashSet<TK>(mComparer);
                foreach (var element in mEnumerable)
                    if (hSet.Add(mKeySelector(element)))
                        yield return element;
            }
        }
        /// <summary>
        /// Gets an IEnumerable mergin similar items on the IEnumerable source
        /// </summary>
        /// <typeparam name="T">IEnumerable element type</typeparam>
        /// <typeparam name="TKey">Key type to identify similar items</typeparam>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="keySelector">Key selector function</param>
        /// <param name="mergeFunction">Merge function</param>
        /// <returns>IEnumerable with elements merged</returns>
        public static IEnumerable<T> Merge<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> keySelector, Func<IEnumerable<T>, T> mergeFunction)
        {
            return enumerable != null ? InnerMerge(enumerable.ToArray(), keySelector, mergeFunction) : null;

            IEnumerable<T> InnerMerge(T[] array, Func<T, TKey> mKeySelector, Func<IEnumerable<T>, T> mMergeFunction)
            {
                var source = array;
                foreach (var item in array)
                {
                    var key = mKeySelector(item);
                    var similarItems = source.Where((i, iItem, iKey, iKeySelector) => !ReferenceEquals(iItem, i) && Equals(iKeySelector(i), iKey), item, key, keySelector).ToList();
                    var nItem = similarItems.Count > 1 ? mMergeFunction(similarItems) : item;
                    source = source.Where((s, mSimilarItems) => !mSimilarItems.Contains(s), similarItems).ToArray();
                    yield return nItem;
                }
            }
        }
        /// <summary>
        /// Returns the Boolean value true if any element in the given collection appears in the second collection.
        /// </summary>
        /// <typeparam name="T">Type of collection</typeparam>
        /// <param name="first">First Collection</param>
        /// <param name="second">Second Collection</param>
        /// <returns>Returns the Boolean value true if any element in the given collection appears in the second collection.</returns>
        public static bool Overlaps<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            Ensure.ArgumentNotNull(first);
            Ensure.ArgumentNotNull(second);
            return second.Intersect(first).Distinct().Any();
        }
        /// <summary>
        /// Returns the Boolean value of true if all of the elements in the given collection are present inthe second collection. Will return the Boolean value of true if the collections share the same elements.
        /// </summary>
        /// <typeparam name="T">Type of collection</typeparam>
        /// <param name="first">First Collection</param>
        /// <param name="second">Second Collection</param>
        /// <returns>Will return the Boolean value of true if the collections share the same elements.</returns>
        public static bool IsSupersetOf<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            Ensure.ArgumentNotNull(first);
            Ensure.ArgumentNotNull(second);
            var secondArray = second as T[] ?? second.ToArray();
            var secondCount = secondArray.Distinct().Count();
            var intersectCount = secondArray.Intersect(first).Distinct().Count();
            return intersectCount == secondCount;
        }

        /// <summary>
        /// Returns the Boolean value of true if all of the elements in the given collection are present in the second collection. 
        /// Will return the Boolean value of false if the collections share exactly the same elements. 
        /// the collection must be an actual subset with at least one element less than that in the second collection.
        /// </summary>
        /// <typeparam name="T">Type of collection</typeparam>
        /// <param name="first">First Collection</param>
        /// <param name="second">Second Collection</param>
        /// <returns> Will return the Boolean value of false if the collections share exactly the same elements. the collection must be an actual subset with at least one element less than that in the second collection.</returns>
        public static bool IsProperSupersetOf<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            Ensure.ArgumentNotNull(first);
            Ensure.ArgumentNotNull(second);
            var firstArray = first as T[] ?? first.ToArray();
            var secondArray = second as T[] ?? second.ToArray();
            var firstCount = firstArray.Distinct().Count();
            var secondCount = secondArray.Distinct().Count();
            var intersectCount = secondArray.Intersect(firstArray).Distinct().Count();
            return (intersectCount < firstCount) && (intersectCount == secondCount);
        }

        /// <summary>
        /// Returns the Boolean value of true if all of the elements in the given collection are present in the second collection. 
        /// Will return the Boolean value of true if the collections share the same elements.
        /// </summary>
        /// <typeparam name="T">Type of collection</typeparam>
        /// <param name="first">First Collection</param>
        /// <param name="second">Second Collection</param>
        /// <returns> Will return the Boolean value of true if the collections share the same elements.</returns>        
        public static bool IsSubsetOf<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            // call the Superset operator and reverse the arguments 
            return IsSupersetOf(second, first);
        }

        /// <summary>
        /// Returns the Boolean value of true if all of the elements in the given collection are present in the second collection. 
        /// Will return the Boolean value of false if the collections share exactly the same elements. the collection must be an actual subset with at least one element less than that in the second collection.
        /// </summary>
        /// <typeparam name="T">Type of collection</typeparam>
        /// <param name="first">First Collection</param>
        /// <param name="second">Second Collection</param>
        /// <returns> Will return the Boolean value of false if the collections share exactly the same elements. the collection must be an actual subset with at least one element less than that in the second collection.</returns>        
        public static bool IsProperSubsetOf<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            // call the Superset operator and reverse the arguments 
            return IsProperSupersetOf(second, first);
        }
        /// <summary>
        /// Combine two IEnumerables using a selector function and an final item function
        /// </summary>
        /// <typeparam name="TItem">Item type</typeparam>
        /// <typeparam name="TKey">Item key type</typeparam>
        /// <param name="lastEnumerable">The last enumerable to check if the items are already on the initial enumerable</param>
        /// <param name="initialEnumerable">Initial enumerable</param>
        /// <param name="keySelector">Key selector for item type</param>
        /// <param name="finalItemFunc">Merge function between two items</param>
        /// <returns>IEnumerable of TItems</returns>
        public static IEnumerable<TItem> Combine<TItem, TKey>(this IEnumerable<TItem> lastEnumerable, IEnumerable<TItem> initialEnumerable, Func<TItem, TKey> keySelector, Func<TItem, TItem, TItem> finalItemFunc)
        {
            var lst = new List<TItem>(initialEnumerable);
            if (lastEnumerable is null) return lst;
            var lastArray = lastEnumerable as TItem[] ?? lastEnumerable.ToArray();
            if (lastArray.Any())
            {
                lastArray.Each(item =>
                {
                    var keyValue = keySelector(item);
                    var oItem = lst.FirstOrDefault((o, iKeyValue, iKeySelector) => Equals(iKeyValue, iKeySelector(o)), keyValue, keySelector);
                    if (oItem == null)
                        lst.Add(item);
                    else
                    {
                        lst.Remove(oItem);
                        var rItem = finalItemFunc(item, oItem);
                        lst.Add(rItem);
                    }
                });
            }
            return lst;
        }
        #endregion

        #region Sort
        /// <summary>
        /// Returns a new collection with all elements sorted using the System.IComparable`1 generic interface implementation of each element of the collection.
        /// </summary>
        /// <typeparam name="T">Type of item of the IEnumerable</typeparam>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <returns>IEnumerable with sorted items</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> Sort<T>(this IEnumerable<T> enumerable) => enumerable.OrderBy(x => x).ToArray();
        /// <summary>
        /// Returns a new collection with all elements sorted using the specified System.Comparison`1.
        /// </summary>
        /// <typeparam name="T">Type of item of the IEnumerable</typeparam>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="comparison">Comparisor function to sort the elements on the enumerable</param>
        /// <returns>IEnumerable with sorted items</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> Sort<T>(this IEnumerable<T> enumerable, Comparison<T> comparison)
        {
            var colArray = enumerable.ToArray();
            Array.Sort(colArray, comparison);
            return colArray;
        }
        /// <summary>
        /// Returns a new collection with all elements sorted using multiple options of sorting.
        /// </summary>
        /// <typeparam name="T">Type of item of the IEnumerable</typeparam>
        /// <typeparam name="TKey">Key to do the sort</typeparam>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="options">Sort options object array</param>
        /// <returns>Sorted IEnumerable</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> Sort<T, TKey>(this IEnumerable<T> enumerable, params SortOption<T, TKey>[] options) where TKey : IComparable
        {
            int InnerSort(T a, T b, SortOption<T, TKey>[] optionsArray, int optionIndex)
            {
                if (optionsArray.Length <= optionIndex)
                    return 0;
                var currentOption = optionsArray[optionIndex];
                var aValue = currentOption.Selector(a);
                var bValue = currentOption.Selector(b);
                if (aValue.CompareTo(bValue) == 0)
                    return InnerSort(a, b, optionsArray, ++optionIndex);
                if (aValue.CompareTo(bValue) < 0) return (currentOption.Direction == SortDirection.Ascending) ? -1 : 1;
                if (aValue.CompareTo(bValue) > 0) return (currentOption.Direction == SortDirection.Ascending) ? 1 : 0;
                return 0;
            }
            return enumerable.Sort((a, b) => InnerSort(a, b, options, 0));
        }
        /// <summary>
        /// Option to do the sort, indicates the direction of the sort and the Key selector 
        /// </summary>
        /// <typeparam name="T">Type of item of the IEnumerable</typeparam>
        /// <typeparam name="TKey">Key to do the sort</typeparam>
        public readonly struct SortOption<T, TKey>
        {
            /// <summary>
            /// Direction of the Sort
            /// </summary>
            public readonly SortDirection Direction;
            /// <summary>
            /// Key Selector of sorting
            /// </summary>
            public readonly Func<T, TKey> Selector;
            /// <summary>
            /// Option to do the sort, indicates the direction of the sort and the Key selector.
            /// </summary>
            /// <param name="direction">Direction of the sort</param>
            /// <param name="selector">Key Selector</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SortOption(SortDirection direction, Func<T, TKey> selector)
            {
                Direction = direction;
                Selector = selector;
            }
        }
        /// <summary>
        /// Indicates the direction of the sort
        /// </summary>
        public enum SortDirection
        {
            /// <summary>
            /// Ascending sort
            /// </summary>
            Ascending,
            /// <summary>
            /// Descending sort
            /// </summary>
            Descending
        }
        #endregion

        #region ICollection
        /// <summary>
        /// Remove items from the collection using the predicate.
        /// </summary>
        /// <param name="collection">Collection object source</param>
        /// <param name="predicate">Predicate to know is an element need to be removed from the ICollection</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveCollection<T>(this ICollection<T> collection, Predicate<T> predicate)
        {
            if (collection is null) return;
            var pool = ReferencePool<List<T>>.Shared;
            var lstWithPredicate = pool.New();
            foreach (var item in collection)
            {
                if (predicate(item))
                    lstWithPredicate.Add(item);
            }
            foreach (var item in lstWithPredicate)
                collection.Remove(item);
            lstWithPredicate.Clear();
            pool.Store(lstWithPredicate);
        }
        /// <summary>
        /// Remove items from the collection using the predicate.
        /// </summary>
        /// <param name="collection">Collection object source</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveCollectionNulls<T>(this ICollection<T> collection) where T : class => RemoveCollection(collection, item => item is null);
        #endregion

        #region Others
        /// <summary>
        /// Convenience method for retrieving a specific page of items within a collection.
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="pageIndex">The index of the page to get.</param>
        /// <param name="pageSize">The size of the pages.</param>
        /// <returns>IEnumerable with the items of a page</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> GetPage<T>(this IEnumerable<T> enumerable, int pageIndex, int pageSize) => enumerable?.Skip(pageIndex * pageSize).Take(pageSize);
        /// <summary>
        /// Converts an enumerable into a readonly collection
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <returns>ReadOnlyCollection instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> ToReadOnly<T>(this IEnumerable<T> enumerable) => new ReadOnlyCollection<T>(enumerable.ToList());
        /// <summary>
        /// Validates that the <paramref name="enumerable"/> is not null and contains items.
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <returns>true if the enumerable is not null or empty; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> enumerable) => enumerable?.Any() == true;
        /// <summary>
        /// Concatenates the members of a collection, using the specified separator between each member.
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="separator">string separator used to join the IEnumerable object</param>
        /// <returns>A string that consists of the members of <paramref name="enumerable"/> delimited by the <paramref name="separator"/> string. If values has no members, the method returns null.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Join<T>(this IEnumerable<T> enumerable, string separator) => enumerable is null ? null : string.Join(separator, enumerable);
        /// <summary>
        /// Yields an object to a IEnumerable
        /// </summary>
        /// <typeparam name="T">Type of IEnumerable</typeparam>
        /// <param name="obj">Object instance</param>
        /// <returns>IEnumerable with the object</returns>
        public static IEnumerable<T> Yield<T>(this T obj)
        {
            yield return obj;
        }
        /// <summary>
        /// Append an item to the enumerable
        /// </summary>
        /// <typeparam name="T">Type of the enumerable</typeparam>
        /// <param name="enumerable">Enumerable instance</param>
        /// <param name="item">Item to append</param>
        /// <returns>IEnumerable of items</returns>
        public static IEnumerable<T> Append<T>(this IEnumerable<T> enumerable, T item)
        {
            if (enumerable is null)
                throw new ArgumentNullException("enumerable");

            return InternalAppend(enumerable, item);

            IEnumerable<T> InternalAppend(IEnumerable<T> innerEnum, T innerItem)
            {
                foreach (var eItem in innerEnum)
                    yield return eItem;
                yield return innerItem;
            }
        }
        /// <summary>
        /// Prepend an item to the enumerable
        /// </summary>
        /// <typeparam name="T">Type of the enumerable</typeparam>
        /// <param name="enumerable">Enumerable instance</param>
        /// <param name="item">Item to prepend</param>
        /// <returns>IEnumerable of items</returns>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> enumerable, T item)
        {
            if (enumerable is null)
                throw new ArgumentNullException("enumerable");

            return InternalPrepend(enumerable, item);

            IEnumerable<T> InternalPrepend(IEnumerable<T> innerEnum, T innerItem)
            {
                yield return innerItem;
                foreach (var eItem in innerEnum)
                    yield return eItem;
            }
        }
        /// <summary>
        /// Compare two IEnumerables with the same type sequentially using a key selector
        /// </summary>
        /// <typeparam name="TSource">Type of the IEnumerable</typeparam>
        /// <typeparam name="TSourceKey">Type of the key</typeparam>
        /// <param name="first">First enumerable</param>
        /// <param name="second">Second enumerable</param>
        /// <param name="keySelector">Key selector used in the enumerables</param>
        /// <returns>True if both enumerables has the same count and keys</returns>
        public static bool SequenceEqual<TSource, TSourceKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TSourceKey> keySelector)
        {
            if (keySelector is null)
                throw new ArgumentNullException("keySelector");
            var comparer = new SequenceEqualFuncComparer<TSource, TSourceKey>(keySelector);
            return Enumerable.SequenceEqual(first, second, comparer);
        }
        /// <summary>
        /// Compare two IEnumerables with different types sequentially using key selectors
        /// </summary>
        /// <typeparam name="TSourceFirst">Type of the first enumerable</typeparam>
        /// <typeparam name="TSourceSecond">Type of the second enumerable</typeparam>
        /// <typeparam name="TSourceKey">Type of the key</typeparam>
        /// <param name="first">First enumerable</param>
        /// <param name="second">Second enumerable</param>
        /// <param name="firstKeySelector">Key selector for the first enumerable</param>
        /// <param name="secondKeySelector">Key selector for the second enumerable</param>
        /// <returns>True if both enumerables has the same count and keys</returns>
        public static bool SequenceEqual<TSourceFirst, TSourceSecond, TSourceKey>(this IEnumerable<TSourceFirst> first, IEnumerable<TSourceSecond> second, 
            Func<TSourceFirst, TSourceKey> firstKeySelector, Func<TSourceSecond, TSourceKey> secondKeySelector)
        {
            if (first is null)
                throw new ArgumentNullException("first");
            if (second is null)
                throw new ArgumentNullException("second");
            if (firstKeySelector is null)
                throw new ArgumentNullException("firstKeySelector");
            if (secondKeySelector is null)
                throw new ArgumentNullException("secondKeySelector");

            var firstEnumerator = first.GetEnumerator();
            var secondEnumerator = second.GetEnumerator();
            while(true)
            {
                var fbool = firstEnumerator.MoveNext();
                var sbool = secondEnumerator.MoveNext();

                if (fbool != sbool) return false;
                if (!fbool) break;

                var firstItem = firstEnumerator.Current;
                var secondItem = secondEnumerator.Current;

                var firstKeyValue = firstKeySelector(firstItem);
                var secondKeyValue = secondKeySelector(secondItem);

                var result = EqualityComparer<TSourceKey>.Default.Equals(firstKeyValue, secondKeyValue);
                if (!result)
                    return false;
            }
            return true;
        }
        private readonly struct SequenceEqualFuncComparer<TSource, TSourceKey> : IEqualityComparer<TSource>
        {
            private readonly Func<TSource, TSourceKey> _keySelector;

            #region .ctor
            public SequenceEqualFuncComparer(Func<TSource, TSourceKey> keySelector)
            {
                _keySelector = keySelector;
            }
            #endregion

            #region Public Methods
            public bool Equals(TSource x, TSource y)
            {
                var xKey = _keySelector(x);
                var yKey = _keySelector(y);
                return EqualityComparer<TSourceKey>.Default.Equals(xKey, yKey);
            }
            public int GetHashCode(TSource obj)
            {
                return _keySelector(obj)?.GetHashCode() ?? -1;
            }
            #endregion
        }
        #endregion

        #region GetCombination
        /// <summary>
        /// Perform combinations on a IEnumerable inside another IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable to combine</param>
        /// <returns>IEnumerable of IEnumerable with all combinations</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<IEnumerable<T>> GetCombination<T>(this IEnumerable<IEnumerable<T>> enumerable)
        {
            if (enumerable is null) yield break;
            var enumerableArray = enumerable as IEnumerable<T>[] ?? enumerable.ToArray();
            var singCol = enumerableArray.FirstOrDefault();
            if (singCol is null) yield break;
            foreach (var item in singCol)
            {
                var innerCol = enumerableArray.Skip(1).ToArray();
                if (innerCol.Length > 0)
                {
                    var innerCombination = innerCol.GetCombination();
                    if (innerCombination is null) continue;
                    foreach (var combination in innerCombination)
                        yield return new[] { item }.Concat(combination).ToArray();
                }
                else
                    yield return new[] { item };
            }
        }
        /// <summary>
        /// Perform combinations on a IEnumerable inside another IEnumerable
        /// </summary>
        /// <param name="enumerable">IEnumerable to combine</param>
        /// <returns>IEnumerable of string with all string combinations</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<string> GetCombination(this IEnumerable<IEnumerable<string>> enumerable)
        {
            var response = new List<string>();
            var coll = enumerable?.Select(item => item.ToArray()).ToArray();
            var singCol = coll?.FirstOrDefault();
            if (singCol is null) return response;
            for (var i = 0; i < singCol.Length; i++)
            {
                var item = singCol[i];
                if (coll.Length > 1)
                {
                    var newCollection = coll.Skip(1).ToArray();
                    var newPermute = newCollection.GetCombination();
                    foreach (var perm in newPermute)
                        response.Add(item + perm);
                }
                else
                    response.Add(item);
            }
            return response;
        }
        #endregion

        #region Index
        /// <summary>
        /// Gets the index of an element in the IEnumerable using a predicate
        /// </summary>
        /// <typeparam name="T">Type of IEnumerable</typeparam>
        /// <param name="enumerable">IEnumerable object source</param>
        /// <param name="predicate">Predicate to perform the search of the index.</param>
        /// <returns>Index of the element</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this IEnumerable<T> enumerable, Predicate<T> predicate)
        {
            if (enumerable is null) return -1;
            if (predicate is null) return -1;
            var idx = 0;
            foreach (var item in enumerable)
            {
                if (predicate(item))
                    return idx;
                idx++;
            }
            return -1;
        }
        /// <summary>
        /// Gets the index of an element in the IEnumerable
        /// </summary>
        /// <typeparam name="T">Type of IEnumerable</typeparam>
        /// <param name="enumerable">IEnumerable object source</param>
        /// <param name="item">Item to search inside the IEnumerable</param>
        /// <param name="comparer">Equality comparer</param>
        /// <returns>Index of the element</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this IEnumerable<T> enumerable, in T item, IEqualityComparer<T> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<T>.Default;
            return enumerable.IndexOf((i, vTuple) => vTuple.comparer.Equals(vTuple.item, i), (comparer, item));
        }
        /// <summary>
        /// Gets the index of an element in the LinkedList
        /// </summary>
        /// <typeparam name="T">Type of item in the Linked list</typeparam>
        /// <param name="list">Linked list</param>
        /// <param name="item">Item to get the index</param>
        /// <param name="comparer">Equality comparer</param>
        /// <returns>Index of the item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this LinkedList<T> list, in T item, IEqualityComparer<T> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<T>.Default;
            var count = 0;
            for (var node = list.First; node != null; node = node.Next, count++)
            {
                if (comparer.Equals(item, node.Value))
                    return count;
            }
            return -1;
        }
        #endregion

        #region Parallel Extensions

        #region IEnumerable<T>
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable using Parallel computing
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> ParallelEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable is null || action is null) return enumerable;
            var parallelEach = enumerable as IList<T> ?? enumerable.ToArray();
            Parallel.ForEach(parallelEach, action);
            return parallelEach;
        }
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable using Parallel computing
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <param name="token">Cancellation token in case the current execution thread is cancelled</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> ParallelEach<T>(this IEnumerable<T> enumerable, Action<T> action, CancellationToken token)
        {
            if (enumerable is null || action is null) return enumerable;
            var parallelEach = enumerable as IList<T> ?? enumerable.ToArray();
            var executor = new ParallelSimpleExecutor<T>(action, token);
            Parallel.ForEach(parallelEach, new ParallelOptions { CancellationToken = token }, executor.ExecuteWithCancellationToken);
            return parallelEach;
        }
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable using Parallel computing
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> ParallelEach<T>(this IEnumerable<T> enumerable, Action<T, long> action)
        {
            if (enumerable is null || action is null) return enumerable;
            var parallelEach = enumerable as IList<T> ?? enumerable.ToArray();
            var executor = new ParallelExecutor<T>(action);
            Parallel.ForEach(parallelEach, new ParallelOptions(), executor.Execute);
            return parallelEach;
        }
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable using Parallel computing
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <param name="token">Cancellation token in case the current execution thread is cancelled</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> ParallelEach<T>(this IEnumerable<T> enumerable, Action<T, long> action, CancellationToken token)
        {
            if (enumerable is null || action is null) return enumerable;
            var parallelEach = enumerable as IList<T> ?? enumerable.ToArray();
            var executor = new ParallelExecutor<T>(action, token);
            Parallel.ForEach(parallelEach, new ParallelOptions { CancellationToken = token }, executor.ExecuteWithCancellationToken);
            return parallelEach;
        }
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable using Parallel computing
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <param name="state">Object to pass to the Action on each element of the IEnumerable</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> ParallelEach<T, TArg>(this IEnumerable<T> enumerable, Action<T, long, TArg> action, in TArg state)
        {
            if (enumerable is null || action is null) return enumerable;
            var parallelEach = enumerable as IList<T> ?? enumerable.ToArray();
            var executor = new ParallelStateExecutor<T, TArg>(state, action);
            Parallel.ForEach(parallelEach, new ParallelOptions(), executor.ExecuteWithState);
            return parallelEach;
        }
        /// <summary>
        /// Performs the specified action on each element of the IEnumerable using Parallel computing
        /// </summary>
        /// <param name="enumerable">IEnumerable source object</param>
        /// <param name="action">The Action delegate to perform on each element of the IEnumerable</param>
        /// <param name="state">Object to pass to the Action on each element of the IEnumerable</param>
        /// <param name="token">Cancellation token in case the current execution thread is cancelled</param>
        /// <returns>IEnumerable instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> ParallelEach<T, TArg>(this IEnumerable<T> enumerable, Action<T, long, TArg> action, in TArg state, CancellationToken token)
        {
            if (enumerable is null || action is null) return enumerable;
            var parallelEach = enumerable as IList<T> ?? enumerable.ToArray();
            var executor = new ParallelStateExecutor<T, TArg>(state, action, token);
            Parallel.ForEach(parallelEach, new ParallelOptions { CancellationToken = token }, executor.ExecuteWithStateAndCancellationToken);
            return parallelEach;
        }

        private readonly struct ParallelSimpleExecutor<T>
        {
            private readonly Action<T> _action;
            private readonly CancellationToken _token;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ParallelSimpleExecutor(in Action<T> action, in CancellationToken token = default)
            {
                _action = action;
                _token = token;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ExecuteWithCancellationToken(T item, ParallelLoopState state)
            {
                if (!_token.IsCancellationRequested)
                    _action(item);
                else if (!state.IsStopped)
                    state.Stop();
            }
        }
        private readonly struct ParallelExecutor<T>
        {
            private readonly Action<T, long> _action;
            private readonly CancellationToken _token;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ParallelExecutor(in Action<T, long> action, in CancellationToken token = default)
            {
                _action = action;
                _token = token;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Execute(T item, ParallelLoopState state, long index)
                => _action(item, index);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ExecuteWithCancellationToken(T item, ParallelLoopState state, long index)
            {
                if (!_token.IsCancellationRequested)
                    _action(item, index);
                else if (!state.IsStopped)
                    state.Stop();
            }
        }
        private readonly struct ParallelStateExecutor<T, TArg>
        {
            private readonly TArg _state;
            private readonly Action<T, long, TArg> _action;
            private readonly CancellationToken _token;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ParallelStateExecutor(in TArg state, in Action<T, long, TArg> action, in CancellationToken token = default)
            {
                _state = state;
                _action = action;
                _token = token;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ExecuteWithState(T item, ParallelLoopState state, long index)
                => _action(item, index, _state);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ExecuteWithStateAndCancellationToken(T item, ParallelLoopState state, long index)
            {
                if (!_token.IsCancellationRequested)
                    _action(item, index, _state);
                else if (!state.IsStopped)
                    state.Stop();
            }
        }
        #endregion

        #region Invokes
        /// <summary>
        /// Performs a parallel invokation on all actions in the IEnumerable
        /// </summary>
        /// <param name="actions">Actions to execute in parallel</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ParallelInvoke(this IEnumerable<Action> actions) => Parallel.ForEach(actions, a => a());
        /// <summary>
        /// Performs a parallel invokation on all actions in the IEnumerable
        /// </summary>
        /// <param name="actions">Actions to execute in parallel</param>
        /// <param name="token">Cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ParallelInvoke(this IEnumerable<Action> actions, CancellationToken token)
        {
            var executor = new ParallelInvokeSimpleExecutor(token);
            Parallel.ForEach(actions, new ParallelOptions { CancellationToken = token }, executor.ExecuteWithCancellationToken);
        }
        /// <summary>
        /// Performs a parallel invokation on all actions in the IEnumerable
        /// </summary>
        /// <param name="funcs">IEnumerable of functions to execute in parallel</param>
        /// <returns>IEnumerable with all functions results</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> ParallelInvoke<T>(this IEnumerable<Func<T>> funcs)
        {
            if (funcs is null) return null;
            var funcsArray = funcs as IList<Func<T>> ?? funcs.ToArray();
            var executor = new ParallelInvokeExecutor<T>(funcsArray.Count);
            Parallel.ForEach(funcsArray, executor.Execute);
            return executor.Response;
        }
        /// <summary>
        /// Performs a parallel invokation on all actions in the IEnumerable
        /// </summary>
        /// <param name="funcs">IEnumerable of functions to execute in parallel</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>IEnumerable with all functions results</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> ParallelInvoke<T>(this IEnumerable<Func<T>> funcs, CancellationToken token)
        {
            if (funcs is null) return null;
            var funcsArray = funcs as IList<Func<T>> ?? funcs.ToArray();
            var executor = new ParallelInvokeExecutor<T>(funcsArray.Count, token);
            Parallel.ForEach(funcsArray, new ParallelOptions { CancellationToken = token }, executor.ExecuteWithCancellationToken);
            return executor.Response;
        }

        private readonly struct ParallelInvokeSimpleExecutor
        {
            private readonly CancellationToken _token;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ParallelInvokeSimpleExecutor(in CancellationToken token = default)
            {
                _token = token;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ExecuteWithCancellationToken(Action item, ParallelLoopState state)
            {
                if (!_token.IsCancellationRequested)
                    item();
                else if (!state.IsStopped)
                    state.Stop();
            }
        }
        private readonly struct ParallelInvokeExecutor<T>
        {
            private readonly object _locker;
            private readonly CancellationToken _token;
            public readonly T[] Response;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ParallelInvokeExecutor(int count, CancellationToken token = default)
            {
                _locker = new object();
                _token = token;
                Response = new T[count];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Execute(Func<T> item, ParallelLoopState state, long index)
            {
                var res = item();
                lock (_locker)
                    Response[(int) index] = res;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ExecuteWithCancellationToken(Func<T> item, ParallelLoopState state, long index)
            {
                if (!_token.IsCancellationRequested)
                {
                    var res = item();
                    lock (_locker)
                        Response[(int) index] = res;
                }
                else if (!state.IsStopped)
                    state.Stop();
            }
        }
        #endregion

        #endregion

        #region Batch
        /// <summary>
        /// Splits an IEnumerable to a batch of IEnumerables 
        /// </summary>
        /// <param name="source">IEnumerable source object</param>
        /// <param name="batchSize">Batch size</param>
        /// <returns>A List of list with all IEnumerable items in batch groups</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<List<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            var response = new List<List<T>>();
            using (var enumerator = source.GetEnumerator())
                while (enumerator.MoveNext())
                    response.Add(BatchElements(enumerator, batchSize - 1));
            return response;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<T> BatchElements<T>(IEnumerator<T> source, int batchSize)
        {
            var response = new List<T> { source.Current };
            for (var i = 0; i < batchSize && source.MoveNext(); i++)
                response.Add(source.Current);
            return response;
        }
        #endregion

        #region Math
        /// <summary>
        /// Gets the standard deviation
        /// </summary>
        /// <param name="values">Values Set</param>
        /// <returns>Standard deviation value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (double Average, double StandardDeviation) GetAverageAndStdDev(this IEnumerable<double> values)
        {
            var valArray = values?.ToArray();
            if (valArray is null || valArray.Length <= 1) return (0, 0);
            var avg = valArray.Average();
            var sum = valArray.Sum(d => Math.Pow(d - avg, 2));
            var res = Math.Sqrt((sum) / (valArray.Length - 1));
            return (avg, res);
        }
        /// <summary>
        /// Gets the standard deviation
        /// </summary>
        /// <param name="values">Values Set</param>
        /// <returns>Standard deviation value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetStdDev(this IEnumerable<double> values)
        {
            var valArray = values?.ToArray();
            if (valArray is null || valArray.Length <= 1) return 0;
            var avg = valArray.Average();
            var sum = valArray.Sum(d => Math.Pow(d - avg, 2));
            var res = Math.Sqrt((sum) / (valArray.Length - 1));
            return res;
        }
        /// <summary>
        /// Gets the standard deviation
        /// </summary>
        /// <typeparam name="T">Enumerable type</typeparam>
        /// <param name="enumerable">Values set</param>
        /// <param name="selectFunction">Select function</param>
        /// <returns>Standard deviation value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetStdDev<T>(this IEnumerable<T> enumerable, Func<T, double> selectFunction) => enumerable.Select(selectFunction).GetStdDev();
        /// <summary>
        /// Gets the standard deviation
        /// </summary>
        /// <typeparam name="T">Enumerable type</typeparam>
        /// <param name="enumerable">Values set</param>
        /// <param name="selectFunction">Select function</param>
        /// <returns>Standard deviation value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetStdDev<T>(this IEnumerable<T> enumerable, Func<T, decimal> selectFunction) => enumerable.Select((i, func) => (double)func(i), selectFunction).GetStdDev();
        /// <summary>
        /// Gets the average and the standard deviation
        /// </summary>
        /// <typeparam name="T">Enumerable type</typeparam>
        /// <param name="enumerable">Values set</param>
        /// <param name="selectFunction">Select function</param>
        /// <returns>Average and Standard deviation value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (double Average, double StandardDeviation) GetAverageAndStdDev<T>(this IEnumerable<T> enumerable, Func<T, double> selectFunction) => enumerable.Select(selectFunction).GetAverageAndStdDev();
        /// <summary>
        /// Gets the average and the standard deviation
        /// </summary>
        /// <typeparam name="T">Enumerable type</typeparam>
        /// <param name="enumerable">Values set</param>
        /// <param name="selectFunction">Select function</param>
        /// <returns>Average and Standard deviation value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (double Average, double StandardDeviation) GetAverageAndStdDev<T>(this IEnumerable<T> enumerable, Func<T, decimal> selectFunction) => enumerable.Select((i, func) => (double)func(i), selectFunction).GetAverageAndStdDev();
        #endregion

        #region KeyedCollections
        /// <summary>
        /// Adds a IEnumerable to the collection
        /// </summary>
        /// <param name="keyedCollection">Keyed collection object</param>
        /// <param name="col">IEnumerable instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<TKey, TItem>(this KeyedCollection<TKey, TItem> keyedCollection, IEnumerable<TItem> col)
        {
            if (keyedCollection is null) return;
            if (col is null) return;
            foreach (var i in col)
                keyedCollection.Add(i);
        }
        /// <summary>
        /// If the collection contains the key, execute a Func to return a value, if fails then another func.
        /// </summary>
        /// <param name="keyedCollection">Keyed collection object</param>
        /// <param name="key">Collection item key</param>
        /// <param name="then">Func to execute when the item was found on the collection</param>
        /// <param name="fail">Func to execute when the item was not found on the collection</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TR ContainsKeyThen<TR, TKey, TItem>(this KeyedCollection<TKey, TItem> keyedCollection, TKey key, Func<TItem, TR> then = null, Func<TR> fail = null)
        {
            if (key != null && keyedCollection?.Contains(key) == true && then != null)
                return then(keyedCollection[key]);
            return fail != null ? fail() : default;
        }
        /// <summary>
        /// Get a item if the key is found on the collection; otherwise returns the default value of the item
        /// </summary>
        /// <param name="keyedCollection">Keyed collection object</param>
        /// <param name="key">Collection item key</param>
        /// <returns>Item value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TItem GetIfContains<TKey, TItem>(this KeyedCollection<TKey, TItem> keyedCollection, TKey key)
        {
            if (key != null && keyedCollection?.Contains(key) == true)
                return keyedCollection[key];
            return default;
        }
        #endregion

        #region ConcurrentDictionary
        /// <summary>Adds a key/value pair to the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2"></see> by using the specified function if the key does not already exist, or returns the existing value if the key exists.</summary>
        /// <param name="dictionary">Source dictionary</param>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key.</param>
        /// <returns>The value for the key.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key">key</paramref> or <paramref name="valueFactory">valueFactory</paramref> is null.</exception>
        /// <exception cref="T:System.OverflowException">The dictionary already contains the maximum number of elements (<see cref="F:System.Int32.MaxValue"></see>).</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<TValue> GetOrAddAsync<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, Task<TValue>> valueFactory)
            => dictionary.TryGetValue(key, out var resultingValue) ? resultingValue : dictionary.GetOrAdd(key, await valueFactory(key).ConfigureAwait(false));
        #endregion

        #region Linq Extensions
        /// <summary>
        /// Finds an item that fulfill a predicate if not, return the one that fulfill the next predicate and so on. 
        /// </summary>
        /// <typeparam name="T">Type of item</typeparam>
        /// <param name="source">IEnumerable source object</param>
        /// <param name="predicates">Predicates to compare</param>
        /// <returns>The item if is found</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T FirstOf<T>(this IEnumerable<T> source, params Predicate<T>[] predicates)
        {
            if (predicates is null) return default;
            var comparer = EqualityComparer<T>.Default;
            var pool = ReferencePool<List<T>>.Shared;
            var foundArray = pool.New();
            for (var i = 0; i < predicates.Length - 1; i++)
                foundArray.Add(default);
            foreach (var item in source)
            {
                for (var i = 0; i < predicates.Length; i++)
                {
                    if (!predicates[i](item)) continue;
                    if (i == 0)
                        return item;
                    if (comparer.Equals(foundArray[i - 1], default))
                        foundArray[i - 1] = item;
                }
            }
            T value = default;
            for (var i = 0; i < foundArray.Count; i++)
            {
                if (!comparer.Equals(foundArray[i], default))
                {
                    value = foundArray[i];
                    break;
                }
            }
            foundArray.Clear();
            pool.Store(foundArray);
            return value;
        }
        /// <summary>
        /// Finds an item that fulfill a predicate if not, return the one that fulfill the next predicate and so on. 
        /// </summary>
        /// <typeparam name="T">Type of item</typeparam>
        /// <typeparam name="TArg">Type of state</typeparam>
        /// <param name="source">IEnumerable source object</param>
        /// <param name="predicates">Predicates to compare</param>
        /// <param name="state">State object instance</param>
        /// <returns>The item if is found</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T FirstOf<T, TArg>(this IEnumerable<T> source, Func<T, TArg, bool>[] predicates, in TArg state)
        {
            if (predicates is null) return default;
            var comparer = EqualityComparer<T>.Default;
            var pool = ReferencePool<List<T>>.Shared;
            var foundArray = pool.New();
            for (var i = 0; i < predicates.Length - 1; i++)
                foundArray.Add(default);
            foreach (var item in source)
            {
                for (var i = 0; i < predicates.Length; i++)
                {
                    if (!predicates[i](item, state)) continue;
                    if (i == 0)
                        return item;
                    if (comparer.Equals(foundArray[i - 1], default))
                        foundArray[i - 1] = item;
                }
            }
            T value = default;
            for (var i = 0; i < foundArray.Count; i++)
            {
                if (!comparer.Equals(foundArray[i], default))
                {
                    value = foundArray[i];
                    break;
                }
            }
            foundArray.Clear();
            pool.Store(foundArray);
            return value;
        }

        /// <summary>
        /// Gets the index of an element in the IEnumerable using a predicate
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TArg">The type of the state.</typeparam>
        /// <param name="source">An System.Collections.Generic.IEnumerable`1 that contains the elements to apply the predicate to.</param>
        /// <param name="predicate">Predicate to perform the search of the index.</param>
        /// <param name="state">State object instance</param>
        /// <returns>Index of the element</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<TSource, TArg>(this IEnumerable<TSource> source, Func<TSource, TArg, bool> predicate, in TArg state)
        {
            if (source is null) return -1;
            if (predicate is null) return -1;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    if (predicate(sourceList[i], state))
                        return i;
                }
                return -1;
            }
            var idx = 0;
            foreach (var item in source)
            {
                if (predicate(item, state))
                    return idx;
                idx++;
            }
            return -1;
        }
        /// <summary>
        /// Determines whether all elements of a sequence satisfy a condition.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TArg">The type of the state.</typeparam>
        /// <param name="source">An System.Collections.Generic.IEnumerable`1 that contains the elements to apply the predicate to.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="state">State object instance</param>
        /// <returns>true if every element of the source sequence passes the test in the specified predicate, or if the sequence is empty; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool All<TSource, TArg>(this IEnumerable<TSource> source, Func<TSource, TArg, bool> predicate, in TArg state)
        {
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    if (!predicate(sourceList[i], state))
                        return false;
                }
                return true;
            }
            foreach (var item in source)
            {
                if (!predicate(item, state))
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Determines whether any element of a sequence satisfies a condition.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TArg">The type of the state.</typeparam>
        /// <param name="source">An System.Collections.Generic.IEnumerable`1 whose elements to apply the predicate to.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="state">State object instance</param>
        /// <returns>true if any elements in the source sequence pass the test in the specified predicate; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Any<TSource, TArg>(this IEnumerable<TSource> source, Func<TSource, TArg, bool> predicate, in TArg state)
        {
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    if (predicate(sourceList[i], state))
                        return true;
                }
                return false;
            }
            foreach (var item in source)
            {
                if (predicate(item, state))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Returns the first element in a sequence that satisfies a specified condition.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TArg">The type of the state.</typeparam>
        /// <param name="source">An System.Collections.Generic.IEnumerable`1 to return an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="state">State object instance</param>
        /// <returns>The first element in the sequence that passes the test in the specified predicate function.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource First<TSource, TArg>(this IEnumerable<TSource> source, Func<TSource, TArg, bool> predicate, in TArg state)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                    if (predicate(sourceList[i], state))
                        return sourceList[i];
            }
            else
            {
                foreach (var item in source)
                    if (predicate(item, state))
                        return item;
            }
            throw new InvalidOperationException("No element satisfies the condition in predicate");
        }
        /// <summary>
        /// Returns the first element of the sequence that satisfies a condition or a default value if no such element is found.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TArg">The type of the state.</typeparam>
        /// <param name="source">An System.Collections.Generic.IEnumerable`1 to return an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="state">State object instance</param>
        /// <returns>default(TSource) if source is empty or if no element passes the test specified by predicate; otherwise, the first element in source that passes the test specified by predicate.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource FirstOrDefault<TSource, TArg>(this IEnumerable<TSource> source, Func<TSource, TArg, bool> predicate, TArg state)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                    if (predicate(sourceList[i], state))
                        return sourceList[i];
            }
            else
            {
                foreach (var item in source)
                    if (predicate(item, state))
                        return item;
            }
            return default;
        }
        /// <summary>
        /// Returns the first element of the sequence that satisfies a condition or a default value if no such element is found.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TArg">The type of the Argument 1.</typeparam>
        /// <typeparam name="TArg2">The type of the Argument 2.</typeparam>
        /// <param name="source">An System.Collections.Generic.IEnumerable`1 to return an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="arg0">Argument 0</param>
        /// <param name="arg1">Argument 1</param>
        /// <returns>default(TSource) if source is empty or if no element passes the test specified by predicate; otherwise, the first element in source that passes the test specified by predicate.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource FirstOrDefault<TSource, TArg, TArg2>(this IEnumerable<TSource> source, Func<TSource, TArg, TArg2, bool> predicate, TArg arg0, TArg2 arg1)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                    if (predicate(sourceList[i], arg0, arg1))
                        return sourceList[i];
            }
            else
            {
                foreach (var item in source)
                    if (predicate(item, arg0, arg1))
                        return item;
            }
            return default;
        }
        /// <summary>
        /// Returns the first element of the sequence that satisfies a condition or a default value if no such element is found.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TArg">The type of the Argument 1.</typeparam>
        /// <typeparam name="TArg2">The type of the Argument 2.</typeparam>
        /// <typeparam name="TArg3">The type of the Argument 3.</typeparam>
        /// <param name="source">An System.Collections.Generic.IEnumerable`1 to return an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="arg0">Argument 0</param>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <returns>default(TSource) if source is empty or if no element passes the test specified by predicate; otherwise, the first element in source that passes the test specified by predicate.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource FirstOrDefault<TSource, TArg, TArg2, TArg3>(this IEnumerable<TSource> source, Func<TSource, TArg, TArg2, TArg3, bool> predicate, TArg arg0, TArg2 arg1, TArg3 arg2)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                    if (predicate(sourceList[i], arg0, arg1, arg2))
                        return sourceList[i];
            }
            else
            {
                foreach (var item in source)
                    if (predicate(item, arg0, arg1, arg2))
                        return item;
            }
            return default;
        }
        /// <summary>
        /// Returns the last element of a sequence that satisfies a specified condition.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TArg">The type of the state.</typeparam>
        /// <param name="source">An System.Collections.Generic.IEnumerable`1 to return an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="state">State object instance</param>
        /// <returns>The last element in the sequence that passes the test in the specified predicate function.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource Last<TSource, TArg>(this IEnumerable<TSource> source, Func<TSource, TArg, bool> predicate, in TArg state)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = length - 1; i >= 0; i--)
                    if (predicate(sourceList[i], state))
                        return sourceList[i];
            }
            else
            {
                foreach (var item in source.Reverse())
                    if (predicate(item, state))
                        return item;
            }
            throw new InvalidOperationException("No element satisfies the condition in predicate");
        }
        /// <summary>
        /// Returns the last element of a sequence that satisfies a condition or a default value if no such element is found.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TArg">The type of the state.</typeparam>
        /// <param name="source">An System.Collections.Generic.IEnumerable`1 to return an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="state">State object instance</param>
        /// <returns>default(TSource) if the sequence is empty or if no elements pass the test in the predicate function; otherwise, the last element that passes the test in the predicate function.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource LastOrDefault<TSource, TArg>(this IEnumerable<TSource> source, Func<TSource, TArg, bool> predicate, in TArg state)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = length - 1; i >= 0; i--)
                    if (predicate(sourceList[i], state))
                        return sourceList[i];
            }
            else
            {
                foreach (var item in source.Reverse())
                    if (predicate(item, state))
                        return item;
            }
            return default;
        }

        /// <summary>
        /// Invokes a transform function on each element of a sequence and returns the first item with the maximum value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The first item with the maximum value in the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MaxItem<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var resItem = default(TSource);
            int? maxValue = null;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    var value = selector(sourceList[i]);
                    if (maxValue.HasValue && maxValue.Value >= value) continue;
                    maxValue = value;
                    resItem = sourceList[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    var value = selector(item);
                    if (maxValue.HasValue && maxValue.Value >= value) continue;
                    maxValue = value;
                    resItem = item;
                }
            }
            if (maxValue is null) throw new InvalidOperationException("The source sequence is empty");
            return resItem;
        }
        /// <summary>
        /// Invokes a transform function on each element of a sequence and returns the first item with the maximum value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The first item with the maximum value in the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MaxItem<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var resItem = default(TSource);
            long? maxValue = null;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    var value = selector(sourceList[i]);
                    if (maxValue.HasValue && maxValue.Value >= value) continue;
                    maxValue = value;
                    resItem = sourceList[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    var value = selector(item);
                    if (maxValue.HasValue && maxValue.Value >= value) continue;
                    maxValue = value;
                    resItem = item;
                }
            }
            if (maxValue is null) throw new InvalidOperationException("The source sequence is empty");
            return resItem;
        }
        /// <summary>
        /// Invokes a transform function on each element of a sequence and returns the first item with the maximum value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The first item with the maximum value in the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MaxItem<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var resItem = default(TSource);
            float? maxValue = null;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    var value = selector(sourceList[i]);
                    if (maxValue.HasValue && !(maxValue.Value < value)) continue;
                    maxValue = value;
                    resItem = sourceList[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    var value = selector(item);
                    if (maxValue.HasValue && !(maxValue.Value < value)) continue;
                    maxValue = value;
                    resItem = item;
                }
            }
            if (maxValue is null) throw new InvalidOperationException("The source sequence is empty");
            return resItem;
        }
        /// <summary>
        /// Invokes a transform function on each element of a sequence and returns the first item with the maximum value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The first item with the maximum value in the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MaxItem<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var resItem = default(TSource);
            double? maxValue = null;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    var value = selector(sourceList[i]);
                    if (maxValue.HasValue && !(maxValue.Value < value)) continue;
                    maxValue = value;
                    resItem = sourceList[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    var value = selector(item);
                    if (maxValue.HasValue && !(maxValue.Value < value)) continue;
                    maxValue = value;
                    resItem = item;
                }
            }
            if (maxValue is null) throw new InvalidOperationException("The source sequence is empty");
            return resItem;
        }
        /// <summary>
        /// Invokes a transform function on each element of a sequence and returns the first item with the maximum value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The first item with the maximum value in the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MaxItem<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var resItem = default(TSource);
            decimal? maxValue = null;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    var value = selector(sourceList[i]);
                    if (maxValue.HasValue && maxValue.Value >= value) continue;
                    maxValue = value;
                    resItem = sourceList[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    var value = selector(item);
                    if (maxValue.HasValue && maxValue.Value >= value) continue;
                    maxValue = value;
                    resItem = item;
                }
            }
            if (maxValue is null) throw new InvalidOperationException("The source sequence is empty");
            return resItem;
        }
        /// <summary>
        /// Invokes a transform function on each element of a sequence and returns the first item with the maximum value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The first item with the maximum value in the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MaxItem<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var resItem = default(TSource);
            int? maxValue = null;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    var value = selector(sourceList[i]);
                    if (value is null) continue;
                    if (maxValue.HasValue && maxValue.Value >= value.Value) continue;
                    maxValue = value;
                    resItem = sourceList[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    var value = selector(item);
                    if (value is null) continue;
                    if (maxValue.HasValue && maxValue.Value >= value.Value) continue;
                    maxValue = value;
                    resItem = item;
                }
            }
            if (maxValue is null) throw new InvalidOperationException("The source sequence is empty");
            return resItem;
        }
        /// <summary>
        /// Invokes a transform function on each element of a sequence and returns the first item with the maximum value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The first item with the maximum value in the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MaxItem<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var resItem = default(TSource);
            long? maxValue = null;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    var value = selector(sourceList[i]);
                    if (value is null) continue;
                    if (maxValue.HasValue && maxValue.Value >= value.Value) continue;
                    maxValue = value;
                    resItem = sourceList[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    var value = selector(item);
                    if (value is null) continue;
                    if (maxValue.HasValue && maxValue.Value >= value.Value) continue;
                    maxValue = value;
                    resItem = item;
                }
            }
            if (maxValue is null) throw new InvalidOperationException("The source sequence is empty");
            return resItem;
        }
        /// <summary>
        /// Invokes a transform function on each element of a sequence and returns the first item with the maximum value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The first item with the maximum value in the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MaxItem<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var resItem = default(TSource);
            float? maxValue = null;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    var value = selector(sourceList[i]);
                    if (value is null) continue;
                    if (maxValue.HasValue && !(maxValue.Value < value.Value)) continue;
                    maxValue = value;
                    resItem = sourceList[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    var value = selector(item);
                    if (value is null) continue;
                    if (maxValue.HasValue && !(maxValue.Value < value.Value)) continue;
                    maxValue = value;
                    resItem = item;
                }
            }
            if (maxValue is null) throw new InvalidOperationException();
            return resItem;
        }
        /// <summary>
        /// Invokes a transform function on each element of a sequence and returns the first item with the maximum value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The first item with the maximum value in the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MaxItem<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var resItem = default(TSource);
            double? maxValue = null;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    var value = selector(sourceList[i]);
                    if (value is null) continue;
                    if (maxValue.HasValue && !(maxValue.Value < value.Value)) continue;
                    maxValue = value;
                    resItem = sourceList[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    var value = selector(item);
                    if (value is null) continue;
                    if (maxValue.HasValue && !(maxValue.Value < value.Value)) continue;
                    maxValue = value;
                    resItem = item;
                }
            }
            if (maxValue is null) throw new InvalidOperationException("The source sequence is empty");
            return resItem;
        }
        /// <summary>
        /// Invokes a transform function on each element of a sequence and returns the first item with the maximum value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The first item with the maximum value in the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MaxItem<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var resItem = default(TSource);
            decimal? maxValue = null;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    var value = selector(sourceList[i]);
                    if (value is null) continue;
                    if (maxValue.HasValue && maxValue.Value >= value.Value) continue;
                    maxValue = value;
                    resItem = sourceList[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    var value = selector(item);
                    if (value is null) continue;
                    if (maxValue.HasValue && maxValue.Value >= value.Value) continue;
                    maxValue = value;
                    resItem = item;
                }
            }
            if (maxValue is null) throw new InvalidOperationException("The source sequence is empty");
            return resItem;
        }

        /// <summary>
        /// Invokes a transform function on each element of a sequence and returns the first item with the minimum value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The first item with the maximum value in the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MinItem<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var resItem = default(TSource);
            int? minValue = null;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    var value = selector(sourceList[i]);
                    if (minValue.HasValue && minValue.Value <= value) continue;
                    minValue = value;
                    resItem = sourceList[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    var value = selector(item);
                    if (minValue.HasValue && minValue.Value <= value) continue;
                    minValue = value;
                    resItem = item;
                }
            }
            if (minValue is null) throw new InvalidOperationException("The source sequence is empty");
            return resItem;
        }
        /// <summary>
        /// Invokes a transform function on each element of a sequence and returns the first item with the minimum value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The first item with the maximum value in the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MinItem<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var resItem = default(TSource);
            long? minValue = null;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    var value = selector(sourceList[i]);
                    if (minValue.HasValue && minValue.Value <= value) continue;
                    minValue = value;
                    resItem = sourceList[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    var value = selector(item);
                    if (minValue.HasValue && minValue.Value <= value) continue;
                    minValue = value;
                    resItem = item;
                }
            }
            if (minValue is null) throw new InvalidOperationException("The source sequence is empty");
            return resItem;
        }
        /// <summary>
        /// Invokes a transform function on each element of a sequence and returns the first item with the minimum value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The first item with the maximum value in the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MinItem<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var resItem = default(TSource);
            float? minValue = null;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    var value = selector(sourceList[i]);
                    if (minValue.HasValue && !(minValue.Value > value)) continue;
                    minValue = value;
                    resItem = sourceList[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    var value = selector(item);
                    if (minValue.HasValue && !(minValue.Value > value)) continue;
                    minValue = value;
                    resItem = item;
                }
            }
            if (minValue is null) throw new InvalidOperationException("The source sequence is empty");
            return resItem;
        }
        /// <summary>
        /// Invokes a transform function on each element of a sequence and returns the first item with the minimum value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The first item with the maximum value in the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MinItem<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var resItem = default(TSource);
            double? minValue = null;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    var value = selector(sourceList[i]);
                    if (minValue.HasValue && !(minValue.Value > value)) continue;
                    minValue = value;
                    resItem = sourceList[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    var value = selector(item);
                    if (minValue.HasValue && !(minValue.Value > value)) continue;
                    minValue = value;
                    resItem = item;
                }
            }
            if (minValue is null) throw new InvalidOperationException("The source sequence is empty");
            return resItem;
        }
        /// <summary>
        /// Invokes a transform function on each element of a sequence and returns the first item with the minimum value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The first item with the maximum value in the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MinItem<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var resItem = default(TSource);
            decimal? minValue = null;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    var value = selector(sourceList[i]);
                    if (minValue.HasValue && minValue.Value <= value) continue;
                    minValue = value;
                    resItem = sourceList[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    var value = selector(item);
                    if (minValue.HasValue && minValue.Value <= value) continue;
                    minValue = value;
                    resItem = item;
                }
            }
            if (minValue is null) throw new InvalidOperationException("The source sequence is empty");
            return resItem;
        }
        /// <summary>
        /// Invokes a transform function on each element of a sequence and returns the first item with the minimum value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The first item with the maximum value in the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MinItem<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var resItem = default(TSource);
            int? minValue = null;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    var value = selector(sourceList[i]);
                    if (value is null) continue;
                    if (minValue.HasValue && minValue.Value <= value.Value) continue;
                    minValue = value;
                    resItem = sourceList[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    var value = selector(item);
                    if (value is null) continue;
                    if (minValue.HasValue && minValue.Value <= value.Value) continue;
                    minValue = value;
                    resItem = item;
                }
            }
            if (minValue is null) throw new InvalidOperationException("The source sequence is empty");
            return resItem;
        }
        /// <summary>
        /// Invokes a transform function on each element of a sequence and returns the first item with the minimum value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The first item with the maximum value in the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MinItem<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var resItem = default(TSource);
            long? minValue = null;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    var value = selector(sourceList[i]);
                    if (value is null) continue;
                    if (minValue.HasValue && minValue.Value <= value.Value) continue;
                    minValue = value;
                    resItem = sourceList[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    var value = selector(item);
                    if (value is null) continue;
                    if (minValue.HasValue && minValue.Value <= value.Value) continue;
                    minValue = value;
                    resItem = item;
                }
            }
            if (minValue is null) throw new InvalidOperationException("The source sequence is empty");
            return resItem;
        }
        /// <summary>
        /// Invokes a transform function on each element of a sequence and returns the first item with the minimum value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The first item with the maximum value in the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MinItem<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var resItem = default(TSource);
            float? minValue = null;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    var value = selector(sourceList[i]);
                    if (value is null) continue;
                    if (minValue.HasValue && !(minValue.Value > value.Value)) continue;
                    minValue = value;
                    resItem = sourceList[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    var value = selector(item);
                    if (value is null) continue;
                    if (minValue.HasValue && !(minValue.Value > value.Value)) continue;
                    minValue = value;
                    resItem = item;
                }
            }
            if (minValue is null) throw new InvalidOperationException("The source sequence is empty");
            return resItem;
        }
        /// <summary>
        /// Invokes a transform function on each element of a sequence and returns the first item with the minimum value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The first item with the maximum value in the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MinItem<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var resItem = default(TSource);
            double? minValue = null;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    var value = selector(sourceList[i]);
                    if (value is null) continue;
                    if (minValue.HasValue && !(minValue.Value > value.Value)) continue;
                    minValue = value;
                    resItem = sourceList[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    var value = selector(item);
                    if (value is null) continue;
                    if (minValue.HasValue && !(minValue.Value > value.Value)) continue;
                    minValue = value;
                    resItem = item;
                }
            }
            if (minValue is null) throw new InvalidOperationException("The source sequence is empty");
            return resItem;
        }
        /// <summary>
        /// Invokes a transform function on each element of a sequence and returns the first item with the minimum value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>The first item with the maximum value in the sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource MinItem<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var resItem = default(TSource);
            decimal? minValue = null;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    var value = selector(sourceList[i]);
                    if (value is null) continue;
                    if (minValue.HasValue && minValue.Value <= value.Value) continue;
                    minValue = value;
                    resItem = sourceList[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    var value = selector(item);
                    if (value is null) continue;
                    if (minValue.HasValue && minValue.Value <= value.Value) continue;
                    minValue = value;
                    resItem = item;
                }
            }
            if (minValue is null) throw new InvalidOperationException("The source sequence is empty");
            return resItem;
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TArg">The type of the state.</typeparam>
        /// <param name="source">An System.Collections.Generic.IEnumerable`1 to return a single element from.</param>
        /// <param name="predicate">A function to test an element for a condition.</param>
        /// <param name="state">State object instance</param>
        /// <returns>The single element of the input sequence that satisfies a condition.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource Single<TSource, TArg>(this IEnumerable<TSource> source, Func<TSource, TArg, bool> predicate, in TArg state)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var @default = default(TSource);
            var found = false;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    if (!predicate(sourceList[i], state)) continue;
                    if (found) throw new InvalidOperationException("The input sequence contains more than one element");
                    @default = sourceList[i];
                    found = true;
                }
            }
            else
            {
                foreach (var item in source)
                {
                    if (!predicate(item, state)) continue;
                    if (found) throw new InvalidOperationException("The input sequence contains more than one element");
                    @default = item;
                    found = true;
                }
            }
            if (found) return @default;
            throw new InvalidOperationException("No element satisfies the condition in predicate");
        }
        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition or a default value if no such element exists; this method throws an exception if more than one element satisfies the condition.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TArg">The type of the state.</typeparam>
        /// <param name="source">An System.Collections.Generic.IEnumerable`1 to return a single element from.</param>
        /// <param name="predicate">A function to test an element for a condition.</param>
        /// <param name="state">State object instance</param>
        /// <returns>The single element of the input sequence that satisfies the condition, or default(TSource) if no such element is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource SingleOrDefault<TSource, TArg>(this IEnumerable<TSource> source, Func<TSource, TArg, bool> predicate, in TArg state)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var @default = default(TSource);
            var found = false;
            if (source is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                {
                    if (!predicate(sourceList[i], state)) continue;
                    if (found) throw new InvalidOperationException("The input sequence contains more than one element");
                    @default = sourceList[i];
                    found = true;
                }
            }
            else
            {
                foreach (var item in source)
                {
                    if (!predicate(item, state)) continue;
                    if (found) throw new InvalidOperationException("The input sequence contains more than one element");
                    @default = item;
                    found = true;
                }
            }
            return @default;
        }
        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <typeparam name="TArg">The type of the argument</typeparam>
        /// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
        /// <param name="enumerable">A sequence of values to invoke a transform function on.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="state">State object value</param>
        /// <returns>An System.Collections.Generic.IEnumerable`1 whose elements are the result of invoking the transform function on each element of source.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<TResult> Select<T, TArg, TResult>(this IEnumerable<T> enumerable, Func<T, TArg, TResult> selector, TArg state)
        {
            if (enumerable is IList<T> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                    yield return selector(sourceList[i], state);
            }
            else
            {
                foreach (var item in enumerable)
                    yield return selector(item, state);
            }
        }
        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <typeparam name="TArg">The type of the argument</typeparam>
        /// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
        /// <param name="enumerable">A sequence of values to invoke a transform function on.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="state">State object value</param>
        /// <returns>An System.Collections.Generic.IEnumerable`1 whose elements are the result of invoking the transform function on each element of source.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<TResult> Select<T, TArg, TResult>(this IEnumerable<T> enumerable, Func<T, int, TArg, TResult> selector, TArg state)
        {
            if (enumerable is IList<T> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                    yield return selector(sourceList[i], i, state);
            }
            else
            {
                var idx = 0;
                foreach (var item in enumerable)
                    yield return selector(item, idx++, state);
            }
        }

        // *** Where ***

        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TArg">The type of the state element.</typeparam>
        /// <param name="enumerable">An System.Collections.Generic.IEnumerable`1 to filter.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="state">State object instance.</param>
        /// <returns>An System.Collections.Generic.IEnumerable`1 that contains elements from the input sequence that satisfy the condition.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<TSource> Where<TSource, TArg>(this IEnumerable<TSource> enumerable, Func<TSource, TArg, bool> predicate, TArg state)
        {
            if (enumerable is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                    if (predicate(sourceList[i], state))
                        yield return sourceList[i];
            }
            else
            {
                foreach (var item in enumerable)
                    if (predicate(item, state))
                        yield return item;
            }
        }
        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TArg">The type of the Argument 1.</typeparam>
        /// <typeparam name="TArg2">The type of the Argument 2.</typeparam>
        /// <param name="enumerable">An System.Collections.Generic.IEnumerable`1 to filter.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="arg0">Argument 1.</param>
        /// <param name="arg1">Argument 2</param>
        /// <returns>An System.Collections.Generic.IEnumerable`1 that contains elements from the input sequence that satisfy the condition.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<TSource> Where<TSource, TArg, TArg2>(this IEnumerable<TSource> enumerable, Func<TSource, TArg, TArg2, bool> predicate, TArg arg0, TArg2 arg1)
        {
            if (enumerable is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                    if (predicate(sourceList[i], arg0, arg1))
                        yield return sourceList[i];
            }
            else
            {
                foreach (var item in enumerable)
                    if (predicate(item, arg0, arg1))
                        yield return item;
            }
        }
        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TArg">The type of the Argument 1.</typeparam>
        /// <typeparam name="TArg2">The type of the Argument 2.</typeparam>
        /// <typeparam name="TArg3">The type of the Argument 3.</typeparam>
        /// <param name="enumerable">An System.Collections.Generic.IEnumerable`1 to filter.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="arg0">Argument 1.</param>
        /// <param name="arg1">Argument 2</param>
        /// <param name="arg2">Argument 3</param>
        /// <returns>An System.Collections.Generic.IEnumerable`1 that contains elements from the input sequence that satisfy the condition.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<TSource> Where<TSource, TArg, TArg2, TArg3>(this IEnumerable<TSource> enumerable, Func<TSource, TArg, TArg2, TArg3, bool> predicate, TArg arg0, TArg2 arg1, TArg3 arg2)
        {
            if (enumerable is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                    if (predicate(sourceList[i], arg0, arg1, arg2))
                        yield return sourceList[i];
            }
            else
            {
                foreach (var item in enumerable)
                    if (predicate(item, arg0, arg1, arg2))
                        yield return item;
            }
        }
        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TArg">The type of the state element.</typeparam>
        /// <param name="enumerable">An System.Collections.Generic.IEnumerable`1 to filter.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="state">State object instance.</param>
        /// <returns>An System.Collections.Generic.IEnumerable`1 that contains elements from the input sequence that satisfy the condition.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<TSource> Where<TSource, TArg>(this IEnumerable<TSource> enumerable, Func<TSource, int, TArg, bool> predicate, TArg state)
        {
            if (enumerable is IList<TSource> sourceList)
            {
                var length = sourceList.Count;
                for (var i = 0; i < length; i++)
                    if (predicate(sourceList[i], i, state))
                        yield return sourceList[i];
            }
            else
            {
                var idx = 0;
                foreach (var item in enumerable)
                    if (predicate(item, idx++, state))
                        yield return item;
            }
        }
        #endregion
    }
}
