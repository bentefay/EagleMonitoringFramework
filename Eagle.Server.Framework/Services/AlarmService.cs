using System;
using System.Collections.Generic;
using System.Linq;
using Eagle.Server.Framework.Generic;
using Serilog;

namespace Eagle.Server.Framework.Services
{
    public class AlarmService
    {
        private readonly object _syncLock = new object();
        private readonly EmailService _emailService;
        private readonly Dictionary<Check, int> _errorCounts;
        private readonly Dictionary<Check, bool> _paused;
        private Check[] _checks; 
        private string _target;

        public AlarmService(EmailService emailService)
        {
            _emailService = emailService;
            _errorCounts = new Dictionary<Check, int>();
            _paused = new Dictionary<Check, bool>();
            _checks = new Check[0];
        }

        public void SetTarget(string emailaddress)
        {
            _target = emailaddress;
        }

        public void ReportError(Check check)
        {
            var tab = check.GetTab();
            var location = check.GetLocation();

            MarkCheckErrored(check);

            if (!AllChecksInError(tab, location)) 
                return;

            var message = String.Format("ALERT - A SERVER MAY BE DOWN - All checks are errored for: {{ tab: '{0}', location: '{1}' }}.", tab, location);

            Log.Warning(message);

            _emailService.SendErrorEmail(_target, message, tab);

            ClearErrors(tab, location);
        }

        public void ReportSuccess(Check check)
        {
            lock (_syncLock)
                _errorCounts[check] = 0;
        }

        public void PrepareList(Check[] checks)
        {
            lock (_syncLock)
                _checks = checks;
        }

        public void MarkPaused(Check check)
        {
            lock (_syncLock)
                _paused[check] = true;
        }

        public void MarkRunning(Check check)
        {
            lock (_syncLock)
                _paused.Remove(check);
        }

        private void MarkCheckErrored(Check check)
        {
            lock (_syncLock)
                _errorCounts.ChangeValue(check, i => i + 1, () => 1);
        }

        private bool AllChecksInError(string tab, string location)
        {
            lock (_syncLock)
                return GetRunningChecks(tab, location).All(p => _errorCounts.GetValue(p, 0) != 0);
        }

        private void ClearErrors(string tab, string location)
        {
            lock (_syncLock)
            {
                foreach (var check in GetRunningChecks(tab, location))
                    _errorCounts.ChangeValue(check, i => 0);
            }
        }

        private IEnumerable<Check> GetRunningChecks(string tab, string location)
        {
            lock (_syncLock)
                return _checks.Where(c => c.GetTab() == tab && c.GetLocation() == location && !_paused.GetValue(c, false));
        }
    }
}
