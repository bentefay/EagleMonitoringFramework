using System;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace Eagle.Server.Framework.Entities.Actions
{
    public class ExecuteFile : Action
    {
        private readonly string _filePath;

        public ExecuteFile(XmlNode input)
        {
            foreach (var childNode in input.ChildNodes.Cast<XmlNode>().Where(childNode => String.Equals(childNode.Name, "FilePath", StringComparison.InvariantCultureIgnoreCase)))
            {
                _filePath = childNode.FirstChild.Value;
            }
        }

        public override void Execute()
        {
            Process.Start(_filePath);
            
        }
    }
}
