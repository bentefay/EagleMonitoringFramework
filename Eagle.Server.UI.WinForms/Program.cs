using System;
using System.IO;
using System.Threading;
using Eagle.Server.Framework;
using Eagle.Server.Framework.Services;
using Eagle.Server.UI.WinForms.DisplayCode;
using Serilog;

namespace Eagle.Server.UI.WinForms
{
    static class Program
    {
        private static readonly string _configFilePathRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config");
        private static Check[] _listOfChecks;
        private static string _tempPath;

        public static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            Log.Information("Application Start");

            try
            {
                _tempPath = AppDomain.CurrentDomain.BaseDirectory + "TEMP";
                var cleanupService = new CleanupService(_tempPath);
                var messageService = new MessageService();
                var screenshotService = new ScreenshotService();
                var emailController = new EmailService(_tempPath, screenshotService, messageService, cleanupService);
                var soundController = new SoundService(messageService);
                var alarmService = new AlarmService(emailController);

                var guiController = new GuiController();

                guiController.StartUp(() => _listOfChecks ?? new Check[0]);

                _listOfChecks = new XmlFile(_configFilePathRoot, messageService, emailController, alarmService, soundController,
                    (s, i) => new Check(i, alarmService, c => guiController.Update(c))).Load();

                alarmService.PrepareList(_listOfChecks);

                foreach (var c in _listOfChecks)
                    c.Activate();

                Thread.Sleep(Timeout.Infinite);

            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to start application");
            }
        }

        public static void PauseCheck(int index)
        {
            if (!_listOfChecks[index].IsPaused())
            {
                _listOfChecks[index].Pause(true);
                _listOfChecks[index].Activate();
            }
        }

        public static void UnpauseCheck(int index)
        {
            if (_listOfChecks[index].IsPaused())
            {
                _listOfChecks[index].Pause(false);
                _listOfChecks[index].Activate();
            }
        }

        public static void Exit()
        {
            //clean up (will not get everything)
            try
            {
                Directory.Delete(_tempPath, true);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to delete directory on exit");
            }

            Log.Information("Application Exit");

            Environment.Exit(0);
        }

        public static Check[] ListOfChecks
        {
            get
            {
                return _listOfChecks;
            }
        }
    }
}
