using System;
using System.Collections.Generic;
using TWCore.Security;
using TWCore.Services;
using TWCore.Data;
using TWCore.Data.SqlServer;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using TWCore.Serialization;
using TWCore.Data.Schema.Generator;
using System.IO;

namespace TWCore.Tests
{
    public class DalGeneratorTest : ContainerParameterService
    {
        public DalGeneratorTest() : base("dalgeneratortest", "DalGenerator Test") { }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting Dal Generator Test");

            string folder = "./dalTest";
            string connectionString = "Data Source=10.10.1.50;Initial Catalog=MRFLY_MIDDLE;User Id=MRFLY_SVC;Password=mISTERfLY13;Pooling=True";

            var ssda = new SqlServerDataAccess(connectionString, DataAccessType.Query);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            Core.Log.InfoBasic("Getting Schema");
            var schema = ssda.GetSchema();
            schema.SerializeToXmlFile(Path.Combine(folder, "dbSchema.xml"));

            Core.Log.InfoBasic("Creating Generator");

            var dGen = new DalGenerator(schema, "MrFly.Data");
            dGen.GetEntityNameDelegate = name =>
            {
                name = name.Replace("_", " ");
                name = name.Substring(0, 3) + "_" + name.Substring(4).CapitalizeEachWords();
                name = name.Replace("-", "_");
                return name.RemoveSpaces();
            };

            Core.Log.InfoBasic("Generating Dal: {0}", folder);
            dGen.Create(folder);
        }
    }
}