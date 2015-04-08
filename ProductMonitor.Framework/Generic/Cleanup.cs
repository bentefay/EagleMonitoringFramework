using System;
using System.Collections;
using System.IO;
using System.Timers;

namespace ProductMonitor.Framework.Generic
{
    //cleans up the temp file
    public class Cleanup
    {
        private readonly string _tempPath;
        ArrayList cleanups;
        Timer cleanuptimer;

        public Cleanup(string tempPath)
        {
            _tempPath = tempPath;
            cleanups = new ArrayList();
            cleanuptimer = new Timer();
            cleanuptimer.Interval = 15 * 60 * 1000; //15 minutes
            cleanuptimer.Elapsed += new ElapsedEventHandler(cleanuptimer_Elapsed);
            cleanuptimer.AutoReset = true;
            cleanuptimer.Start();
        }

        void  cleanuptimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (string D in Directory.GetDirectories(_tempPath))
            {
                Directory.Delete(D, true);
            }
        }

        public void AddCleanup(string filepath)
        {
            /*cleanups.Add(new CleanupRequest(filepath, DateTime.Now));*/
        }

        struct CleanupRequest
        {
            public string File;
            public DateTime TimeAdded;

            public CleanupRequest(string file, DateTime added)
            {
                File = file;
                TimeAdded = added;
            }
        }
    }
}
