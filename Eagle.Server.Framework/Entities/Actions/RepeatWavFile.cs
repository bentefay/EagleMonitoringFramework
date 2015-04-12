using System.Linq;
using System.Xml;
using Eagle.Server.Framework.Services;

namespace Eagle.Server.Framework.Entities.Actions
{
    public class RepeatWavFile : Action
    {
        private readonly SoundService _soundService;
        private readonly string _soundFile;

        public RepeatWavFile(XmlNode input, SoundService soundService)
        {
            _soundService = soundService;
            _soundFile = input.ChildNodes.Cast<XmlNode>().Where(childNode => childNode.Name == "File").Select(childNode => childNode.FirstChild.Value).Last();
        }

        public override void Execute()
        {
            _soundService.PlayRepeating(_soundFile, Trigger.GetCheck().GetStatus());            
        }
    }
}
