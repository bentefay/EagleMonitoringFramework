using System.Diagnostics;
using System.Xml;

namespace ProductMonitor.Framework.ProgramCode.Actions
{
    public class ExecuteFile : Action
    {
        private string FilePath;

        public ExecuteFile(XmlNode input)
        {
            foreach (XmlNode childNode in input.ChildNodes)
            {
                if (childNode.Name.ToUpper() == "FilePath".ToUpper())
                {
                    FilePath = childNode.FirstChild.Value;
                }
            }
        }

        public override void Execute()
        {
            Process.Start(FilePath);
            
        }
    }
}
