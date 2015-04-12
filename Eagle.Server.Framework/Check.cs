using System;
using System.Collections.Generic;
using System.Text;
using Eagle.Server.Framework.Entities.Frequencies;
using Eagle.Server.Framework.Entities.Queries;
using Eagle.Server.Framework.Entities.Triggers;
using Eagle.Server.Framework.Services;
using Serilog;

namespace Eagle.Server.Framework
{
    public class Check : ICheckDisplay
    {
        private Frequency _frequency;
        private Query _query;
        private readonly LinkedList<Trigger> _triggers = new LinkedList<Trigger>();
        private readonly int _index;
        private readonly Action<Check> _update;
        private readonly AlarmService _alarmService;
        private string _type;
        private string _tabName;
        private bool _hasError;
        private string _errorMessage;

        public const string TimeAtQueryExecution = "Time at Query Execution";

        private object _result;
        bool _actionActivated;

        public Check(int index, AlarmService alarmService, Action<Check> update)
        {
            _index = index;
            _update = update;
            _alarmService = alarmService;
        }

        public string DefaultOkStatusMessage { get; set; }

        public void SetTab(string name)
        {
            _tabName = name;
        }

        public int GetIndex()
        {
            return _index;
        }

        public void SetFrequency(Frequency frequency)
        {
            _frequency = frequency;
        }

        public void SetQuery(Query query)
        {
            _query = query;
        }

        public void SetType(string type)
        {
            _type = type;
        }

        public void AddTrigger(Trigger trigger)
        {
            var myNode = new LinkedListNode<Trigger>(trigger);
            _triggers.AddLast(myNode);
            trigger.setCheck(this);
        }

        public string GetLongLocation()
        {
            return _query.GetLongLocation();
        }

        public void Activate()
        {
            _errorMessage = null;
            _actionActivated = false;
            _hasError = false;

            if (!IsPaused())
            {
                //run the query (this may have some side effect if the query 
                //is testing if something has changed since the last test etc)
                try
                {
                    _result = _query.Test();
                }
                catch (Exception e)
                {
                    Log.Warning(e, "The error handler has failed");
                    //tell the error handler that it failed
                    _hasError = true;
                    _errorMessage = e.ToString();
                    _result = e.Message;
                    _alarmService.ReportError(this);
                }
                // System.Windows.Forms.MessageBox.Show(query.GetDescription());

                if (!_hasError)
                {

                    foreach (var t in _triggers)
                    {
                        try
                        {
                            //if true alarm will go off (part of test method)
                            if (t.Test(_result))
                            {
                                _actionActivated = true;
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Warning(e, "Failed to test alarm");
                        }
                    }

                    //tell the error handler that it is working
                    _alarmService.ReportSuccess(this);
                }
            }
            else
            {
                _result = "paused";
            }

            //update the GUI
            // GuiController.Update(this);
            _update(this);
        }

        public void Pause(bool paused)
        {
            if (_frequency != null)
            {
                if (paused)
                {
                    _alarmService.MarkPaused(this);
                    _frequency.Pause(true);
                }
                else
                {
                    _alarmService.MarkRunning(this);
                    _frequency.Pause(false);
                }
            }
        }

        public Dictionary<String, String> GetExtraValues()
        {
            return _query.GetAdditionalValues();
        }

        public bool ActivationForcable()
        {
            return _frequency.ActivationForceable();
        }

        public string GetTab()
        {
            return _tabName;
        }

        public string GetLocation()
        {
            return _query.GetLocation();
        }

        public bool IsPaused()
        {
            return _frequency.IsPaused();
        }

        public bool HasError()
        {
            return _hasError;
        }

        public bool IsTriggered()
        {

            return _actionActivated;
        }
        public string GetError()
        {
            return _errorMessage;
        }
        public string GetResult()
        {
            if (_result != null)
            {
                if (_result is TimeSpan)
                {
                    string formattedResult;
                    if (((TimeSpan)_result).Days > 0)
                    {
                        formattedResult = ((TimeSpan)_result).Days + " Day";

                        if (((TimeSpan)_result).Days != 1)
                        {
                            formattedResult += "s";
                        }
                        formattedResult += " " + ((TimeSpan)_result).Hours + " Hour";

                        if (((TimeSpan)_result).Hours != 1)
                        {
                            formattedResult += "s";
                        }

                    }
                    else if (((TimeSpan)_result).Hours > 0)
                    {
                        formattedResult =
                            ((TimeSpan)_result).Hours.ToString() + " Hour";

                        if (((TimeSpan)_result).Hours != 1)
                        {
                            formattedResult += "s";
                        }
                        formattedResult += " " + ((TimeSpan)_result).Minutes.ToString() + " Minute";

                        if (((TimeSpan)_result).Minutes != 1)
                        {
                            formattedResult += "s";
                        }

                    }
                    else if (((TimeSpan)_result).Minutes > 0)
                    {
                        formattedResult = ((TimeSpan)_result).Minutes.ToString() + " Minute";

                        if (((TimeSpan)_result).Minutes != 1)
                        {
                            formattedResult += "s";
                        }

                        formattedResult += " " + ((TimeSpan)_result).Seconds.ToString() + " Seconds";

                    }
                    else if (((TimeSpan)_result).Seconds > 0)
                    {
                        formattedResult = ((TimeSpan)_result).Seconds.ToString() + " Seconds";
                    }
                    else
                    {
                        formattedResult = "Up to date";
                    }
                    return formattedResult;
                }
                else
                {
                    return _result.ToString();
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(DefaultOkStatusMessage))
                {
                    return DefaultOkStatusMessage;
                }
                return _type;
            }
        }

        public string GetStatus()
        {
            StringBuilder msg = new StringBuilder();
            var values = _query.GetAdditionalValues();
            if (_result != null && _result is TimeSpan)
            {
                msg.AppendLine("It has been " + GetResult() + " since the " + _query.GetDescription() + " has updated");

                if (values.ContainsKey(TimeAtQueryExecution))
                {
                    msg.AppendLine("Last Update At: " + values[TimeAtQueryExecution]);
                }
            }

            foreach (var key in values.Keys)
            {
                msg.AppendLine(key + ": " + values[key]);
            }
            return msg.ToString();
        }

        public string GetCheckType()
        {
            return _type;
        }

    }
}
