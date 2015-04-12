using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eagle.Server.Framework;
using Eagle.Server.Framework.Services;
using Eagle.Server.UI.Web.Hubs;
using Eagle.Server.UI.Web.Services;
using Microsoft.Owin.Hosting;
using Serilog;

namespace Eagle.Server.UI.Web
{
    class Program
    {
        private static readonly string _configFilePathRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config");

        // Must run as administrator, or run the following command:
        // netsh http add urlacl url=http://+:8080/ user=DOMAIN\username
        // DOMAIN = domain or computer name
        // username = run whoami at command prompt
        
        // To undo command:
        // netsh http delete urlacl url=http://+:8080/
        
        // Executing the command using code: GetPermission(url);

        private static void Main()
        {
            const string url = "http://localhost:12345/";

            Log.Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            Log.Information("Starting webapp at {0}", url);

            using (WebApp.Start<Startup>(url))
            {
                Log.Information("Started successfully");

                Log.Information("Opening browser");

                Process.Start(new ProcessStartInfo
                {
                    FileName = url
                });

                var hubService = ProductMonitorHubService.Instance;

                StartMonitoringFramework(hubService);

                ProcessUserInput(url);
            }
        }

        private static void StartMonitoringFramework(ProductMonitorHubService hubService)
        {
            Log.Information("Initializing services");

            var tempPath = AppDomain.CurrentDomain.BaseDirectory + "TEMP";
            var cleanup = new CleanupService(tempPath);
            var messageService = new MessageService();
            var screenshotService = new ScreenshotService();
            var emailController = new EmailService(tempPath, screenshotService, messageService, cleanup);
            var soundController = new SoundService(messageService);
            var alarmService = new AlarmService(emailController);

            Log.Information("Loading configuration");

            var xmlFile = new XmlFile(_configFilePathRoot, messageService, emailController, alarmService, soundController,
                checkFactory: (s, i) => new Check(i, alarmService, hubService.UpdateCheck));

            var listOfChecks = xmlFile.Load();

            alarmService.PrepareList(listOfChecks);

            hubService.UpdateChecks(listOfChecks);

            Log.Information("Running all checks offthread");

            Task.Factory.StartNew(() => Parallel.ForEach(listOfChecks, check => check.Activate()), TaskCreationOptions.LongRunning);
        }

        private static void ProcessUserInput(string url)
        {
            Log.Information("Waiting for user input");
            Log.Information("q => quit");
            Log.Information("Anything else => launch your default browser");

            while (true)
            {
                switch (Console.ReadLine())
                {
                    case "q":
                    case "Q":
                        Log.Information("Shutting down");
                        return;
                    default:
                        Log.Information("Opening browser");
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
