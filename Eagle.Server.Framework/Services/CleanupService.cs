using System.IO;
using System.Timers;

namespace Eagle.Server.Framework.Services
{
    public class CleanupService
    {
        private readonly string _tempPath;

        public CleanupService(string tempPath)
        {
            _tempPath = tempPath;
            var cleanuptimer = new Timer
            {
                Interval = 15 * 60 * 1000,
                AutoReset = true
            };
            cleanuptimer.Elapsed += OnTimerElapsed;
            cleanuptimer.Start();
        }

        void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var d in Directory.GetDirectories(_tempPath))
            {
                Directory.Delete(d, true);
            }
        }

        public void AddCleanup(string filepath)
        {
            // Not implemented
        }
    }
}
