using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TWCore.Collections;
using TWCore.Diagnostics.Api.Models;
using TWCore.Diagnostics.Api.Models.Counters;
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
            if (toDate == DateTime.MinValue) toDate = Core.Now.Date;
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
            if (toDate == DateTime.MinValue) toDate = Core.Now.Date;
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
            if (toDate == DateTime.MinValue) toDate = Core.Now.Date;
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

        /// <summary>
        /// Get Trace object
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="id">Trace id</param>
        /// <returns>Trace object</returns>
        [HttpGet("{environment}/traces/raw/{id}")]
        public Task<SerializedObject> GetTraceObjectAsync([FromRoute] string environment, [FromRoute] string id)
        {
            return DbHandlers.Instance.Query.GetTraceObjectAsync(id);
        }
        /// <summary>
        /// Get Trace object in xml
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="id">Trace id</param>
        /// <returns>Trace object</returns>
        [HttpGet("{environment}/traces/xml/{id}")]
        public async Task<string> GetTraceObjectValueInXmlAsync([FromRoute] string environment, [FromRoute] string id)
        {
            id = WebUtility.UrlDecode(id);
            var xmlData = await DbHandlers.Instance.Query.GetTraceXmlAsync(id).ConfigureAwait(false);
            if (xmlData != null)
                return xmlData;
            var serObject = await DbHandlers.Instance.Query.GetTraceObjectAsync(id).ConfigureAwait(false);
            if (serObject is null) return null;
            try
            {
                var value = serObject.GetValueOrGenericValue();
                if (value is null) return null;
                if (value is ResponseMessage rsMessage)
                    return rsMessage.Body?.GetValueOrGenericValue()?.SerializeToXml();
                if (value is RequestMessage rqMessage)
                    return rqMessage.Body?.GetValueOrGenericValue()?.SerializeToXml();
                if (value is string strValue)
                {
                    try
                    {
                        var fValue = PrintXML(strValue);
                        if (!string.IsNullOrEmpty(fValue))
                            return fValue;
                    }
                    catch { }
                    return strValue;
                }
                return value.SerializeToXml();
            }
            catch (Exception ex)
            {
                return new SerializableException(ex).SerializeToXml();
            }
        }
        /// <summary>
        /// Get Trace object in json
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="id">Trace id</param>
        /// <returns>Trace object</returns>
        [HttpGet("{environment}/traces/json/{id}")]
        public async Task<string> GetTraceObjectValueInJsonAsync([FromRoute] string environment, [FromRoute] string id)
        {
            id = WebUtility.UrlDecode(id);
            var jsonData = await DbHandlers.Instance.Query.GetTraceJsonAsync(id).ConfigureAwait(false);
            if (jsonData != null)
                return jsonData;
            var serObject = await DbHandlers.Instance.Query.GetTraceObjectAsync(id).ConfigureAwait(false);
            if (serObject is null) return null;
            try
            {
                var value = serObject.GetValueOrGenericValue();
                if (value is null) return null;
                if (value is ResponseMessage rsMessage)
                {
                    var rsBody = rsMessage.Body?.GetValueOrGenericValue();
                    return rsBody != null ? JsonSerializer.SerializeToString(rsBody, rsBody.GetType()) : null;
                }
                if (value is RequestMessage rqMessage)
                {
                    var rqBody = rqMessage.Body?.GetValueOrGenericValue();
                    return rqBody != null ? JsonSerializer.SerializeToString(rqBody, rqBody.GetType()) : null;
                }
                if (value is string strValue)
                {
                    try
                    {
                        var indentedValue = JValue.Parse(strValue).ToString(Newtonsoft.Json.Formatting.Indented);
                        return indentedValue;
                    }
                    catch { }
                    return strValue;

                }
                return JsonSerializer.SerializeToString(value, value.GetType());
            }
            catch (Exception ex)
            {
                return JsonSerializer.SerializeToString(new SerializableException(ex));
            }
        }
        /// <summary>
        /// Get Trace object in txt
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="id">Trace id</param>
        /// <returns>Trace object</returns>
        [HttpGet("{environment}/traces/txt/{id}")]
        public async Task<string> GetTraceObjectValueInTxtAsync([FromRoute] string environment, [FromRoute] string id)
        {
            id = WebUtility.UrlDecode(id);
            var txtData = await DbHandlers.Instance.Query.GetTraceTxtAsync(id).ConfigureAwait(false);
            if (txtData != null)
            {
                if ((txtData.StartsWith('{') || txtData.StartsWith('[')) && (txtData.EndsWith('}') || txtData.EndsWith(']')))
                {
                    try
                    {
                        var indentedValue = JValue.Parse(txtData).ToString(Newtonsoft.Json.Formatting.Indented);
                        return indentedValue;
                    }
                    catch { }
                    return txtData;
                }
                if (txtData.StartsWith("<?xml") || txtData.StartsWith('<'))
                {
                    try
                    {
                        var indentedValue = PrintXML(txtData);
                        if (!string.IsNullOrEmpty(indentedValue))
                            return indentedValue;
                    }
                    catch { }
                    return txtData;
                }

                //<?xml
            }
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
            if (toDate == DateTime.MinValue) toDate = Core.Now.Date;
            fromDate = fromDate.Date;
            toDate = toDate.Date.AddDays(1).AddSeconds(-1);
            return DbHandlers.Instance.Query.SearchAsync(environment, searchTerm, fromDate, toDate);
        }
        /// <summary>
        /// Gets the metadata from a group name
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <returns>Metadata results</returns>
        [HttpGet("{environment}/metadata/{groupName}")]
        public async Task<KeyValue[]> GetMetadatasAsync([FromRoute]string environment, [FromRoute]string groupName)
        {
            groupName = groupName?.Trim();
            if (groupName == null) return Array.Empty<KeyValue>();
            var result = await DbHandlers.Instance.Query.GetMetadatas(groupName).ConfigureAwait(false);
            return result.OrderBy(i => i.Key).ToArray();
        }
        /// <summary>
        /// Get Statuses
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="machine">Machine name</param>
        /// <param name="application">Application name</param>
        /// <param name="fromDate">From date and time</param>
        /// <param name="toDate">To date and time</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Status items</returns>
        [HttpGet("{environment}/status")]
        public Task<PagedList<NodeStatusItem>> GetStatusesAsync([FromRoute] string environment, string machine, string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
        {
            if (toDate == DateTime.MinValue) toDate = Core.Now.Date;
            fromDate = fromDate.Date;
            toDate = toDate.Date.AddDays(1).AddSeconds(-1);
            return DbHandlers.Instance.Query.GetStatusesAsync(environment, machine, application, fromDate, toDate, page, pageSize);
        }
        /// <summary>
        /// Gets the current status 
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="machine">Machine name</param>
        /// <param name="application">Application name</param>
        /// <returns>Current status</returns>
        [HttpGet("{environment}/status/current")]
        public Task<List<NodeStatusItem>> GetCurrentStatus([FromRoute] string environment, string machine, string application)
        {
            return DbHandlers.Instance.Query.GetCurrentStatus(environment, machine, application);
        }

        /// <summary>
        /// Get Counters
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <returns>List of counters</returns>
        [HttpGet("{environment}/counters")]
        public Task<List<NodeCountersQueryItem>> GetCounters([FromRoute] string environment)
        {
            return DbHandlers.Instance.Query.GetCounters(environment);
        }
        /// <summary>
        /// Get Counters
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <returns>List of counters</returns>
        [HttpGet("{environment}/counters/{counterId}")]
        public Task<NodeCountersQueryItem> GetCounters([FromRoute] Guid counterId)
        {
            return DbHandlers.Instance.Query.GetCounter(counterId);
        }
        /// <summary>
        /// Get Counter Values
        /// </summary>
        /// <param name="counterId">Counter id</param>
        /// <param name="fromDate">From date and time</param>
		/// <param name="toDate">To date and time</param>
		/// <param name="limit">Values count limit</param>
        /// <returns>List of counter values</returns>
        [HttpGet("{environment}/countervalues/{counterId}")]
        public Task<List<NodeCountersQueryValue>> GetCounterValues([FromRoute] string environment, [FromRoute] Guid counterId, DateTime fromDate, DateTime toDate, int limit = 3600)
        {
            if (toDate == DateTime.MinValue) toDate = Core.Now.Date;
            fromDate = fromDate.Date;
            toDate = toDate.Date.AddDays(1).AddSeconds(-1);
            return DbHandlers.Instance.Query.GetCounterValues(counterId, fromDate, toDate, limit);
        }
        /// <summary>
        /// Get Last Counter Values
        /// </summary>
        /// <param name="counterId">Counter id</param>
        /// <param name="valuesDivision">Counter values division</param>
        /// <param name="samples">Samples quantity</param>
        /// <returns>Values list</returns>
        [HttpGet("{environment}/lastcountervalues/{counterId}/{valuesDivision}/{samples?}")]
        public Task<List<NodeLastCountersValue>> GetLastCounterValues(Guid counterId, CounterValuesDivision valuesDivision, int samples = 0, DateTime? lastDate = default)
        {
            return DbHandlers.Instance.Query.GetLastCounterValues(counterId, valuesDivision, samples, lastDate);
        }


        #region Private Methods
        private static string PrintXML(string xml)
        {
            string result = null;

            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
            XmlDocument document = new XmlDocument();

            try
            {
                // Load the XmlDocument with the XML.
                document.LoadXml(xml);

                writer.Formatting = Formatting.Indented;

                // Write the XML into a formatting XmlTextWriter
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();

                // Have to rewind the MemoryStream in order to read
                // its contents.
                mStream.Position = 0;

                // Read MemoryStream contents into a StreamReader.
                StreamReader sReader = new StreamReader(mStream);

                // Extract the text from the StreamReader.
                string formattedXml = sReader.ReadToEnd();

                result = formattedXml;
            }
            catch (XmlException)
            {
                // Handle the exception
            }

            mStream.Close();
            writer.Close();

            return result;
        }
        #endregion
    }
}