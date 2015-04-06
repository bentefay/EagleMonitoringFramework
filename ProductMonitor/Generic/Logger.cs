using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Product_Monitor.Generic
{
    class Logger
    {
        static Logger instance;
        string path = "Logs\\";

        private Logger () {
            Directory.CreateDirectory(path);
        }

        private FileStream getFile(string location)
        {
            if (File.Exists(location))
            {
                return File.OpenWrite(location);
            }
            else
            {
                
                return File.Create(location);
            }
        }


        //singleton pattern
        public static Logger getInstance()
        {
            if (instance == null)
            {
                instance = new Logger();
            }
            return instance;
        }

        public void Log(Exception e)
        {
            lock (this)
            {
                try
                {
                    using (FileStream logFileStream = getFile(path + DateTime.Today.ToString("yyyyMMdd") + ".txt"))
                    {

                        logFileStream.Position = logFileStream.Length;

                        using (StreamWriter logWritter = new StreamWriter(logFileStream))
                        {

                            logWritter.WriteLine(logWritter.NewLine + DateTime.Now.ToString()
                                + " - " + e.Message + logWritter.NewLine
                                + "====Stack Trace===" + logWritter.NewLine + e.StackTrace);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Logger expereienced exception: " + ex.Message);
                }
            }
        }

        public void Log(string message)
        {
            lock (this)
            {
                try
                {
                    using (FileStream logFileStream = getFile(path + DateTime.Today.ToString("yyyyMMdd") + ".txt"))
                    {

                        logFileStream.Position = logFileStream.Length;

                        using (StreamWriter logWritter = new StreamWriter(logFileStream))
                        {

                            logWritter.WriteLine(logWritter.NewLine + DateTime.Now.ToString()
                                + " - " + message + logWritter.NewLine);

                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Logger experiences exception: " + ex.Message);
                }
            }
        }

        public string GetLog()
        {
            lock (this)
            {
                try
                {
                    String log;
                    using (StreamReader logFileStream = File.OpenText(path + DateTime.Today.ToString("yyyyMMdd") + ".txt"))
                    {

                        log = logFileStream.ReadToEnd();

                    }

                        return log;
                    
                }
                catch { }
            }
            return "Unable to read log";
        }
    }
}
