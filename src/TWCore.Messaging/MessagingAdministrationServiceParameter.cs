/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

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


namespace TWCore.Services
{
    /// <summary>
    /// Messaging administration service parameter
    /// </summary>
    public class MessagingAdministrationServiceParameter : ContainerParameterService
    {
        public MessagingAdministrationServiceParameter() : base("createqueues", "Reads the default configuration and create all needed queues.") { }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            var queues = Core.Services.GetDefaultQueuesConfiguration();
            if (queues == null)
            {
                Core.Log.Warning("Nothing to do.");
                return;
            }

            Core.Log.WriteEmptyLine();
            Core.Log.InfoBasic("Checking, Creating and settings permissions to the Queues:");
            Core.Log.WriteEmptyLine();

            foreach (var queue in queues.Items)
            {
                Core.Log.InfoBasic("Processing queue = {0}", queue.Name);
                if (queue.Types.Admin.IsNotNullOrEmpty())
                {
                    var manager = queue.GetQueueManager();
                    manager.CreateClientQueues();
                    manager.CreateServerQueues();
                }
                else
                    Core.Log.Warning("Queue was skipped because the Admin type wasn't found in the queues.xml");
                Core.Log.WriteEmptyLine();
            }
        }
    }
}
