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
// ReSharper disable CoVariantArrayConversion

namespace TWCore.Serialization.WSerializer.Serializer
{
    internal class SerializerScope : SerializerPlan
    {
        public int Index;
        public readonly object Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializerScope(SerializerPlanItem.RuntimeValue[] runtimePlan, Type type) : base(runtimePlan, type, false, false) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializerScope(SerializerPlan plan, object value) : base(plan.Plan, plan.Type, plan.IsIList, plan.IsIDictionary)
        {
            Value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ReplacePlan(SerializerPlanItem[] plan)
        {
            Plan = plan;
            PlanLength = plan?.Length ?? 0;
            Index = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializerPlanItem Next()
        {
            return Plan[Index++];
        }
    }
}
