using System;
using System.Collections.Generic;
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
    public class QueryController : Controller, IDiagnosticQueryHandler
    {
        [HttpGet("applications")]
        public Task<List<BasicInfo>> GetEnvironmentsAndApps()
            => DbHandlers.Instance.Query.GetEnvironmentsAndApps();

        [HttpGet("logs/group/{group}/{application?}")]
        public Task<List<NodeLogItem>> GetLogsByGroup(string group, string application, DateTime fromDate, DateTime toDate)
            => DbHandlers.Instance.Query.GetLogsByGroup(group, application, fromDate, toDate);

        [HttpGet("logs/search/{search?}")]
        public Task<List<NodeLogItem>> GetLogsAsync(string search, string application, DateTime fromDate, DateTime toDate)
            => DbHandlers.Instance.Query.GetLogsAsync(search, application, fromDate, toDate);

        [HttpGet("logs/level/{level}/{search?}")]
        public Task<List<NodeLogItem>> GetLogsAsync(string search, string application, LogLevel level, DateTime fromDate, DateTime toDate)
            => DbHandlers.Instance.Query.GetLogsAsync(search, application, level, fromDate, toDate);

        [HttpGet("traces/group/{group}/{application?}")]
        public Task<List<NodeTraceItem>> GetTracesByGroupAsync(string group, string application, DateTime fromDate, DateTime toDate)
            => DbHandlers.Instance.Query.GetTracesByGroupAsync(group, application, fromDate, toDate);
        
        [HttpGet("traces/search/{search?}")]
        public Task<List<NodeTraceItem>> GetTracesAsync(string search, string application, DateTime fromDate, DateTime toDate)
            => DbHandlers.Instance.Query.GetTracesAsync(search, application, fromDate, toDate);

        public Task<SerializedObject> GetTraceObjectAsync(NodeTraceItem item)
            => DbHandlers.Instance.Query.GetTraceObjectAsync(item);

        /*
        public async Task<object> GetTraceObjectAsync(string id)
        {
            
        }
        */

        [HttpGet("{environment}/status/")]
        public Task<List<NodeStatusItem>> GetStatusesAsync(string environment, string machine, string application, DateTime fromDate, DateTime toDate)
            => DbHandlers.Instance.Query.GetStatusesAsync(environment, machine, application, fromDate, toDate);
    }
}