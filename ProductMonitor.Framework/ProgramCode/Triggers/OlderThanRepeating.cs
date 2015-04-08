using System;
using System.Xml;
using Action = ProductMonitor.Framework.ProgramCode.Actions.Action;

namespace ProductMonitor.Framework.ProgramCode.Triggers
{
    public class OlderThanRepeating : Trigger
    {
        private TimeSpan origionalTime;
        private TimeSpan timeTillOutOfDate;
        private TimeSpan increaseBy;

        //exists for testing purposes
        public OlderThanRepeating(object[] input)
        {
            this.input = input;
            timeTillOutOfDate = origionalTime = (TimeSpan)input[0];
            increaseBy = (TimeSpan)input[1];

        }

        public OlderThanRepeating(XmlNode input)
        {
            foreach (XmlNode childNode in input.ChildNodes)
            {
                if (childNode.Name == "Minutes")
                {
                    timeTillOutOfDate = origionalTime =
                        TimeSpan.FromMinutes(double.Parse(childNode.FirstChild.Value));
                }
                else if (childNode.Name == "IncreaseBy")
                {
                    increaseBy =
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
                foreach (Action a in this.actions)
                {
                    a.Execute();
                    
                }
                
                //increase the time till the next check
                timeTillOutOfDate = ((TimeSpan)value).Add(increaseBy);

                return true;
            }
            else
            {
                //reset the time span
                if (origionalTime >= (TimeSpan)value)
                {
                    timeTillOutOfDate = origionalTime;
                }

                return false;
            }
        }
    }
}
