using System;
using System.Collections.Generic;
using TWCore.Services;
using TWCore.Data;
using TWCore.Data.SqlServer;
using System.Linq;
using System.Threading.Tasks;

namespace TWCore.Tests
{
    public class SqlServerTest : ContainerParameterService
    {
        public SqlServerTest() : base("sqlservertest", "SqlServer Test") { }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting SQL Server Test");

            IDalCity dalCity = new DalCity();

            using (Watch.Create("Sync Test"))
            {
                for (var i = 0; i < 30; i++)
                {
                    using (var w = Watch.Create())
                    {
                        var values = dalCity.GetAll();
                        w.Tap("GetAll");
                        var arr = values.ToArray();
                        w.Tap($"Entity Binding: {arr.Length} items");
                    }
                }
            }


            Console.ReadLine();

            Task.Run(async () =>
            {
                using (Watch.Create("Async Test"))
                {
                    for (var i = 0; i < 30; i++)
                    {
                        using (var w = Watch.Create())
                        {
                            var values = await dalCity.GetAllAsync().ConfigureAwait(false);
                            w.Tap("GetAll");
                            var arr = values.ToArray();
                            w.Tap($"Entity Binding: {arr.Length} items");
                        }
                    }
                }
            });

            Console.ReadLine();
        }
    }
    public class MiddleDalSettings : EntityDalSettings
    {
        public MiddleDalSettings()
        {
            ConnectionString = "Data Source=10.10.1.50;Initial Catalog=MRFLY_MIDDLE;User Id=MRFLY_SVC;Password=mISTERfLY13;Pooling=True";
            QueryType = DataAccessType.StoredProcedure;
        }
    }
    public class MiddleDatabaseDal : EntityDal
    {
        protected override IDataAccess OnGetDataAccess(EntityDalSettings settings)
        {
            Core.Log.Warning("Creating DataAccess Object");
            return new SqlServerDataAccess(Settings.ConnectionString, settings.QueryType);
        }
        protected override IDataAccessAsync OnGetDataAccessAsync(EntityDalSettings settings)
        {
            Core.Log.Warning("Creating DataAccess Object");
            return new SqlServerDataAccess(Settings.ConnectionString, settings.QueryType);
        }
        protected override EntityDalSettings OnGetSettings()
        {
            Core.Log.Warning("Creating Settings Object");
            return new MiddleDalSettings();
        }
    }

    public class EntCity
    {
        public Guid CityId { get; set; }
        public string Name { get; set; }
        public string Iata { get; set; }
        public int NumericCode { get; set; }
        public string Icao { get; set; }
        public string UIC { get; set; }
        public string ISO31662 { get; set; }
        public decimal? Gmt { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
    public interface IDalCity
    {
        IEnumerable<EntCity> GetAll();
        IEnumerable<EntCity> GetAll(string cultureInfo);
        EntCity GetByCityId(Guid cityId);
        EntCity GetByCityId(Guid cityId, string cultureInfo);
        EntCity GetByIata(string iata);
        EntCity GetByIata(string iata, string cultureInfo);
        int Delete(Guid cityId);
        int Insert(EntCity entity);
        int Update(EntCity entity);

        Task<IEnumerable<EntCity>> GetAllAsync();
        Task<IEnumerable<EntCity>> GetAllAsync(string cultureInfo);
        Task<EntCity> GetByCityIdAsync(Guid cityId);
        Task<EntCity> GetByCityIdAsync(Guid cityId, string cultureInfo);
        Task<EntCity> GetByIataAsync(string iata);
        Task<EntCity> GetByIataAsync(string iata, string cultureInfo);
        Task<int> DeleteAsync(Guid cityId);
        Task<int> InsertAsync(EntCity entity);
        Task<int> UpdateAsync(EntCity entity);

    }
    public class DalCity : MiddleDatabaseDal, IDalCity
    {
        public IEnumerable<EntCity> GetAll()
            => Data.SelectElements<EntCity>("sp_GEO_CITIES_GetAll");

        public IEnumerable<EntCity> GetAll(string cultureInfo)
            => Data.SelectElements<EntCity>("sp_GEO_CITIES_GetAllCultureInfo", new { CultureInfo = cultureInfo });

        public EntCity GetByCityId(Guid cityId)
            => Data.SelectElement<EntCity>("sp_GEO_CITIES_GetByCityId", new { CityId = cityId });

        public EntCity GetByCityId(Guid cityId, string cultureInfo)
            => Data.SelectElement<EntCity>("sp_GEO_CITIES_GetByCityIdCultureInfo", new { CityId = cityId, CultureInfo = cultureInfo });

        public EntCity GetByIata(string iata)
            => Data.SelectElement<EntCity>("sp_GEO_CITIES_GetByIata", new { Iata = iata });

        public EntCity GetByIata(string iata, string cultureInfo)
            => Data.SelectElement("sp_GEO_CITIES_GetByIataCultureInfo", new { Iata = iata, CultureInfo = cultureInfo }, FillEntity);

        public int Delete(Guid cityId)
            => Data.ExecuteNonQuery("sp_GEO_CITIES_Del", new { CityId = cityId });

        public int Insert(EntCity entity)
            => Data.ExecuteNonQuery("sp_GEO_CITIES_Ins", entity);

        public int Update(EntCity entity)
            => Data.ExecuteNonQuery("sp_GEO_CITIES_Upd", entity);

        //

        public Task<IEnumerable<EntCity>> GetAllAsync()
            => DataAsync.SelectElementsAsync<EntCity>("sp_GEO_CITIES_GetAll");

        public Task<IEnumerable<EntCity>> GetAllAsync(string cultureInfo)
            => DataAsync.SelectElementsAsync<EntCity>("sp_GEO_CITIES_GetAllCultureInfo", new { CultureInfo = cultureInfo });

        public Task<EntCity> GetByCityIdAsync(Guid cityId)
            => DataAsync.SelectElementAsync<EntCity>("sp_GEO_CITIES_GetByCityId", new { CityId = cityId });

        public Task<EntCity> GetByCityIdAsync(Guid cityId, string cultureInfo)
            => DataAsync.SelectElementAsync<EntCity>("sp_GEO_CITIES_GetByCityIdCultureInfo", new { CityId = cityId, CultureInfo = cultureInfo });

        public Task<EntCity> GetByIataAsync(string iata)
            => DataAsync.SelectElementAsync<EntCity>("sp_GEO_CITIES_GetByIata", new { Iata = iata });

        public Task<EntCity> GetByIataAsync(string iata, string cultureInfo)
            => DataAsync.SelectElementAsync("sp_GEO_CITIES_GetByIataCultureInfo", new { Iata = iata, CultureInfo = cultureInfo }, FillEntity);

        public Task<int> DeleteAsync(Guid cityId)
            => DataAsync.ExecuteNonQueryAsync("sp_GEO_CITIES_Del", new { CityId = cityId });

        public Task<int> InsertAsync(EntCity entity)
            => DataAsync.ExecuteNonQueryAsync("sp_GEO_CITIES_Ins", entity);

        public Task<int> UpdateAsync(EntCity entity)
            => DataAsync.ExecuteNonQueryAsync("sp_GEO_CITIES_Upd", entity);



        EntCity FillEntity(EntityBinder binder, object[] rowValues)
        {
            var EntCity = binder.Bind<EntCity>(rowValues);
            return EntCity;
        }

    }
    

}