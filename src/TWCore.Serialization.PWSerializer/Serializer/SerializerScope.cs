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
// ReSharper disable CoVariantArrayConversion

namespace TWCore.Serialization.PWSerializer.Serializer
{
    internal class SerializerScope : SerializerPlan
    {
        public int Index;
        public object Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            Plan = null;
            Type = null;
            PlanLength = 0;
            Value = null;
            Index = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(SerializerPlan plan, Type originalType, object value)
        {
            if (plan != null)
            {
                Plan = plan.Plan;
                Type = originalType;
                IsIList = plan.IsIList;
                IsIDictionary = plan.IsIDictionary;
                PlanLength = plan.Plan.Length;
            }
            Value = value;
            Index = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(SerializerPlanItem.RuntimeValue[] runtimePlan, Type type)
        {
            if (runtimePlan != null)
            {
                Plan = runtimePlan;
                Type = type;
                IsIList = false;
                IsIDictionary = false;
                PlanLength = runtimePlan.Length;
            }
            Index = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeScopePlan(SerializerPlanItem[] plan)
        {
            Plan = plan;
            PlanLength = plan?.Length ?? 0;
            Index = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializerPlanItem Next() => Plan[Index++];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasPendingIndex() => Index < PlanLength;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializerPlanItem NextIfAvailable() => Index < PlanLength ? Plan[Index++] : null;
    }
}
