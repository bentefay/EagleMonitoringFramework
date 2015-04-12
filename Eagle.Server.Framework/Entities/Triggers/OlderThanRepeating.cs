using System;
using System.Xml;
using Action = Eagle.Server.Framework.Entities.Actions.Action;

namespace Eagle.Server.Framework.Entities.Triggers
{
    public class OlderThanRepeating : Trigger
    {
        private readonly TimeSpan _origionalTime;
        private TimeSpan _timeTillOutOfDate;
        private readonly TimeSpan _increaseBy;

        //exists for testing purposes
        public OlderThanRepeating(object[] input)
        {
            this.Input = input;
            _timeTillOutOfDate = _origionalTime = (TimeSpan)input[0];
            _increaseBy = (TimeSpan)input[1];

        }

        public OlderThanRepeating(XmlNode input)
        {
            foreach (XmlNode childNode in input.ChildNodes)
            {
                if (childNode.Name == "Minutes")
                {
                    _timeTillOutOfDate = _origionalTime =
                        TimeSpan.FromMinutes(double.Parse(childNode.FirstChild.Value));
                }
                else if (childNode.Name == "IncreaseBy")
                {
                    _increaseBy =
                        TimeSpan.FromMinutes(double.Parse(childNode.FirstChild.Value));
                }
            }
        }

        public override Type GetValueType()
        {
            return Type.GetType("DateTime");
        }

        public override bool Test(object value)
        {
            if (_timeTillOutOfDate <= (TimeSpan)value)
            {
                foreach (Action a in Actions)
                {
                    a.Execute();
                    
                }
                
                //increase the time till the next check
                _timeTillOutOfDate = ((TimeSpan)value).Add(_increaseBy);

                return true;
            }
            
            //reset the time span
            if (_origionalTime >= (TimeSpan)value)
            {
                _timeTillOutOfDate = _origionalTime;
            }

            return false;
        }
    }
}
