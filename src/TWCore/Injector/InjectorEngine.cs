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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TWCore.Collections;
using TWCore.Diagnostics.Log;
using TWCore.Text;
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Injector
{
    /// <summary>
    /// Injector delegate for type instance resolve.
    /// </summary>
    /// <param name="type">Object type</param>
    /// <param name="name">Name of the instance</param>
    /// <returns>Instance of the object type</returns>
    public delegate object TypeInstanceResolverDelegate(Type type, string name);

    /// <inheritdoc />
    /// <summary>
    /// Inject instances for non instantiable class
    /// </summary>
    public class InjectorEngine : IDisposable
    {
        private readonly ConcurrentDictionary<(Type, string), RegisteredValues> _registeredDelegates = new ConcurrentDictionary<(Type, string), RegisteredValues>();
        private readonly ConcurrentDictionary<Instantiable, ActivatorItem> _instantiableCache = new ConcurrentDictionary<Instantiable, ActivatorItem>();
        private InjectorSettings _settings;
        private bool _attributesRegistered;
        private bool _useOnlyLoadedAssemblies = true;

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
            get => _settings;
            set
            {
                _settings = value;
                _attributesRegistered = false;
            }
        }
        /// <summary>
        /// Use only assemblies loaded, false if uses all assemblies in the folder
        /// </summary>
        public bool UseOnlyLoadedAssemblies
        {
            get => _useOnlyLoadedAssemblies;
            set
            {
                _useOnlyLoadedAssemblies = value;
                _attributesRegistered = false;
            }
        }
        /// <summary>
        /// Throw exception on instance creation error
        /// </summary>
        public bool ThrowExceptionOnInstanceCreationError { get; set; } = true;
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
            if (OnTypeInstanceResolve != null)
            {
                var tmpObj = OnTypeInstanceResolve(type, name);
                if (tmpObj != null)
                    return tmpObj;
            }
            if (_registeredDelegates.TryGetValue((type, name), out var regValue))
            {
                if (!regValue.Singleton)
                    return regValue.Delegate();
                return regValue.SingletonValue ?? (regValue.SingletonValue = regValue.Delegate());
            }
            if (type.IsInterface)
                return CreateInterfaceInstance(type.AssemblyQualifiedName, name);
            if (type.IsAbstract)
                return CreateAbstractInstance(type.AssemblyQualifiedName, name);
            if (type.IsClass)
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
            if (Settings is null) throw new NullReferenceException("The injector settings is null.");
            if (type.IsInterface)
                return Settings.GetInterfaceDefinition(type.AssemblyQualifiedName)?.ClassDefinitions?.Select(c => c.Name).ToArray() ?? Array.Empty<string>();
            if (type.IsAbstract)
                return Settings.GetAbstractDefinition(type.AssemblyQualifiedName)?.ClassDefinitions?.Select(c => c.Name).ToArray() ?? Array.Empty<string>();
            if (!type.IsClass) return Array.Empty<string>();
            return Settings.GetInstantiableClassDefinition(type.AssemblyQualifiedName)?.Select(c => c.Name).ToArray() ?? Array.Empty<string>();
        }

        /// <summary>
        /// Get all instances of a type
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <returns>All instances array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] GetAllInstances<T>()
            => GetNames<T>().Select(New<T>).ToArray();
        /// <summary>
        /// Get all instances of a type
        /// </summary>
        /// <param name="type">Type of object</param>
        /// <returns>All instances array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object[] GetAllInstances(Type type)
            => GetNames(type).Select((n, mType) => New(mType, n), type).ToArray();
        /// <summary>
        /// Register a instantiable type to a non instantiable type
        /// </summary>
        /// <typeparam name="TNt">Non instantiable type</typeparam>
        /// <typeparam name="TIt">Instantiable type</typeparam>
        /// <param name="name">Instance name, leave null for instantiable type name</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Register<TNt, TIt>(string name = null)
            => Register(typeof(TNt), typeof(TIt), name, null);
        /// <summary>
        /// Register a instantiable type to a non instantiable type
        /// </summary>
        /// <typeparam name="TNt">Non instantiable type</typeparam>
        /// <typeparam name="TIt">Instantiable type</typeparam>
        /// <param name="name">Instance name, leave null for instantiable type name</param>
        /// <param name="args">Arguments to register on the constructor</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Register<TNt, TIt>(string name, params object[] args)
            => Register(typeof(TNt), typeof(TIt), name, args);
        /// <summary>
        /// Register a instantiable type to a non instantiable type
        /// </summary>
        /// <typeparam name="TNt">Non instantiable type</typeparam>
        /// <typeparam name="TIt">Instantiable type</typeparam>
        /// <param name="singleton">Indicates that this instantiable type is a singleton</param>
        /// <param name="name">Instance name, leave null for instantiable type name</param>
        /// <param name="args">Arguments to register on the constructor</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Register<TNt, TIt>(bool singleton, string name, params object[] args)
            => Register(typeof(TNt), typeof(TIt), singleton, name, args);
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
            if (Settings is null) throw new NullReferenceException("The injector settings is null.");
            var nonTypeInfo = noninstantiableType.GetTypeInfo();
            NonInstantiable def = null;
            if (nonTypeInfo.IsInterface)
            {
                def = Settings.GetInterfaceDefinition(noninstantiableType.AssemblyQualifiedName);
                if (def is null)
                {
                    def = new NonInstantiable { Type = noninstantiableType.AssemblyQualifiedName };
                    Settings.Interfaces.Add(def);
                }
            }
            else if (nonTypeInfo.IsAbstract)
            {
                def = Settings.GetAbstractDefinition(noninstantiableType.AssemblyQualifiedName);
                if (def is null)
                {
                    def = new NonInstantiable { Type = noninstantiableType.AssemblyQualifiedName };
                    Settings.Abstracts.Add(def);
                }
            }
            if (def is null) return;
            if (name is null)
            {
                if (def.ClassDefinitions.FirstOrDefault((d, iType) => d.Type == iType.AssemblyQualifiedName, instantiableType) != null) return;
                var inst = new Instantiable
                {
                    Type = instantiableType.AssemblyQualifiedName,
                    Name = instantiableType.Name,
                    Singleton = singleton
                };
                if (args?.Any() == true)
                {
                    inst.Parameters = new NameCollection<Parameter>();
                    var ctor = instantiableType.GetTypeInfo().DeclaredConstructors.FirstOrDefault((c, aLength) => c.GetParameters().Length == aLength, args.Length);
                    if (ctor != null)
                    {
                        var pargs = ctor.GetParameters();
                        for (var i = 0; i < pargs.Length; i++)
                        {
                            var parg = pargs[i];
                            inst.Parameters.Add(new Parameter { Name = parg.Name, Type = ArgumentType.Raw, Value = args[i].ToString() });
                        }
                    }
                    else
                    {
                        ctor = instantiableType.GetTypeInfo().DeclaredConstructors.FirstOrDefault((c, aLength) => c.GetParameters().Count(p => !p.HasDefaultValue) == aLength, args.Length);
                        var pargs = ctor.GetParameters().Where(p => !p.HasDefaultValue).ToArray();
                        for (var i = 0; i < pargs.Length; i++)
                        {
                            var parg = pargs[i];
                            inst.Parameters.Add(new Parameter { Name = parg.Name, Type = ArgumentType.Raw, Value = args[i].ToString() });
                        }
                    }
                }
                def.ClassDefinitions.Insert(0, inst);
                _instantiableCache[inst] = new ActivatorItem { Type = instantiableType, Arguments = args };
            }
            else if (!def.ClassDefinitions.Contains(name))
            {
                var inst = new Instantiable { Type = instantiableType.AssemblyQualifiedName, Name = name, Singleton = singleton };
                if (args?.Any() == true)
                {
                    inst.Parameters = new NameCollection<Parameter>();
                    var ctor = instantiableType.GetTypeInfo().DeclaredConstructors.FirstOrDefault((c, aLength) => c.GetParameters().Length == aLength, args.Length);
                    if (ctor != null)
                    {
                        var pargs = ctor.GetParameters();
                        for (var i = 0; i < pargs.Length; i++)
                        {
                            var parg = pargs[i];
                            inst.Parameters.Add(new Parameter { Name = parg.Name, Type = ArgumentType.Raw, Value = args[i].ToString() });
                        }
                    }
                    else
                    {
                        ctor = instantiableType.GetTypeInfo().DeclaredConstructors.FirstOrDefault((c, aLength) => c.GetParameters().Count(p => !p.HasDefaultValue) == aLength, args.Length);
                        var pargs = ctor.GetParameters().Where(p => !p.HasDefaultValue).ToArray();
                        for (var i = 0; i < pargs.Length; i++)
                        {
                            var parg = pargs[i];
                            inst.Parameters.Add(new Parameter { Name = parg.Name, Type = ArgumentType.Raw, Value = args[i].ToString() });
                        }
                    }
                }
                def.ClassDefinitions.Add(inst);
                _instantiableCache[inst] = new ActivatorItem { Type = instantiableType, Arguments = args };
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
            if (Settings is null) throw new NullReferenceException("The injector settings is null.");
            var nonTypeInfo = noninstantiableType.GetTypeInfo();
            NonInstantiable def = null;
            if (nonTypeInfo.IsInterface)
            {
                def = Settings.GetInterfaceDefinition(noninstantiableType.AssemblyQualifiedName);
                if (def is null)
                {
                    def = new NonInstantiable { Type = noninstantiableType.AssemblyQualifiedName };
                    Settings.Interfaces.Add(def);
                }
            }
            else if (nonTypeInfo.IsAbstract)
            {
                def = Settings.GetAbstractDefinition(noninstantiableType.AssemblyQualifiedName);
                if (def is null)
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
            _registeredDelegates.TryRemove(key, out var _);
            _registeredDelegates.TryAdd(key, new RegisteredValues
            {
                Delegate = createInstanceDelegate,
                Singleton = singleton
            });
        }
        /// <summary>
        /// Preload all declared types
        /// </summary>
        /// <returns>Types tuples array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyCollection<InjectorSettings.TypeLoadResult> PreloadAllTypes() 
            => Settings.PreloadAllTypes();
        /// <summary>
        /// Get all missing types
        /// </summary>
        /// <returns>Type AssemblyQualifiedName array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] GetAllMissingTypes()
            => Settings.PreloadAllTypes().Where(t => !t.Loaded).Select(t => t.Type).ToArray();
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object CreateInterfaceInstance(string type, string name)
        {
            if (Settings is null) throw new NullReferenceException("The injector settings is null.");
            RegisterAttributes();
            var instanceDefinition = Settings.GetInterfaceInstanceDefinition(type, name);
            if (instanceDefinition is null)
            {
                if (name.IsNullOrEmpty())
                    throw new NotImplementedException($"The instace definition for the type: {type} can't be found.");
                throw new NotImplementedException($"The instace definition '{name}' for the type: {type} can't be found.");
            }
            if (instanceDefinition.Singleton && _instantiableCache.TryGetValue(instanceDefinition, out var activator) && activator?.SingletonValue != null)
                return activator.SingletonValue;
            return CreateInstance(instanceDefinition);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object CreateAbstractInstance(string type, string name)
        {
            if (Settings is null) throw new NullReferenceException("The injector settings is null.");
            RegisterAttributes();
            var instanceDefinition = Settings.GetAbstractInstanceDefinition(type, name);
            if (instanceDefinition is null)
            {
                if (name.IsNullOrEmpty())
                    throw new NotImplementedException($"The instace definition for the type: {type} can't be found.");
                throw new NotImplementedException($"The instace definition '{name}' for the type: {type} can't be found.");
            }
            if (instanceDefinition.Singleton && _instantiableCache.TryGetValue(instanceDefinition, out var activator) && activator?.SingletonValue != null)
                return activator.SingletonValue;
            return CreateInstance(instanceDefinition);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object CreateClassInstance(string type, string name)
        {
            if (Settings is null) throw new NullReferenceException("The injector settings is null.");
            RegisterAttributes();
            var instanceDefinition = Settings.GetInstantiableClassDefinition(type).FirstOrDefault((i, mName) => i.Name == mName, name);
            if (instanceDefinition is null) return null;
            if (instanceDefinition.Singleton && _instantiableCache.TryGetValue(instanceDefinition, out var activator) && activator?.SingletonValue != null)
                return activator.SingletonValue;
            return CreateInstance(instanceDefinition);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RegisterAttributes()
        {
            if (_attributesRegistered) return;
            _attributesRegistered = true;
            var assemblies = _useOnlyLoadedAssemblies ? Factory.GetAssemblies() : Factory.GetAllAssemblies();
            var type = typeof(InjectionAttribute);
            foreach (var assembly in assemblies)
            {
                try
                {
                    var attributes = assembly.GetCustomAttributes(type).ToArray();
                    if (attributes.Length <= 0) continue;
                    foreach (var attr in attributes)
                        if (attr != null && attr is InjectionAttribute iAttr)
                            Register(iAttr.NonInstantiableType, iAttr.InstantiableType, iAttr.Singleton, iAttr.Name);
                }
                catch (Exception ex)
                {
                    Core.Log.Write(LogLevel.Warning, string.Format("Error loading InjectionAttributes on Assembly: {0}. {1}", assembly.FullName, ex.Message), ex);
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object CreateInstance(Instantiable instanceDefinition)
        {
            if (!_instantiableCache.TryGetValue(instanceDefinition, out var activatorItem))
            {
                activatorItem = GetActivatorItem(instanceDefinition);
                if (activatorItem is null || activatorItem.EnableCache)
                    _instantiableCache.TryAdd(instanceDefinition, activatorItem);
            }
            if (activatorItem is null) return null;
            
            var response = activatorItem.CreateInstance();
            if (response != null && instanceDefinition.PropertiesSets?.Any() == true)
            {
                foreach (var set in instanceDefinition.PropertiesSets)
                {
                    var valueType = response.GetMemberObjectType(set.Name);
                    if (valueType is null) continue;
                    var value = set.ArgumentName.IsNotNullOrEmpty() ? 
                        GetArgumentValue(GetArgument(set.ArgumentName), valueType) : 
                        GetArgumentValue(set, valueType);
                    response.SetMemberObjectValue(set.Name, value);
                }
            }
            if (instanceDefinition.Singleton)
            {
                activatorItem.SingletonValue = response;
                _instantiableCache.TryAdd(instanceDefinition, activatorItem);
            }
            return response;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ActivatorItem GetActivatorItem(Instantiable definition)
        {
            var type = Core.GetType(definition.Type, ThrowExceptionOnInstanceCreationError);
            if (type is null) return null;
            var typeInfo = type.GetTypeInfo();
            if (definition.Parameters is null || definition.Parameters.Count == 0)
                return new ActivatorItem { Type = type, EnableCache = true };

            var parameters = definition.Parameters;
            foreach (var ctor in typeInfo.DeclaredConstructors)
            {
                var ctorArgs = ctor.GetParameters();
                if (ctorArgs.Length != parameters.Count)
                    ctorArgs = ctorArgs.Where(p => !p.HasDefaultValue).ToArray();
                if (ctorArgs.Length != parameters.Count) continue;
                if (!ctorArgs.SequenceEqual(parameters, p => p.Name, p => p.Name)) continue;
                
                var argsLst = new List<object>();
                var enableCache = true;

                #region Create and parsing Parameters
                for (var i = 0; i < ctorArgs.Length; i++)
                {
                    var parameter = parameters[i];
                    if (parameter.Type != ArgumentType.Raw && parameter.Type != ArgumentType.Settings)
                        enableCache = false;
                    argsLst.Add(parameter.ArgumentName.IsNotNullOrWhitespace()
                        ? GetArgumentValue(GetArgument(parameter.ArgumentName), ctorArgs[i].ParameterType)
                        : GetArgumentValue(parameter, ctorArgs[i].ParameterType));
                }
                #endregion

                return new ActivatorItem { Type = type, Arguments = argsLst.ToArray(), EnableCache = enableCache };
            }

            if (ThrowExceptionOnInstanceCreationError)
                throw new EntryPointNotFoundException("A valid constructor can't be found for the type: " + type.AssemblyQualifiedName);

            return null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Argument GetArgument(string name)
        {
            if (!Settings.Arguments.Contains(name)) 
                throw new ArgumentNullException(name);
            return Settings.Arguments[name];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object GetArgumentValue(Argument argument, Type valueType)
        {
            object argumentValue;
            switch (argument.Type)
            {
                case ArgumentType.Raw:
                    argumentValue = StringParser.Parse(argument.Value, valueType, null);
                    break;
                case ArgumentType.Settings:
                    argumentValue = StringParser.Parse(Core.Settings[argument.Value], valueType, null);
                    break;
                case ArgumentType.Interface:
                    argumentValue = CreateInterfaceInstance(argument.Value, argument.ClassName);
                    break;
                case ArgumentType.Abstract:
                    argumentValue = CreateAbstractInstance(argument.Value, argument.ClassName);
                    break;
                case ArgumentType.Instance:
                    argumentValue = CreateClassInstance(argument.Value, argument.ClassName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return argumentValue;
        }
        #endregion

        #region Nested Class
        private class ActivatorItem
        {
            public Type Type;
            public object[] Arguments;
            public object SingletonValue;
            public bool EnableCache;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public object CreateInstance()
            {
                return Arguments?.Any() == true ? 
                    Activator.CreateInstance(Type, Arguments) : 
                    Activator.CreateInstance(Type);
            }
        }
        private class RegisteredValues
        {
            public bool Singleton;
            public object SingletonValue;
            public Func<object> Delegate;
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Dispose resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _instantiableCache?.Clear();
            _settings = null;
        }
    }
}
