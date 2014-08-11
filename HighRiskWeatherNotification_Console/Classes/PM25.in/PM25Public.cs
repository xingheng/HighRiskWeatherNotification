using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighRiskWeatherNotification
{
    class PM25Public
    {
        const string PM25AppKey = "***REMOVED***";

        const string PM25RequestPrefix = "http://www.pm25.in/api/querys";
        const string PM25RequestSuffix = ".json";

        public static string PM25Request(PM25RequestKind args)
        {
            string result = "";

            switch (args)
            {
                case PM25RequestKind.AllMonitorPM25:
                    result = string.Format("{0}{1}{2}", PM25RequestPrefix, "/pm2_5", PM25RequestSuffix);
                    break;
                case PM25RequestKind.AllMonitorAQIDetails:
                    result = string.Format("{0}{1}{2}", PM25RequestPrefix, "/aqi_details", PM25RequestSuffix);
                    break;
                case PM25RequestKind.AllMonitorAQI:
                    result = string.Format("{0}{1}{2}", PM25RequestPrefix, "/only_aqi", PM25RequestSuffix);
                    break;
                case PM25RequestKind.OneMonitorAQIDetails:
                    result = string.Format("{0}{1}{2}", PM25RequestPrefix, "/aqis_by_station", PM25RequestSuffix);
                    break;
                case PM25RequestKind.AllMonitorName:
                    result = string.Format("{0}{1}{2}", PM25RequestPrefix, "/station_names", PM25RequestSuffix);
                    break;
                case PM25RequestKind.AllCityName:
                    result = string.Format("{0}{1}{2}", PM25RequestPrefix, ""/*It's really an empty string*/, PM25RequestSuffix);
                    break;
                case PM25RequestKind.AllCityAQI:
                    result = string.Format("{0}{1}{2}", PM25RequestPrefix, "/all_cities", PM25RequestSuffix);
                    break;
                case PM25RequestKind.AllCityRanking:
                    result = string.Format("{0}{1}{2}", PM25RequestPrefix, "/aqi_ranking", PM25RequestSuffix);
                    break;
            }

            if (!string.IsNullOrEmpty(result))
                result += "?token=" + PM25AppKey;

            return result;
        }
    }

    public enum PM25RequestKind
    {
        AllMonitorPM25,
        AllMonitorAQIDetails,
        AllMonitorAQI,
        OneMonitorAQIDetails,
        AllMonitorName,
        AllCityName,
        AllCityAQI,
        AllCityRanking,
    }
}
