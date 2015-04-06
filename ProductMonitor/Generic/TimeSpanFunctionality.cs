using System;
using System.Collections.Generic;
using System.Text;

namespace Product_Monitor.Generic
{
    class TimeSpanFunctionality
    {
        public static int extraDaysFromMonthAndYears(DateTime greaterTime, 
            DateTime lesserTime){

            int days = 0;

            if (lesserTime.Year < greaterTime.Year)
            {

                //first get the number of days from the years between the lesserTime 
                //and the greater time
                for (int i = lesserTime.Year; i < greaterTime.Year; i++)
                {
                    if (DateTime.IsLeapYear(i))
                    {
                        days += 366;
                    }
                    else
                    {
                        days += 365;
                    }
                }

                //get the days from the month
                //lesser time to year end
                for (int i = lesserTime.Month; i <= 12; i++)
                {
                    if (i == 1 || i == 3 || i == 5 || i == 7 || i == 8 || i == 10 || i == 12)
                    {
                        days += 31;
                    }
                    else if (i == 4 || i == 6 || i == 9 || i == 11)
                    {
                        days += 30;
                    }
                    else if (i == 2)
                    {
                        if (DateTime.IsLeapYear(lesserTime.Year))
                        {
                            days += 29;
                        }
                        else
                        {
                            days += 28;
                        }
                    }
                }
                //start of year to greater time
                for (int i = 1; i < greaterTime.Month; i++)
                {
                    if (i == 1 || i == 3 || i == 5 || i == 7 || i == 8 || i == 10 || i == 12)
                    {
                        days += 31;
                    }
                    else if (i == 4 || i == 6 || i == 9 || i == 11)
                    {
                        days += 30;
                    }
                    else if (i == 2)
                    {
                        if (DateTime.IsLeapYear(greaterTime.Year))
                        {
                            days += 29;
                        }
                        else
                        {
                            days += 28;
                        }
                    }
                }
            }
            else
            {
                for (int i = lesserTime.Month+1; i < greaterTime.Month; i++)
                {
                    if (i == 1 || i == 3 || i == 5 || i == 7 || i == 8 || i == 10 || i == 12)
                    {
                        days += 31;
                    }
                    else if (i == 4 || i == 6 || i == 9 || i == 11)
                    {
                        days += 30;
                    }
                    else if (i == 2)
                    {
                        if (DateTime.IsLeapYear(greaterTime.Year))
                        {
                            days += 29;
                        }
                        else
                        {
                            days += 28;
                        }
                    }
                }
            }

                return days;
        }
    }
}
