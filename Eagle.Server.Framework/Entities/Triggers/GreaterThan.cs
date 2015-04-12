using System;
using System.Linq;
using System.Xml;

namespace Eagle.Server.Framework.Entities.Triggers
{
    public class GreaterThan : Trigger
    {
        private readonly int _triggerLevel;
        private bool _triggeredLastTime; //to stop the trigger activating alarms repeatedly

        public GreaterThan(XmlNode input)
        {
            foreach (var childNode in input.ChildNodes.Cast<XmlNode>().Where(childNode => String.Equals(childNode.Name, "value", StringComparison.InvariantCultureIgnoreCase)))
            {
                _triggerLevel = int.Parse(childNode.FirstChild.Value);
            }
        }

        public override Type GetValueType()
        {
            return typeof(Int32);
        }

        public override bool Test(object value)
        {
            if (_triggerLevel <= (int)value)
            {
                if (_triggeredLastTime == false)
                {
                    foreach (var a in Actions)
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
