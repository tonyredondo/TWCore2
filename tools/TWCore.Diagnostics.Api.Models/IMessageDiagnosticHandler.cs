using System.Collections.Generic;
using System.Threading.Tasks;
using TWCore.Diagnostics.Log;
using TWCore.Diagnostics.Status;
using TWCore.Diagnostics.Trace.Storages;
// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Api.Models
{
    /// <summary>
    /// Message diagnostic handler interface
    /// </summary>
    public interface IMessageDiagnosticHandler
    {
        /// <summary>
        /// Process LogItems message
        /// </summary>
        /// <param name="message">Message to handle</param>
        /// <returns>Process task</returns>
        Task ProcessLogItemsMessage(List<LogItem> message);
        /// <summary>
        /// Process TraceItems message
        /// </summary>
        /// <param name="message">Message to handle</param>
        /// <returns>Process task</returns>
        Task ProcessTraceItemsMessage(List<MessagingTraceItem> message);
        /// <summary>
        /// Process Status message
        /// </summary>
        /// <param name="message">Message to handle</param>
        /// <returns>Process task</returns>
        Task ProcessStatusMessage(StatusItemCollection message);
    }
}