using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TWCore.Diagnostics.Api.Models;
using TWCore.Diagnostics.Api.Models.Log;
using TWCore.Diagnostics.Api.Models.Status;
using TWCore.Diagnostics.Api.Models.Trace;
using TWCore.Diagnostics.Log;
using TWCore.Messaging;
using TWCore.Serialization;

namespace TWCore.Diagnostics.Api.Controllers
{
    [Route("api/query")]
    public class QueryController : Controller
    {
        private static readonly JsonTextSerializer JsonSerializer = new JsonTextSerializer
        {
            Indent = true,
            EnumsAsStrings = true,
            UseCamelCase = true
        };

        /// <summary>
        /// Gets the environments
        /// </summary>
        /// <returns>List of BasicInfo</returns>
        [HttpGet("")]
        public Task<List<string>> GetEnvironments()
        {
            return DbHandlers.Instance.Query.GetEnvironmentsAsync();
        }
        /// <summary>
        /// Gets the Applications with logs by environment
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="fromDate">From date and time</param>
        /// <param name="toDate">To date and time</param>
        /// <returns>List of applications</returns>
        [HttpGet("{environment}/logs/applications")]
        public Task<LogSummary> GetLogsApplicationsLevelsByEnvironment([FromRoute] string environment, DateTime fromDate, DateTime toDate)
        {
            if (toDate == DateTime.MinValue) toDate = DateTime.Now.Date;
            fromDate = fromDate.Date;
            toDate = toDate.Date.AddDays(1).AddSeconds(-1);
            return DbHandlers.Instance.Query.GetLogsApplicationsLevelsByEnvironmentAsync(environment, fromDate, toDate);
        }
        /// <summary>
        /// Gets the Logs by Application Levels and Environment
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="application">Application name</param>
        /// <param name="level">Log level</param>
        /// <param name="fromDate">From date and time</param>
        /// <param name="toDate">To date and time</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Logs</returns>
        [HttpGet("{environment}/logs/{application}/{level?}")]
        public Task<PagedList<NodeLogItem>> GetLogsByApplicationLevelsEnvironment([FromRoute]string environment, [FromRoute] string application, [FromRoute]LogLevel level, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
        {
            if (toDate == DateTime.MinValue) toDate = DateTime.Now.Date;
            fromDate = fromDate.Date;
            toDate = toDate.Date.AddDays(1).AddSeconds(-1);
            return DbHandlers.Instance.Query.GetLogsByApplicationLevelsEnvironmentAsync(environment, application, level, fromDate, toDate, page, pageSize);
        }
        /// <summary>
        /// Gets the traces objects by environment and dates
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="fromDate">From date and time</param>
        /// <param name="toDate">To date and time</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Traces</returns>
        [HttpGet("{environment}/traces")]
        public Task<PagedList<TraceResult>> GetTracesByEnvironmentAsync([FromRoute]string environment, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
        {
            if (toDate == DateTime.MinValue) toDate = DateTime.Now.Date;
            fromDate = fromDate.Date;
            toDate = toDate.Date.AddDays(1).AddSeconds(-1);
            return DbHandlers.Instance.Query.GetTracesByEnvironmentAsync(environment, fromDate, toDate, page, pageSize);
        }
        /// <summary>
        /// Get the traces from a Trace Group
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="groupName">Group name</param>
        /// <returns>Traces from that group</returns>
        [HttpGet("{environment}/traces/{groupName}")]
        public Task<List<NodeTraceItem>> GetTracesByGroupIdAsync([FromRoute]string environment, [FromRoute]string groupName)
        {
            return DbHandlers.Instance.Query.GetTracesByGroupIdAsync(environment, groupName);
        }
        

        [HttpGet("{environment}/traces/raw/{id}")]
        public Task<SerializedObject> GetTraceObjectAsync([FromRoute] string environment, [FromRoute] string id)
        {
            return DbHandlers.Instance.Query.GetTraceObjectAsync(id);
        }
        [HttpGet("{environment}/traces/xml/{id}")]
        public async Task<string> GetTraceObjectValueInXmlAsync([FromRoute] string environment, [FromRoute] string id)
        {
            id = WebUtility.UrlDecode(id);
            var xmlData = await DbHandlers.Instance.Query.GetTraceXmlAsync(id).ConfigureAwait(false);
            if (xmlData != null)
                return xmlData;
            var serObject = await DbHandlers.Instance.Query.GetTraceObjectAsync(id).ConfigureAwait(false);
            try
            {
                var value = serObject?.GetValue();
                if (value is null) return null;
                if (value is ResponseMessage rsMessage)
                    return rsMessage.Body?.GetValue()?.SerializeToXml();
                if (value is RequestMessage rqMessage)
                    return rqMessage.Body?.GetValue()?.SerializeToXml();
                if (value is string strValue)
                    return strValue;
                return value.SerializeToXml();
            }
            catch(Exception ex)
            {
                return new SerializableException(ex).SerializeToXml();
            }
        }
        [HttpGet("{environment}/traces/json/{id}")]
        public async Task<string> GetTraceObjectValueInJsonAsync([FromRoute] string environment, [FromRoute] string id)
        {
            id = WebUtility.UrlDecode(id);
            var jsonData = await DbHandlers.Instance.Query.GetTraceJsonAsync(id).ConfigureAwait(false);
            if (jsonData != null)
                return jsonData;
            var serObject = await DbHandlers.Instance.Query.GetTraceObjectAsync(id).ConfigureAwait(false);
            try
            {
                var value = serObject?.GetValue();
                if (value is null) return null;
                if (value is ResponseMessage rsMessage)
                {
                    var rsBody = rsMessage.Body?.GetValue();
                    return rsBody != null ? JsonSerializer.SerializeToString(rsBody, rsBody.GetType()) : null;
                }
                if (value is RequestMessage rqMessage)
                {
                    var rqBody = rqMessage.Body?.GetValue();
                    return rqBody != null ? JsonSerializer.SerializeToString(rqBody, rqBody.GetType()) : null;
                }
                if (value is string strValue)
                    return strValue;
                return JsonSerializer.SerializeToString(value, value.GetType());
            }
            catch (Exception ex)
            {
                return JsonSerializer.SerializeToString(new SerializableException(ex));
            }
        }
        [HttpGet("{environment}/traces/txt/{id}")]
        public async Task<string> GetTraceObjectValueInTxtAsync([FromRoute] string environment, [FromRoute] string id)
        {
            id = WebUtility.UrlDecode(id);
            var txtData = await DbHandlers.Instance.Query.GetTraceTxtAsync(id).ConfigureAwait(false);
            return txtData;
        }
        

        /// <summary>
        /// Search a term in the database
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="searchTerm">Term to search in the database</param>
        /// <param name="fromDate">From date and time</param>
        /// <param name="toDate">To date and time</param>
        /// <returns>Search results</returns>
        [HttpGet("{environment}/search/{searchTerm}")]
        public Task<SearchResults> SearchAsync([FromRoute]string environment, [FromRoute]string searchTerm, DateTime fromDate, DateTime toDate)
        {
            searchTerm = searchTerm?.Trim();
            if (toDate == DateTime.MinValue) toDate = DateTime.Now.Date;
            fromDate = fromDate.Date;
            toDate = toDate.Date.AddDays(1).AddSeconds(-1);
            return DbHandlers.Instance.Query.SearchAsync(environment, searchTerm, fromDate, toDate);
        }




        [HttpGet("{environment}/status")]
        public Task<PagedList<NodeStatusItem>> GetStatusesAsync([FromRoute] string environment, string machine, string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
        {
            if (toDate == DateTime.MinValue) toDate = DateTime.Now.Date;
            fromDate = fromDate.Date;
            toDate = toDate.Date.AddDays(1).AddSeconds(-1);
            return DbHandlers.Instance.Query.GetStatusesAsync(environment, machine, application, fromDate, toDate, page, pageSize);
        }

        [HttpGet("{environment}/status/current")]
        public Task<List<NodeStatusItem>> GetCurrentStatus([FromRoute] string environment, string machine, string application)
        {
            return DbHandlers.Instance.Query.GetCurrentStatus(environment, machine, application);
        }
    }
}