using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MoreLinq;
using ProductMonitor.Framework;
using ProductMonitor.Framework.ProgramCode;
using Serilog;

namespace ProductMonitor.DisplayCode
{
    public class GuiController
    {
        private static TabDisplay[] _tabs;
        private static GUI _mainForm;

        private void StartGui(Func<Check[]> getChecks)
        {
            _mainForm = new GUI(getChecks);
            System.Windows.Forms.Application.Run(_mainForm);
        }

        public void StartUp(Func<Check[]> getChecks)
        {
            _tabs = new TabDisplay[0];

            var myThread = new Thread(() => StartGui(getChecks));
            myThread.Start();

            while (_mainForm == null)
            {
                Thread.Sleep(1000);
            }
        }

        public void Update(ICheckDisplay check)
        {
            try
            {
                if (_mainForm == null)
                {
                    throw new Exception("The form has not yet been created");
                }
                else
                {
                    TabDisplay myTab = null; //must be outside of lock

                    lock (_tabs)
                    {
                        myTab = _tabs.FirstOrDefault(t => t.GetName() == check.GetTab());

                        if (myTab == null)
                        {
                            myTab = new TabDisplay(check.GetTab());
                            _mainForm.AddTab(check.GetTab());
                            _tabs = _tabs.Where(t => t != null).Concat(myTab).ToArray();
                        }

                    }

                    if (myTab.AddCheck(check))
                    {
                        lock (_mainForm)
                        {
                            _mainForm.DrawTable(myTab.GetName(), myTab.GetLocations(), myTab.GetTypes());
                        }

                        for (int i = 0; i < myTab.GetTypes().Count; i++)
                        {
                            for (int j = 0; j < myTab.GetLocations().Count; j++)
                            {
                                var cell = myTab.GetTable()[i, j];
                                if (cell != null)
                                {
                                    string mouseOverText;
                                    System.Drawing.Color cellColour;
                                    if (cell.IsTriggered())
                                    {
                                        cellColour = System.Drawing.Color.Red;
                                        mouseOverText = cell.GetStatus();
                                    }
                                    else if (cell.HasError())
                                    {
                                        cellColour = System.Drawing.Color.Yellow;
                                        mouseOverText = cell.GetError();
                                    }
                                    else if (cell.IsPaused())
                                    {
                                        cellColour = System.Drawing.Color.LightBlue;
                                        mouseOverText = cell.GetStatus();
                                    }
                                    else
                                    {
                                        cellColour = System.Drawing.Color.White;
                                        mouseOverText = cell.GetStatus();
                                    }

                                    lock (_mainForm)
                                    {
                                        var result = cell.GetResult();
                                        
                                        _mainForm.SetCell(myTab.GetName(), i, j, result, cellColour, mouseOverText);
                                    }
                                }
                                else
                                {
                                    lock (_mainForm)
                                    {
                                        _mainForm.SetCell(myTab.GetName(), i, j,
                                            "Not Applicable", System.Drawing.Color.Gray,
                                            "Not Applicable");
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        //just update the cell
                        lock (_mainForm)
                        {
                            System.Drawing.Color cellColour;
                            if (check.IsTriggered())
                            {
                                cellColour = System.Drawing.Color.Red;
                            }
                            else if (check.HasError())
                            {
                                cellColour = System.Drawing.Color.Yellow;
                            }
                            else if (check.IsPaused())
                            {
                                cellColour = System.Drawing.Color.LightBlue;
                            }
                            else
                            {
                                cellColour = System.Drawing.Color.White;
                            }

                            _mainForm.SetCell(myTab.GetName(), myTab.LastRow, myTab.LastColumn,
                                   check.GetResult(), cellColour, check.GetStatus());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Update failed");
            }
        }

        public ICheckDisplay[,] GetChecksInTable(string tab)
        {
            return _tabs.Where(t => t.GetName() == tab).Select(t => t.GetTable()).FirstOrDefault();
        }

        public IReadOnlyList<String> LocationsInTab(string tab)
        {
            return _tabs.Where(t => t.GetName() == tab).Select(t => t.GetLocations()).FirstOrDefault();
        }

        public void TakeScreenshot(string tab, string saveLocation)
        {
            _mainForm.TakeScreenshot(tab, saveLocation);
        }
    }
}


