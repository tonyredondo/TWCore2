using TWCore.Services;
using TWCore.Data;
using TWCore.Data.SqlServer;
using TWCore.Serialization;
using TWCore.Data.Schema.Generator;
using System.IO;
using TWCore.Data.PostgreSQL;
using TWCore.Data.MySql;
using TWCore.Data.SQLite;
// ReSharper disable UnusedVariable
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable FieldCanBeMadeReadOnly.Local

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

            var dGen = new DalGenerator(schema, "MrFly.Data")
            {
                GetEntityNameDelegate = name =>
                {
                    name = name.Replace("_", " ");
                    name = name.Substring(0, 3) + "_" + name.Substring(4).CapitalizeEachWords();
                    name = name.Replace("-", "_");
                    return name.RemoveSpaces();
                },
                GeneratorType = DalGeneratorType.Embedded
            };


            Core.Log.InfoBasic("Generating SQLServer Dal: {0}", folder);
            dGen.Create(folder);

            Core.Log.InfoBasic("Generating PostgreSQL Dal: {0}", folder);
            string connectionString2 = "Server=10.10.1.50;Port=5432;Database=FLY_MIDDLE;User Id=postgres;Password=genesis;";
            var pdal = new PostgreSQLDataAccess(connectionString2, DataAccessType.Query);


            var postgresSchema = pdal.GetSchema();

            dGen.Create(folder, pdal);


            Core.Log.InfoBasic("Generating MySql Dal: {0}", folder);
            string connectionString3 = "MySqlConnString";
            var sdal = new MySqlDataAccess(connectionString3, DataAccessType.Query);
            dGen.Create(folder, sdal);


            Core.Log.InfoBasic("Generating Sqlite Dal: {0}", folder);
            string connectionString4 = "SqliteConnString";
            var sldal = new SQLiteDataAccess(connectionString4);
            dGen.Create(folder, sldal);
        }
    }
}