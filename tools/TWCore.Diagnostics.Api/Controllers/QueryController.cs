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
    public class QueryController : Controller
    {
        [HttpGet("applications")]
        public Task<List<BasicInfo>> GetEnvironmentsAndApps()
        {
            return DbHandlers.Instance.Query.GetEnvironmentsAndApps();
        }

        [HttpGet("{environment}/logs/group/{group}/{application?}")]
        public Task<PagedList<NodeLogItem>> GetLogsByGroup([FromRoute] string environment, string group, [FromRoute] string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
        {
            if (toDate == DateTime.MinValue) toDate = DateTime.Now.Date;
            return DbHandlers.Instance.Query.GetLogsByGroup(environment, group, application, fromDate, toDate, page, pageSize);
        }

        [HttpGet("{environment}/logs/search/{search?}")]
        public Task<PagedList<NodeLogItem>> GetLogsAsync([FromRoute] string environment, [FromRoute] string search, string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
        {
            if (toDate == DateTime.MinValue) toDate = DateTime.Now.Date;
            return DbHandlers.Instance.Query.GetLogsAsync(environment, search, application, fromDate, toDate, page, pageSize);
        }

        [HttpGet("{environment}/logs/level/{level}/{search?}")]
        public Task<PagedList<NodeLogItem>> GetLogsAsync([FromRoute] string environment, [FromRoute] string search, string application, [FromRoute] LogLevel level, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
        {
            if (toDate == DateTime.MinValue) toDate = DateTime.Now.Date;
            return DbHandlers.Instance.Query.GetLogsAsync(environment, search, application, level, fromDate, toDate, page, pageSize);
        }

        [HttpGet("{environment}/traces/group/{group}/{application?}")]
        public Task<PagedList<NodeTraceItem>> GetTracesByGroupAsync([FromRoute] string environment, [FromRoute] string group, [FromRoute] string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
        {
            if (toDate == DateTime.MinValue) toDate = DateTime.Now.Date;
            return DbHandlers.Instance.Query.GetTracesByGroupAsync(environment, group, application, fromDate, toDate, page, pageSize);
        }

        [HttpGet("{environment}/traces/search/{search?}")]
        public Task<PagedList<NodeTraceItem>> GetTracesAsync([FromRoute] string environment, [FromRoute] string search, string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
        {
            if (toDate == DateTime.MinValue) toDate = DateTime.Now.Date;
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
            var serObject = await DbHandlers.Instance.Query.GetTraceObjectAsync(id).ConfigureAwait(false);
            return serObject?.GetValue();
        }

        [HttpGet("{environment}/status")]
        public Task<PagedList<NodeStatusItem>> GetStatusesAsync([FromRoute] string environment, string machine, string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
        {
            if (toDate == DateTime.MinValue) toDate = DateTime.Now.Date;
            return DbHandlers.Instance.Query.GetStatusesAsync(environment, machine, application, fromDate, toDate, page, pageSize);
        }

        [HttpGet("{environment}/status/current")]
        public Task<List<NodeStatusItem>> GetCurrentStatus([FromRoute] string environment, string machine, string application)
        {
            return DbHandlers.Instance.Query.GetCurrentStatus(environment, machine, application);
        }
    }
}