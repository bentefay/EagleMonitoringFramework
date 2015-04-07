using System;
using System.Collections;
using System.IO;
using System.Timers;
using ProductMonitor.ProgramCode;

namespace ProductMonitor.Generic
{
    //cleans up the temp file
    class Cleanup
    {
        ArrayList cleanups;
        Timer cleanuptimer;

        static Cleanup instance;

        private Cleanup()
        {
            cleanups = new ArrayList();
            cleanuptimer = new Timer();
            cleanuptimer.Interval = 15 * 60 * 1000; //15 minutes
            cleanuptimer.Elapsed += new ElapsedEventHandler(cleanuptimer_Elapsed);
            cleanuptimer.AutoReset = true;
            cleanuptimer.Start();
        }

        public static Cleanup GetInstance()
        {
            if (instance == null)
            {
                instance = new Cleanup();
            }

            return instance;
        }

        void  cleanuptimer_Elapsed(object sender, ElapsedEventArgs e)
        {




            foreach (string D in Directory.GetDirectories(Program.TempPath))
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
