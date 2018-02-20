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


namespace TWCore.Object.Descriptor
{
    /// <summary>
    /// Member type
    /// </summary>
    public enum MemberType
    {
        /// <summary>
        /// Field member type
        /// </summary>
        Field,
        /// <summary>
        /// Property member type
        /// </summary>
        Property,
        /// <summary>
        /// Method member type
        /// </summary>
        Method,
        /// <summary>
        /// Enumerable item member
        /// </summary>
        EnumerableItem
    }
}
