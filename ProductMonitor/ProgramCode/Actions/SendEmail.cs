using System;
using System.Collections.Generic;
using System.Xml;

namespace ProductMonitor.ProgramCode.Actions
{
    class SendEmail : Action
    {
        private string address;

        //exists for testing purposes
        public SendEmail(object[] input)
        {
            this.input = input;
            address = (string)input[0];
        }

        public SendEmail(XmlNode input)
        {
            foreach (XmlNode childNode in input.ChildNodes)
            {
                if (childNode.Name.ToUpper() == "Address".ToUpper())
                {
                    address = childNode.FirstChild.Value;
                }
            }
        }

        public override void Execute()
        {
            string message =
                trigger.getCheck().GetStatus() + "\n" +
                "Additional Data: \n";

            Dictionary<String, String> values = trigger.getCheck().GetExtraValues();

            Dictionary<String, String>.Enumerator enumerator = values.GetEnumerator();

            while(enumerator.MoveNext())
            {
                
                message += enumerator.Current.Key + " = " + enumerator.Current.Value + "\n";
            }

            EmailController.getInstance().sendEmailAlert(address, message, 
                trigger.getCheck().GetTab());
        }
    }
}
