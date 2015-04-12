using System;
using System.Xml;
using Action = Eagle.Server.Framework.Entities.Actions.Action;

namespace Eagle.Server.Framework.Entities.Triggers
{
    public class OlderThan : Trigger
    {
        private readonly TimeSpan _timeTillOutOfDate;
        private bool _triggeredLastTime; //to stop the trigger activating alarms repeatedly

        //exists for testing purposes
        public OlderThan(object[] input)
        {
            this.Input = input;
            _timeTillOutOfDate = (TimeSpan)input[0];

        }

        public OlderThan(XmlNode input)
        {
            
            foreach (XmlNode childNode in input.ChildNodes)
            {
                if (childNode.Name == "Minutes")
                {
                    _timeTillOutOfDate =
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
                if (!_triggeredLastTime)
                {
                    foreach (Action a in Actions)
                    {
                        a.Execute();
                    }
                }

                _triggeredLastTime = true;
                return true;
            }
            
            _triggeredLastTime = false;
            return false;
        }
    }
}
