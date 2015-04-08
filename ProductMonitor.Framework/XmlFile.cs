using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using ProductMonitor.Framework.Entities.Actions;
using ProductMonitor.Framework.Entities.Frequencies;
using ProductMonitor.Framework.Entities.Queries;
using ProductMonitor.Framework.Entities.Triggers;
using ProductMonitor.Framework.Services;
using Serilog;

namespace ProductMonitor.Framework
{
   public class XmlFile
    {
        private readonly string _configFilePathRoot;
        private readonly IMessageService _messageService;
        private readonly EmailService _emailService;
        private readonly GlobalAlarmService _globalAlarmService;
        private readonly SoundService _soundService;
        private readonly Func<string, int, Check> _checkFactory;

        public XmlFile(string configFilePathRoot, IMessageService messageService, EmailService emailService, GlobalAlarmService globalAlarmService, SoundService soundService, Func<string, int, Check> checkFactory)
        {
            _configFilePathRoot = configFilePathRoot;
            _messageService = messageService;
            _emailService = emailService;
            _globalAlarmService = globalAlarmService;
            _soundService = soundService;
            _checkFactory = checkFactory;
        }

        public Check[] Load()
        {
            var mainDoc = new XmlDocument();
            try
            {
                var myReader = new XmlTextReader(Path.Combine(_configFilePathRoot, @"main.xml"));
                mainDoc.Load(myReader);
                myReader.Close();
            }
            catch (Exception e)
            {
                _messageService.ShowError(@"Unable to load main xml file");
                Log.Error(e, "Unable to load main xml file");
            }

            var checks = new List<Check>();

            foreach (var monitorNode in mainDoc.ChildNodes.OfType<XmlNode>().Where(childNode => childNode.Name.ToUpper() == "PRODUCTMONITOR")
                .SelectMany(childNode => childNode.ChildNodes.OfType<XmlNode>()))
            {
                switch (monitorNode.Name.ToUpper())
                {
                    case "PROGRAMALARMS":
                        SetupAlarms(monitorNode);
                        break;
                    case "TABS":
                        LoadTabs(monitorNode, checks);
                        break;
                }
            }

            Log.Information("Configuration Loaded");

            return checks.ToArray();
        }

        private void SetupAlarms(XmlNode alarmsNode)
        {
            foreach (XmlNode child in alarmsNode)
            {
                switch (child.Name.ToUpper())
                {
                    case "EMAILDELAY":
                        _emailService.SetTimer(int.Parse(child.FirstChild.Value));
                        break;
                    case "SMTPHOST":
                        _emailService.Host = child.FirstChild.Value;
                        break;
                    case "SMTPUSER":
                        _emailService.Username = child.FirstChild.Value;
                        break;
                    case "SMTPPASSWORD":
                        _emailService.Password = child.FirstChild.Value;
                        break;
                    case "SMTPPORT":
                        _emailService.Port = int.Parse(child.FirstChild.Value);
                        break;
                    case "EMAIL1":
                        _globalAlarmService.SetTarget(child.FirstChild.Value);
                        break;
                }
            }
        }

        private void LoadTabs(XmlNode tabsNode, List<Check> checks)
        {
            foreach (var tab in tabsNode.ChildNodes.OfType<XmlNode>().Where(tab => tab.Name.ToUpper() == "TAB"))
            {
                LoadTab(tab, checks);
            }
        }

        private void LoadTab(XmlNode tab, List<Check> checks)
        {
            string name = "";
            string tabLocation = "";
            bool online = false;

            foreach (XmlNode childNode in tab.ChildNodes)
            {
                switch (childNode.Name.ToUpper())
                {
                    case "NAME":
                        name = childNode.FirstChild.Value;
                        break;
                    case "ONLINE":
                        online = childNode.FirstChild.Value.ToUpper() == "TRUE";
                        break;
                    case "CONFIG":
                        tabLocation = childNode.FirstChild.Value;
                        break;
                }
            }

            var configFilePath = Path.Combine(_configFilePathRoot, tabLocation);
            var myReader = new XmlTextReader(configFilePath);
            var tabDocument = new XmlDocument();

            try
            {
                tabDocument.Load(myReader);
                myReader.Close();
            }
            catch (Exception e)
            {
                _messageService.ShowError(@"Unable to connect to " + tabLocation);
                Log.Error(e, "Unable to connect to " + tabLocation);
            }

            var checkNodes = tabDocument.LastChild.LastChild;
            foreach (var checkNode in checkNodes.ChildNodes.OfType<XmlNode>().Where(checkNode => checkNode.Name.ToUpper() == "CHECK"))
            {
                LoadCheck(name, online, checkNode, checks);
            }
        }

