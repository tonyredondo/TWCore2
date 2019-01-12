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

using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
// ReSharper disable UnusedMember.Global

namespace TWCore.Reflection
{
    /// <summary>
    /// DuckType Container for Proxy objects
    /// </summary>
    public abstract class DuckTypeProxy
	{
		#region Private fields
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private ConcurrentDictionary<string, MethodAccessorDelegate> _methodsDelegates;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private ConcurrentDictionary<string, FastPropertyInfo> _propertyInfo;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private ConcurrentDictionary<(string Method, string Types), CallSite> _dynMethodDelegates;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private object _realObject;
		#endregion

		#region Properties
		/// <summary>
		/// Get current object
		/// </summary>
		public object RealObject => _realObject;
		#endregion

		#region Private Methods
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void SetRealObject(object realObject)
		{
			if (realObject is null) return;
			_realObject = realObject;
			var containerType = realObject.GetType();
			if (_realObject is DynamicObject)
			{
				_dynMethodDelegates = new ConcurrentDictionary<(string, string), CallSite>();
			}
			else
			{
				_methodsDelegates = new ConcurrentDictionary<string, MethodAccessorDelegate>();
				var methods = containerType.GetMethods();
				for (var i = 0; i < methods.Length; i++)
				{
					var method = methods[i];
					var accessor = method.GetMethodAccessor();
					method.GetParameters();
					_methodsDelegates.TryAdd(method.Name, accessor);
				}
			}
			_propertyInfo = new ConcurrentDictionary<string, FastPropertyInfo>();
			var properties = containerType.GetProperties();
			for (var i = 0; i < properties.Length; i++)
			{
				var property = properties[i];
				var fastProperty = property.GetFastPropertyInfo();
				_propertyInfo.TryAdd(property.Name, fastProperty);
			}
		}
		#endregion

		#region Protected Methods
		/// <summary>
		/// Calls a method on the real object dynamically
		/// </summary>
		/// <param name="methodName">Method name</param>
		/// <param name="args">Method parameters</param>
		/// <returns>Method return value</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public object CallMethod(string methodName, params object[] args)
		{
            args = args ?? Array.Empty<object>();

			if (_methodsDelegates != null && _methodsDelegates.TryGetValue(methodName, out var mDelegate))
				return mDelegate(_realObject, args);

			if (!(_realObject is DynamicObject dContainer))
				throw new MissingMethodException(_realObject?.GetType().Name, methodName);

			var types = new List<Type>(args.Length);
			var typesKey = string.Empty;
			for (var i = 0; i < args.Length; i++)
			{
				var arg = args[i];
				var type = arg?.GetType() ?? typeof(object);
				types.Add(type);
				typesKey += type.FullName;
			}

			var callSite = _dynMethodDelegates.GetOrAdd((methodName, typesKey), mName =>
			{
				var csArgs = Enumerable.Range(0, args.Length + 1)
									   .Select(i => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null));

				var cSiteBinder = Microsoft.CSharp.RuntimeBinder.Binder.InvokeMember(CSharpBinderFlags.None, mName.Method, types, GetType(), csArgs);
					
				switch (args.Length)
				{
					case 0:
						return CallSite<Func<CallSite, object, object>>.Create(cSiteBinder);
					case 1:
						return CallSite<Func<CallSite, object, object, object>>.Create(cSiteBinder);
					case 2:
						return CallSite<Func<CallSite, object, object, object, object>>.Create(cSiteBinder);
					case 3:
						return CallSite<Func<CallSite, object, object, object, object, object>>.Create(cSiteBinder);
					case 4:
						return CallSite<Func<CallSite, object, object, object, object, object, object>>.Create(cSiteBinder);
					case 5:
						return CallSite<Func<CallSite, object, object, object, object, object, object, object>>.Create(cSiteBinder);
					case 6:
						return CallSite<Func<CallSite, object, object, object, object, object, object, object, object>>.Create(cSiteBinder);
					case 7:
						return CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object>>.Create(cSiteBinder);
					case 8:
						return CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object>>.Create(cSiteBinder);
					case 9:
						return CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object, object>>.Create(cSiteBinder);
					case 10:
						return CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object>>.Create(cSiteBinder);
				}
				return null;
			});

