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


namespace TWCore.Collections
{
    /// <summary>
    /// Define how to combine two elements of the collection
    /// </summary>
    public interface ICombinable<T>
    {
        /// <summary>
        /// Gets the combination of the current instance with another item
        /// </summary>
        /// <param name="item">Item to combine with</param>
        /// <returns>Combination between the current instance and the item</returns>
        T Combine(T item);
    }
}
