using System.Collections;
using System.IO;
using System.Timers;

namespace ProductMonitor.Framework.Generic
{
    //cleans up the temp file
    public class Cleanup
    {
        private readonly string _tempPath;

        public Cleanup(string tempPath)
        {
            _tempPath = tempPath;
            var cleanuptimer = new Timer();
            cleanuptimer.Interval = 15 * 60 * 1000; // 15 minutes
            cleanuptimer.Elapsed += OnTimerElapsed;
            cleanuptimer.AutoReset = true;
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
