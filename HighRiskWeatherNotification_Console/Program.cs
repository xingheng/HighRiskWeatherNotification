using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using UTILITIES_HAN;
using Newtonsoft.Json;

namespace HighRiskWeatherNotification
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomizedHRWNCheckerReceiver();

            do
            {
                // First, we could use ReadLine to check the current datetime.
                // Second, easy to compare the log according to the log.
                // Third, explicitly write the log to file here.
                ConsoleMe.ReadLine();
                ConsoleMe.WriteToLogFile();
            } while (true);

            //Thread.Sleep(Timeout.Infinite);
        }

        public static void CustomizedHRWNCheckerReceiver()
        {
            ConsoleMe.WriteLine("\n----------------------------------------------------");
            ConsoleMe.WriteLine("High Risk Weather Notification (HRWN) is running....\n");
#if DEBUG
            ConsoleMe.WriteLine("[DEBUG MODE]\n");
#endif

            var list = HRWNReceiver.Instance.GetReceivers();
            foreach (var man in list)
            {
                man.Format();
                ConsoleMe.WriteLine("User: {0}, number: {1}", man.UserName, man.FeitionAccount);

                HRWNChecker checker = new HRWNChecker(man);
#if DEBUG
                //checker.StartOnce();
                checker.Start();
#else
                checker.Start();
#endif

            }
        }
    }
}
