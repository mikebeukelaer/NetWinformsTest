using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetWinformsTest
{
    public class Logger
    {

        public enum Level
        {
            ERROR,
            WARNING,
            INFO,
            WARN
        }

        private static readonly string logFilePath = "log.txt";

        public static void Log(Level logLevel, string message)
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"[{DateTime.Now.ToString("mm-dd-yyyy:hh:mm:ss")}] [{logLevel.ToString()}] {message}");
            }
        }
        public static void LogMessage(string message)
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"[{DateTime.Now.ToString("mm-dd-yyyy:hh:mm:ss")}] [{Level.INFO.ToString()}] {message}");
            }
        }

    }
}
