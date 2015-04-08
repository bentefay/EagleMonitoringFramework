using System.Diagnostics;

namespace Eagle.Server.Handlers
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