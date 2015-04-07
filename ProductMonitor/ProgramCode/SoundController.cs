using System.Media;
using System.Threading;

namespace ProductMonitor.ProgramCode
{
    static class SoundController
    {
        static SoundPlayer _alarmSound = new SoundPlayer();
        static bool _playingOnce;

        public static void PlayOnce(string file)
        {
            if (_alarmSound == null)
            {
                _playingOnce = true;
                _alarmSound = new SoundPlayer(file);
                _alarmSound.PlaySync();
                _alarmSound = null;
                
            }
        }

        public static void PlayRepeating(string file, string message)
        {
            lock (_alarmSound)
            {
                if (_alarmSound.SoundLocation == "" || _playingOnce)
                {
                    _playingOnce = false;
                    _alarmSound = new SoundPlayer(file);

                    _boxMessage = message;

                    try
                    {
                        _alarmSound.PlayLooping();
                    }
                    catch
                    {
                        //Do some really hard number crunching here
                    }
                    finally
                    {
                        var stopSoundFunction = new ThreadStart(StopSound);
                        var soundStoppingThread = new Thread(stopSoundFunction)
                        {
                            IsBackground = true
                        };
                        soundStoppingThread.Start();
                    }
                }
            }
        }

        static string _boxMessage;

        private static void StopSound() 
        {
            System.Windows.Forms.MessageBox.Show(_boxMessage);
            _alarmSound.Stop();
            _alarmSound = new SoundPlayer();
        }
    }
}
