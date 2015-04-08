using System.Xml;

namespace ProductMonitor.Framework.ProgramCode.Actions
{
    public class PlayWavFile : Action
    {
        private readonly SoundController _soundController;
        private readonly string soundFile;

        public PlayWavFile(XmlNode input, SoundController soundController)
        {
            _soundController = soundController;
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
            _soundController.PlayOnce(soundFile);
        }
    }
}
