/*
Copyright 2015-2018 Daniel Adrian Redondo Suarez

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
 
    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
 */

using System;
using System.Threading.Tasks;
using TWCore.Diagnostics.Api.MessageHandlers.RavenDb;
using TWCore.Diagnostics.Api.Models;
using TWCore.Serialization;
using TWCore.Services;
// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Api
{
    public class DiagnosticMessagingServiceAsync : BusinessMessagesServiceAsync<DiagnosticMessagingBusinessAsync>
    {
        protected override void OnInit(string[] args)
        {
            EnableMessagesTrace = false;
            SerializerManager.SupressFileExtensionWarning = true;
            base.OnInit(args);

	        //var data = DbHandlers.Instance.Query.GetEnvironmentsAndApps().WaitAndResults();
	        //var data2 = DbHandlers.Instance.Query.GetEnvironmentsAndApps().WaitAndResults();
	        //var data3 = DbHandlers.Instance.Query.GetEnvironmentsAndApps().WaitAndResults();

	        //var status = ((RavenDbQueryHandler) DbHandlers.Instance.Query).GetCurrentStatus("DEV", null, null);
	        

         //   Task.Delay(6000).ContinueWith(async _ =>
         //   {
         //       while (true)
         //       {

         //           Core.Trace.Write("Hola Mundo");

         //           await Task.Delay(6000).ConfigureAwait(false);
         //       }
         //   });


            /*
	        var logs = DbHandlers.Instance.Query.GetLogsAsync("Processing message", null, DateTime.MinValue, DateTime.Now).WaitAndResults();
	        var logs2 = DbHandlers.Instance.Query.GetLogsAsync("Processing message", null, DateTime.MinValue, DateTime.Now).WaitAndResults();
	        var logs3 = DbHandlers.Instance.Query.GetLogsAsync("Processing message", null, DateTime.MinValue, DateTime.Now).WaitAndResults();

	        Task.Delay(2000).ContinueWith(async _ =>
	        {
		        while (true)
		        {
			        Core.Log.ErrorGroup(new Exception("Test de Error"), Guid.NewGuid().ToString(), "Reporte de error.");
			        await Task.Delay(2000).ConfigureAwait(false);
		        }
	        });
	        */
        }
    }
}