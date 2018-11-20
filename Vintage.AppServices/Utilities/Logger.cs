namespace Vintage.AppServices.Utilities
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;

    public enum LogLevel
    {
        Detailed,
        Normal,
        ExceptionOnly
    }

    public static class Log
    {
        public static void Write(string message, LogLevel level)
        {
            string eol = "\r\n";

            try
            {
                string currentLogLevel = ConfigurationManager.AppSettings["LogLevel"];

                LogLevel minLevel = LogLevel.ExceptionOnly;

                switch (currentLogLevel.ToLower())
                {
                    case "detailed":
                        minLevel = LogLevel.Detailed;
                        break;
                    case "normal":
                        minLevel = LogLevel.Normal;
                        break;
                    case "exceptiononly":
                        minLevel = LogLevel.ExceptionOnly;
                        break;
                }

                if (level >= minLevel)
                {
                    string logentry =
                        DateTime.Now.ToString("dd MMM yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture) + eol +
                        message + eol +
                        "------------------------------------------------------------" + eol;

                    string logFile = ConfigurationManager.AppSettings["LogFileName"];
                    string startdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
                    startdir = startdir.Substring(6);  // removes "file:\" prefix

                    if (!startdir.EndsWith(@"\"))
                    {
                        startdir += @"\";
                    }

                    File.AppendAllText(startdir + logFile, logentry);

                }

            }

            catch (Exception ex)
            {
                // Write to Event Log - if problem writing to Application Log
                try
                {
                    string LogError = "Error writing message to Application Log: " + ex.Message + eol;
                    using (EventLog el = new EventLog())
                    {
                        el.WriteEntry(LogError + message, EventLogEntryType.Information);
                    }
                }
                catch { }  // just swallow exception if this fails
            }

            return;
        }
    }
}
