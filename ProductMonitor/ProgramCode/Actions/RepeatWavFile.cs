using System.Xml;

namespace ProductMonitor.ProgramCode.Actions
{
    class RepeatWavFile : Action
    {
        private string soundFile;

        //exists for testing purposes
        public RepeatWavFile(object[] input)
        {
            this.input = input;
            soundFile = (string)input[0];
        }

        public RepeatWavFile(XmlNode input)
        {
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
            SoundController.PlayRepeating(soundFile, 
                trigger.getCheck().GetStatus());            
        }
    }
}
