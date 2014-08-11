using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighRiskWeatherNotification
{
    // Link: http://openweathermap.org/appid

    class OWMPublic
    {
        const string OWMAppKey = "***REMOVED***";
        const string OWMRequestPrefix = "http://api.openweathermap.org/data/2.5/";

        public static string OWMRequest(OWMRequestKind kind)
        {
            string request = "";

            switch (kind)
            {
                case OWMRequestKind.CurrentWeather:
                    request = string.Format("{0}{1}APPID={2}", OWMRequestPrefix, "weather?", OWMAppKey);
                    break;
                case OWMRequestKind.HourlyForecast:
                    request = string.Format("{0}{1}APPID={2}", OWMRequestPrefix, "forecast?", OWMAppKey);
                    break;
            }

            return request;
        }

    }

    enum OWMRequestKind
    {
        CurrentWeather,
        HourlyForecast
    }
}
