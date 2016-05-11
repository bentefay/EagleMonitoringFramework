using System;
using Emf.Web.Ui.AppStartup;
using Emf.Web.Ui.Services;
using Emf.Web.Ui.Services.CredentialManagement;
using Microsoft.Owin.Hosting;
using Microsoft.Practices.Unity;
using Serilog;

namespace Emf.Web.Ui
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            SerilogConfig.Initialize();

            try
            {
                var unity = UnityConfig.GetConfiguredContainer();
                var credentialService = unity.Resolve<CredentialsService>();
                var connectionSettingsService = unity.Resolve<ConnectionSettingsService>();

                const string url = "http://+:8080";

                using (WebApp.Start<Startup>(url))
                {
                    Console.WriteLine("Running on {0}", url);
                    Console.WriteLine("Press:");
                    Console.WriteLine("b - to open your default browser");
                    Console.WriteLine("dcred - to delete credentials");
                    Console.WriteLine("dconn - to delete TFS connection settings");
                    Console.WriteLine("q - to exit");

                    while (true)
                    {
                        var commands = Console.ReadLine() ?? string.Empty;
                        switch (commands.ToLowerInvariant())
                        {
                            case "b":
                                System.Diagnostics.Process.Start(url.Replace("+", "localhost"));
                                break;
                            case "q":
                                return;
                            case "dcred":
                                credentialService.Delete();
                                Console.WriteLine("Deleted credentials");
                                break;
                            case "dconn":
                                connectionSettingsService.Delete();
                                Console.WriteLine("Deleted TFS connection settings");
                                break;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Log.Fatal(e, "Application has terminated");
            }
        }
    }
}