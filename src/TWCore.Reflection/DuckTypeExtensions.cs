using System;
using TWCore.Reflection;

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
		/// <typeparam name="T">Interface type to proxy</typeparam>
		/// <param name="obj">Real object instance</param>
		/// <returns>Proxy object instance</returns>
		public static DuckTypeProxy GetDuckTypeProxy(this object obj, Type interfaceType)
		{
			if (obj is DuckTypeProxy)
				return obj as DuckTypeProxy;
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
			if (proxy == null) return null;
			if (proxy is DuckTypeProxy dtProxy)
				return dtProxy.RealObject as T;
			return proxy as T;
		}
	}
}
