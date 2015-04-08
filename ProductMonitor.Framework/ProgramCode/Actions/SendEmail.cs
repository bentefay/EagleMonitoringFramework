using System.Xml;

namespace ProductMonitor.Framework.ProgramCode.Actions
{
    public class SendEmail : Action
    {
        private readonly EmailController _emailController;
        private readonly string _address;

        public SendEmail(XmlNode input, EmailController emailController)
        {
            _emailController = emailController;
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

            _emailController.sendEmailAlert(_address, message, trigger.getCheck().GetTab());
        }
    }
}
