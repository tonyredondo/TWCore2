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


namespace TWCore.Diagnostics.Status
{
    /// <summary>
    /// Fetch the current status delegate.
    /// </summary>
    /// <returns>Status item collection with the current status</returns>
    public delegate StatusItemCollection FetchStatusDelegate();
    /// <summary>
    /// Delegate to create a new status engine
    /// </summary>
    /// <returns>Status engine instance.</returns>
    public delegate IStatusEngine CreateStatusEngineDelegate();
}
