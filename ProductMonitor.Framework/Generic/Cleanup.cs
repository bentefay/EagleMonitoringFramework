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
            cleanuptimer.Interval = 15 * 60 * 1000; //15 minutes
            cleanuptimer.Elapsed += cleanuptimer_Elapsed;
            cleanuptimer.AutoReset = true;
            cleanuptimer.Start();
        }

        void  cleanuptimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (string d in Directory.GetDirectories(_tempPath))
            {
                Directory.Delete(d, true);
            }
        }

        public void AddCleanup(string filepath)
        {
        }
    }
}
