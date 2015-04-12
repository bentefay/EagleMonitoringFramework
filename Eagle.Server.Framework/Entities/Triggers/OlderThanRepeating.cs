using System;
using System.Xml;

namespace Eagle.Server.Framework.Entities.Triggers
{
    public class OlderThanRepeating : Trigger
    {
        private readonly TimeSpan _origionalTime;
        private TimeSpan _timeTillOutOfDate;
        private readonly TimeSpan _increaseBy;

        public OlderThanRepeating(XmlNode input)
        {
            foreach (XmlNode childNode in input.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "Minutes":
                        _timeTillOutOfDate = _origionalTime = TimeSpan.FromMinutes(double.Parse(childNode.FirstChild.Value));
                        break;
                    case "IncreaseBy":
                        _increaseBy = TimeSpan.FromMinutes(double.Parse(childNode.FirstChild.Value));
                        break;
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
                foreach (var a in Actions)
                    a.Execute();
                
                // Increase the time till the next check
                _timeTillOutOfDate = ((TimeSpan)value).Add(_increaseBy);

                return true;
            }
            
            // Reset the time span
            if (_origionalTime >= (TimeSpan)value)
            {
                _timeTillOutOfDate = _origionalTime;
            }

            return false;
        }
    }
}
