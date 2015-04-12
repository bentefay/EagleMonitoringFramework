using System;
using System.Linq;
using System.Xml;

namespace Eagle.Server.Framework.Entities.Triggers
{
    public class OlderThan : Trigger
    {
        private readonly TimeSpan _timeTillOutOfDate;
        private bool _triggeredLastTime; //to stop the trigger activating alarms repeatedly

        public OlderThan(XmlNode input)
        {
            foreach (var childNode in input.ChildNodes.Cast<XmlNode>().Where(childNode => childNode.Name == "Minutes"))
            {
                _timeTillOutOfDate = TimeSpan.FromMinutes(double.Parse(childNode.FirstChild.Value));
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
