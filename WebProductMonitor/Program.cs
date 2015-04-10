using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using ProductMonitor.Framework;
using ProductMonitor.Framework.Generic;
using ProductMonitor.Framework.Services;
using Serilog;
using WebProductMonitor.Hubs;
using WebProductMonitor.Services;

namespace WebProductMonitor
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

        static void Main()
        {
            const string url = "http://localhost:12345/";

            Log.Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            Log.Information("Application Start");

            var tempPath = AppDomain.CurrentDomain.BaseDirectory + "TEMP";
            var cleanup = new Cleanup(tempPath);
            var messageService = new MessageService();
            var screenshotService = new ScreenshotService();
            var emailController = new EmailService(tempPath, screenshotService, messageService, cleanup);
            var soundController = new SoundService(messageService);
            var globalAlarm = new GlobalAlarmService(emailController);

            var xmlFile = new XmlFile(_configFilePathRoot, messageService, emailController, globalAlarm, soundController, (s, i) => new Check(i, c => { }, globalAlarm));
            var listOfChecks = xmlFile.Load();

            globalAlarm.PrepareList(listOfChecks);

            foreach (var c in listOfChecks)
                c.Activate();

            var random = new Random();

            using (WebApp.Start<Startup>(url))
            {
                using (new Timer(state => GlobalHost
                    .ConnectionManager
                    .GetHubContext<ProductMonitorHub>()
                    .Clients
                    .All
                    .BroadcastMessage(
                        random.Next(2) == 0 ? "Ben" : "Dylan", 
                        random.Next(2) == 0 ? "This is some kind of example message." : "This is a similar but different message."), 
                        null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3)))
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
