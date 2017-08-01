using System;
using System.Text;
using System.Threading.Tasks;
using TWCore.Net.Browser;
using TWCore.Services;

namespace TWCore.Tests
{
    public class BrowserEmulatorTest : ContainerParameterServiceAsync
    {
        public BrowserEmulatorTest() : base("browserTest", "Browser Emulator Test") { }
        protected override async Task OnHandlerAsync(ParameterHandlerInfo info)
        {
            try
            {
                Core.Log.Warning("Starting BROWSER EMULATOR TEST");
                var be = new BrowserEmulator();
                Core.Log.Verbose("Getting: www.google.com");
                var br = await be.NavigateAsync(new BrowserRequest { RequestUrl = "http://www.google.com" }).ConfigureAwait(false);
                var response = Encoding.UTF8.GetString(br.Content);
                Core.Log.Verbose("Results:");
                Core.Log.InfoBasic(response);
                Core.Log.InfoBasic("END");
            }
            catch(Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
    }
}