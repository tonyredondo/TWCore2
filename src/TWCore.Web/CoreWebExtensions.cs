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

using Microsoft.Extensions.Logging;

namespace TWCore.Web
{
    /// <summary>
    /// TWCore Web Extensions
    /// </summary>
    public static class CoreWebExtensions
    {
        /// <summary>
        /// Adds the TWCore logger provider for Core logging system
        /// </summary>
        public static void AddTWCoreLogger(this ILoggerFactory loggerFactory)
            => loggerFactory.AddProvider(new TWCoreLoggerProvider());
    }
}
