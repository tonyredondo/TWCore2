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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using TWCore.Net.RPC.Attributes;
using TWCore.Security;

namespace TWCore.Net.RPC.Descriptors
{
    /// <summary>
    /// Defines a RPC Service
    /// </summary>
    [Serializable, DataContract]
    public class ServiceDescriptor
    {
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
        /// <returns>Service descriptor instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ServiceDescriptor GetDescriptor(Type serviceType)
        {
            var descriptor = new ServiceDescriptor()
            {
                Name = serviceType.FullName
            };
            bool isInterface = serviceType.GetTypeInfo().IsInterface;
            var methodsInfo = serviceType.GetRuntimeMethods().OrderBy(i => i.Name + "[" + i.GetParameters()?.Select(p => p.Name).Join(", ") + "]");
            foreach (var mInfo in methodsInfo)
            {
                if (mInfo.IsPublic && !mInfo.IsSpecialName)
                {
                    var isRPCMethod = mInfo.IsDefined(typeof(RPCMethodAttribute)) || isInterface;
                    if (isRPCMethod)
                    {
                        var mDesc = new MethodDescriptor()
                        {
                            Method = mInfo.GetMethodAccessor(),
                            Name = mInfo.Name,
                            ReturnType = GetTypeName(mInfo.ReturnType)
                        };
                        RegisterServiceDescriptorType(descriptor, mInfo.ReturnType);

                        var pars = mInfo.GetParameters();
                        mDesc.Parameters = new ParameterDescriptor[pars.Length];
                        for (var i = 0; i < pars.Length; i++)
                        {
                            var p = pars[i];
                            var pDes = new ParameterDescriptor()
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
                }
            }

            var eventsInfo = serviceType.GetRuntimeEvents();
            foreach (var eInfo in eventsInfo)
            {
                if (!eInfo.IsSpecialName)
                {
                    var isRPCEvent = eInfo.IsDefined(typeof(RPCEventAttribute)) || isInterface;
                    if (isRPCEvent)
                    {
                        var name = eInfo.Name;
                        var eventHandler = GetTypeName(eInfo.EventHandlerType);
                        RegisterServiceDescriptorType(descriptor, eInfo.EventHandlerType);
                        descriptor.Events.Add(name, new EventDescriptor { Name = name, Type = eventHandler, Event = eInfo });
                    }
                }
            }
            return descriptor;
        }

        #region Private methods
        static IHash hash = HashManager.Get("SHA1");
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Guid GetMethodId(ServiceDescriptor serviceDescriptor, MethodDescriptor methodDescriptor)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(serviceDescriptor.Name + "." + methodDescriptor.Name);
            sb.Append("(");
            if (methodDescriptor.Parameters != null)
                foreach (var p in methodDescriptor.Parameters)
                    sb.Append(string.Format(" {0}[{1}] ", p.Name, p.Type));
            sb.Append(") : " + methodDescriptor.ReturnType);
			return hash.GetGuid(sb.ToString());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static string GetTypeName(Type type)
        {
            var genericTypes = new List<string>();
            if (type.IsConstructedGenericType)
            {
                type.GenericTypeArguments.Each(a => genericTypes.Add(a.FullName));
                return type.Name + "[" + genericTypes.Join(", ") + "]";
            }
            else
                return type.FullName;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void RegisterServiceDescriptorType(ServiceDescriptor descriptor, Type type)
        {
            if (type.IsConstructedGenericType)
                type.GenericTypeArguments.Each(t => RegisterServiceDescriptorType(descriptor, t));

            var typeInfo = type.GetTypeInfo();
            var asmName = typeInfo.Assembly.GetName();

            if (asmName.Name != "mscorlib")
            {
                var name = GetTypeName(type);
                if (!descriptor.Types.ContainsKey(name))
                {
                    var tDesc = new TypeDescriptor { Name = name, FullName = type.AssemblyQualifiedName };
                    descriptor.Types.Add(name, tDesc);
                    var props = type.GetRuntimeProperties().ToArray();
                    tDesc.Properties = new PropertyDescriptor[props.Length];
                    for (var i = 0; i < props.Length; i++)
                    {
                        var p = props[i];
                        var pDesc = new PropertyDescriptor()
                        {
                            Name = p.Name,
                            Type = GetTypeName(p.PropertyType)
                        };
                        RegisterServiceDescriptorType(descriptor, p.PropertyType);
                        tDesc.Properties[i] = pDesc;
                    }
                }
            }
            else
            {
                if (typeInfo.GenericTypeArguments.Any(t => t.GetTypeInfo().Assembly.GetName().Name != "mscorlib"))
                {
                    var name = GetTypeName(type);
                    if (!descriptor.Types.ContainsKey(name))
                    {
                        var tDesc = new TypeDescriptor { Name = name, FullName = type.AssemblyQualifiedName };
                        descriptor.Types.Add(name, tDesc);
                    }
                }
            }
        }
        #endregion
    }
}
