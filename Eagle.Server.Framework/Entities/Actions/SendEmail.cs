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
            string message = Trigger.GetCheck().GetStatus() + "\n" + "Additional Data: \n";

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
