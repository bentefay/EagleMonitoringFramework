using System;
using System.Xml;
using Action = ProductMonitor.ProgramCode.Actions.Action;

namespace ProductMonitor.ProgramCode.Triggers
{
    class OlderThan : Trigger
    {
        private TimeSpan timeTillOutOfDate;
        private bool triggeredLastTime = false; //to stop the trigger activating alarms repeatedly

        //exists for testing purposes
        public OlderThan(object[] input)
        {
            this.input = input;
            timeTillOutOfDate = (TimeSpan)input[0];

        }

        public OlderThan(XmlNode input)
        {
            
            foreach (XmlNode childNode in input.ChildNodes)
            {
                if (childNode.Name == "Minutes")
                {
                    timeTillOutOfDate =
                        TimeSpan.FromMinutes(double.Parse(childNode.FirstChild.Value));
                }
            }
        }

        public override Type GetValueType()
        {
            return System.Type.GetType("DateTime");
        }

        public override bool Test(object value)
        {
            if (timeTillOutOfDate <= (TimeSpan)value)
            {
                if (!triggeredLastTime)
                {
                    foreach (Action a in this.actions)
                    {
                        a.Execute();
                    }
                }

                triggeredLastTime = true;
                return true;
            }
            else
            {
                triggeredLastTime = false;
                return false;
            }
        }
    }
}
