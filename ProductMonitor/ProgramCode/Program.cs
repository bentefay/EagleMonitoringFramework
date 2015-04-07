using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml;
using ProductMonitor.Display_Code;
using ProductMonitor.Generic;
using ProductMonitor.ProgramCode.Actions;
using ProductMonitor.ProgramCode.Frequencies;
using ProductMonitor.ProgramCode.Queries;
using ProductMonitor.ProgramCode.Triggers;
using Serilog;
using SendSms = ProductMonitor.ProgramCode.Actions.SendSms;

namespace ProductMonitor.ProgramCode
{
    static class Program
    {
        private static Check[] _listOfChecks;
        private static ArrayList _arrayBuilder;
        public static string TempPath = AppDomain.CurrentDomain.BaseDirectory + "TEMP";

        //public for testing
        public static void PreStartUp()
        {
            _arrayBuilder = new ArrayList();
            GuiController.StartUp();
        }

        //public for testing
        public static void PostStartUp()
        {
            _listOfChecks = (Check[])_arrayBuilder.ToArray(_arrayBuilder[0].GetType());

            GlobalAlarm.PrepareList(_listOfChecks.Length);

            foreach (Check c in _listOfChecks)
            {
                c.Activate();
            }


        }

        static readonly string _configFilePathRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config");

        public static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            Log.Information("Application Start");

