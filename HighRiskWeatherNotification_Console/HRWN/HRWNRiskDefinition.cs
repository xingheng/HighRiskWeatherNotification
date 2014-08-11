#if DEBUG
//#define         RiskRangeEnabled    // In debug mode, comment/uncomment this line to open/close the switcher.
#else
#define         RiskRangeEnabled
#endif

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UTILITIES_HAN;

namespace HighRiskWeatherNotification
{
    class HRWNRiskDefinition
    {
        private static HRWNRiskDefinition instance;
        public static HRWNRiskDefinition Instance
        {
            get
            {
                if (instance == null)
                    instance = new HRWNRiskDefinition();
                return instance;
            }
        }

        private static HRWNRiskThreshold thresholds = null;
        static object locker = new object();

        private HRWNRiskDefinition()
        {
            lock (locker)
            {
                if (thresholds == null)
                    LoadDataFile();
            }
        }

        private bool LoadDataFile()
        {
            bool result = false;

            string filename = "";
            string value = ConfigurationManager.AppSettings["HRWNRiskDefinitionFile"];
            if (!string.IsNullOrEmpty(value) && File.Exists(value))
                filename = value;

            if (!string.IsNullOrEmpty(filename))
            {
                using (FileStream stream = new FileStream(filename, FileMode.Open))
                {
                    var serializer = new XmlSerializer(typeof(HRWNRiskThreshold));

                    object resultObj;
                    resultObj = serializer.Deserialize(stream);
                    if (resultObj != null)
                    {
                        thresholds = resultObj as HRWNRiskThreshold;
                        result = true;
                    }
                }
            }

            if (!result)
            {
                ConsoleMe.WriteLine("Failed to load risk definition file {0}.", value ?? "");
                ConsoleMe.ReadyToExit();
            }

            return result;
        }

        private static void SwitcherDisableLog()
        {
#if !RiskRangeEnabled
            ConsoleMe.WriteLine("RiskRangeEnabled is disabled.");
#endif
        }

        #region IsRiskXXX
        public bool IsRiskAQI(int aqi)
        {
            SwitcherDisableLog();
#if RiskRangeEnabled
            return aqi > thresholds.PM25.AQI;
#else
            return true;
#endif
        }

        public bool IsRiskAQI(AQIRecord record)
        {
            SwitcherDisableLog();
#if RiskRangeEnabled
            int aqi = -1;
            int.TryParse(record.AQI, out aqi);
            return aqi > thresholds.PM25.AQI;
#else
            return true;
#endif
        }

        public bool IsRiskWeather(OWMCurWeather curWeather)
        {
            SwitcherDisableLog();
#if RiskRangeEnabled
            return this.IsRiskWeather(curWeather.Weather.Value) || this.IsRiskTemperature(curWeather.Temperature.Value);
#else
            return true;
#endif
        }

        public bool IsRiskWeather(OWMForecastItem forecast)
        {
            SwitcherDisableLog();
#if RiskRangeEnabled
            return this.IsRiskWeather(forecast.Symbol.Var) || this.IsRiskTemperature(forecast.Temperature.Value);
#else
            return true;
#endif
        }

        public bool IsRiskWeather(string weather)
        {
            SwitcherDisableLog();
#if RiskRangeEnabled
            var res = (from c in thresholds.OWM.Weathers.Items
                       where weather.Contains(c)
                       select c).ToList();
            return res.Count != 0;
#else
            return true;
#endif
        }

        public bool IsRiskTemperature(int temperature)
        {
            SwitcherDisableLog();
#if RiskRangeEnabled
            var res = (from c in thresholds.OWM.Temperatures.Items
                       where temperature >= c.Min && temperature <= c.Max
                       select c).ToList();
            return res.Count != 0;
#else
            return true;
#endif
        }

        public bool IsRiskTemperature(string temperature)
        {
            SwitcherDisableLog();
#if RiskRangeEnabled
            int tempe = -1;
            int.TryParse(temperature, out tempe);
            return this.IsRiskTemperature(tempe);
#else
            return true;
#endif
        }
        #endregion
    }


    [XmlRoot("HRWN")]
    public class HRWNRiskThreshold
    {
        [XmlElement("PM25.in")]
        public HRWNRiskPM25 PM25 { get; set; }
        [XmlElement("OWM")]
        public HRWNRiskOWM OWM { get; set; }

        public class HRWNRiskPM25
        {
            public int AQI { get; set; }
        }

        public class HRWNRiskOWM
        {
            [XmlElement("Weathers")]
            public HRWNRiskWeather Weathers { get; set; }
            [XmlElement("Temperatures")]
            public HRWNRiskTemperature Temperatures { get; set; }

            public class HRWNRiskWeather
            {
                [XmlElementAttribute("Items")]
                public string[] Items { get; set; }
            }

            public class HRWNRiskTemperature
            {
                [XmlElementAttribute("Items")]
                public HRWNRiskTemperatureItem[] Items { get; set; }

                public class HRWNRiskTemperatureItem
                {
                    public int Min { get; set; }
                    public int Max { get; set; }
                }
            }
        }
    }
}
