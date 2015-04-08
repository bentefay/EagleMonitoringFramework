using System.Linq;
using System.Xml;

namespace ProductMonitor.Framework.ProgramCode.Actions
{
    public class RepeatWavFile : Action
    {
        private readonly SoundController _soundController;
        private readonly string _soundFile;

        public RepeatWavFile(XmlNode input, SoundController soundController)
        {
            _soundController = soundController;
            _soundFile = input.ChildNodes.Cast<XmlNode>().Where(childNode => childNode.Name == "File").Select(childNode => childNode.FirstChild.Value).Last();
        }

        public override void Execute()
        {
            _soundController.PlayRepeating(_soundFile, trigger.getCheck().GetStatus());            
        }
    }
}
