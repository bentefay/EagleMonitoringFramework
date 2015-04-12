using System;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Xml;
using Timer = System.Timers.Timer;

namespace Eagle.Server.Framework.Entities.Frequencies
{
    public class RegularFrequency : Frequency
    {
        public RegularFrequency(Check check, object[] input)
        {
            Check = check;
            Input = input;

            var minutes = (int)input[0];

            SetUpTimer(minutes);

        }

        public RegularFrequency(Check check, XmlNode frequencyNode)
        {
            Check = check;

            var value = frequencyNode.ChildNodes.Cast<XmlNode>().First(childNode => String.Equals(childNode.Name, "Frequency", StringComparison.InvariantCultureIgnoreCase)).FirstChild.Value;
            
            var minutes = int.Parse(value);

            SetUpTimer(minutes);
        }

        private void Tick(object sender, ElapsedEventArgs e)
        {
            var myActivatorThread = new Thread(Activate)
            {
                IsBackground = true
            };
            myActivatorThread.Start();
        }

        private void SetUpTimer(int minutes)
        {
            Timer = new Timer
            {
                Interval = minutes * 60 * 1000
            };
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
