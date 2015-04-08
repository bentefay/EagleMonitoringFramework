using System.Xml;
using ProductMonitor.Framework.Services;

namespace ProductMonitor.Framework.Entities.Actions
{
    public class PlayWavFile : Action
    {
        private readonly SoundService _soundService;
        private readonly string soundFile;

        public PlayWavFile(XmlNode input, SoundService soundService)
        {
            _soundService = soundService;
            foreach (XmlNode childNode in input.ChildNodes)
            {
                if (childNode.Name == "File")
                {
                    soundFile = childNode.FirstChild.Value;
                }
            }
        }

        public override void Execute()
        {
            _soundService.PlayOnce(soundFile);
        }
    }
}
