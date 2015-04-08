using System;
using System.Linq;

namespace ProductMonitor.Framework.Services
{
    public class GlobalAlarmService
    {
        private readonly EmailService _emailService;
        private int[] _checksInError;
        private string _target;
        private Check[] _listOfChecks;

        public GlobalAlarmService(EmailService emailService)
        {
            _emailService = emailService;
        }

        public void ReportError(int check)
        {
            // If an entire server for a product is down, send a paniced email

            var tab = _listOfChecks[check].GetTab();
            var location = _listOfChecks[check].GetLocation();

            lock (_checksInError)
            {
                _checksInError[check] += 1;                
            }

            var allInError = _checksInError.Where((t, i) => _listOfChecks[i].GetTab() == tab && _listOfChecks[i].GetLocation() == location).All(t => t != 0);

            if (!allInError) return;

            var message = "!! SERVER DOWN: " + tab + Environment.NewLine + "(DANGER WILL ROBINSON)";

            _emailService.sendEmailAlert(_target, message, tab);

            for (int i = 0; i < _checksInError.Length; i++)
            {
                if (_listOfChecks[i].GetTab() == tab && _listOfChecks[i].GetLocation() == location)
                {
                    _checksInError[i] = 0;
                }
            }
        }

        public void ReportSuccess(int check)
        {
            lock (_checksInError)
            {
                _checksInError[check] = 0;
            }
        }

        public void PrepareList(Check[] listOfChecks)
        {
            _listOfChecks = listOfChecks;
            _checksInError = new int[listOfChecks.Length];
        }

        public void SetTarget(string emailaddress)
        {
            _target = emailaddress;
        }

        public void MarkPaused(int check)
        {
            _checksInError[check] = -1;
        }

        public void MarkUnPaused(int check)
        {
            _checksInError[check] = 0;
        }
    }
}
