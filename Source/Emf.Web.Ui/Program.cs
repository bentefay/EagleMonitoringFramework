using System;
using Emf.Web.Ui.AppStartup;
using Microsoft.Owin.Hosting;

namespace Emf.Web.Ui
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            SerilogConfig.Initialize();         

            const string url = "http://+:8080";

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Running on {0}", url);
                Console.WriteLine("Press:");
                Console.WriteLine("b - to open your default browser");
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
                    }
                }
            }
        }
    }
}