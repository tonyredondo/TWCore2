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

using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TWCore
{
    /// <summary>
    /// Adds a dynamic wrapper to an object
    /// </summary>
    public sealed class DynObject : DynamicObject
    {
        #region Properties
        /// <summary>
        /// Base object
        /// </summary>
        public object BaseObject { get; set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Adds a dynamic wrapper to an object
        /// </summary>
        /// <param name="baseObject">Base object to wrap</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DynObject(object baseObject)
        {
            BaseObject = baseObject;
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Returns the enumeration of all dynamic member names.
        /// </summary>
        /// <returns>A sequence that contains dynamic member names.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var type = BaseObject.GetType();
            var lstMembers = new List<string>();
            type.GetRuntimeProperties().Each(i => lstMembers.Add(i.Name));
            type.GetRuntimeMethods().Each(i => lstMembers.Add(i.Name));
            return lstMembers;
        }
        /// <summary>
        /// Provides the implementation for operations that get member values.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation.</param>
        /// <param name="result">The result of the get operation.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var type = BaseObject.GetType();
            result = null;
            try
            {
                var prop = type.GetTypeInfo().GetDeclaredProperty(binder.Name);
                result = prop.GetValue(BaseObject);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Provides the implementation for operations that invoke a member.
        /// </summary>
        /// <param name="binder">Provides information about the dynamic operation.</param>
        /// <param name="args">The arguments that are passed to the object member during the invoke operation.</param>
        /// <param name="result">true if the operation is successful; otherwise, false.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var type = BaseObject.GetType();
            result = null;
            try
            {
                var method = type.GetTypeInfo().GetDeclaredMethod(binder.Name);
                result = method.Invoke(BaseObject, args);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Provides the implementation for operations that set member values.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation.</param>
        /// <param name="value">The value to set to the member.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var type = BaseObject.GetType();
            try
            {
                var prop = type.GetTypeInfo().GetDeclaredProperty(binder.Name);
                prop.SetValue(BaseObject, value);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Provides the implementation for operations that get a value by index.
        /// </summary>
        /// <param name="binder">Provides information about the operation.</param>
        /// <param name="indexes">The indexes that are used in the operation.</param>
        /// <param name="result">The result of the index operation.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            var type = BaseObject.GetType();
            result = null;
            try
            {
                var prop = type.GetTypeInfo().GetDeclaredProperty("Item");
                result = prop.GetValue(BaseObject, indexes);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Provides the implementation for operations that set a value by index.
        /// </summary>
        /// <param name="binder">Provides information about the operation.</param>
        /// <param name="indexes">The indexes that are used in the operation.</param>
        /// <param name="value">The value to set to the object that has the specified index.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            var type = BaseObject.GetType();
            try
            {
                var prop = type.GetTypeInfo().GetDeclaredProperty("Item");
                prop.SetValue(BaseObject, value, indexes);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Gets a string representation of the object.
        /// </summary>
        /// <returns>String representation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            var KeyValueList = new List<string>();
            var type = BaseObject.GetType();
            var props = type.GetRuntimeProperties();

            foreach (var prop in props)
            {
                var pValue = prop.GetValue(BaseObject);
                KeyValueList.Add(string.Format("{0} = {1}", prop.Name, pValue?.ToString()));
            }

            return "{ " + string.Join(", ", KeyValueList.ToArray()) + " }";
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns the base object
        /// </summary>
        /// <returns>Base object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object ToObject()
        {
            return BaseObject;
        }
        #endregion
    }
}
