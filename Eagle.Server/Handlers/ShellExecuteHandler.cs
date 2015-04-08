using System.Diagnostics;
using Eagle.Server.Handlers;

namespace Eagle.Server.Notifications
{
    public class ShellExecuteHandler : Handler
    {
        public string FilePath { get; set; }

        public override void Execute()
        {
            Process.Start(FilePath);
        }
    }
}