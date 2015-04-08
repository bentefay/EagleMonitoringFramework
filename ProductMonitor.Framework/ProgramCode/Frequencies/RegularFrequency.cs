using System.Threading;
using System.Timers;
using System.Xml;

namespace ProductMonitor.Framework.ProgramCode.Frequencies
{
    public class RegularFrequency : Frequency
    {
        private int minutes;

        public RegularFrequency(Check check, object[] input)
        {
            this.check = check;
            this.input = input;
            this.minutes = (int)input[0];

            setUpTimer(minutes);

        }

        public RegularFrequency(Check check, XmlNode FrequencyNode)
        {
            this.check = check;

            foreach (XmlNode childNode in FrequencyNode.ChildNodes)
            {
                if (childNode.Name.ToUpper() == "Frequency".ToUpper())
                {
                    this.minutes = int.Parse(childNode.FirstChild.Value);
                    break;
                }
            }

            setUpTimer(minutes);
        }

        void tick(object sender, ElapsedEventArgs e)
        {
            //run the check in a new thread
            ThreadStart ActivateStarter = new ThreadStart(activate);
            Thread myActivatorThread = new Thread(ActivateStarter);
            myActivatorThread.IsBackground = true;
            myActivatorThread.Start();

        }

        private void setUpTimer(int minutes)
        {
            this.timer = new System.Timers.Timer();
            timer.Interval = minutes * 60 * 1000;
            timer.Elapsed += new ElapsedEventHandler(tick);
            timer.Start();
        }

        public override void Pause(bool paused)
        {
            if (paused == true)
            {
                timer.Stop();
                this.paused = true;
            }
            else
            {
                timer.Start();
                this.paused = false;
            }
        }

        public override bool isPaused()
        {
            return paused;
        }
    }
}
