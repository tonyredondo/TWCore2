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

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TWCore
{
    /// <summary>
    /// Creates a singleton instance from a normal object
    /// </summary>
    public static class Singleton
    {
        /// <summary>
        /// Creates a singleton instance from a normal object
        /// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T InstanceOf<T>() => Singleton<T>.Instance;
    }
    /// <summary>
    /// Creates a singleton instance from a normal object
    /// </summary>
    /// <typeparam name="T">Object Type to create the singleton instance</typeparam>
    public static class Singleton<T>
    {
        /// <summary>
        /// Single Instance
        /// </summary>
        public static T Instance => InstanceBox.Value;

        private static class InstanceBox
        {
            public static readonly T Value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static InstanceBox()
            {
				var type = typeof(T);
				try 
				{
	                var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
	                foreach (var ctor in constructors)
	                {
	                    if (ctor.GetParameters().Any(p => !p.HasDefaultValue) == false)
	                    {
	                        Value = (T)ctor.Invoke(new object[0]);
	                        return;
	                    }
	                }
	                Value = Activator.CreateInstance<T>();
				}
				catch(Exception ex)
				{
					Core.Log.Write(ex);
					Core.Log.Error(ex, "Error creating type: " + type.FullName);
				}
            }
        }
    }
}
