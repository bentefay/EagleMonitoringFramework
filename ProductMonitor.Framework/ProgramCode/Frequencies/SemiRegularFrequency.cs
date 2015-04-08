using System;
using System.Threading;
using System.Timers;
using System.Xml;

namespace ProductMonitor.Framework.ProgramCode.Frequencies
{
    public class SemiRegularFrequency : Frequency
    {
        private DateTime[] activationTimes;

        public SemiRegularFrequency(Check check, object[] input)
        {
            this.check = check;
            activationTimes = new DateTime[input.Length];
            for (int i = 0; i < activationTimes.Length; i++ )
            {
                activationTimes[i] = (DateTime)input[i];
            }

            TimeSpan leastTimeToGo = new TimeSpan(365, 23, 59, 59);
            do
            {
                for (int i = 0; i < activationTimes.Length; i++){
                
                    if (DateTime.Now.CompareTo(activationTimes[i]) > 0)
                    {
                        activationTimes[i] = activationTimes[i].AddDays(7.0);
                    }
                    else if ((activationTimes[i] - DateTime.Now) < leastTimeToGo)
                    {
                        leastTimeToGo = activationTimes[i] - DateTime.Now;
                    }
                }
            } while (leastTimeToGo.Equals(new TimeSpan(365, 23, 59, 59)));

            setUpTimer(leastTimeToGo);
        }

        public SemiRegularFrequency(Check check, XmlNode input)
        {
            this.check = check;
            System.Collections.ArrayList activationTimes = new System.Collections.ArrayList();

            //fill the activation times
            foreach (XmlNode childNode in input.FirstChild.ChildNodes)
            {   
                if (childNode.Name.ToUpper() == "TIME")
                {
                    DateTime myTime = new DateTime();
                    foreach (XmlNode timeDetails in childNode.ChildNodes)
                    {
                        if (timeDetails.Name.ToUpper() == "DAY")
                        {
                            DayOfWeek myDay = DayOfWeek.Sunday;
                            switch (timeDetails.FirstChild.Value.ToUpper())
                            {
                                case "SUNDAY":
                                    myDay = DayOfWeek.Sunday;
                                    break;
                                case "MONDAY":
                                    myDay = DayOfWeek.Monday;
                                    break;
                                case "TUESDAY":
                                    myDay = DayOfWeek.Tuesday;
                                    break;
                                case "WEDNESDAY":
                                    myDay = DayOfWeek.Wednesday;
                                    break;
                                case "THURSDAY":
                                    myDay = DayOfWeek.Thursday;
                                    break;
                                case "FRIDAY":
                                    myDay = DayOfWeek.Friday;
                                    break;
                                case "SATURDAY":
                                    myDay = DayOfWeek.Saturday;
                                    break;
                            }

                            while (myTime.DayOfWeek != myDay)
                            {
                                myTime = myTime.AddDays(1.0);
                            }
                        }
                        else if (timeDetails.Name.ToUpper() == "HOUR")
                        {
                            myTime = myTime.AddHours(double.Parse(timeDetails.FirstChild.Value));
                        }
                    }
                    myTime = myTime.AddYears(DateTime.Now.Year - 1);
                        activationTimes.Add(myTime);
                    }
            }
                    this.activationTimes = (DateTime[])activationTimes.ToArray(DateTime.Now.GetType());

                    TimeSpan leastTimeToGo = new TimeSpan(365, 23, 59, 59);
                    do
                    {
                        for (int i = 0; i < activationTimes.Count; i++)
                        {

                            if (DateTime.Now.CompareTo(activationTimes[i]) > 0)
                            {
                                activationTimes[i] = ((DateTime)activationTimes[i]).AddDays(7.0);
                            }
                            else if ((((DateTime)activationTimes[i]) - DateTime.Now) < leastTimeToGo)
                            {
                                leastTimeToGo = ((DateTime)activationTimes[i]) - DateTime.Now;
                            }
                        }
                    } while (leastTimeToGo.Equals(new TimeSpan(365, 23, 59, 59)));

                    setUpTimer(leastTimeToGo);
                }

        

        private void setUpTimer(TimeSpan timeTillActivation)
        {
            this.timer = new System.Timers.Timer();
            timer.Interval = timeTillActivation.TotalMilliseconds;
            timer.Elapsed += new ElapsedEventHandler(tick);
            timer.Start();
        }

        void tick(object sender, ElapsedEventArgs e)
        {

            //run the check in a new thread
            ThreadStart ActivateStarter = new ThreadStart(activate);
            Thread myActivatorThread = new Thread(ActivateStarter);
            myActivatorThread.IsBackground = true;
            //myActivatorThread.Start();

            timer.Stop();

            TimeSpan leastTimeToGo = new TimeSpan(365, 23, 59, 59);
            do
            {
                for (int i = 0; i < activationTimes.Length; i++)
                {

                    if (DateTime.Now.CompareTo(activationTimes[i]) > 0)
                    {
                        activationTimes[i] = activationTimes[i].AddDays(7.0);
                    }
                    else if ((activationTimes[i] - DateTime.Now) < leastTimeToGo)
                    {
                        leastTimeToGo = activationTimes[i] - DateTime.Now;
                    }
                }
            } while (leastTimeToGo.Equals(new TimeSpan(365, 23, 59, 59)));

            timer.Interval = leastTimeToGo.TotalMilliseconds;

            timer.Start();

        }

        private void setTimer()
        {
            TimeSpan leastTimeToGo = new TimeSpan(365, 23, 59, 59);
            do
            {
                for (int i = 0; i < activationTimes.Length; i++)
                {

                    if (DateTime.Now.CompareTo(activationTimes[i]) > 0)
                    {
                        activationTimes[i] = activationTimes[i].AddDays(7.0);
                    }
                    else if ((activationTimes[i] - DateTime.Now) < leastTimeToGo)
                    {
                        leastTimeToGo = activationTimes[i] - DateTime.Now;
                    }
                }
            } while (leastTimeToGo.Equals(new TimeSpan(365, 23, 59, 59)));

            timer.Interval = leastTimeToGo.TotalMilliseconds;

            timer.Start();
        }

        public override void Pause(bool paused)
        {
            if (paused == true)
            {
                this.paused = true;
                timer.Stop();
            }
            else
            {
                this.paused = false;
                setTimer();
            }
        }

        public override bool isPaused()
        {
            return paused;
        }

        public override bool ActivationForceable()
        {
            return false;
        }
    }
}
