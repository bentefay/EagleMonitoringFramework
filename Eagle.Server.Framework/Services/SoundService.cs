using System.Media;
using System.Threading;

namespace Eagle.Server.Framework.Services
{
    public class SoundService
    {
        private readonly IMessageService _messageService;
        private SoundPlayer _alarmSound = new SoundPlayer();
        private bool _playingOnce;

        public SoundService(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public void PlayOnce(string file)
        {
            if (_alarmSound == null)
            {
                _playingOnce = true;
                _alarmSound = new SoundPlayer(file);
                _alarmSound.PlaySync();
                _alarmSound = null;
                
            }
        }

        public void PlayRepeating(string file, string message)
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
                        // TODO: Logging here
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

        private string _boxMessage;

        private void StopSound() 
        {
            _messageService.ShowInformation(_boxMessage);
            _alarmSound.Stop();
            _alarmSound = new SoundPlayer();
        }
    }
}
