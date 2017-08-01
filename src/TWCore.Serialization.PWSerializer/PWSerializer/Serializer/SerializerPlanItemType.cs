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

namespace TWCore.Serialization.PWSerializer.Serializer
{
    internal class SerializerPlanItemType
    {
        public const byte WriteBytes = 1;

        public const byte TypeStart = 10;
        public const byte TypeEnd = 11;

        public const byte ListStart = 20;
        public const byte ListEnd = 21;

        public const byte DictionaryStart = 30;
        public const byte DictionaryEnd = 31;

        public const byte PropertyValue = 60;
        public const byte PropertyReference = 61;

        public const byte Value = 100;
        public const byte RuntimeValue = 200;
    }
}
