using System;
using System.Xml;
using Action = Eagle.Server.Framework.Entities.Actions.Action;

namespace Eagle.Server.Framework.Entities.Triggers
{
    public class GreaterThan : Trigger
    {
        private readonly int _triggerLevel;
        private bool _triggeredLastTime; //to stop the trigger activating alarms repeatedly

        //exists for testing purposes
        public GreaterThan(object[] input)
        {
            this.Input = input;
            _triggerLevel = (int)input[0];

        }

        public GreaterThan(XmlNode input)
        {
            
            foreach (XmlNode childNode in input.ChildNodes)
            {
                if (childNode.Name.ToUpper() == "value".ToUpper())
                {
                    _triggerLevel =
                        int.Parse(childNode.FirstChild.Value);
                }
            }
        }

        public override Type GetValueType()
        {
            return (1).GetType();
        }

        public override bool Test(object value)
        {
            if (_triggerLevel <= (int)value)
            {
                if (_triggeredLastTime == false)
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
