using System;
using System.Collections.Generic;
using System.Text;
using ProductMonitor.Framework.Entities.Frequencies;
using ProductMonitor.Framework.Entities.Queries;
using ProductMonitor.Framework.Entities.Triggers;
using ProductMonitor.Framework.Services;
using Serilog;

namespace ProductMonitor.Framework
{
    public class Check : ICheckDisplay
    {
        private Frequency _frequency;
        private Query _query;
        private readonly LinkedList<Trigger> triggers = new LinkedList<Trigger>();
        private readonly int _index;
        private readonly Action<Check> _update;
        private readonly GlobalAlarmService _globalAlarmService;
        private string _type;
        private string _tabName;
        private bool _hasError;
        private string _errorMessage;

        public const string TimeAtQueryExecution = "Time at Query Execution";

        //provide variables for display functionality
        private object result;
        bool actionActivated = false;

        public Check(string name, int index, Action<Check> update, GlobalAlarmService globalAlarmService)
        {
            _index = index;
            _update = update;
            _globalAlarmService = globalAlarmService;
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
            this._frequency = frequency;
        }

        public void SetQuery(Query query)
        {
            this._query = query;
        }

        public void SetType(string type)
        {
            _type = type;
        }

        public void AddTrigger(Trigger trigger)
        {
            var myNode = new LinkedListNode<Trigger>(trigger);
            triggers.AddLast(myNode);
            trigger.setCheck(this);
        }

        public string GetLongLocation()
        {
            return _query.GetLongLocation();
        }

        public void Activate()
        {
            _errorMessage = null;
            actionActivated = false;
            _hasError = false;


            if (!IsPaused())
            {
                //run the query (this may have some side effect if the query 
                //is testing if something has changed since the last test etc)
                try
                {
                    result = _query.Test();
                }
                catch (Exception e)
                {
                    Log.Warning(e, "The error handler has failed");
                    //tell the error handler that it failed
                    _hasError = true;
                    _errorMessage = e.ToString();
                    result = e.Message;
                    _globalAlarmService.ReportError(_index);
                }
                // System.Windows.Forms.MessageBox.Show(query.GetDescription());

                if (!_hasError)
                {

                    foreach (var t in triggers)
                    {
                        try
                        {
                            //if true alarm will go off (part of test method)
                            if (t.Test(result))
                            {
                                actionActivated = true;
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Warning(e, "Failed to test alarm");
                        }
                    }

                    //tell the error handler that it is working
                    _globalAlarmService.ReportSuccess(_index);
                }
            }
            else
            {
                result = "paused";
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
                    _globalAlarmService.MarkPaused(_index);
                    _frequency.Pause(true);
                }
                else
                {
                    _globalAlarmService.MarkUnPaused(_index);
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

        #region Check Display Code

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
            return _frequency.isPaused();
        }

        public bool HasError()
        {
            return _hasError;
        }

        public bool IsTriggered()
        {

            return actionActivated;
        }
        public string GetError()
        {
            return _errorMessage;
        }
        public string GetResult()
        {
            if (result != null)
            {
                if (result is TimeSpan)
                {
                    string formattedResult;
                    if (((TimeSpan)result).Days > 0)
                    {
                        formattedResult = ((TimeSpan)result).Days + " Day";

                        if (((TimeSpan)result).Days != 1)
                        {
                            formattedResult += "s";
                        }
                        formattedResult += " " + ((TimeSpan)result).Hours + " Hour";

                        if (((TimeSpan)result).Hours != 1)
                        {
                            formattedResult += "s";
                        }

                    }
                    else if (((TimeSpan)result).Hours > 0)
                    {
                        formattedResult =
                            ((TimeSpan)result).Hours.ToString() + " Hour";

                        if (((TimeSpan)result).Hours != 1)
                        {
                            formattedResult += "s";
                        }
                        formattedResult += " " + ((TimeSpan)result).Minutes.ToString() + " Minute";

                        if (((TimeSpan)result).Minutes != 1)
                        {
                            formattedResult += "s";
                        }

                    }
                    else if (((TimeSpan)result).Minutes > 0)
                    {
                        formattedResult = ((TimeSpan)result).Minutes.ToString() + " Minute";

                        if (((TimeSpan)result).Minutes != 1)
                        {
                            formattedResult += "s";
                        }

                        formattedResult += " " + ((TimeSpan)result).Seconds.ToString() + " Seconds";

                    }
                    else if (((TimeSpan)result).Seconds > 0)
                    {
                        formattedResult = ((TimeSpan)result).Seconds.ToString() + " Seconds";
                    }
                    else
                    {
                        formattedResult = "Up to date";
                    }
                    return formattedResult;
                }
                else
                {
                    return result.ToString();
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
            if (result != null && result is TimeSpan)
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

        #endregion

    }
}
