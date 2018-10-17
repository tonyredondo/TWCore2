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
using System.Runtime.CompilerServices;
// ReSharper disable PossibleNullReferenceException
#pragma warning disable 1591

namespace TWCore.Serialization.RawSerializer
{
    public class GenericDeserializerMetaDataOfType : IEquatable<GenericDeserializerMetaDataOfType>
    {
        public readonly string Type;
        public readonly bool IsArray;
        public readonly bool IsList;
        public readonly bool IsDictionary;
        public readonly string[] Properties;

        #region .ctor
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GenericDeserializerMetaDataOfType(string type, bool isArray, bool isList, bool isDictionary, string[] properties)
        {
            Type = type;
            IsArray = isArray;
            IsList = isList;
            IsDictionary = isDictionary;
            Properties = properties;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Create Object
        /// </summary>
        /// <returns>GenericObject instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GenericObject CreateObject()
            => new GenericObject(Type, IsArray, IsList, IsDictionary, Properties);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(GenericDeserializerMetaDataOfType other)
        {
            if (other is null) return false;
            if (Type != other.Type) return false;
            if (IsArray != other.IsArray) return false;
            if (IsList != other.IsList) return false;
            if (IsDictionary != other.IsDictionary) return false;
            if (Properties is null && other.Properties != null) return false;
            if (Properties != null && other.Properties is null) return false;
            if (Properties is null && other.Properties is null) return true;
            if (Properties.Length != other.Properties.Length) return false;
            var length = Math.Min(Properties.Length, other.Properties.Length);
            for (var i = 0; i < length; i++)
                if (Properties[i] != other.Properties[i]) return false;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj.GetType() != typeof(GenericDeserializerMetaDataOfType)) return false;
            return Equals((GenericDeserializerMetaDataOfType)obj);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            var hash = 13;
            hash = (hash * 7) + Type.GetHashCode();
            hash = (hash * 7) + IsArray.GetHashCode();
            hash = (hash * 7) + IsList.GetHashCode();
            hash = (hash * 7) + IsDictionary.GetHashCode();
            if (Properties != null)
            {
                var length = Properties.Length;
                for (var i = 0; i < length; i++)
                    hash = (hash * 7) + Properties[i].GetHashCode();
            }
            return hash;
        }
        #endregion

        #region Operator Overloads
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(GenericDeserializerMetaDataOfType meta1, GenericDeserializerMetaDataOfType meta2)
            => !meta1.Equals(meta2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(GenericDeserializerMetaDataOfType meta1, GenericDeserializerMetaDataOfType meta2)
            => meta1.Equals(meta2);
        #endregion
    }
}