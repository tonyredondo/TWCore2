﻿/*
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TWCore.Diagnostics.Counters;
using TWCore.Net.RPC.Attributes;
using TWCore.Security;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Net.RPC.Descriptors
{
    /// <summary>
    /// Defines a RPC Service
    /// </summary>
    [Serializable, DataContract]
    public class ServiceDescriptor
    {
        private static readonly IHash Hash = HashManager.Get("SHA1");

        /// <summary>
        /// Service name
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        /// Service methods
        /// </summary>
        [DataMember]
		public Dictionary<Guid, MethodDescriptor> Methods { get; set; } = new Dictionary<Guid, MethodDescriptor>();
        /// <summary>
        /// Service events
        /// </summary>
        [DataMember]
		public Dictionary<string, EventDescriptor> Events { get; set; } = new Dictionary<string, EventDescriptor>(StringComparer.Ordinal);
        /// <summary>
        /// Service types
        /// </summary>
        [DataMember]
		public Dictionary<string, TypeDescriptor> Types { get; set; } = new Dictionary<string, TypeDescriptor>(StringComparer.Ordinal);


        /// <summary>
        /// Gets a service descriptor from a service type
        /// </summary>
        /// <param name="serviceType">Object service type for descriptor creation</param>
        /// <param name="counterCategory">Counter category</param>
        /// <param name="counterLevel">Counter level</param>
        /// <param name="counterKind">Counter kind</param>
        /// <returns>Service descriptor instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ServiceDescriptor GetDescriptor(Type serviceType, string counterCategory = null, CounterLevel counterLevel = CounterLevel.User, CounterKind counterKind = CounterKind.Application)
        {
            var descriptor = new ServiceDescriptor
            {
                Name = serviceType.FullName
            };
            var isInterface = serviceType.IsInterface;
            var methodsInfo = serviceType.AllMethods().OrderBy(i => i.Name + "[" + i.GetParameters()?.Select(p => p.Name).Join(", ") + "]");
            foreach (var mInfo in methodsInfo)
            {
                if (!mInfo.IsPublic || mInfo.IsSpecialName) continue;
                var isRpcMethod = mInfo.IsDefined(typeof(RPCMethodAttribute)) || isInterface;
                if (!isRpcMethod) continue;
                var mDesc = new MethodDescriptor
                {
                    Method = mInfo.GetMethodAccessor(),
                    Name = mInfo.Name,
                    ReturnType = GetTypeName(mInfo.ReturnType),
                    TypeOfReturnType = mInfo.ReturnType
                };
                if (mInfo.ReturnType == typeof(Task) || mInfo.ReturnType.BaseType == typeof(Task))
                {
                    mDesc.ReturnIsTask = true;
                    if (mInfo.ReturnType.GenericTypeArguments.Length > 0)
                    {
                        mDesc.ReturnTaskResult = mInfo.ReturnType.GetProperty("Result").GetFastPropertyInfo();
                        mDesc.CreateTaskFromResult = typeof(Task).GetMethod("FromResult").MakeGenericMethod(new[] { mInfo.ReturnType.GenericTypeArguments[0] }).GetMethodAccessor();
                    }
                    else
                    {
                        mDesc.ReturnTaskResult = null;
                        mDesc.CreateTaskFromResult = null;
                    }
                }
                if (counterCategory != null)
                    mDesc.Counter = Core.Counters.GetDoubleCounter(counterCategory, serviceType.FullName + "\\" + mDesc.Name, CounterType.Average, counterLevel, counterKind, CounterUnit.Milliseconds);

                RegisterServiceDescriptorType(descriptor, mInfo.ReturnType);

                var pars = mInfo.GetParameters();
                mDesc.Parameters = new ParameterDescriptor[pars.Length];
                for (var i = 0; i < pars.Length; i++)
                {
                    var p = pars[i];
                    var pDes = new ParameterDescriptor
                    {
                        Parameter = p,
                        Name = p.Name,
                        Index = p.Position,
                        Type = GetTypeName(p.ParameterType)
                    };
                    RegisterServiceDescriptorType(descriptor, p.ParameterType);
                    mDesc.Parameters[i] = pDes;
                }
                mDesc.Id = GetMethodId(descriptor, mDesc);
                descriptor.Methods.Add(mDesc.Id, mDesc);
            }

            var eventsInfo = serviceType.GetRuntimeEvents();
            foreach (var eInfo in eventsInfo)
            {
                if (eInfo.IsSpecialName) continue;
                var isRpcEvent = eInfo.IsDefined(typeof(RPCEventAttribute)) || isInterface;
                if (!isRpcEvent) continue;
                var name = eInfo.Name;
                var eventHandler = GetTypeName(eInfo.EventHandlerType);
                RegisterServiceDescriptorType(descriptor, eInfo.EventHandlerType);
                descriptor.Events.Add(name, new EventDescriptor { Name = name, Type = eventHandler, Event = eInfo });
            }
            return descriptor;
        }

        #region Private methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Guid GetMethodId(ServiceDescriptor serviceDescriptor, MethodDescriptor methodDescriptor)
        {
            var sb = new StringBuilder();
            sb.Append(serviceDescriptor.Name + "." + methodDescriptor.Name);
            sb.Append("(");
            if (methodDescriptor.Parameters != null)
                foreach (var p in methodDescriptor.Parameters)
                    sb.Append(string.Format(" {0}[{1}] ", p.Name, p.Type));
            sb.Append(") : " + methodDescriptor.ReturnType);
			return Hash.GetGuid(sb.ToString());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetTypeName(Type type)
        {
            var genericTypes = new List<string>();
            if (!type.IsConstructedGenericType) return type.FullName;
            type.GenericTypeArguments.Each(a => genericTypes.Add(a.FullName));
            return type.Name + "[" + genericTypes.Join(", ") + "]";
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RegisterServiceDescriptorType(ServiceDescriptor descriptor, Type type)
        {
            if (type.IsConstructedGenericType)
                type.GenericTypeArguments.Each(t => RegisterServiceDescriptorType(descriptor, t));

            var typeInfo = type.GetTypeInfo();
            var asmName = typeInfo.Assembly.GetName();

            if (asmName.Name != "mscorlib" && asmName.Name != "System.Private.CoreLib")
            {
                var name = GetTypeName(type);
                if (descriptor.Types.ContainsKey(name)) return;

                var tDesc = new TypeDescriptor { Name = name, FullName = type.AssemblyQualifiedName };
                descriptor.Types.Add(name, tDesc);
                var props = type.GetRuntimeProperties().ToArray();
                tDesc.Properties = new PropertyDescriptor[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    var p = props[i];
                    var pDesc = new PropertyDescriptor
                    {
                        Name = p.Name,
                        Type = GetTypeName(p.PropertyType)
                    };
                    RegisterServiceDescriptorType(descriptor, p.PropertyType);
                    tDesc.Properties[i] = pDesc;
                }
            }
            else
            {
                if (typeInfo.GenericTypeArguments.Any(t => t.Assembly.GetName().Name != "mscorlib" && t.Assembly.GetName().Name != "System.Private.CoreLib"))
                {
                    var name = GetTypeName(type);
                    if (descriptor.Types.ContainsKey(name)) return;

                    var tDesc = new TypeDescriptor { Name = name, FullName = type.AssemblyQualifiedName };
                    descriptor.Types.Add(name, tDesc);
                }
            }
        }
        #endregion
    }
}
