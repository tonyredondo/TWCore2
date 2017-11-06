using System;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using TWCore.Settings;

namespace TWCore.Diagnostics.Api
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