using System.Xml;

namespace Eagle.Server.Framework.Entities.Actions
{
    public class SendSms : Action
    {
        private readonly string _number;

        //exists for testing purposes
        public SendSms(object[] input)
        {
            Input = input;
            _number = (string)input[0];
        }

        public SendSms(XmlNode input)
        {
            foreach (XmlNode childNode in input.ChildNodes)
            {
                if (childNode.Name.ToUpper() == "Number".ToUpper())
                {
                    _number = childNode.FirstChild.Value;
                }
            }
        }

        public override void Execute()
        {
            var sms = new Generic.SendSms("SMS00568", "ez2getSome6",
                "The product monitor warns that " + Trigger.GetCheck().GetStatus(), _number);
            sms.Send();
        }
    }
}
