using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;

namespace WebProductMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            const string url = "http://localhost:12345/";

            // netsh http add urlacl url=http://+:8080/ user=DOMAIN\username
            // netsh http delete urlacl url=http://+:8080/
            // DOMAIN = domain or computer name
            // username = run whoami at command prompt
            // GetPermission(url);

            using (WebApp.Start<Startup>(url))
            {
                using (new System.Threading.Timer(state =>
                {
                    GlobalHost.ConnectionManager.GetHubContext<ProductMonitorHub>().Clients.All.BroadcastMessage("Bob", "Message");
                }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5)))
                {
                    ProcessUserInput(url);
                }
            }
        }

        private static void ProcessUserInput(string url)
        {
            Console.WriteLine("Running at {0}...", url);
            Console.WriteLine("q to quit, anything else to launch your browser...");

            Process.Start(new ProcessStartInfo
            {
                FileName = url
            });

            while (true)
            {
                switch (Console.ReadLine())
                {
                    case "q":
                    case "Q":
                        return;
                    default:
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = url
                        });
                        break;
                }
            }
        }

        private static void GetPermission(string url)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = String.Format(@"http add urlacl url=""{0}"" user=""{1}\{2}""", ForwardSlashTerminate(url), 
                    Environment.UserDomainName, Environment.UserName)
            };

            if (Environment.OSVersion.Version.Major >= 6)
                startInfo.Verb = "runas";

            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit();
            }
        }

        private static string ForwardSlashTerminate(string url)
        {
            return url.Last() != '/' ? url + "/" : url;
        }
    }
}
