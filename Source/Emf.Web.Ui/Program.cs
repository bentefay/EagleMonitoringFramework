using System;
using Emf.Web.Ui.AppStartup;
using Emf.Web.Ui.Services.CredentialManagement;
using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.Services.Common;
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
                Credentials = CredentialsService.Get();

                UnityConfig.GetConfiguredContainer();

                const string url = "http://+:8080";

                using (WebApp.Start<Startup>(url))
                {
                    Console.WriteLine("Running on {0}", url);
                    Console.WriteLine("Press:");
                    Console.WriteLine("b - to open your default browser");
                    Console.WriteLine("d - to delete credentials");
                    Console.WriteLine("q - to exit");

                    while (true)
                    {
                        var key = Console.ReadKey();
                        switch (key.Key)
                        {
                            case ConsoleKey.B:
                                System.Diagnostics.Process.Start(url.Replace("+", "localhost"));
                                break;
                            case ConsoleKey.Q:
                                return;
                            case ConsoleKey.D:
                                CredentialsService.Delete();
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

        public static VssCredentials Credentials { get; private set; }
    }
}