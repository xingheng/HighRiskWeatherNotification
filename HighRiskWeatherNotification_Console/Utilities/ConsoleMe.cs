using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Redirect all the console output to this class
namespace UTILITIES_HAN
{
    class ConsoleMe
    {
        public static LogFile LogFileInstance
        {
            get
            {
                return LogFile.Instance;
            }
        }

        public static void WriteLine()
        {
            Console.WriteLine();
            LogFileInstance.AppendLog("\n");
        }

        public static void WriteLine(string value)
        {
            Console.WriteLine(value);
            LogFileInstance.AppendLog(value + "\n");
        }

        public static void WriteLine(string format, params object[] arg)
        {
            Console.WriteLine(format, arg);
            LogFileInstance.AppendLog(string.Format(format, arg) + "\n");
        }

        public static bool WriteToLogFile()
        {
            return LogFileInstance.Write(true);
        }

        public static string ReadLine()
        {
            string input = Console.ReadLine();
            ConsoleMe.WriteLine(string.Format("{0}, user input: '{1}'.", DateTime.Now.ToLongTimeString(), input));
            return input;
        }

        public static void ReadyToExit(int exitCode = 0)
        {
            WriteToLogFile();
            System.Environment.Exit(exitCode);
        }
    }
}
