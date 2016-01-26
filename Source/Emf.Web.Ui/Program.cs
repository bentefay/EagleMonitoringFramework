using System;
using Emf.Web.Ui.AppStartup;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.Services.Common;
using Owin;

namespace Emf.Web.Ui
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            SerilogConfig.Initialize();

            var credentials = new VssCredentials(null, CredentialPromptType.PromptIfNeeded);
            

            const string url = "http://+:8080";

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Running on {0}", url);
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            SignalRConfig.Initialize(app);
            FileSystemConfig.Initialize(app);
            app.UseNancy();
        }        
    }
}