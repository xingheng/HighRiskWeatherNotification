using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighRiskWeatherNotification
{
    class FeitionPublic
    {
        private const string _UserName = "***REMOVED***";
        public static string UserName
        {
            get { return _UserName; }
        }

        private const string _Password = "***REMOVED***";
        public static string Password
        {
            get { return _Password; }
        }

        private const string _AppSuffix = " [来自***REMOVED***和***REMOVED***的专有天气预警]";
        public static string AppSuffix
        {
            get { return _AppSuffix; }
        }
    }
}
