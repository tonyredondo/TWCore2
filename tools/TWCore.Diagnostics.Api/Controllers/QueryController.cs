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
using TWCore.Serialization;

namespace TWCore.Diagnostics.Api.Controllers
{
    [Route("api/query")]
    public class QueryController : Controller
    {
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




        [HttpGet("{environment}/logs/group/{group}/{application?}")]
        public Task<PagedList<NodeLogItem>> GetLogsByGroup([FromRoute] string environment, string group, [FromRoute] string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
        {
            if (toDate == DateTime.MinValue) toDate = DateTime.Now.Date;
            fromDate = fromDate.Date;
            toDate = toDate.Date.AddDays(1).AddSeconds(-1);
            return DbHandlers.Instance.Query.GetLogsByGroupAsync(environment, group, application, fromDate, toDate, page, pageSize);
        }

        [HttpGet("{environment}/logs/search/{search?}")]
        public Task<PagedList<NodeLogItem>> GetLogsAsync([FromRoute] string environment, [FromRoute] string search, string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
        {
            if (toDate == DateTime.MinValue) toDate = DateTime.Now.Date;
            fromDate = fromDate.Date;
            toDate = toDate.Date.AddDays(1).AddSeconds(-1);
            return DbHandlers.Instance.Query.GetLogsAsync(environment, search, application, fromDate, toDate, page, pageSize);
        }

        [HttpGet("{environment}/logs/level/{level}/{search?}")]
        public Task<PagedList<NodeLogItem>> GetLogsAsync([FromRoute] string environment, [FromRoute] string search, string application, [FromRoute] LogLevel level, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
        {
            if (toDate == DateTime.MinValue) toDate = DateTime.Now.Date;
            fromDate = fromDate.Date;
            toDate = toDate.Date.AddDays(1).AddSeconds(-1);
            return DbHandlers.Instance.Query.GetLogsAsync(environment, search, application, level, fromDate, toDate, page, pageSize);
        }

        [HttpGet("{environment}/traces/group/{group}/{application?}")]
        public Task<PagedList<NodeTraceItem>> GetTracesByGroupAsync([FromRoute] string environment, [FromRoute] string group, [FromRoute] string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
        {
            if (toDate == DateTime.MinValue) toDate = DateTime.Now.Date;
            fromDate = fromDate.Date;
            toDate = toDate.Date.AddDays(1).AddSeconds(-1);
            return DbHandlers.Instance.Query.GetTracesByGroupAsync(environment, group, application, fromDate, toDate, page, pageSize);
        }

        [HttpGet("{environment}/traces/search/{search?}")]
        public Task<PagedList<NodeTraceItem>> GetTracesAsync([FromRoute] string environment, [FromRoute] string search, string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
        {
            if (toDate == DateTime.MinValue) toDate = DateTime.Now.Date;
            fromDate = fromDate.Date;
            toDate = toDate.Date.AddDays(1).AddSeconds(-1);
            return DbHandlers.Instance.Query.GetTracesAsync(environment, search, application, fromDate, toDate, page, pageSize);
        }

        [HttpGet("{environment}/traces/raw/{id}")]
        public Task<SerializedObject> GetTraceObjectAsync([FromRoute] string environment, [FromRoute] string id)
        {
            return DbHandlers.Instance.Query.GetTraceObjectAsync(id);
        }

        [HttpGet("{environment}/traces/object/{id}")]
        public async Task<object> GetTraceObjectValueAsync([FromRoute] string environment, [FromRoute] string id)
        {
            id = WebUtility.UrlDecode(id);
            var serObject = await DbHandlers.Instance.Query.GetTraceObjectAsync(id).ConfigureAwait(false);
            return serObject?.GetValue();
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