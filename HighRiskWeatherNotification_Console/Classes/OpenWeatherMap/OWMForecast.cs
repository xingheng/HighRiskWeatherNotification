using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HighRiskWeatherNotification
{
    [XmlRoot("weatherdata")]
    public class OWMForecast : IOWMEntity
    {
        [XmlElementAttribute("location")]
        public OWMLocation Location { get; set; }
        [XmlIgnore]
        public OWMMeta Meta { get; set; }

        [XmlElementAttribute("forecast")]
        public OWMForecastArray Forecasts;

        public string GetReadableText(string cityName = "")
        {
            string result = "";
            string location = this.Location.Name;
            if (!string.IsNullOrEmpty(cityName))
                location = cityName;

            int subArrLength = this.Forecasts.TimeArray.Count();
            if (subArrLength > 0)
                result = string.Format("There are {0} forecast records for {1}, the first one is {2}.",
                        subArrLength, location, this.Forecasts.TimeArray[0].ToString());
            else
                result = string.Format("There are {0} forecast records for {1}", subArrLength, location);

            return result;
        }
    }

    public class OWMLocation
    {
        [XmlElementAttribute("name")]
        public string Name { get; set; }
        [XmlElementAttribute("type")]
        public string Type { get; set; }
        [XmlElementAttribute("country")]
        public string Country { get; set; }
        [XmlElementAttribute("timezone")]
        public string Timezone { get; set; }
        [XmlElementAttribute("location")]
        public OWMLocationInfo Location { get; set; }
        [XmlElementAttribute("credit")]
        public string Credit { get; set; }
    }

    public class OWMLocationInfo : OWMCoordinary
    {
        [XmlAttribute("altitude")]
        public string Altitude { get; set; }
        [XmlAttribute("geobase")]
        public string Geobase { get; set; }
        [XmlAttribute("geobaseid")]
        public string Geobaseid { get; set; }
    }

    public class OWMMeta
    { 
    }


    public class OWMForecastArray
    {
        [XmlElementAttribute("time")]
        public OWMForecastItem[] TimeArray;
    }

    public class OWMForecastItem
    {
        [XmlAttribute("from")]
        public DateTime FromDate { get; set; }
        [XmlAttribute("to")]
        public DateTime ToDate { get; set; }
        [XmlElementAttribute("symbol")]
        public OWMSymbol Symbol { get; set; }
        [XmlElementAttribute("precipitation")]
        public string Precipitation { get; set; }
        [XmlElementAttribute("windDirection")]
        public OWMWindDirection WindDirection { get; set; }
        [XmlElementAttribute("windSpeed")]
        public OWMWindSpeed WindSpeed { get; set; }
        [XmlElementAttribute("temperature")]
        public OWMTemperature Temperature { get; set; }
        [XmlElementAttribute("pressure")]
        public OWMPressure Pressure { get; set; }
        [XmlElementAttribute("humidity")]
        public OWMHumidity Humidity { get; set; }
        [XmlElementAttribute("clouds")]
        public OWMCloudDetail Clouds { get; set; }

        public override string ToString()
        {
            double interval = (ToDate - FromDate).TotalHours;

            string result = string.Format("In next {0} hour(s):\nweather: {1} temperature: {2} humidity: {3} cloud: {4}", interval, this.Symbol.ToString(),
                this.Temperature.ToString(), this.Humidity.ToString(), this.Clouds.ToString());
            return result;
        }

        public override bool Equals(object obj)
        {
            OWMForecastItem newObj = obj as OWMForecastItem;
            if (newObj != null)
            {
                return newObj.FromDate == this.FromDate &&
                    newObj.ToDate == this.ToDate &&
                    newObj.Symbol.ToString() == this.Symbol.ToString() &&
                    newObj.Temperature.RoughlyEqual(this.Temperature);
            }
            return base.Equals(obj);
        }
    }

    public class OWMSymbol
    {
        [XmlAttribute("number")]
        public string Number { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("var")]
        public string Var { get; set; }

        public override string ToString()
        {
            return string.Format("{0}.", this.Name);
        }
    }

    public class OWMWindDirection
    {
        [XmlAttribute("deg")]
        public string Deg { get; set; }
        [XmlAttribute("dode")]
        public string Code { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }

        public override string ToString()
        {
            return string.Format("{0}.", this.Name);
        }
    }

    public class OWMWindSpeed
    {
        [XmlAttribute("mps")]
        public string Mps { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }

        public override string ToString()
        {
            return string.Format("{0}{1}.", this.Mps, this.Name);
        }
    }

    public class OWMCloudDetail
    {
        [XmlAttribute("value")]
        public string Value { get; set; }
        [XmlAttribute("all")]
        public string All { get; set; }
        [XmlAttribute("unit")]
        public string Unit { get; set; }

        public override string ToString()
        {
            return string.Format("{0}, {1}{2}.", this.Value, this.All, this.Unit);
        }
    }
}
