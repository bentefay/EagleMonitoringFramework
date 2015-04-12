using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Xml;
using Timer = System.Timers.Timer;

namespace Eagle.Server.Framework.Entities.Frequencies
{
    public class SemiRegularFrequency : Frequency
    {
        private readonly DateTime[] _activationTimes;

        public SemiRegularFrequency(Check check, XmlNode input)
        {
            Check = check;
            var activationTimes = new List<DateTime>();

            foreach (var childNode in input.FirstChild.ChildNodes.OfType<XmlNode>().Where(childNode => childNode.Name.ToUpper() == "TIME"))
            {
                    var myTime = new DateTime();
                    foreach (XmlNode timeDetails in childNode.ChildNodes)
                    {
                        switch (timeDetails.Name.ToUpper())
                        {
                            case "DAY":
                                var myDay = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), timeDetails.FirstChild.Value.ToUpper());

                                while (myTime.DayOfWeek != myDay)
                                {
                                    myTime = myTime.AddDays(1.0);
                                }
                                break;
                            case "HOUR":
                                myTime = myTime.AddHours(double.Parse(timeDetails.FirstChild.Value));
                                break;
                        }
                    }

                    myTime = myTime.AddYears(DateTime.Now.Year - 1);
                    activationTimes.Add(myTime);
            }

            _activationTimes = activationTimes.ToArray();

            var leastTimeToGo = GetLeastTimeToGo(_activationTimes);

            SetUpTimer(leastTimeToGo);
        }

        private static TimeSpan GetLeastTimeToGo(DateTime[] activationTimes)
        {
            var leastTimeToGo = TimeSpan.MaxValue;
            do
            {
                for (int i = 0; i < activationTimes.Length; i++)
                {
                    if (DateTime.Now > activationTimes[i])
                    {
                        activationTimes[i] = activationTimes[i].AddDays(7.0);
                    }
                    else
                    {
                        leastTimeToGo = new[] { activationTimes[i] - DateTime.Now, leastTimeToGo }.Min();
                    }
                }
            } while (leastTimeToGo.Equals(TimeSpan.MaxValue));

            return leastTimeToGo;
        }


        private void SetUpTimer(TimeSpan timeTillActivation)
        {
            Timer = new Timer
            {
                Interval = timeTillActivation.TotalMilliseconds
            };
            Timer.Elapsed += Tick;
            Timer.Start();
        }

        private void Tick(object sender, ElapsedEventArgs e)
        {
            var myActivatorThread = new Thread(Activate)
            {
                IsBackground = true
            };
            myActivatorThread.Start();

            Timer.Stop();

            var leastTimeToGo = GetLeastTimeToGo(_activationTimes);

            Timer.Interval = leastTimeToGo.TotalMilliseconds;

            Timer.Start();

        }

        private void SetTimer()
        {
            var leastTimeToGo = GetLeastTimeToGo(_activationTimes);

            Timer.Interval = leastTimeToGo.TotalMilliseconds;

            Timer.Start();
        }

        public override void Pause(bool paused)
        {
            if (paused)
            {
                Paused = true;
                Timer.Stop();
            }
            else
            {
                Paused = false;
                SetTimer();
            }
        }

        public override bool IsPaused()
        {
            return Paused;
        }

        public override bool ActivationForceable()
        {
            return false;
        }
    }
}
