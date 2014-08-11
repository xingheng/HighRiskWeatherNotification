using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Xml.Serialization;
using UTILITIES_HAN;

namespace HighRiskWeatherNotification
{
    class AQITextConverter
    {
        private static AQITextConverter instance;
        public static AQITextConverter Instance
        {
            get 
            {
                if (instance == null)
                    instance = new AQITextConverter();
                return instance;
            }
        }

        private PM25Thresholds thresholds = null;
        static object locker = new object();

        private AQITextConverter()
        {
            lock (locker)
            {
                if (thresholds == null)
                    LoadTextConverterFile();
            }
        }

        bool LoadTextConverterFile()
        {
            bool result = false;

            string filename = "";
            string value = ConfigurationManager.AppSettings["AQIReadableTextConverterFile"];
            if (!string.IsNullOrEmpty(value) && File.Exists(value))
                filename = value;

            if (!string.IsNullOrEmpty(filename))
            {
                using (FileStream stream = new FileStream(filename, FileMode.Open))
                {
                    var serializer = new XmlSerializer(typeof(PM25Thresholds));

                    object resultObj;
                    resultObj = serializer.Deserialize(stream);
                    if (resultObj != null)
                    {
                        thresholds = resultObj as PM25Thresholds;
                        result = true;
                    }
                }
            }

            if (!result)
            {
                ConsoleMe.WriteLine("Failed to load AQI readable text converter file {0}.", value ?? "");
                ConsoleMe.ReadyToExit();
            }

            return result;
        }

        public string GetReadableTextByAQI(int aqi)
        {
            string result = "";
            if (thresholds != null)
            {
                var obj = (from c in thresholds.ThresholdsItemArray
                           where c.Min <= aqi && c.Max >= aqi
                           select c).ToList();

                if (obj != null && obj.Count > 0)
                {
                    PM25ThresholdsItem item = obj[0] as PM25ThresholdsItem;
                    result = string.Format("AQI: {0}. Pollution Level: {1}. Health Implications: {2}", aqi, item.PollutionLevel, item.HealthImplications);
                }
            }

            if (string.IsNullOrEmpty(result))
                result += "Invalid AQI value.";

            return result;
        }
    }

    [XmlRoot("PM25.in")]
    public class PM25Thresholds
    {
        [System.Xml.Serialization.XmlElementAttribute("Level")]
        public PM25ThresholdsItem[] ThresholdsItemArray { get; set; }
    }

    public class PM25ThresholdsItem
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public string PollutionLevel { get; set; }
        public string HealthImplications { get; set; }
    }
}
