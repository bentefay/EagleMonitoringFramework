using System.Xml;

namespace ProductMonitor.Framework.ProgramCode.Actions
{
    public class SendSms : Action
    {
        private string number;

        //exists for testing purposes
        public SendSms(object[] input)
        {
            this.input = input;
            number = (string)input[0];
        }

        public SendSms(XmlNode input)
        {
            foreach (XmlNode childNode in input.ChildNodes)
            {
                if (childNode.Name.ToUpper() == "Number".ToUpper())
                {
                    number = childNode.FirstChild.Value;
                }
            }
        }

        public override void Execute()
        {
            Generic.SendSms sms = new Generic.SendSms("SMS00568", "ez2getSome6",
                "The product monitor warns that " + trigger.getCheck().GetStatus(), number);
            sms.Send();
        }
    }
}
