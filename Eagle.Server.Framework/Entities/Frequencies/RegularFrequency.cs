using System.Threading;
using System.Timers;
using System.Xml;
using Timer = System.Timers.Timer;

namespace Eagle.Server.Framework.Entities.Frequencies
{
    public class RegularFrequency : Frequency
    {
        private readonly int _minutes;

        public RegularFrequency(Check check, object[] input)
        {
            Check = check;
            Input = input;
            _minutes = (int)input[0];

            SetUpTimer(_minutes);

        }

        public RegularFrequency(Check check, XmlNode frequencyNode)
        {
            Check = check;

            foreach (XmlNode childNode in frequencyNode.ChildNodes)
            {
                if (childNode.Name.ToUpper() == "Frequency".ToUpper())
                {
                    _minutes = int.Parse(childNode.FirstChild.Value);
                    break;
                }
            }

            SetUpTimer(_minutes);
        }

        private void Tick(object sender, ElapsedEventArgs e)
        {
            var myActivatorThread = new Thread(Activate);
            myActivatorThread.IsBackground = true;
            myActivatorThread.Start();
        }

        private void SetUpTimer(int minutes)
        {
            Timer = new Timer();
            Timer.Interval = minutes * 60 * 1000;
            Timer.Elapsed += Tick;
            Timer.Start();
        }

        public override void Pause(bool paused)
        {
            if (paused)
            {
                Timer.Stop();
                Paused = true;
            }
            else
            {
                Timer.Start();
                Paused = false;
            }
        }

        public override bool IsPaused()
        {
            return Paused;
        }
    }
}
