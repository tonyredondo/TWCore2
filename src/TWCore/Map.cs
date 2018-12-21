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
using System.Linq.Expressions;

namespace TWCore
{
    /// <summary>
    /// Maps an Object Type to Another Type
    /// </summary>
    /// <typeparam name="TFrom">Source Type</typeparam>
    /// <typeparam name="TTo">Destination Type</typeparam>
    public static class Map<TFrom, TTo>
    {
        /// <summary>
        /// Mapping function
        /// </summary>
        private static Func<TFrom, TTo> MapFunc;

        /// <summary>
        /// Sets the Map expression to convert an object from one Type to another
        /// </summary>
        /// <param name="mapExpression">Map expression</param>
        public static void Set(Expression<Func<TFrom, TTo>> mapExpression)
        {
            if (mapExpression == null)
                throw new ArgumentNullException("mapExpression", "The expression can't be null");
            MapFunc = mapExpression.Compile();
        }

        /// <summary>
        /// Apply Mapping
        /// </summary>
        /// <param name="from">Object source instance</param>
        /// <param name="handleDefault">Indicate if the default value is handled automatically</param>
        /// <returns>Object destination instance </returns>
        public static TTo Apply(TFrom from, bool handleDefault = true)
        {
            if (MapFunc == null)
                throw new NullReferenceException($"A Map func can't be found to cast the item of type {typeof(TFrom).FullName} to {typeof(TTo).FullName}.");
            if (handleDefault && from == default)
                return default;
            return MapFunc(from);
        }
    }
}
