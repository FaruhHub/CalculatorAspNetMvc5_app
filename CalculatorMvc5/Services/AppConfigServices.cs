using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace CalculatorMvc5.Services
{
    public class AppConfigServices
    {
        private static string logFolder;
        public static string LogFolder
        {
            get
            {
                if (string.IsNullOrEmpty(logFolder))
                    logFolder = ConfigurationManager.AppSettings["logFolder"];
                if (Directory.Exists(logFolder) == false)
                    Directory.CreateDirectory(logFolder);

                return logFolder;
            }
        }

        private static string logFile;
        public static string LogFile
        {
            get
            {
                if (string.IsNullOrEmpty(logFile))
                    logFile = Path.Combine(LogFolder, ConfigurationManager.AppSettings["logFilePath"]);

                return logFile;
            }
        }
    }
}