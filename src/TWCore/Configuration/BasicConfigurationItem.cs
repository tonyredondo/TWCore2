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
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Collections;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TWCore.Configuration
{
    /// <summary>
    /// Defines a basic configuration item with a type factory definition
    /// </summary>
    [DataContract]
    public class BasicConfigurationItem
    {
        /// <summary>
        /// Type Name
        /// </summary>
        [XmlAttribute, DataMember]
        public string Name { get; set; }
        /// <summary>
        /// Type Factory
        /// </summary>
        [XmlAttribute, DataMember]
        public string TypeFactory { get; set; }
        /// <summary>
        /// Gets or sets if the item is enabled
        /// </summary>
        [XmlAttribute, DataMember]
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// Initialization parameters
        /// </summary>
        [XmlElement("Param"), DataMember]
        public KeyValueCollection Parameters { get; set; } = new KeyValueCollection();

        /// <summary>
        /// Create a instance using the factory and the parameters
        /// </summary>
        /// <typeparam name="T">Type expected</typeparam>
        /// <returns>Instance of type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T CreateInstance<T>()
        {
            if (!Enabled)
                return default(T);
			Ensure.ReferenceNotNull(TypeFactory, "The TypeFactory property is null.");
            var factory = (ITypeFactory)Activator.CreateInstance(Core.GetType(TypeFactory));
            Ensure.ReferenceNotNull(factory, string.Format("The factory type could not be created, please check the TypeFactory: {0} and ensure the assembly exists.", TypeFactory));
            return factory.Create<T>(this);
        }
    }
}
