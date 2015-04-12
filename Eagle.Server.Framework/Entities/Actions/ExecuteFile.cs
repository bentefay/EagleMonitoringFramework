using System.Diagnostics;
using System.Xml;

namespace Eagle.Server.Framework.Entities.Actions
{
    public class ExecuteFile : Action
    {
        private readonly string _filePath;

        public ExecuteFile(XmlNode input)
        {
            foreach (XmlNode childNode in input.ChildNodes)
            {
                if (childNode.Name.ToUpper() == "FilePath".ToUpper())
                {
                    _filePath = childNode.FirstChild.Value;
                }
            }
        }

        public override void Execute()
        {
            Process.Start(_filePath);
            
        }
    }
}
