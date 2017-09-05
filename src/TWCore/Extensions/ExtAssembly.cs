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

using System.IO;
using System.Linq;
using System.Reflection;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace

namespace TWCore
{
    /// <summary>
    /// Helper to retrieve embedded resources.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Gets the stream object from an embedded resource.
        /// </summary>
        /// <param name="assembly">Assembly with the embedded resources</param>
        /// <param name="resourceName">Resource name to extract</param>
        /// <returns>Stream object with the resource content</returns>
        public static Stream GetResourceStream(this Assembly assembly, string resourceName)
        {
            var currentAssembly = assembly;
            if (currentAssembly == null || !resourceName.IsNotNullOrEmpty()) return null;
            var asmNames = currentAssembly.GetManifestResourceNames();
            var validResourceName = asmNames.FirstOrDefault(name => name.EndsWith(resourceName));
            return validResourceName != null ? 
                currentAssembly.GetManifestResourceStream(validResourceName) : 
                null;
        }
        /// <summary>
        /// Gets the string object from an embedded resource
        /// </summary>
        /// <param name="assembly">Assembly with the embedded resources</param>
        /// <param name="resourceName">Resource name to extract</param>
        /// <returns>String object value with the resource content</returns>
        public static string GetResourceString(this Assembly assembly, string resourceName) 
            => GetResourceStream(assembly, resourceName).TextReadToEnd();
    }
}
