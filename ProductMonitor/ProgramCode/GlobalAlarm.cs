using System;

namespace ProductMonitor.ProgramCode
{
    static class GlobalAlarm
    {
        private static int[] _checksInError;
        private static string _target;

        public static void ReportError(int check)
        {
            //if an entire server on for a product is down, send a paniced email

            string tab = Program.ListOfChecks[check].GetTab();
            string location = Program.ListOfChecks[check].GetLocation();

            bool allInError = true;


            lock (_checksInError)
            {
                _checksInError[check] += 1;                
            }

            for (int i = 0; i < _checksInError.Length; i++)
            {
                if (Program.ListOfChecks[i].GetTab() == tab
                    && Program.ListOfChecks[i].GetLocation() == location)
                {
                    if (_checksInError[i] == 0)
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
                    _target, message, tab);

                for (int i = 0; i < _checksInError.Length; i++)
                {
                    if (Program.ListOfChecks[i].GetTab() == tab
                       && Program.ListOfChecks[i].GetLocation() == location)
                    {
                        _checksInError[i] = 0;
                    }
                }
            }

            // TODO: Test for alarms to be activated
        }

        public static void ReportSuccess(int check)
        {
            lock (_checksInError)
            {
                _checksInError[check] = 0;
            }
        }

        public static void PrepareList(int length)
        {
            _checksInError = new int[length];
        }

        public static void SetTarget(string emailaddress)
        {
            _target = emailaddress;
        }

        public static void MarkPaused(int check)
        {
            _checksInError[check] = -1;
        }

        public static void MarkUnPaused(int check)
        {
            _checksInError[check] = 0;
        }
    }
}
