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

namespace TWCore.Data
{
    /// <summary>
    /// Data access layer register utility
    /// </summary>
    public class DalRegister : IDalRegister
    {
        /// <summary>
        /// Register the dal on the injector
        /// </summary>
        public void Register() => Register(this.GetAssembly());

        /// <summary>
        /// Register the dal on the injector
        /// </summary>
        /// <param name="dalAssembly">Data access layer assembly</param>
        public void Register(Assembly dalAssembly)
        {
            dalAssembly?.DefinedTypes.Where(t => !t.IsAbstract && !t.IsAutoClass && !t.IsInterface && t.IsClass && !t.IsGenericType && t.ImplementedInterfaces.Any()).Each(t =>
            {
                var iface = t.ImplementedInterfaces.First();
                try
                {
                    Core.Injector.Register(iface, t.AsType(), t.Name);
                }
                catch(Exception ex)
                {
                    Core.Log.Write(ex);
                }
            });
        }
    }
}