			switch (args.Length)
			{
				case 0:
					return ((CallSite<Func<CallSite, object, object>>)callSite).Target(callSite, dContainer);
				case 1:
					return ((CallSite<Func<CallSite, object, object, object>>)callSite).Target(callSite, dContainer, args[0]);
				case 2:
					return ((CallSite<Func<CallSite, object, object, object, object>>)callSite).Target(callSite, dContainer, args[0], args[1]);
				case 3:
					return ((CallSite<Func<CallSite, object, object, object, object, object>>)callSite).Target(callSite, dContainer, args[0], args[1], args[2]);
				case 4:
					return ((CallSite<Func<CallSite, object, object, object, object, object, object>>)callSite).Target(callSite, dContainer, args[0], args[1], args[2], args[3]);
				case 5:
					return ((CallSite<Func<CallSite, object, object, object, object, object, object, object>>)callSite).Target(callSite, dContainer, args[0], args[1], args[2], args[3], args[4]);
				case 6:
					return ((CallSite<Func<CallSite, object, object, object, object, object, object, object, object>>)callSite).Target(callSite, dContainer, args[0], args[1], args[2], args[3], args[4], args[5]);
				case 7:
					return ((CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object>>)callSite).Target(callSite, dContainer, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
				case 8:
					return ((CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object>>)callSite).Target(callSite, dContainer, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
				case 9:
					return ((CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object, object>>)callSite).Target(callSite, dContainer, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
				case 10:
					return ((CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object>>)callSite).Target(callSite, dContainer, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]);
			}
			return null;
		}
		/// <summary>
		/// Get a Property value from the real object dynamically
		/// </summary>
		/// <param name="propertyName">Property name</param>
		/// <returns>Property value</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected object GetProperty(string propertyName)
		{
			if (!_propertyInfo.TryGetValue(propertyName, out var fProperty))
				throw new MissingMemberException(_realObject?.GetType().Name, propertyName);
			return fProperty.GetValue(_realObject);
		}
		/// <summary>
		/// Set a Property value to the real object dynamically
		/// </summary>
		/// <param name="propertyName">Property name</param>
		/// <param name="value">Property value</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void SetProperty(string propertyName, object value)
		{
			if (!_propertyInfo.TryGetValue(propertyName, out var fProperty))
				throw new MissingMemberException(_realObject?.GetType().Name, propertyName);
			fProperty.SetValue(_realObject, value);
		}
		#endregion

		#region Public Static Methods
		/// <summary>
		/// Create a new Proxy object using an interface for ducktyping.
		/// </summary>
		/// <typeparam name="T">Type of interface</typeparam>
		/// <param name="realObject">Object to be proxied</param>
		/// <returns>Proxy instance</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Create<T>(object realObject)
			=> (T)(object)Create(typeof(T), realObject);
		/// <summary>
		/// Create a new Proxy object using an interface for ducktyping.
		/// </summary>
		/// <param name="interfaceType">Type of interface</param>
		/// <param name="realObject">Object to be proxied</param>
		/// <returns>Proxy instance</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static DuckTypeProxy Create(Type interfaceType, object realObject)
		{
			if (interfaceType is null)
				throw new ArgumentNullException(nameof(interfaceType), "The Interface type can't be null");
			if (realObject is null)
				throw new ArgumentNullException(nameof(realObject), "The original object can't be null");
			if (realObject is Task)
				throw new ArgumentNullException(nameof(realObject), "The original object can't be a Task");

			var typeSignature = "ProxyTo" + realObject.GetType().Name;
			//Create Type
			var an = new AssemblyName(typeSignature + "Assembly");
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
			var typeBuilder = moduleBuilder.DefineType(typeSignature, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
				typeof(DuckTypeProxy), new[] { interfaceType });
			//var constructor = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
			typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

			//Create Members
			CreateInterfaceMethods(interfaceType, typeBuilder);
			CreateInterfaceProperties(interfaceType, typeBuilder);

			//Create Type instance
			var type = typeBuilder.CreateTypeInfo().AsType();
			var objType = (DuckTypeProxy)Activator.CreateInstance(type);
			objType.SetRealObject(realObject);
			return objType;
		}
		#endregion

		#region Private Static Methods
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void CreateInterfaceProperties(Type interfaceType, TypeBuilder typeBuilder)
		{
			var getPropertyMethod = typeof(DuckTypeProxy).GetMethod("GetProperty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);
			var setPropertyMethod = typeof(DuckTypeProxy).GetMethod("SetProperty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(object) }, null);
			var debAttrCtor = typeof(DebuggerBrowsableAttribute).GetConstructor(new[] { typeof(DebuggerBrowsableState) });
			var interfaceProperties = interfaceType.GetProperties();
			for (var i = 0; i < interfaceProperties.Length; i++)
			{
				var iProperty = interfaceProperties[i];
				PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(iProperty.Name, PropertyAttributes.HasDefault, iProperty.PropertyType, null);
				propertyBuilder.SetCustomAttribute(new CustomAttributeBuilder(debAttrCtor, new object[] { DebuggerBrowsableState.Never }));
				if (iProperty.CanRead)
				{
					MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod("get_" + iProperty.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, iProperty.PropertyType, Type.EmptyTypes);
					var gen = getPropMthdBldr.GetILGenerator();
					var label = gen.DefineLabel();
					gen.Emit(OpCodes.Nop);
					gen.Emit(OpCodes.Ldarg_0);
					gen.Emit(OpCodes.Ldstr, iProperty.Name);
					gen.Emit(OpCodes.Call, getPropertyMethod);
				    gen.Emit(iProperty.PropertyType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, iProperty.PropertyType);
				    gen.Emit(OpCodes.Stloc_0);
					gen.Emit(OpCodes.Br_S, label);
					gen.MarkLabel(label);
					gen.Emit(OpCodes.Ldloc_0);
					gen.Emit(OpCodes.Ret);
					propertyBuilder.SetGetMethod(getPropMthdBldr);
				}
				if (iProperty.CanWrite)
				{
					MethodBuilder setPropMthdBldr = typeBuilder.DefineMethod("set_" + iProperty.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new[] { iProperty.PropertyType });
					var gen = setPropMthdBldr.GetILGenerator();
					gen.Emit(OpCodes.Nop);
					gen.Emit(OpCodes.Ldarg_0);
					gen.Emit(OpCodes.Ldstr, iProperty.Name);
					gen.Emit(OpCodes.Ldarg_1);
					if (iProperty.PropertyType.IsValueType)
						gen.Emit(OpCodes.Box, iProperty.PropertyType);
					gen.Emit(OpCodes.Call, setPropertyMethod);
					gen.Emit(OpCodes.Nop);
					gen.Emit(OpCodes.Ret);
					propertyBuilder.SetSetMethod(setPropMthdBldr);
				}
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void CreateInterfaceMethods(Type interfaceType, TypeBuilder typeBuilder)
		{
			var callMethod = typeof(DuckTypeProxy).GetMethod("CallMethod", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(object[]) }, null);
			var interfaceMethods = interfaceType.GetMethods();
			for (var i = 0; i < interfaceMethods.Length; i++)
			{
				var iMethod = interfaceMethods[i];
				var parameters = iMethod.GetParameters();

				var paramBuilders = new ParameterBuilder[parameters.Length];
				var methodBuilder = typeBuilder.DefineMethod(iMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot, iMethod.ReturnType, parameters.Select(p => p.ParameterType).ToArray());
				for (var j = 0; j < parameters.Length; j++)
					paramBuilders[j] = methodBuilder.DefineParameter(j, ParameterAttributes.None, parameters[j].Name);

				var gen = methodBuilder.GetILGenerator();

				//LocalBuilder loc = null;
				Label label = new Label();
				if (iMethod.ReturnType != typeof(void))
				{
					//loc = gen.DeclareLocal(iMethod.ReturnType);
				    gen.DeclareLocal(iMethod.ReturnType);
                    label = gen.DefineLabel();
				}

				gen.Emit(OpCodes.Nop);
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldstr, iMethod.Name);
				if (parameters.Length == 0) gen.Emit(OpCodes.Ldc_I4_0);
				if (parameters.Length == 1) gen.Emit(OpCodes.Ldc_I4_1);
				if (parameters.Length == 2) gen.Emit(OpCodes.Ldc_I4_2);
				if (parameters.Length == 3) gen.Emit(OpCodes.Ldc_I4_3);
				if (parameters.Length == 4) gen.Emit(OpCodes.Ldc_I4_4);
				if (parameters.Length == 5) gen.Emit(OpCodes.Ldc_I4_5);
				if (parameters.Length == 6) gen.Emit(OpCodes.Ldc_I4_6);
				if (parameters.Length == 7) gen.Emit(OpCodes.Ldc_I4_7);
				if (parameters.Length == 8) gen.Emit(OpCodes.Ldc_I4_8);
				if (parameters.Length == 9) gen.Emit(OpCodes.Ldc_I4_S, 9);
				gen.Emit(OpCodes.Newarr, typeof(object));

				if (parameters.Length > 0)
				{

					for (var j = 0; j < parameters.Length; j++)
					{
						gen.Emit(OpCodes.Dup);
						switch (j)
						{
							case 0:
								gen.Emit(OpCodes.Ldc_I4_0);
								gen.Emit(OpCodes.Ldarg_1);
								break;
							case 1:
								gen.Emit(OpCodes.Ldc_I4_1);
								gen.Emit(OpCodes.Ldarg_2);
								break;
							case 2:
								gen.Emit(OpCodes.Ldc_I4_2);
								gen.Emit(OpCodes.Ldarg_3);
								break;
							case 3:
								gen.Emit(OpCodes.Ldc_I4_3);
								gen.Emit(OpCodes.Ldarg_S, j + 1);
								break;
							case 4:
								gen.Emit(OpCodes.Ldc_I4_4);
								gen.Emit(OpCodes.Ldarg_S, j + 1);
								break;
							case 5:
								gen.Emit(OpCodes.Ldc_I4_5);
								gen.Emit(OpCodes.Ldarg_S, j + 1);
								break;
							case 6:
								gen.Emit(OpCodes.Ldc_I4_6);
								gen.Emit(OpCodes.Ldarg_S, j + 1);
								break;
							case 7:
								gen.Emit(OpCodes.Ldc_I4_7);
								gen.Emit(OpCodes.Ldarg_S, j + 1);
								break;
							case 8:
								gen.Emit(OpCodes.Ldc_I4_8);
								gen.Emit(OpCodes.Ldarg_S, j + 1);
								break;
						}
						if (parameters[j].ParameterType.IsValueType)
							gen.Emit(OpCodes.Box, parameters[j].ParameterType);
						gen.Emit(OpCodes.Stelem_Ref);
					}
				}
				gen.Emit(OpCodes.Call, callMethod);
				if (iMethod.ReturnType != typeof(void))
				{
				    gen.Emit(iMethod.ReturnType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, iMethod.ReturnType);
				    gen.Emit(OpCodes.Stloc_0);
					gen.Emit(OpCodes.Br_S, label);
					gen.MarkLabel(label);
					gen.Emit(OpCodes.Ldloc_0);
				}
				else
					gen.Emit(OpCodes.Pop);

				gen.Emit(OpCodes.Ret);
			}
		}
		#endregion
	}
}
