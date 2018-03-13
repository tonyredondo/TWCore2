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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeMadeStatic.Global

namespace TWCore.Data
{
    /// <inheritdoc />
    /// <summary>
    /// Data access layer register utility
    /// </summary>
    public class DalRegister : IDalRegister
    {
        private readonly object _locker = new object();
        private bool _loaded;
        private List<(Type Interface, Type Implementation)> _typesToRegister;

        /// <inheritdoc />
        /// <summary>
        /// Types in the registration
        /// </summary>
        public IEnumerable<(Type Interface, Type Implementation)> Types => _typesToRegister ?? (_typesToRegister = GetInterfacesToRegister());

        /// <inheritdoc />
        /// <summary>
        /// Register the dal on the injector
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Register()
        {
            lock(_locker)
            {
                if (_loaded) return;
                var res = 0;
                foreach(var typePair in Types)
                {
                    try
                    {
                        res++;
                        Core.Injector.Register(typePair.Interface, typePair.Implementation, true, typePair.Implementation.Name);
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(ex);
                    }
                }
                _loaded = true;
                if (res == 0)
                    Core.Log.Warning("No IEntityDal found on the assembly, DAL weren't registered.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private List<(Type Interface, Type Implementation)> GetInterfacesToRegister()
        {
            var response = new List<(Type Interface, Type Implementation)>();

            var types = this.GetAssembly().DefinedTypes.Where(t =>
                !t.IsAbstract && !t.IsAutoClass && !t.IsInterface && t.IsClass && !t.IsGenericType &&
                t.ImplementedInterfaces.Any(i => i == typeof(IEntityDal)));

            foreach(var t in types)
            {
                var ifaces = t.ImplementedInterfaces.Where(i => i != typeof(IEntityDal));
                foreach (var iface in ifaces)
                {
                    try
                    {
                        response.Add((iface, t.AsType()));
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(ex);
                    }
                }
            }

            return response;
        }
    }
}
