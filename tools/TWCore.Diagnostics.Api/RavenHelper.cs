using System;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace TWCore.Diagnostics.Api
{
    public static class RavenHelper
    {
        public static void Execute(Action<IDocumentSession> sessionAction)
        {
            using (var store = new DocumentStore() {Urls = new[] {"http://10.10.0.100:8080"}, Database = "Diagnostics"})
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
    }
}