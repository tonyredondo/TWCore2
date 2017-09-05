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

using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TWCore.Data
{
    /// <summary>
    /// Data access extensions methods
    /// </summary>
    public static class DataAccessExtensions
    {
        private static readonly ConcurrentDictionary<(Assembly, string), string> ResourceCache = new ConcurrentDictionary<(Assembly, string), string>();

		/// <summary>
		/// Gets the Sql query from a embedded resource
		/// </summary>
		/// <param name="dAccess">Associated data access</param>
		/// <param name="resourceName">Resource name</param>
		/// <returns>Sql query from the resources</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetSqlResource(this IDataAccess dAccess, string resourceName)
		{
			var assembly = Assembly.GetCallingAssembly();
			return ResourceCache.GetOrAdd((assembly, resourceName), key => GetStringResource(key.Item1, key.Item2));
		}
		/// <summary>
		/// Gets the Sql query from a embedded resource
		/// </summary>
		/// <param name="dAccess">Associated data access</param>
		/// <param name="assembly">Assembly containing the SQL embedded resources</param>
		/// <param name="resourceName">Resource name</param>
		/// <returns>Sql query from the resources</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetSqlResource(this IDataAccess dAccess, Assembly assembly, string resourceName)
			=> ResourceCache.GetOrAdd((assembly, resourceName), key => GetStringResource(key.Item1, key.Item2));
		/// <summary>
		/// Gets the Sql query from a embedded resource
		/// </summary>
		/// <param name="dAccess">Associated data access</param>
		/// <param name="resourceName">Resource name</param>
		/// <returns>Sql query from the resources</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetSqlResource(this IDataAccessAsync dAccess, string resourceName)
		{
			var assembly = Assembly.GetCallingAssembly();
			return ResourceCache.GetOrAdd((assembly, resourceName), key => GetStringResource(key.Item1, key.Item2));
		}
		/// <summary>
		/// Gets the Sql query from a embedded resource
		/// </summary>
		/// <param name="dAccess">Associated data access</param>
		/// <param name="assembly">Assembly containing the SQL embedded resources</param>
		/// <param name="resourceName">Resource name</param>
		/// <returns>Sql query from the resources</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetSqlResource(this IDataAccessAsync dAccess, Assembly assembly, string resourceName)
			=> ResourceCache.GetOrAdd((assembly, resourceName), key => GetStringResource(key.Item1, key.Item2));


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static string GetStringResource(Assembly assembly, string resourceName)
		{
			return assembly.GetResourceString(string.Format("Sql.{0}.sql", resourceName)) ??
				assembly.GetResourceString(string.Format("sql.{0}.sql", resourceName)) ??
				assembly.GetResourceString(string.Format("{0}.sql", resourceName));
		}
    }
}