        private void LoadCheck(string tabName, bool tabOnline, XmlNode checkNode, List<Check> checks)
        {

            string name = "";
            bool checkOnline = tabOnline;
            //get the checks name (usually the first node so this shouldn't be too in-efficient
            foreach (XmlNode childNode in checkNode.ChildNodes.Cast<XmlNode>().Where(childNode => childNode.Name.ToUpper() == "NAME"))
            {
                name = childNode.FirstChild.Value;
                break;
            }

            //create the check
            var checkBeingLoaded = _checkFactory(name, checks.Count);
            checks.Add(checkBeingLoaded);
            checkBeingLoaded.SetTab(tabName);

            int nodesLoaded = 0;

            //load the other parts
            foreach (XmlNode childNode in checkNode.ChildNodes)
            {
                switch (childNode.Name.ToUpper())
                {
                    case "ONLINE":
                        if (childNode.FirstChild.Value.ToUpper() == "TRUE")
                        {
                            if (tabOnline)
                            {
                                checkOnline = true;
                            }
                        }
                        else
                        {
                            checkOnline = false;
                        }
                        break;
                    case "FREQUENCY":
                        try
                        {
                            LoadFrequency(checkBeingLoaded, childNode);
                            nodesLoaded++;
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, "Failed to load FREQUENCY");
                        }
                        break;
                    case "QUERY":
                        try
                        {
                            LoadQuery(checkBeingLoaded, childNode);
                            nodesLoaded++;
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, "Failed to load QUERY");
                        }
                        break;
                    case "TRIGGERS":
                        nodesLoaded++;
                        foreach (var triggerNode in childNode.ChildNodes.Cast<XmlNode>().Where(triggerNode => triggerNode.Name.ToUpper() == "TRIGGER"))
                        {
                            try
                            {
                                LoadTrigger(checkBeingLoaded, triggerNode);
                            }
                            catch (Exception e)
                            {
                                Log.Error(e, "Failed to load TRIGGER");
                            }
                        }
                        break;
                    default:
                        if (childNode.Name.ToUpperInvariant() == "DefaultOkStatusMessage".ToUpperInvariant())
                        {
                            checkBeingLoaded.DefaultOkStatusMessage = childNode.InnerText;
                        }
                        break;
                }
            }

            if (nodesLoaded != 3)
            {
                _messageService.ShowError(@"Error in loadCheck");
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

        private void LoadFrequency(Check checkBeingLoaded, XmlNode frequencyNode)
        {
            string type = "";
            XmlNode input = null;

            //get XML properties
            foreach (XmlNode childNode in frequencyNode.ChildNodes)
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

        private void LoadQuery(Check checkBeingLoaded, XmlNode queryNode)
        {
            string type = "";
            XmlNode input = null;

            //load the xml data for the query
            foreach (XmlNode childNode in queryNode.ChildNodes)
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

            var myType = typeof(XmlFile).Assembly.GetTypes().First(t => String.Equals(t.Name, type, StringComparison.InvariantCultureIgnoreCase));
            var myQuery = (Query)Activator.CreateInstance(myType, input);
            checkBeingLoaded.SetQuery(myQuery);

        }

        private void LoadTrigger(Check checkBeingLoaded, XmlNode triggerNode)
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
            switch (type.ToUpper())
            {
                case "OLDERTHAN":
                    myTrigger =
                        new OlderThan(input);

                    checkBeingLoaded.AddTrigger(myTrigger);
                    break;
                case "OLDERTHANREPEATING":
                    myTrigger =
                        new OlderThanRepeating(input);

                    checkBeingLoaded.AddTrigger(myTrigger);
                    break;
                case "GREATERTHAN":
                    myTrigger =
                        new GreaterThan(input);

                    checkBeingLoaded.AddTrigger(myTrigger);
                    break;
                default:
                    if (type.ToUpperInvariant() == typeof(ResultNotNull).Name.ToUpperInvariant())
                    {
                        myTrigger = new ResultNotNull(input);

                        checkBeingLoaded.AddTrigger(myTrigger);
                    }
                    break;
            }
            //---add new trigger types here---

            //load the actions for the trigger
            Debug.Assert(actionsNode != null, "actionsNode != null");
            foreach (XmlNode action in actionsNode.ChildNodes)
            {
                if (action.Name.ToUpper() == "ACTION")
                {
                    LoadAction(myTrigger, action);
                }
            }
        }

        private void LoadAction(Trigger triggerBeingLoaded, XmlNode actionNode)
        {
            string type = "";
            XmlNode input = null;

            //get the XML inputs
            foreach (XmlNode childNode in actionNode.ChildNodes)
            {
                switch (childNode.Name.ToUpper())
                {
                    case "TYPE":
                        type = childNode.FirstChild.Value;
                        break;
                    case "INPUT":
                        input = childNode;
                        break;
                }
            }

            //create the action of the correct type and add it to the trigger
            if (type.ToUpper() == "WAV")
            {
                var myAction = new PlayWavFile(input, _soundService);
                triggerBeingLoaded.AddAction(myAction);
            }
            else if (type.ToUpper() == "WAVRepeating".ToUpper())
            {
                var myAction = new RepeatWavFile(input, _soundService);
                triggerBeingLoaded.AddAction(myAction);
            }
            else if (type.ToUpper() == "SENDEMAIL")
            {
                var myAction = new SendEmail(input, _emailService);
                triggerBeingLoaded.AddAction(myAction);
            }
            else if (type.ToUpper() == "SENDSMS")
            {
                var myAction = new SendSms(input);
                triggerBeingLoaded.AddAction(myAction);
            }
            else if (type.ToUpperInvariant() == typeof(ExecuteFile).Name.ToUpperInvariant())
            {
                var myAction = new ExecuteFile(input);
                triggerBeingLoaded.AddAction(myAction);
            }
        }

    }
}
