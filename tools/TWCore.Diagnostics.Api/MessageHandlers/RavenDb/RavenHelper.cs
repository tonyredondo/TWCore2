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
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using TWCore.Settings;

namespace TWCore.Diagnostics.Api.MessageHandlers.RavenDb
{
    public static class RavenHelper
    {
        public static readonly RavenDbSettings Settings = Core.GetSettings<RavenDbSettings>();

        public static void Execute(Action<IDocumentSession> sessionAction)
        {
            using (var store = new DocumentStore { Urls = Settings.Urls, Database = Settings.Database })
            {
                store.Initialize();
                using (var session = store.OpenSession())
                {
                    try
                    {
                        sessionAction(session);
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(ex);
                    }
                }
            }
        }


        public class RavenDbSettings : SettingsBase
        {
            [SettingsArray(",")]
            public string[] Urls { get; set; }
            public string Database { get; set; } = "Diagnostics";
        }
    }
}