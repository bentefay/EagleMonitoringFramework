using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Eagle.Server.UI
{
    class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int pid);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length > 0 && String.Equals(args[0], "c"))
            {
                if (!AttachConsole(-1))
                    AllocConsole();

                // Do console stuff

                FreeConsole();

                SendKeys.SendWait("{ENTER}");
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                // Application.Run(new MainForm());
            }
        }
    }
}
