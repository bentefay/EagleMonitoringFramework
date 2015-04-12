using System;
using System.Linq;
using System.Xml;
using Eagle.Server.Framework.Services;

namespace Eagle.Server.Framework.Entities.Actions
{
    public class PlayWavFile : Action
    {
        private readonly SoundService _soundService;
        private readonly string _soundFile;

        public PlayWavFile(XmlNode input, SoundService soundService)
        {
            _soundService = soundService;
            foreach (var childNode in input.ChildNodes.Cast<XmlNode>().Where(childNode => String.Equals(childNode.Name, "File", StringComparison.InvariantCultureIgnoreCase)))
            {
                _soundFile = childNode.FirstChild.Value;
            }
        }

        public override void Execute()
        {
            _soundService.PlayOnce(_soundFile);
        }
    }
}
