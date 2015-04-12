using System;
using System.Xml;
using Action = Eagle.Server.Framework.Entities.Actions.Action;

namespace Eagle.Server.Framework.Entities.Triggers
{
    public class ResultNotNull : Trigger
    {
        private bool _triggeredLastTime; //to stop the trigger activating alarms repeatedly
        
        // ReSharper disable once UnusedParameter.Local
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
