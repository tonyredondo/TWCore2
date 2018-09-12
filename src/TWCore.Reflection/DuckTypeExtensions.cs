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
using TWCore.Reflection;
// ReSharper disable CheckNamespace

namespace TWCore
{
    /// <summary>
    /// DuckType extensions
    /// </summary>
    public static class DuckTypeExtensions
	{
		/// <summary>
		/// Creates a new proxy object arround the current object using an interface type for duck typing
		/// </summary>
		/// <typeparam name="T">Interface type to proxy</typeparam>
		/// <param name="obj">Real object instance</param>
		/// <returns>Proxy object instance</returns>
		public static T ActAs<T>(this object obj) where T : class
		{
			if (obj is T && obj is DuckTypeProxy)
				return obj as T;
			return DuckTypeProxy.Create<T>(obj);
		}
	    /// <summary>
	    /// Creates a new proxy object arround the current object using an interface type for duck typing
	    /// </summary>
	    /// <param name="obj">Real object instance</param>
	    /// <param name="interfaceType">Interface type to proxy</param>
	    /// <returns>Proxy object instance</returns>
	    public static DuckTypeProxy GetDuckTypeProxy(this object obj, Type interfaceType)
		{
			if (obj is DuckTypeProxy proxy)
				return proxy;
			return DuckTypeProxy.Create(interfaceType, obj);
		}
		/// <summary>
		/// Gets the original object from the proxy instance
		/// </summary>
		/// <typeparam name="T">Type of real object</typeparam>
		/// <param name="proxy">Proxy object instance</param>
		/// <returns>Original object instance</returns>
		public static T UnwrapProxy<T>(this object proxy) where T : class
		{
			switch (proxy)
			{
			    case null:
			        return null;
			    case DuckTypeProxy dtProxy:
			        return dtProxy.RealObject as T;
			}
		    return proxy as T;
		}
	}
}
