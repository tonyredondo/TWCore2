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

using System.Linq;
using TWCore.Diagnostics.Log;
using TWCore.Diagnostics.Log.Storages;
// ReSharper disable CheckNamespace

namespace TWCore
{
    /// <summary>
    /// ILogEngine extension methods
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Adds a console log storage instance to the log engine
        /// </summary>
        /// <param name="logEngine">Log engine</param>
        public static void AddConsoleStorage(this ILogEngine logEngine)
        {
            if (logEngine?.Storages?.GetAllStorages()?.Any(s => s is ConsoleLogStorage) == false)
                logEngine.Storages.Add(new ConsoleLogStorage());
        }
        /// <summary>
        /// Adds a console log storage instance to the log engine
        /// </summary>
        /// <param name="logEngine">Log engine</param>
        /// <param name="writeLevel">Write level for the log storage</param>
        public static void AddConsoleStorage(this ILogEngine logEngine, LogLevel writeLevel)
        {
            if (logEngine?.Storages?.GetAllStorages()?.Any(s => s is ConsoleLogStorage) == false)
                logEngine.Storages.Add(new ConsoleLogStorage(), writeLevel);
        }
        /// <summary>
        /// Adds a simple file log storage instance to the log engine
        /// </summary>
        /// <param name="logEngine">Log engine</param>
        /// <param name="fileName">File name with path</param>
        /// <param name="createByDay">True if a new log file is created each day; otherwise, false</param>
        public static void AddSimpleFileStorage(this ILogEngine logEngine, string fileName, bool createByDay = true)
        {
            if (logEngine?.Storages?.GetAllStorages()?.Any(s => s is SimpleFileLogStorage) == false)
                logEngine.Storages.Add(new SimpleFileLogStorage(fileName, createByDay));
        }
        /// <summary>
        /// Adds a simple file log storage instance to the log engine
        /// </summary>
        /// <param name="logEngine">Log engine</param>
        /// <param name="fileName">File name with path</param>
        /// <param name="createByDay">True if a new log file is created each day; otherwise, false</param>
        /// <param name="writeLevel">Write level for the log storage</param>
        public static void AddSimpleFileStorage(this ILogEngine logEngine, string fileName, bool createByDay, LogLevel writeLevel)
        {
            if (logEngine?.Storages?.GetAllStorages()?.Any(s => s is SimpleFileLogStorage) == false)
                logEngine.Storages.Add(new SimpleFileLogStorage(fileName, createByDay), writeLevel);
        }
		/// <summary>
		/// Adds an html log storage instance to the log engine
		/// </summary>
		/// <param name="logEngine">Log engine</param>
		/// <param name="fileName">File name with path</param>
		/// <param name="createByDay">True if a new log file is created each day; otherwise, false</param>
		public static void AddHtmlFileStorage(this ILogEngine logEngine, string fileName, bool createByDay = true)
		{
			if (logEngine?.Storages?.GetAllStorages()?.Any(s => s is HtmlFileLogStorage) == false)
				logEngine.Storages.Add(new HtmlFileLogStorage(fileName, createByDay));
		}
		/// <summary>
		/// Adds an html log storage instance to the log engine
		/// </summary>
		/// <param name="logEngine">Log engine</param>
		/// <param name="fileName">File name with path</param>
		/// <param name="createByDay">True if a new log file is created each day; otherwise, false</param>
		/// <param name="writeLevel">Write level for the log storage</param>
		public static void AddHtmlFileStorage(this ILogEngine logEngine, string fileName, bool createByDay, LogLevel writeLevel)
		{
			if (logEngine?.Storages?.GetAllStorages()?.Any(s => s is HtmlFileLogStorage) == false)
				logEngine.Storages.Add(new HtmlFileLogStorage(fileName, createByDay), writeLevel);
		}


        /// <summary>
        /// Adds an ElasticSearch log storage instance to the log engine
        /// </summary>
        /// <param name="logEngine">Log engine</param>
        /// <param name="url">ElasticSearch Json api url</param>
        /// <param name="indexName">Index name</param>
        public static void AddElasticSearchStorage(this ILogEngine logEngine, string url, string indexName)
        {
            if (logEngine?.Storages?.GetAllStorages()?.Any(s => s is ElasticSearchLogStorage) == false)
                logEngine.Storages.Add(new ElasticSearchLogStorage(url, indexName));
        }

        /// <summary>
        /// Adds an ElasticSearch log storage instance to the log engine
        /// </summary>
        /// <param name="logEngine">Log engine</param>
        /// <param name="url">ElasticSearch Json api url</param>
        /// <param name="indexName">Index name</param>
        /// <param name="writeLevel">Write level for the log storage</param>
        public static void AddElasticSearchStorage(this ILogEngine logEngine, string url, string indexName, LogLevel writeLevel)
        {
            if (logEngine?.Storages?.GetAllStorages()?.Any(s => s is ElasticSearchLogStorage) == false)
                logEngine.Storages.Add(new ElasticSearchLogStorage(url, indexName), writeLevel);
        }

    }
}
