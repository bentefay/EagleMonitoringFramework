using System;
using System.Collections;
using System.Linq;
using System.Net.Mail;
using System.Timers;
using ProductMonitor.Framework.Generic;
using Serilog;

namespace ProductMonitor.Framework.Services
{
    public class EmailService
    {
        private readonly string _tempPath;
        private readonly IScreenshotService _screenshotService;
        private readonly IMessageService _messageService;
        private readonly Cleanup _cleanup;
        private readonly ArrayList _messages;
        private readonly ArrayList _tabs;
        private readonly Timer _timer;
        private static EmailService _instance;

        public EmailService(string tempPath, IScreenshotService screenshotService, IMessageService messageService, Cleanup cleanup) 
        {
            _tempPath = tempPath;
            _screenshotService = screenshotService;
            _messageService = messageService;
            _cleanup = cleanup;

            System.IO.Directory.CreateDirectory(tempPath + "\\Screenshots");
            _messages = new ArrayList();
            _tabs = new ArrayList();
            
            _timer = new Timer(10 * 60 * 1000); //10 minutes ---------------------
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs exception)
        {
            try
            {

                Log.Information("Checking list of emails to send (email count: {0}).", _messages.Count);

                //get a list of problem tabs
                string subject = "Error: ";
                foreach (string s in _tabs)
                {
                    subject += s + " & ";
                }

                subject = subject.Remove(subject.Length - 3);

                if (!System.IO.Directory.Exists(_tempPath + "\\Screenshots\\"))
                {
                    System.IO.Directory.CreateDirectory(_tempPath + "\\Screenshots\\");
                }

                //take screenshots;
                var screenshots = new ArrayList();
                foreach (string s in _tabs)
                {
                    var saveLocation = _tempPath + "\\Screenshots\\" + s.Replace(' ', '_') + "_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".png";
                    
                    _screenshotService.TakeScreenshot(s, saveLocation);
                    if (System.IO.File.Exists(saveLocation))
                    {
                        screenshots.Add(saveLocation);
                    }
                }


                //get a list of addresses
                var targets = new ArrayList();
                foreach (Message m in _messages)
                {
                    if (!targets.Contains(m.target))
                    {
                        targets.Add(m.target);
                    }
                }

                //send a combined email to each address
                foreach (String s in targets)
                {
                    var bigMessage = new ArrayList();
                    foreach (Message m in _messages)
                    {
                        if (m.target == s)
                        {
                            if (!bigMessage.Contains(m.message))
                            {
                                bigMessage.Add(m.message);
                            }
                        }
                    }
                    String message = "";
                    foreach (String s2 in bigMessage)
                    {
                        message += s2 + "\n";
                    }
                    message += "\n";

                    //send the email
                    var email = new MailMessage("productMonitor@global-roam.com", s)
                    {
                        Subject = subject, 
                        Body = message
                    };

                    foreach (string f in screenshots)
                    {
                        email.Attachments.Add(new Attachment(f));
                        _cleanup.AddCleanup(f);
                    }

                    //add any file attachments
                    foreach (Message mes in _messages)
                    {
                        //HACK: uses internal knowledge of the check's dictionaries to work
                        if (mes.message.Contains("Download Location"))
                        {
                            //find the correct line
                            string[] lines = mes.message.Split('\n');

                            foreach (string li in lines)
                            {
                                if (li.Contains("Download Location"))
                                {
                                    string[] lisplit = li.Split(new string[] { " = " }, System.StringSplitOptions.None);
                                    string downloadLocation = lisplit[1];
                                    if (System.IO.File.Exists(downloadLocation))
                                    {
                                        email.Attachments.Add(new Attachment(downloadLocation));
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    email.Priority = MailPriority.High;

                    var emailClient = new SmtpClient();
                    emailClient.Host = this.host;
                    emailClient.Port = this.port;
                    emailClient.Credentials =
                        new System.Net.NetworkCredential(this.username, this.password);
                    emailClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                    try
                    {
                        Log.Information("Sending email: {{ Addresses: [ {0} ], Body: {1} }}", String.Join(", ", email.To.Select(em => em.Address)), email.Body);
                        emailClient.Send(email);
                    }
                    catch (Exception e2)
                    {
                        Log.Warning(e2, "Failed to send email. Stripping attachments.");

                        email.Attachments.Clear();

                        //re-attach the screenshots
                        foreach (string f in screenshots)
                        {
                            email.Attachments.Add(new Attachment(f));
                        }

                        //add warning to message

                        email.Body += "\n \n THIS MESSAGE WAS STRIPPED OF SOME ATTACHMENTS AFTER TIMING OUT ONCE.";

                        try
                        {
                            Log.Information("Sending stripped email: {{ Addresses: [ {0} ], Body: {1} }}", String.Join(", ", email.To.Select(em => em.Address)), email.Body);
                            emailClient.Send(email);
                        }
                        catch (Exception e3)
                        {
                            Log.Warning(e3, "Failed to send email again. Stripping all attachments.");

                            email.Attachments.Clear();
                            //add warning to message

                            email.Body += "\n \n THIS MESSAGE WAS STRIPPED OF SOME ATTACHMENTS AFTER TIMING OUT TWICE.";

                            try
                            {
                                Log.Information("Sending stripped email: {{ Addresses: [ {0} ], Body: {1} }}", String.Join(", ", email.To.Select(em => em.Address)), email.Body);
                                emailClient.Send(email);
                            }
                            catch (Exception e4)
                            {
                                Log.Warning(e4, "Failed to send email again. Sending warning email.");

                                MailMessage warningEmail = new MailMessage(s, "Error@global-roam.com");
                                warningEmail.Body = "The Product Monitor failed to send an email to this address 3 times.";

                                try
                                {
                                    Log.Information("Sending warning email: {{ Addresses: [ {0} ], Body: {1} }}", String.Join(", ", email.To.Select(em => em.Address)), email.Body);
                                    emailClient.Send(warningEmail);
                                }
                                catch
                                {
                                    new System.Threading.Thread(new System.Threading.ThreadStart(warningpopup)).Start();

                                }
                            }
                        }
                    }
                }
                _messages.Clear();
                _tabs.Clear();

            }
            catch (Exception e1)
            {
                Log.Information(e1, "Failed to send email.");
            }
        }

        public void warningpopup()
        {
            _messageService.ShowError("Email is failing to send. Check settings.");
        }

        public void sendEmailAlert(String target, String message, String tab)
        {
            Log.Information("Adding email to queue. {{ Target: {0}, Message: {1}, Tab: {2} }}", target, message, tab);

            _messages.Add(new Message(target, message));
            if (!_tabs.Contains(tab))
            {
                _tabs.Add(tab);
            }

            //force all checks on that tab to check their data
            /*  foreach (Check c in Program.GetChecks())
              {
                  if (c.GetTab() == tab && c.ActivationForcable())
                  {
                      c.Activate();
                  }
              }  */
            
        }

        class Message{

            public String target;
            public String message;

            public Message(String target, String message)
            {
                this.target = target;
                this.message = message;
            }
        }

        public void SetTimer(int minutes)
        {
            this._timer.Interval = 1000 * 60 * minutes;
        }

        #region Settings

        string host = "mail.internode.on.net";
        string username = "groaminternode2@internode.on.net";
        string password = "hh48633yz";
        int port = 25;

        public string Host
        {
            get { return host; }
            set { host = value; }
        }

        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        #endregion
    }
}
