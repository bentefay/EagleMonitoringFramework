using System;
using System.Linq;
using System.Xml;

namespace Eagle.Server.Framework.Entities.Actions
{
    public class SendSms : Action
    {
        private readonly string _number;

        public SendSms(XmlNode input)
        {
            foreach (var childNode in input.ChildNodes.Cast<XmlNode>().Where(childNode => String.Equals(childNode.Name, "Number", StringComparison.InvariantCultureIgnoreCase)))
            {
                _number = childNode.FirstChild.Value;
            }
        }

        public override void Execute()
        {
            var sms = new Generic.SendSms("SMS00568", "ez2getSome6", "The product monitor warns that " + Trigger.GetCheck().GetStatus(), _number);
            sms.Send();
        }
    }
}