            try
            {

                PreStartUp();
                var configFilePath = Path.Combine(_configFilePathRoot, @"main.xml");

                Program.Load(new Uri(configFilePath));

                PostStartUp();

                //pause the main thread. The application runs in many other threads.
                System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);

            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to start application");
            }
        }

        public static Check[] GetChecks()
        {
            return _listOfChecks;
        }

        //-----------------------------------
        // Loading from XML Code
        //-----------------------------------

        #region Loading

        public static void Load(Uri filePath)
        {

            //open the document

            XmlDocument mainDoc = new XmlDocument();
            try
            {
                XmlTextReader myReader = new XmlTextReader(filePath.AbsoluteUri);
                mainDoc.Load(myReader);
                myReader.Close();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Unable to load main xml file");
                Log.Error(e, "Unable to load main xml file");
            }

            //get the nodes
            foreach (XmlNode childNode in mainDoc.ChildNodes)
            {
                if (childNode.Name.ToUpper() == "PRODUCTMONITOR")
                {
                    foreach (XmlNode monitorNode in childNode.ChildNodes)
                    {
                        if (monitorNode.Name.ToUpper() == "PROGRAMALARMS")
                        {
                            setupAlarms(monitorNode);
                        }
                        else if (monitorNode.Name.ToUpper() == "TABS")
                        {
                            loadTabs(monitorNode);
                        }
                    }
                }
            }

            Log.Information("Configuration Loaded");
        }

        private static void setupAlarms(XmlNode alarmsNode)
        {
            foreach (XmlNode child in alarmsNode)
            {
                if (child.Name.ToUpper() == "EMAILDELAY")
                {
                    EmailController.getInstance().SetTimer(int.Parse(child.FirstChild.Value));
                }
                else if (child.Name.ToUpper() == "SMTPHOST")
                {
                    EmailController.getInstance().Host = child.FirstChild.Value;
                }
                else if (child.Name.ToUpper() == "SMTPUSER")
                {
                    EmailController.getInstance().Username = child.FirstChild.Value;
                }
                else if (child.Name.ToUpper() == "SMTPPASSWORD")
                {
                    EmailController.getInstance().Password = child.FirstChild.Value;
                }
                else if (child.Name.ToUpper() == "SMTPPORT")
                {
                    EmailController.getInstance().Port = int.Parse(child.FirstChild.Value);
                }
                else if (child.Name.ToUpper() == "EMAIL1")
                {
                    GlobalAlarm.SetTarget(child.FirstChild.Value);
                }
            }

            //TODO: Set up the global alarms
        }

        private static void loadTabs(XmlNode tabsNode)
        {
            foreach (XmlNode tab in tabsNode.ChildNodes)
            {
                if (tab.Name.ToUpper() == "TAB")
                {
                    loadTab(tab);
                }
            }
        }

        //loads a tab file
        private static void loadTab(XmlNode tab)
        {
            string name = "";
            string tabLocation = "";
            bool online = false;

            foreach (XmlNode childNode in tab.ChildNodes)
            {
                if (childNode.Name.ToUpper() == "NAME")
                {
                    name = childNode.FirstChild.Value;
                }
                else if (childNode.Name.ToUpper() == "ONLINE")
                {
                    if (childNode.FirstChild.Value.ToUpper() == "TRUE")
                    {
                        online = true;
                    }
                    else
                    {
                        online = false;
                    }
                }
                else if (childNode.Name.ToUpper() == "CONFIG")
                {
                    tabLocation = childNode.FirstChild.Value;
                }
            }


            var configFilePath = Path.Combine(_configFilePathRoot, tabLocation);
            XmlTextReader myReader = new XmlTextReader(configFilePath);
            XmlDocument tabDocument = new XmlDocument();
            try
            {
                tabDocument.Load(myReader);
                myReader.Close();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Unable to connect to " + tabLocation);
                Log.Error(e, "Unable to connect to " + tabLocation);
            }


            //get the checks
            XmlNode Checks = tabDocument.LastChild.LastChild;
            foreach (XmlNode check in Checks.ChildNodes)
            {
                if (check.Name.ToUpper() == "CHECK")
                {
                    loadCheck(name, online, check);
                }
            }
        }

        private static void loadCheck(string tabName, bool tabOnline, XmlNode checkNode)
        {

            string name = "";
            bool checkOnline = tabOnline;
            //get the checks name (usually the first node so this shouldn't be too in-efficient
            foreach (XmlNode childNode in checkNode.ChildNodes)
            {
                if (childNode.Name.ToUpper() == "NAME")
                {
                    name = childNode.FirstChild.Value;
                    break;
                }
            }

            //create the check
            Check checkBeingLoaded = new Check(name, _arrayBuilder.Count);
            _arrayBuilder.Add(checkBeingLoaded);
            checkBeingLoaded.SetTab(tabName);

            int nodesLoaded = 0;

            //load the other parts
            foreach (XmlNode childNode in checkNode.ChildNodes)
            {
                if (childNode.Name.ToUpper() == "ONLINE")
                {
                    if (childNode.FirstChild.Value.ToUpper() == "TRUE")
                    {
                        if (tabOnline == true)
                        {
                            checkOnline = true;
                        }
                    }
                    else
                    {
                        checkOnline = false;
                    }
                }
                else if (childNode.Name.ToUpper() == "FREQUENCY")
                {
                    try
                    {
                        loadFrequency(checkBeingLoaded, childNode);
                        nodesLoaded++;
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Failed to load FREQUENCY");
                    }
                }
                else if (childNode.Name.ToUpper() == "QUERY")
                {
                    try
                    {
                        loadQuery(checkBeingLoaded, childNode);
                        nodesLoaded++;
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Failed to load QUERY");
                    }
                }
                else if (childNode.Name.ToUpper() == "TRIGGERS")
                {
                    nodesLoaded++;
                    //load each of the triggers
                    foreach (XmlNode triggerNode in childNode.ChildNodes)
                    {
                        if (triggerNode.Name.ToUpper() == "TRIGGER")
                        {
                            try
                            {
                                loadTrigger(checkBeingLoaded, triggerNode);
                            }
                            catch (Exception e)
                            {
                                Log.Error(e, "Failed to load TRIGGER");
                            }
                        }
                    }
                }
                else if (childNode.Name.ToUpperInvariant() == "DefaultOkStatusMessage".ToUpperInvariant())
                {
                    checkBeingLoaded.DefaultOkStatusMessage = childNode.InnerText;
                }
            }

            if (nodesLoaded != 3)
            {
                System.Windows.Forms.MessageBox.Show("Error in loadCheck");
                Log.Error("Error in loadCheck");
            }
            try
            {
                checkBeingLoaded.SetType(name);

                checkBeingLoaded.Pause(!checkOnline);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed");
            }
        }

        private static void loadFrequency(Check checkBeingLoaded, XmlNode FrequencyNode)
        {
            string type = "";
            XmlNode input = null;

            //get XML properties
            foreach (XmlNode childNode in FrequencyNode.ChildNodes)
            {
                if (childNode.Name.ToUpper() == "TYPE")
                {
                    type = childNode.FirstChild.Value;
                }
                else if (childNode.Name.ToUpper() == "INPUT")
                {
                    input = childNode;
                }
            }

            Frequency myFrequency = null;

            if (type.ToUpper() == "REGULAR")
            {
                myFrequency =
                    new RegularFrequency(
                    checkBeingLoaded,
                    input);
            }
            else if (type.ToUpper() == "SEMI-REGULAR" || type.ToUpper() == "SEMIREGULAR")
            {
                myFrequency = new SemiRegularFrequency(checkBeingLoaded, input);
            }
            //add new else if statements here

            checkBeingLoaded.SetFrequency(myFrequency);
        }

        private static void loadQuery(Check checkBeingLoaded, XmlNode QueryNode)
        {
            string type = "";
            XmlNode input = null;

            //load the xml data for the query
            foreach (XmlNode childNode in QueryNode.ChildNodes)
            {
                if (childNode.Name.ToUpper() == "TYPE")
                {
                    type = childNode.FirstChild.Value;
                }
                else if (childNode.Name.ToUpper() == "INPUT")
                {
                    input = childNode;
                }
            }

            var myType = typeof(Program).Assembly.GetTypes().First(t => String.Equals(t.Name, type, StringComparison.InvariantCultureIgnoreCase));
            var myQuery = (Query)Activator.CreateInstance(myType, input);
            checkBeingLoaded.SetQuery(myQuery);
            
        }

        private static void loadTrigger(Check checkBeingLoaded, XmlNode triggerNode)
        {
            string type = "";
            XmlNode input = null;
            XmlNode actionsNode = null;
            Trigger myTrigger = null;

            //get the xml data
            foreach (XmlNode childNode in triggerNode.ChildNodes)
            {
                if (childNode.Name.ToUpper() == "TYPE")
                {
                    type = childNode.FirstChild.Value;
                }
                else if (childNode.Name.ToUpper() == "INPUT")
                {
                    input = childNode;
                }
                else if (childNode.Name.ToUpper() == "ACTIONS")
                {
                    actionsNode = childNode;
                }
            }

            //create the trigger of the correct type and add it to the check
            if (type.ToUpper() == "OLDERTHAN")
            {
                myTrigger =
                    new OlderThan(input);

                checkBeingLoaded.AddTrigger(myTrigger);
            }
            else if (type.ToUpper() == "OLDERTHANREPEATING")
            {
                myTrigger =
                    new OlderThanRepeating(input);

                checkBeingLoaded.AddTrigger(myTrigger);
            }
            else if (type.ToUpper() == "GREATERTHAN")
            {
                myTrigger =
                    new GreaterThan(input);

                checkBeingLoaded.AddTrigger(myTrigger);
            }
            else if (type.ToUpperInvariant() == typeof(ResultNotNull).Name.ToUpperInvariant())
            {
                myTrigger = new ResultNotNull(input);

                checkBeingLoaded.AddTrigger(myTrigger);
            }
            //---add new trigger types here---

            //load the actions for the trigger
            foreach (XmlNode action in actionsNode.ChildNodes)
            {
                if (action.Name.ToUpper() == "ACTION")
                {
                    loadAction(myTrigger, action);
                }
            }
        }

        private static void loadAction(Trigger triggerBeingLoaded,
            XmlNode actionNode)
        {
            string type = "";
            XmlNode input = null;

            //get the XML inputs
            foreach (XmlNode childNode in actionNode.ChildNodes)
            {
                if (childNode.Name.ToUpper() == "TYPE")
                {
                    type = childNode.FirstChild.Value;
                }
                else if (childNode.Name.ToUpper() == "INPUT")
                {
                    input = childNode;
                }
            }

            //create the action of the correct type and add it to the trigger
            if (type.ToUpper() == "WAV")
            {
                PlayWavFile myAction =
                    new PlayWavFile(input);

                triggerBeingLoaded.AddAction(myAction);
            }
            else if (type.ToUpper() == "WAVRepeating".ToUpper())
            {
                RepeatWavFile myAction =
                    new RepeatWavFile(input);

                triggerBeingLoaded.AddAction(myAction);
            }
            else if (type.ToUpper() == "SENDEMAIL")
            {
                SendEmail myAction =
                    new SendEmail(input);

                triggerBeingLoaded.AddAction(myAction);
            }
            else if (type.ToUpper() == "SENDSMS")
            {
                SendSms myAction =
                    new SendSms(input);

                triggerBeingLoaded.AddAction(myAction);
            }
            else if (type.ToUpperInvariant() == typeof(ExecuteFile).Name.ToUpperInvariant())
            {
                var myAction = new ExecuteFile(input);

                triggerBeingLoaded.AddAction(myAction);
            }
        }

        #endregion

        public static void PauseCheck(int index)
        {
            if (!_listOfChecks[index].IsPaused())
            {
                _listOfChecks[index].Pause(true);
                _listOfChecks[index].Activate();
            }
        }

        public static void UnpauseCheck(int index)
        {
            if (_listOfChecks[index].IsPaused())
            {
                _listOfChecks[index].Pause(false);
                _listOfChecks[index].Activate();
            }
        }

        public static void Exit()
        {
            //clean up (will not get everything)
            try
            {
                System.IO.Directory.Delete(TempPath, true);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to delete directory on exit");
            }

            Log.Information("Application Exit");

            Environment.Exit(0);
        }

        public static Check[] ListOfChecks
        {
            get
            {
                return _listOfChecks;
            }
        }
    }
}
