using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Timers;
using Serilog;
using Timer = System.Timers.Timer;

namespace Eagle.Server.Framework.Services
{
    public class EmailService
    {
        private readonly string _tempPath;
        private readonly IScreenshotService _screenshotService;
        private readonly IMessageService _messageService;
        private readonly CleanupService _cleanupService;
        private readonly List<Message> _pendingMessages;
        private readonly List<string> _erroredTabs;
        private readonly Timer _timer;

        private const string ProductMonitorEmailAddress = "productMonitor@global-roam.com";

        string _host = "mail.internode.on.net";
        string _username = "groaminternode2@internode.on.net";
        string _password = "hh48633yz";
        int _port = 25;

        public EmailService(string tempPath, IScreenshotService screenshotService, IMessageService messageService, CleanupService cleanupService) 
        {
            _tempPath = tempPath;
            _screenshotService = screenshotService;
            _messageService = messageService;
            _cleanupService = cleanupService;

            Directory.CreateDirectory(tempPath + "\\Screenshots");
            _pendingMessages = new List<Message>();
            _erroredTabs = new List<string>();
            
            _timer = new Timer(10 * 60 * 1000); // Default timer is 10 minutes
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }

        public void SendErrorEmail(String target, String message, String tab)
        {
            Log.Information("Adding email to queue. {{ Target: {0}, Message: {1}, Tab: {2} }}", target, message, tab);

            _pendingMessages.Add(new Message(target, message));

            if (!_erroredTabs.Contains(tab))
                _erroredTabs.Add(tab);

        }

        public void SetEmailBufferInterval(int minutes)
        {
            _timer.Interval = 1000 * 60 * minutes;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs exception)
        {
            try
            {
                Log.Information("Checking list of emails to send (email count: {0}).", _pendingMessages.Count);

                var subject = "Tabs containing errors: " + String.Join(", ", _erroredTabs);

                if (!Directory.Exists(_tempPath + "\\Screenshots\\"))
                    Directory.CreateDirectory(_tempPath + "\\Screenshots\\");

                // Take screenshots
                var screenshotSaveLocations = TakeScreenshotsOfTabs(_erroredTabs, _tempPath).Where(File.Exists).ToList();

                // Aggregate messages for a given location
                foreach (var messagesForTargets in _pendingMessages.ToLookup(m => m.Target))
                {
                    SendMessagesToTarget(messagesForTargets.Key, subject, messagesForTargets.ToList(), screenshotSaveLocations);
                }

                _pendingMessages.Clear();
                _erroredTabs.Clear();

            }
            catch (Exception e1)
            {
                Log.Information(e1, "Failed to send email.");
            }
        }

        private List<string> TakeScreenshotsOfTabs(IEnumerable<string> tabs, string basePath)
        {
            var screenshotSaveLocations = new List<string>();
            foreach (var tab in tabs)
            {
                var saveLocation = basePath + "\\Screenshots\\" + tab.Replace(' ', '_') + "_" +
                                   DateTime.Now.ToString("yyyyMMddHHmm") + ".png";

                _screenshotService.TakeScreenshot(tab, saveLocation);
                
                if (File.Exists(saveLocation))
                    screenshotSaveLocations.Add(saveLocation);
            }

            return screenshotSaveLocations;
        }

        private void SendMessagesToTarget(string target, string subject, List<Message> messages, List<string> screenshotSaveLocations)
        {
            var aggregatedMessage = String.Join("\n", messages) + "\n";

            var email = new MailMessage(ProductMonitorEmailAddress, target)
            {
                Subject = subject,
                Body = aggregatedMessage
            };

            // Attach screenshots
            foreach (var location in screenshotSaveLocations)
            {
                email.Attachments.Add(new Attachment(location));
                _cleanupService.AddCleanup(location);
            }

            // Attach files
            var attachmentLocations = GetAttachmentLocations(messages);

            foreach (var attachmentLocation in attachmentLocations.Where(File.Exists))
                email.Attachments.Add(new Attachment(attachmentLocation));

            email.Priority = MailPriority.High;

            var emailClient = new SmtpClient();
            emailClient.Host = _host;
            emailClient.Port = _port;
            emailClient.Credentials = new NetworkCredential(_username, _password);
            emailClient.DeliveryMethod = SmtpDeliveryMethod.Network;

            try
            {
                Log.Information("Sending email: {{ Addresses: [ {0} ], Body: {1} }}", String.Join(", ", email.To.Select(em => em.Address)), email.Body);
                emailClient.Send(email);
            }
            catch (Exception e)
            {
                Log.Warning(e, "Failed to send email. Stripping some attachments.");

                HandleSendEmailFailed(target, screenshotSaveLocations, email, emailClient);
            }
        }

        private void HandleSendEmailFailed(string target, List<string> screenshotSaveLocations, MailMessage email, SmtpClient emailClient)
        {
            email.Attachments.Clear();

            foreach (string f in screenshotSaveLocations)
                email.Attachments.Add(new Attachment(f));

            email.Body += "\n\nTHIS MESSAGE WAS STRIPPED OF SOME ATTACHMENTS AFTER TIMING OUT ONCE.";

            try
            {
                Log.Information("Sending stripped email: {{ Addresses: [ {0} ], Body: {1} }}",
                    String.Join(", ", email.To.Select(em => em.Address)), email.Body);
                emailClient.Send(email);
            }
            catch (Exception e1)
            {
                Log.Warning(e1, "Failed to send email again. Stripping all attachments.");

                email.Attachments.Clear();

                email.Body += "\n \n THIS MESSAGE WAS STRIPPED OF ALL ATTACHMENTS AFTER TIMING OUT TWICE.";

                try
                {
                    Log.Information("Sending stripped email: {{ Addresses: [ {0} ], Body: {1} }}",
                        String.Join(", ", email.To.Select(em => em.Address)), email.Body);
                    emailClient.Send(email);
                }
                catch (Exception e2)
                {
                    Log.Warning(e2, "Failed to send email again. Sending warning email.");

                    var warningEmail = new MailMessage(ProductMonitorEmailAddress, target);
                    warningEmail.Body = "The Product Monitor failed to send an email to this address 3 times.";

                    try
                    {
                        Log.Information("Sending warning email: {{ Addresses: [ {0} ], Body: {1} }}",
                            String.Join(", ", email.To.Select(em => em.Address)), email.Body);
                        emailClient.Send(warningEmail);
                    }
                    catch
                    {
                        new Thread(ShowWarning).Start();
                    }
                }
            }
        }

        private static IEnumerable<string> GetAttachmentLocations(IEnumerable<Message> messages)
        {
            var messagesWithAttachments = messages.Where(m => m.Contents.Contains("Download Location"));

            var lines = messagesWithAttachments
                .Select(m => m.Contents.Split('\n'))
                .SelectMany(l => l);

            return lines
                .Where(l => l.Contains("Download Location"))
                .Select(l => l.Split(new[] { " = " }, StringSplitOptions.None))
                .Where(t => t.Length > 1)
                .Select(t => t[1]);
        }

        private void ShowWarning()
        {
            _messageService.ShowError("An email has failed to send. Check the log files for more details.");
        }

        private class Message {

            public String Target { get; private set; }
            public String Contents { get; private set; }

            public Message(String target, String message)
            {
                Target = target;
                Contents = message;
            }
        }

        public string Host
        {
            get { return _host; }
            set { _host = value; }
        }

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }
    }
}
