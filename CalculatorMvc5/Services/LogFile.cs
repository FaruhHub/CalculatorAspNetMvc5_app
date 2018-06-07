using System;
using System.IO;
using System.Diagnostics;
using System.Configuration;
using System.Text;
using System.Collections.Generic;

namespace CalculatorMvc5.Services
{
    public class LogFile
    {
        public static Boolean CreateLogFile(String message)
        {
            try
            {
                string location = System.Environment.CurrentDirectory + "\\AF_Schedule_job_log.txt";
                if (!File.Exists(location))
                {
                    FileStream fs;
                    using (fs = new FileStream(location, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                    }
                    fs.Close();
                }
                Console.WriteLine(message);
                //Release the File that is created               
                StreamWriter sw = new StreamWriter(location, true);
                sw.Write(message + Environment.NewLine);
                sw.Close();
                sw = null;
                return true;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("MIDocShare", "Error in CreateLogFile" + ex.Message.ToString(), EventLogEntryType.Error, 6000);
                return false;
            }
        }

        public static bool WriteToLog(string message, string execptionMsg = null, string path = null)
        {
            try
            {
                if (path == null)// || (!string.IsNullOrEmpty(path) && !File.Exists(path)))
                    path = AppDomain.CurrentDomain.BaseDirectory + AppConfigServices.LogFile;

                String text = Environment.NewLine + DateTime.Now.ToString() + ": " + message;
                text = execptionMsg != null ? text + " ,Error Details: " + execptionMsg : text;
                File.AppendAllText(path, text, Encoding.Default);

                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
        
    }
}