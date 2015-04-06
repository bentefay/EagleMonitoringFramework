using System;
using System.Media;
using System.Threading;

namespace ProductMonitor.ProgramCode
{
    static class SoundController
    {
        static SoundPlayer alarmSound = new SoundPlayer();
        static bool playingOnce = false;

        public static void PlayOnce(string file)
        {
            if (alarmSound == null)
            {
                playingOnce = true;
                alarmSound = new SoundPlayer(file);
                alarmSound.PlaySync();
                alarmSound = null;
                
            }
        }

        public static void PlayRepeating(string file, string message)
        {
            lock (alarmSound)
            {
                if (alarmSound.SoundLocation == "" || playingOnce == true)
                {
                    playingOnce = false;
                    alarmSound = new SoundPlayer(file);

                    boxMessage = message;

                    try
                    {
                        alarmSound.PlayLooping();
                    }
                    catch (Exception)
                    {
                        //Do some really hard number crunching here
                    }
                    finally
                    {
                        ThreadStart stopSoundFunction = new ThreadStart(stopSound);
                        Thread soundStoppingThread = new Thread(stopSoundFunction);
                        soundStoppingThread.IsBackground = true;
                        soundStoppingThread.Start();
                    }
                }
            }
        }

        static string boxMessage;

        private static void stopSound() {
            System.Windows.Forms.MessageBox.Show(boxMessage);
            alarmSound.Stop();
            alarmSound = new SoundPlayer();
        }
    }
}
