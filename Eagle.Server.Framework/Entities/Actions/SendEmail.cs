using System;
using System.Linq;
using System.Xml;
using Eagle.Server.Framework.Services;

namespace Eagle.Server.Framework.Entities.Actions
{
    public class SendEmail : Action
    {
        private readonly EmailService _emailService;
        private readonly string _address;

        public SendEmail(XmlNode input, EmailService emailService)
        {
            _emailService = emailService;
            foreach (var childNode in input.ChildNodes.Cast<XmlNode>().Where(childNode => String.Equals(childNode.Name, "Address", StringComparison.InvariantCultureIgnoreCase)))
            {
                _address = childNode.FirstChild.Value;
            }
        }

        public override void Execute()
        {
            var message = Trigger.GetCheck().GetStatus() + "\n" + "Additional Data: \n";

            var values = Trigger.GetCheck().GetExtraValues();

            var enumerator = values.GetEnumerator();

            while(enumerator.MoveNext())
            {
                message += enumerator.Current.Key + " = " + enumerator.Current.Value + "\n";
            }

            _emailService.SendErrorEmail(_address, message, Trigger.GetCheck().GetTab());
        }
    }
}
