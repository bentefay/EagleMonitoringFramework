using System.Xml;
using ProductMonitor.Framework.Services;

namespace ProductMonitor.Framework.Entities.Actions
{
    public class SendEmail : Action
    {
        private readonly EmailService _emailService;
        private readonly string _address;

        public SendEmail(XmlNode input, EmailService emailService)
        {
            _emailService = emailService;
            foreach (XmlNode childNode in input.ChildNodes)
            {
                if (childNode.Name.ToUpper() == "Address".ToUpper())
                {
                    _address = childNode.FirstChild.Value;
                }
            }
        }

        public override void Execute()
        {
            string message = trigger.getCheck().GetStatus() + "\n" + "Additional Data: \n";

            var values = trigger.getCheck().GetExtraValues();

            var enumerator = values.GetEnumerator();

            while(enumerator.MoveNext())
            {
                message += enumerator.Current.Key + " = " + enumerator.Current.Value + "\n";
            }

            _emailService.SendErrorEmail(_address, message, trigger.getCheck().GetTab());
        }
    }
}
