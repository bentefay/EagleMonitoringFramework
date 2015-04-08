using System;
using System.Xml;
using Action = ProductMonitor.Framework.ProgramCode.Actions.Action;

namespace ProductMonitor.Framework.ProgramCode.Triggers
{
    public class ResultNotNull : Trigger
    {
        private bool triggeredLastTime = false; //to stop the trigger activating alarms repeatedly


        public ResultNotNull(XmlNode input)
        {
        }

        public override Type GetValueType()
        {
            return typeof(object);
        }

        public override bool Test(object value)
        {
            if (value != null)
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
