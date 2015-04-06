using System;
using System.Collections;
using System.Linq;
using System.Net.Mail;
using System.Timers;
using ProductMonitor.Display_Code;
using Product_Monitor.Generic;

namespace ProductMonitor.ProgramCode
{
    class EmailController
    {
        
        ArrayList messages;
        ArrayList tabs;
        Timer timer;
        static EmailController instance;

        private EmailController() {
           
            System.IO.Directory.CreateDirectory(Program.TempPath + "\\Screenshots");
            messages = new ArrayList();
            tabs = new ArrayList();
            
            timer = new Timer(10 * 60 * 1000); //10 minutes ---------------------
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Start();
        }
        void timer_Elapsed(object sender, ElapsedEventArgs exception)
        {
            try
            {

                Logger.getInstance().Log(String.Format("Checking list of emails to send (email count: {0}).", messages.Count));

                //get a list of problem tabs
                string subject = "Error: ";
                foreach (string s in tabs)
                {
                    subject += s + " & ";
                }

                subject = subject.Remove(subject.Length - 3);

                if (!System.IO.Directory.Exists(Program.TempPath + "\\Screenshots\\"))
                {
                    System.IO.Directory.CreateDirectory(Program.TempPath + "\\Screenshots\\");
                }

                //take screenshots;
                ArrayList screenshots = new ArrayList();
                foreach (string s in tabs)
                {
                    String saveLocation = Program.TempPath + "\\Screenshots\\" + s.Replace(' ', '_')
                                          + "_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".png";
                    ;
                    GuiController.TakeScreenshot(s, saveLocation);
                    if (System.IO.File.Exists(saveLocation))
                    {
                        screenshots.Add(saveLocation);
                    }
                }


                //get a list of addresses
                ArrayList targets = new ArrayList();
                foreach (Message m in messages)
                {
                    if (!targets.Contains(m.target))
                    {
                        targets.Add(m.target);
                    }
                }

                //send a combined email to each address
                foreach (String s in targets)
                {
                    ArrayList bigMessage = new ArrayList();
                    foreach (Message m in messages)
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
                    MailMessage email = new MailMessage("productMonitor@global-roam.com", s);
                    email.Subject = subject;
                    email.Body = message;

                    foreach (string f in screenshots)
                    {
                        email.Attachments.Add(new Attachment(f));
                        Product_Monitor.Generic.Cleanup.GetInstance().AddCleanup(f);
                    }

                    //add any file attachments
                    foreach (Message mes in messages)
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

                    SmtpClient emailClient = new SmtpClient();
                    emailClient.Host = this.host;
                    emailClient.Port = this.port;
                    emailClient.Credentials =
                        new System.Net.NetworkCredential(this.username, this.password);
                    emailClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                    try
                    {
                        Logger.getInstance().Log(String.Format("Sending email: {{ Addresses: [ {0} ], Body: {1} }}", String.Join(", ", email.To.Select(em => em.Address)), email.Body));
                        emailClient.Send(email);
                    }
                    catch (Exception e2)
                    {
                        Logger.getInstance().Log(e2);

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
                            Logger.getInstance().Log(String.Format("Sending stripped email: {{ Addresses: [ {0} ], Body: {1} }}", String.Join(", ", email.To.Select(em => em.Address)), email.Body));
                            emailClient.Send(email);
                        }
                        catch (Exception e3)
                        {
                            Logger.getInstance().Log(e3);

                            email.Attachments.Clear();
                            //add warning to message

                            email.Body += "\n \n THIS MESSAGE WAS STRIPPED OF SOME ATTACHMENTS AFTER TIMING OUT TWICE.";

                            try
                            {
                                Logger.getInstance().Log(String.Format("Sending stripped email: {{ Addresses: [ {0} ], Body: {1} }}", String.Join(", ", email.To.Select(em => em.Address)), email.Body));
                                emailClient.Send(email);
                            }
                            catch (Exception e4)
                            {
                                Logger.getInstance().Log(e4);

                                MailMessage warningEmail = new MailMessage(s, "Error@global-roam.com");
                                warningEmail.Body = "The Product Monitor failed to send an email to this address 3 times.";

                                try
                                {
                                    Logger.getInstance().Log(String.Format("Sending warning email: {{ Addresses: [ {0} ], Body: {1} }}", String.Join(", ", email.To.Select(em => em.Address)), email.Body));
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
                messages.Clear();
                tabs.Clear();

            }
            catch (Exception e1)
            {
                Logger.getInstance().Log("An exception has occurred.");
                Logger.getInstance().Log(e1);
            }
        }

        public void warningpopup()
        {
            System.Windows.Forms.MessageBox.Show("Email is failing to send. Check settings.");
        }

        static public EmailController getInstance()
        {
            if (instance == null)
            {
                instance = new EmailController();
            }
            return instance;
        }

        public void sendEmailAlert(String target, String message, String tab)
        {
            Logger.getInstance().Log(String.Format("Adding email to queue. {{ Target: {0}, Message: {1}, Tab: {2} }}", target, message, tab));

            messages.Add(new Message(target, message));
            if (!tabs.Contains(tab))
            {
                tabs.Add(tab);
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
            this.timer.Interval = 1000 * 60 * minutes;
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
