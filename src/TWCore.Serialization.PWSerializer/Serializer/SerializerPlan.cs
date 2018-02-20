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
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace TWCore.Serialization.PWSerializer.Serializer
{
    internal class SerializerPlan
    {
        public SerializerPlanItem[] Plan;
        public Type Type;
        public bool IsIList;
        public bool IsIDictionary;
        public int PlanLength;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(SerializerPlanItem[] plan, Type type, bool isIList, bool isIDictionary)
        {
            Plan = plan;
            PlanLength = plan?.Length ?? 0;
            Type = type;
            IsIList = isIList;
            IsIDictionary = isIDictionary;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void ReplacePlan(SerializerPlanItem[] plan)
        {
            Plan = plan;
            PlanLength = plan?.Length ?? 0;
        }
    }
}
