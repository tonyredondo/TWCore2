﻿/*
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TWCore.Collections;
using TWCore.Diagnostics.Log;
using TWCore.Text;

namespace TWCore.Injector
{
    /// <summary>
    /// Injector delegate for type instance resolve.
    /// </summary>
    /// <param name="type">Object type</param>
    /// <param name="name">Name of the instance</param>
    /// <returns>Instance of the object type</returns>
    public delegate object TypeInstanceResolverDelegate(Type type, string name);

    /// <summary>
    /// Inject instances for non instantiable class
    /// </summary>
    public class InjectorEngine : IDisposable
    {
        readonly ConcurrentDictionary<(Type, string), RegisteredValues> RegisteredDelegates = new ConcurrentDictionary<(Type, string), RegisteredValues>();
        readonly ConcurrentDictionary<Instantiable, ActivatorItem> InstantiableCache = new ConcurrentDictionary<Instantiable, ActivatorItem>();
        static readonly string[] EmptyStringArray = new string[0];
        InjectorSettings settings;
        bool attributesRegistered;
        bool useOnlyLoadedAssemblies = true;

        #region Events
        /// <summary>
        /// Event occurs when a new instance is requested for a non instantiable type
        /// </summary>
        public event TypeInstanceResolverDelegate OnTypeInstanceResolve;
        #endregion

        #region Properties
        /// <summary>
        /// Injector settings
        /// </summary>
        public InjectorSettings Settings
        {
            get
            {
                return settings;
            }
            set
            {
                settings = value;
                attributesRegistered = false;
            }
        }
        /// <summary>
        /// Use only assemblies loaded, false if uses all assemblies in the folder
        /// </summary>
        public bool UseOnlyLoadedAssemblies
        {
            get
            {
                return useOnlyLoadedAssemblies;
            }
            set
            {
                useOnlyLoadedAssemblies = value;
                attributesRegistered = false;
            }
        }
        #endregion

        #region .ctor
        /// <summary>
        /// Inject instances for non instantiable class
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InjectorEngine()
        {
            UseOnlyLoadedAssemblies = Core.GlobalSettings.InjectorUseOnlyLoadedAssemblies;
            Settings = new InjectorSettings();
        }
        /// <summary>
        /// Inject instances for non instantiable class
        /// </summary>
        /// <param name="settings">Injector settings</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InjectorEngine(InjectorSettings settings)
        {
            UseOnlyLoadedAssemblies = Core.GlobalSettings.InjectorUseOnlyLoadedAssemblies;
            Settings = settings;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~InjectorEngine()
        {
            Dispose();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Create a new instance for a Interface, Abstract, or setted class
        /// </summary>
        /// <typeparam name="T">Type of object to create</typeparam>
        /// <param name="name">Name of the instance, if is null the default one</param>
        /// <returns>A new instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T New<T>(string name = null)
            => (T)New(typeof(T), name);
        /// <summary>
        /// Create a new instance for a Interface, Abstract, or setted class
        /// </summary>
        /// <param name="type">Type of object to create</param>
        /// <param name="name">Name of the instance, if is null the default one</param>
        /// <returns>A new instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object New(Type type, string name = null)
        {
            var tmpObj = OnTypeInstanceResolve?.Invoke(type, name);
            if (tmpObj != null)
                return tmpObj;
            if (RegisteredDelegates.TryGetValue((type, name), out var regValue))
            {
                if (!regValue.Singleton)
                    return regValue.Delegate();
                else
                {
                    if (regValue.SingletonValue == null)
                        regValue.SingletonValue = regValue.Delegate();
                    return regValue.SingletonValue;
                }
            }
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsInterface)
                return CreateInterfaceInstance(type.AssemblyQualifiedName, name);
            if (typeInfo.IsAbstract)
                return CreateAbstractInstance(type.AssemblyQualifiedName, name);
            if (typeInfo.IsClass)
                return CreateClassInstance(type.AssemblyQualifiedName, name) ?? Activator.CreateInstance(type);
            return null;
        }
        /// <summary>
        /// Get all defined instance names for a type
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <returns>String array with all names defined</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] GetNames<T>()
            => GetNames(typeof(T));
        /// <summary>
        /// Get all defined instance names for a type
        /// </summary>
        /// <param name="type">Type of object</param>
        /// <returns>String array with all names defined</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] GetNames(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            Instantiable instantiable = null;
            if (typeInfo.IsInterface)
                return Settings.GetInterfaceDefinition(type.AssemblyQualifiedName)?.ClassDefinitions?.Select(c => c.Name).ToArray() ?? EmptyStringArray;
            if (typeInfo.IsAbstract)
                return Settings.GetAbstractDefinition(type.AssemblyQualifiedName)?.ClassDefinitions?.Select(c => c.Name).ToArray() ?? EmptyStringArray;
            if (typeInfo.IsClass)
            {
                instantiable = Settings.GetInstantiableClassDefinition(type.AssemblyQualifiedName);
                if (instantiable != null)
                    return new string[] { instantiable.Name };
            }
            return EmptyStringArray;
        }

        /// <summary>
        /// Get all instances of a type
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <returns>All instances array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] GetAllInstances<T>()
            => GetNames<T>().Select(n => New<T>(n)).ToArray();
        /// <summary>
        /// Get all instances of a type
        /// </summary>
        /// <param name="type">Type of object</param>
        /// <returns>All instances array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object[] GetAllInstances(Type type)
            => GetNames(type).Select(n => New(type, n)).ToArray();
        /// <summary>
        /// Register a instantiable type to a non instantiable type
        /// </summary>
        /// <typeparam name="NT">Non instantiable type</typeparam>
        /// <typeparam name="IT">Instantiable type</typeparam>
        /// <param name="name">Instance name, leave null for instantiable type name</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Register<NT, IT>(string name = null)
            => Register(typeof(NT), typeof(IT), name, null);
        /// <summary>
        /// Register a instantiable type to a non instantiable type
        /// </summary>
        /// <typeparam name="NT">Non instantiable type</typeparam>
        /// <typeparam name="IT">Instantiable type</typeparam>
        /// <param name="name">Instance name, leave null for instantiable type name</param>
        /// <param name="args">Arguments to register on the constructor</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Register<NT, IT>(string name, params object[] args)
            => Register(typeof(NT), typeof(IT), name, args);
        /// <summary>
        /// Register a instantiable type to a non instantiable type
        /// </summary>
        /// <typeparam name="NT">Non instantiable type</typeparam>
        /// <typeparam name="IT">Instantiable type</typeparam>
        /// <param name="singleton">Indicates that this instantiable type is a singleton</param>
        /// <param name="name">Instance name, leave null for instantiable type name</param>
        /// <param name="args">Arguments to register on the constructor</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Register<NT, IT>(bool singleton, string name, params object[] args)
            => Register(typeof(NT), typeof(IT), singleton, name, args);
        /// <summary>
        /// Register a instantiable type to a non instantiable type
        /// </summary>
        /// <param name="noninstantiableType">Non instantiable type</param>
        /// <param name="instantiableType">Instantiable type</param>
        /// <param name="name">Instance name, leave null for instantiable type name</param>
        /// <param name="args">Arguments to register on the constructor</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Register(Type noninstantiableType, Type instantiableType, string name, params object[] args)
            => Register(noninstantiableType, instantiableType, false, name, args);
        /// <summary>
        /// Register a instantiable type to a non instantiable type
        /// </summary>
        /// <param name="noninstantiableType">Non instantiable type</param>
        /// <param name="instantiableType">Instantiable type</param>
        /// <param name="singleton">Indicates that this instantiable type is a singleton</param>
        /// <param name="name">Instance name, leave null for instantiable type name</param>
        /// <param name="args">Arguments to register on the constructor</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Register(Type noninstantiableType, Type instantiableType, bool singleton, string name, params object[] args)
        {
            var nonTypeInfo = noninstantiableType.GetTypeInfo();
            NonInstantiable def = null;
            if (nonTypeInfo.IsInterface)
            {
                def = Settings.GetInterfaceDefinition(noninstantiableType.AssemblyQualifiedName);
                if (def == null)
                {
                    def = new NonInstantiable { Type = noninstantiableType.AssemblyQualifiedName };
                    Settings.Interfaces.Add(def);
                }
            }
            else if (nonTypeInfo.IsAbstract)
            {
                def = Settings.GetAbstractDefinition(noninstantiableType.AssemblyQualifiedName);
                if (def == null)
                {
                    def = new NonInstantiable { Type = noninstantiableType.AssemblyQualifiedName };
                    Settings.Abstracts.Add(def);
                }
            }

            if (def != null)
            {
                if (name == null)
                {
                    if (def.ClassDefinitions.FirstOrDefault(d => d.Type == instantiableType.AssemblyQualifiedName) == null)
                    {
                        var inst = new Instantiable
                        {
                            Type = instantiableType.AssemblyQualifiedName,
                            Name = name ?? instantiableType.Name,
                            Singleton = singleton
                        };
                        if (args?.Any() == true)
                        {
                            inst.Parameters = new NameCollection<Parameter>();
                            var ctor = instantiableType.GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Length == args.Length);
                            if (ctor != null)
                            {
                                var pargs = ctor.GetParameters();
                                for (var i = 0; i < pargs.Length; i++)
                                {
                                    var parg = pargs[i];
                                    inst.Parameters.Add(new Parameter { Name = parg.Name, Type = ArgumentType.Raw, Value = args[i].ToString() });
                                }
                            }
                            else if (ctor == null)
                            {
                                ctor = instantiableType.GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Where(p => !p.HasDefaultValue).Count() == args.Length);
                                var pargs = ctor.GetParameters().Where(p => !p.HasDefaultValue).ToArray();
                                for (var i = 0; i < pargs.Length; i++)
                                {
                                    var parg = pargs[i];
                                    inst.Parameters.Add(new Parameter { Name = parg.Name, Type = ArgumentType.Raw, Value = args[i].ToString() });
                                }
                            }
                        }
                        def.ClassDefinitions.Insert(0, inst);
                        InstantiableCache[inst] = new ActivatorItem { Type = instantiableType, Arguments = args };
                    }
                }
                else if (!def.ClassDefinitions.Contains(name))
                {
                    var inst = new Instantiable { Type = instantiableType.AssemblyQualifiedName, Name = name ?? instantiableType.Name, Singleton = singleton };
                    if (args?.Any() == true)
                    {
                        inst.Parameters = new NameCollection<Parameter>();
                        var ctor = instantiableType.GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Length == args.Length);
                        if (ctor != null)
                        {
                            var pargs = ctor.GetParameters();
                            for (var i = 0; i < pargs.Length; i++)
                            {
                                var parg = pargs[i];
                                inst.Parameters.Add(new Parameter { Name = parg.Name, Type = ArgumentType.Raw, Value = args[i].ToString() });
                            }
                        }
                        else if (ctor == null)
                        {
                            ctor = instantiableType.GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Where(p => !p.HasDefaultValue).Count() == args.Length);
                            var pargs = ctor.GetParameters().Where(p => !p.HasDefaultValue).ToArray();
                            for (var i = 0; i < pargs.Length; i++)
                            {
                                var parg = pargs[i];
                                inst.Parameters.Add(new Parameter { Name = parg.Name, Type = ArgumentType.Raw, Value = args[i].ToString() });
                            }
                        }
                    }
                    def.ClassDefinitions.Add(inst);
                    InstantiableCache[inst] = new ActivatorItem { Type = instantiableType, Arguments = args };
                }
            }
        }
        /// <summary>
        /// Sets a default name for a non instantiable type
        /// </summary>
        /// <param name="noninstantiableType">Non instantiable type</param>
        /// <param name="name">Instance name</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTypeDefault(Type noninstantiableType, string name)
        {
            var nonTypeInfo = noninstantiableType.GetTypeInfo();
            NonInstantiable def = null;
            if (nonTypeInfo.IsInterface)
            {
                def = Settings.GetInterfaceDefinition(noninstantiableType.AssemblyQualifiedName);
                if (def == null)
                {
                    def = new NonInstantiable { Type = noninstantiableType.AssemblyQualifiedName };
                    Settings.Interfaces.Add(def);
                }
            }
            else if (nonTypeInfo.IsAbstract)
            {
                def = Settings.GetAbstractDefinition(noninstantiableType.AssemblyQualifiedName);
                if (def == null)
                {
                    def = new NonInstantiable { Type = noninstantiableType.AssemblyQualifiedName };
                    Settings.Abstracts.Add(def);
                }
            }
            if (def != null)
                def.DefaultClassName = name;
        }
        /// <summary>
        /// Register a Func delegate for create a non instantiable type
        /// </summary>
        /// <param name="noninstantiableType">Non instantiable type</param>
        /// <param name="createInstanceDelegate">Create instance delegate</param>
        /// <param name="name">Instance name</param>
        /// <param name="singleton">Indicates that this instantiable type is a singleton</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Register(Type noninstantiableType, Func<object> createInstanceDelegate, string name = null, bool singleton = false)
        {
            var key = (noninstantiableType, name);
            RegisteredDelegates.TryRemove(key, out var _);
            RegisteredDelegates.TryAdd(key, new RegisteredValues
            {
                Delegate = createInstanceDelegate,
                Singleton = singleton
            });
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object CreateInterfaceInstance(string type, string name)
        {
            RegisterAttributes();
            var instanceDefinition = Settings.GetInterfaceInstanceDefinition(type, name);
            if (instanceDefinition == null)
            {
                if (name.IsNullOrEmpty())
                    throw new NotImplementedException($"The instace definition for the type: {type} can't be found.");
                else
                    throw new NotImplementedException($"The instace definition '{name}' for the type: {type} can't be found.");
            }
            if (instanceDefinition.Singleton && InstantiableCache.TryGetValue(instanceDefinition, out var activator))
                return activator.SingletonValue;
            return CreateInstance(instanceDefinition);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object CreateAbstractInstance(string type, string name)
        {
            RegisterAttributes();
            var instanceDefinition = Settings.GetAbstractInstanceDefinition(type, name);
            if (instanceDefinition == null)
            {
                if (name.IsNullOrEmpty())
                    throw new NotImplementedException($"The instace definition for the type: {type} can't be found.");
                else
                    throw new NotImplementedException($"The instace definition '{name}' for the type: {type} can't be found.");
            }
            if (instanceDefinition.Singleton && InstantiableCache.TryGetValue(instanceDefinition, out var activator))
                return activator.SingletonValue;
            return CreateInstance(instanceDefinition);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object CreateClassInstance(string type, string name)
        {
            RegisterAttributes();
            var instanceDefinition = Settings.GetInstantiableClassDefinition(type);
            if (instanceDefinition == null)
            {
                if (name.IsNullOrEmpty())
                    throw new NotImplementedException($"The instace definition for the type: {type} can't be found.");
                else
                    throw new NotImplementedException($"The instace definition '{name}' for the type: {type} can't be found.");
            }
            if (instanceDefinition.Singleton && InstantiableCache.TryGetValue(instanceDefinition, out var activator))
                return activator.SingletonValue;
            return CreateInstance(instanceDefinition);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RegisterAttributes()
        {
            if (!attributesRegistered)
            {
                attributesRegistered = true;
                var assemblies = useOnlyLoadedAssemblies ? Factory.GetAssemblies() : Factory.GetAllAssemblies();
                var type = typeof(InjectionAttribute);
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var attributes = assembly.GetCustomAttributes(type).ToArray();
                        if (attributes.Length > 0)
                        {
                            foreach (InjectionAttribute attr in attributes)
                                if (attr != null)
                                    Register(attr.NonInstantiableType, attr.InstantiableType, attr.Singleton, attr.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(LogLevel.Warning, string.Format("Error loading InjectionAttributes on Assembly: {0}. {1}", assembly.FullName, ex.Message), ex);
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object CreateInstance(Instantiable instanceDefinition)
        {
            object response = null;
            var activatorItem = InstantiableCache.GetOrAdd(instanceDefinition, definition =>
            {
                var type = Core.GetType(definition.Type, false);
                if (type != null)
                {
                    var typeInfo = type.GetTypeInfo();
                    if (definition.Parameters?.Any() == true)
                    {
                        var parameters = definition.Parameters;
                        foreach (var ctor in typeInfo.DeclaredConstructors)
                        {
                            var ctorArgs = ctor.GetParameters().ToArray();
                            if (ctorArgs.Length != parameters.Count)
                                ctorArgs = ctor.GetParameters().Where(p => !p.HasDefaultValue).ToArray();
                            if (ctorArgs.Length == parameters.Count)
                            {
                                var ctorArgsNames = ctorArgs.Select(c => c.Name).Join(", ").ToLowerInvariant();
                                var paramsNames = parameters.Select(c => c.Name).Join(", ").ToLowerInvariant();
                                if (ctorArgsNames == paramsNames)
                                {
                                    var argsLst = new List<object>();

                                    #region Create and parsing Parameters
                                    for (var i = 0; i < ctorArgs.Length; i++)
                                    {
                                        var parameter = parameters[i];
                                        if (parameter.ArgumentName.IsNotNullOrWhitespace())
                                            argsLst.Add(GetArgumentValue(parameter.ArgumentName, ctorArgs[i].ParameterType));
                                        else
                                            argsLst.Add(GetArgumentValue(parameter, ctorArgs[i].ParameterType));
                                    }
                                    #endregion

                                    return new ActivatorItem { Type = type, Arguments = argsLst.ToArray() };
                                }
                            }
                        }
                    }
                    else
                        return new ActivatorItem { Type = type };
                }
                return null;
            });
            if (activatorItem != null)
            {
                response = activatorItem.CreateInstance();

                if (response != null && instanceDefinition.PropertiesSets?.Any() == true)
                {
                    foreach (var set in instanceDefinition.PropertiesSets)
                    {
                        var valueType = response.GetMemberObjectType(set.Name);
                        if (valueType != null)
                        {
                            object value;
                            if (set.ArgumentName.IsNotNullOrEmpty())
                                value = GetArgumentValue(set.ArgumentName, valueType);
                            else
                                value = GetArgumentValue(set, valueType);
                            response.SetMemberObjectValue(set.Name, value);
                        }
                    }
                }
                if (instanceDefinition.Singleton)
                    activatorItem.SingletonValue = response;
            }
            return response;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object GetArgumentValue(string name, Type valueType)
        {
            if (Settings.Arguments.Contains(name))
            {
                var argument = Settings.Arguments[name];
                return GetArgumentValue(argument, valueType);
            }
            else
                throw new ArgumentNullException(name);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object GetArgumentValue(Argument argument, Type valueType)
        {
            object argumentValue = null;
            if (argument.Type == ArgumentType.Raw)
                argumentValue = StringParser.Parse(argument.Value, valueType, null);
            else if (argument.Type == ArgumentType.Settings)
                argumentValue = StringParser.Parse(Core.Settings[argument.Value], valueType, null);
            else if (argument.Type == ArgumentType.Interface)
                argumentValue = CreateInterfaceInstance(argument.Value, argument.ClassName);
            else if (argument.Type == ArgumentType.Abstract)
                argumentValue = CreateAbstractInstance(argument.Value, argument.ClassName);
            else if (argument.Type == ArgumentType.Instance)
                argumentValue = CreateClassInstance(argument.Value, argument.ClassName);
            return argumentValue;
        }
        #endregion

        #region Nested Class
        class ActivatorItem
        {
            public Type Type;
            public object[] Arguments;
            public object SingletonValue;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public object CreateInstance()
            {
                if (Arguments?.Any() == true)
                    return Activator.CreateInstance(Type, Arguments);
                else
                    return Activator.CreateInstance(Type);
            }
        }
        class RegisteredValues
        {
            public bool Singleton;
            public object SingletonValue;
            public Func<object> Delegate;
        }
        #endregion

        /// <summary>
        /// Dispose resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            InstantiableCache?.Clear();
            settings = null;
        }
    }
}
