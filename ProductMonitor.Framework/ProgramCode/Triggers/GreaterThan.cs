using System;
using System.Xml;
using Action = ProductMonitor.Framework.ProgramCode.Actions.Action;

namespace ProductMonitor.Framework.ProgramCode.Triggers
{
    public class GreaterThan : Trigger
    {
        private int triggerLevel;
        private bool triggeredLastTime = false; //to stop the trigger activating alarms repeatedly

        //exists for testing purposes
        public GreaterThan(object[] input)
        {
            this.input = input;
            triggerLevel = (int)input[0];

        }

        public GreaterThan(XmlNode input)
        {
            
            foreach (XmlNode childNode in input.ChildNodes)
            {
                if (childNode.Name.ToUpper() == "value".ToUpper())
                {
                    triggerLevel =
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
            if (triggerLevel <= (int)value)
            {
                if (triggeredLastTime == false)
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
