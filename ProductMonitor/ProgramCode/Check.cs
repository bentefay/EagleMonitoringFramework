using System;
using System.Collections.Generic;
using System.Text;
using ProductMonitor.Display_Code;
using ProductMonitor.ProgramCode.Triggers;

namespace ProductMonitor.ProgramCode
{
    class Check : ICheckDisplay
    {
        private Frequencies.Frequency frequency;
        private Queries.Query query;
        private LinkedList<Triggers.Trigger> triggers = new LinkedList<Trigger>();
        private int index;
        private string type;
        private string tabName;
        private string name;
        private bool hasError;
        private string errorMessage;

        public const string TimeAtQueryExecution = "Time at Query Execution";

        //provide variables for display functionality
        private object result;
        bool actionActivated = false;

        public Check(string name, int index)
        {
            this.name = name;
            this.index = index;
        }

        public string DefaultOkStatusMessage { get; set; }

        public void SetTab(string name)
        {
            tabName = name;
        }

        public int getIndex()
        {
            return index;
        }

        public void SetFrequency(Frequencies.Frequency frequency)
        {
            this.frequency = frequency;
        }

        public void SetQuery(Queries.Query query)
        {
            this.query = query;
        }

        public void SetType(string type)
        {
            this.type = type;
        }

        public void AddTrigger(Triggers.Trigger trigger)
        {
            LinkedListNode<Triggers.Trigger> myNode =
                new LinkedListNode<Trigger>(trigger);
            triggers.AddLast(myNode);

            trigger.setCheck(this);
        }

        public string GetLongLocation()
        {
            return query.GetLongLocation();
        }

        public void Activate()
        {
            errorMessage = null;
            actionActivated = false;
            hasError = false;


            if (!IsPaused())
            {
                //run the query (this may have some side effect if the query 
                //is testing if something has changed since the last test etc)
                try
                {
                    result = query.Test();
                }
                catch (Exception e)
                {
                    Product_Monitor.Generic.Logger.getInstance().Log(e);
                    //tell the error handler that it failed
                    hasError = true;
                    errorMessage = e.ToString();
                    result = e.Message;
                    GlobalAlarm.ReportError(index);
                }
                // System.Windows.Forms.MessageBox.Show(query.GetDescription());

                if (!hasError)
                {

                    foreach (Triggers.Trigger t in triggers)
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
                            Product_Monitor.Generic.Logger.getInstance().Log(e);
                        }
                    }

                    //tell the error handler that it is working
                    GlobalAlarm.ReportSuccess(index);
                }
            }
            else
            {
                result = "paused";
            }

            //update the GUI
            GuiController.Update(this);
        }

        public void Pause(bool paused)
        {
            if (frequency != null)
            {
                if (paused == true)
                {
                    GlobalAlarm.MarkPaused(index);
                    frequency.Pause(true);
                }
                else
                {
                    GlobalAlarm.MarkUnPaused(index);
                    frequency.Pause(false);
                }
            }
        }

        public Dictionary<String, String> GetExtraValues()
        {
            return query.GetAdditionalValues();
        }

        public bool ActivationForcable()
        {
            return frequency.ActivationForceable();
        }

        #region Check Display Code

        public string GetTab()
        {
            return tabName;
        }

        public string GetLocation()
        {
            return query.GetLocation();
        }

        public bool IsPaused()
        {
            return frequency.isPaused();
        }

        public bool HasError()
        {
            return hasError;
        }

        public bool IsTriggered()
        {

            return actionActivated;
        }
        public string GetError()
        {
            return errorMessage;
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
                        formattedResult =
                            ((TimeSpan)result).Days.ToString() + " Day";

                        if (((TimeSpan)result).Days != 1)
                        {
                            formattedResult += "s";
                        }
                        formattedResult += " " + ((TimeSpan)result).Hours.ToString() + " Hour";

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
                return type;
            }
        }

        public string GetStatus()
        {
            StringBuilder msg = new StringBuilder();
            var values = query.GetAdditionalValues();
            if (result != null && result is TimeSpan)
            {
                msg.AppendLine("It has been " + GetResult() + " since the " + query.GetDescription() + " has updated");

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
            return type;
        }

        #endregion

    }
}
