using System;

namespace ProductMonitor.ProgramCode
{
    static class GlobalAlarm
    {
        private static int[] checksInError;

        private static string target;

        public static void ReportError(int check)
        {
            //if an entire server on for a product is down, send a paniced email

            string tab = Program.ListOfChecks[check].GetTab();
            string location = Program.ListOfChecks[check].GetLocation();

            bool allInError = true;


            lock (checksInError)
            {
                checksInError[check] += 1;                
            }

            for (int i = 0; i < checksInError.Length; i++)
            {
                if (Program.ListOfChecks[i].GetTab() == tab
                    && Program.ListOfChecks[i].GetLocation() == location)
                {
                    if (checksInError[i] == 0)
                    {
                        allInError = false;
                        break;
                    }
                }
            }

            if (allInError)
            {
                string message = "!! SERVER DOWN: " + tab + Environment.NewLine
                    + "(DANGER WILL ROBINSON)";

                EmailController.getInstance().sendEmailAlert(
                    target, message, tab);

                for (int i = 0; i < checksInError.Length; i++)
                {
                    if (Program.ListOfChecks[i].GetTab() == tab
                       && Program.ListOfChecks[i].GetLocation() == location)
                    {
                        checksInError[i] = 0;
                    }
                }
            }

            // TODO: Test for alarms to be activated
        }

        public static void ReportSuccess(int check)
        {
            lock (checksInError)
            {
                checksInError[check] = 0;
            }
        }

        public static void PrepareList(int length)
        {
            checksInError = new int[length];
        }

        public static void SetTarget(string emailaddress)
        {
            target = emailaddress;
        }

        public static void MarkPaused(int check)
        {
            checksInError[check] = -1;
        }

        public static void MarkUnPaused(int check)
        {
            checksInError[check] = 0;
        }
    }
}
