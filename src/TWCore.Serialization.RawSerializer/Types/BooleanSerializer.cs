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
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
#pragma warning disable 1591

namespace TWCore.Serialization.RawSerializer
{
    public partial class SerializersTable
    {
        #region Expressions
        private static readonly MethodInfo BoolValueProperty = typeof(bool?).GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)?.GetMethod;

        internal static Expression WriteBooleanExpression(Expression value, ParameterExpression serTable)
        {
            var boolParam = Expression.Parameter(typeof(bool));
            var block = Expression.Block(new[] { boolParam },
                Expression.Assign(boolParam, value),
                Expression.IfThenElse(
                    Expression.Equal(boolParam, Expression.Constant(true)),
                    Expression.Call(serTable, WriteByteMethodInfo, Expression.Constant(DataBytesDefinition.BoolTrue)),
                    Expression.Call(serTable, WriteByteMethodInfo, Expression.Constant(DataBytesDefinition.BoolFalse))));
            return block.Reduce();
        }
        internal static Expression WriteNulleableBooleanExpression(Expression value, ParameterExpression serTable)
        {
            var boolParam = Expression.Parameter(typeof(bool?));
            var block = Expression.Block(new[] { boolParam },
                Expression.Assign(boolParam, value),
                Expression.IfThenElse(
                    Expression.Equal(boolParam, Expression.Constant(null, typeof(bool?))),
                    Expression.Call(serTable, WriteByteMethodInfo, Expression.Constant(DataBytesDefinition.ValueNull)),
                    WriteBooleanExpression(Expression.Call(boolParam, BoolValueProperty), serTable))
                );
            return block.Reduce();
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(bool value)
        {
            if (value)
                WriteByte(DataBytesDefinition.BoolTrue);
            else
                WriteByte(DataBytesDefinition.BoolFalse);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(bool? value)
        {
            if (value == null)
                WriteByte(DataBytesDefinition.ValueNull);
            else if (value.Value)
                WriteByte(DataBytesDefinition.BoolTrue);
            else
                WriteByte(DataBytesDefinition.BoolFalse);
        }
    }




    public partial class DeserializersTable
    {
        [DeserializerMethod(DataBytesDefinition.BoolTrue, DataBytesDefinition.BoolFalse, ReturnType = typeof(bool))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBool(byte value)
        {
            if (value == DataBytesDefinition.BoolTrue)
                return true;
            if (value == DataBytesDefinition.BoolFalse)
                return false;
            throw new InvalidOperationException("Invalid type value.");
        }

        [DeserializerMethod(ReturnType = typeof(bool?))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool? ReadBoolNullable(byte value)
        {
            if (value == DataBytesDefinition.ValueNull)
                return null;
            if (value == DataBytesDefinition.BoolTrue)
                return true;
            if (value == DataBytesDefinition.BoolFalse)
                return false;
            throw new InvalidOperationException("Invalid type value.");
        }
    }
}