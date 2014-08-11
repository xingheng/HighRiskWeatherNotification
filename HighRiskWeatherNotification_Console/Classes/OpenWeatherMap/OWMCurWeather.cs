using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace HighRiskWeatherNotification
{
    interface IOWMEntity
    {
        string GetReadableText(string cityName);
    }

    [XmlRoot("current")]
    public class OWMCurWeather : IOWMEntity
    {
        [XmlElementAttribute("city")]
        public OWMCity City { get; set; }
        [XmlElementAttribute("temperature")]
        public OWMTemperature Temperature { get; set; }
        [XmlElementAttribute("humidity")]
        public OWMHumidity Humidity { get; set; }
        [XmlElementAttribute("pressure")]
        public OWMPressure Pressure { get; set; }
        [XmlElementAttribute("wind")]
        public OWMWind Wind { get; set; }
        [XmlElementAttribute("clouds")]
        public OWMCloud Cloud { get; set; }

        [XmlElementAttribute("visibility")]
        public string Visibility { get; set; }
        [XmlElementAttribute("precipitation")]
        public OWMPrecipitation Precipitation { get; set; }
        [XmlElementAttribute("weather")]
        public OWMWeather Weather { get; set; }
        [XmlElementAttribute("lastupdate")]
        public OWMLastUpdate LastUpdate { get; set; }

        public override string ToString()
        {
            string result = string.Format("City: {0} weather: {1} temperature: {2} lastupdate: {3}", 
                this.City.Name, this.Weather.ToString(), this.Temperature.ToString(), this.LastUpdate.Value);
            return result;
        }

        public override bool Equals(object obj)
        {
            OWMCurWeather newObj = obj as OWMCurWeather;
            if (newObj != null)
            {
                return newObj.City.Name == this.City.Name &&
                    newObj.Weather.ToString() == this.Weather.ToString() &&
                    newObj.Temperature.RoughlyEqual(this.Temperature);
            }
            return base.Equals(obj);
        }

        public string GetReadableText(string cityName = "")
        {
            string location = this.City.Name;
            if (!string.IsNullOrEmpty(cityName))
                location = cityName;

            string result = string.Format("Today in {0}:\nweather: {1} temperature: {2} humidity: {3} wind: {4} cloud: {5}",
                location, this.Weather.ToString(), this.Temperature.ToString(),
                this.Humidity.ToString(), this.Wind.ToString(), this.Cloud.ToString());
            return result;
        }
    }


    public class OWMCity
    {
        [XmlAttribute("id")]
        public string ID { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlElement("coord")]
        public OWMCoordinary Coord { get; set; }
        [XmlElement("country")]
        public string Country { get; set; }
        [XmlElement("sun")]
        public OWMSun Sun { get; set; }
    }

    public class OWMSun
    {
        [XmlAttribute("rise")]
        public string Rise { get; set; }
        [XmlAttribute("set")]
        public string Set { get; set; }
    }

    public class OWMTemperature
    {
        [XmlAttribute("value")]
        public string Value { get; set; }
        [XmlAttribute("min")]
        public string Min { get; set; }
        [XmlAttribute("max")]
        public string Max { get; set; }
        [XmlAttribute("unit")]
        public string Unit { get; set; }

        private double _value, _min, _max;
        private string _unit;

        public bool Convert()
        {
            bool res = false;

            double.TryParse(this.Value, out _value);
            double.TryParse(this.Min, out _min);
            double.TryParse(this.Max, out _max);

            switch (this.Unit.ToLower())
            {
                case "kelvin":
                    const double kelvin = 273.15;
                    _value -= kelvin;
                    _min -= kelvin;
                    _max -= kelvin;
                    _unit = "℃";
                    res = true;
                    break;

                case "celsius":
                    _unit = "℃";
                    res = true;
                    break;

                default:
                    break;
            }
            return res;
        }

        public string ToString(string info = "short")
        {
            string result = "";
            this.Convert();

            switch (info)
            {
                case "long":
                    result = string.Format("current:{0}, min: {1}, max: {2}, unit:{3}.", _value, _min, _max, _unit);
                    break;
                case "short":
                default:
                    result = string.Format("{0}{1}.", _value, _unit);
                    break;
            }
            return result;
        }

        public bool RoughlyEqual(OWMTemperature newObj)
        {
            this.Convert();

            int roughValue = 5;
            return (this._value >= (newObj._value - roughValue)) && (this._value <= (newObj._value + roughValue));
        }
    }

    public class OWMHumidity
    {
        [XmlAttribute("value")]
        public string Value { get; set; }
        [XmlAttribute("unit")]
        public string Unit { get; set; }

        public override string ToString()
        {
            return string.Format("{0}{1}.", Value, Unit);
        }
    }

    public class OWMPressure
    {
        [XmlAttribute("value")]
        public string Value { get; set; }
        [XmlAttribute("unit")]
        public string Unit { get; set; }

        public override string ToString()
        {
            return string.Format("{0}{1}.", Value, Unit);
        }
    }

    public class OWMWind
    {
        [XmlElementAttribute("speed")]
        public OWMSpeed Speed { get; set; }
        [XmlElementAttribute("direction")]
        public OWMDirection Direction { get; set; }

        public override string ToString()
        {
            return string.Format("{0} level(s), {1}.", this.Speed.Value, this.Speed.Name);
        }
    }

    public class OWMSpeed
    {
        [XmlAttribute("value")]
        public string Value { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
    }

    public class OWMDirection
    {
        [XmlAttribute("value")]
        public string Value { get; set; }
        [XmlAttribute("code")]
        public string Code { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
    }

    public class OWMCloud
    {
        [XmlAttribute("value")]
        public string Value { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }

        public override string ToString()
        {
            return string.Format("{0}.", this.Name);
        }
    }

    public class OWMPrecipitation
    {
        [XmlAttribute("mode")]
        public string Mode { get; set; }
    }

    public class OWMWeather
    {
        [XmlAttribute("number")]
        public string Number { get; set; }
        [XmlAttribute("value")]
        public string Value { get; set; }
        [XmlAttribute("icon")]
        public string Icon { get; set; }

        public override string ToString()
        {
            return string.Format("{0}.", Value);
        }
    }

    public class OWMLastUpdate
    {
        [XmlAttribute("value")]
        public string Value { get; set; }
    }

    public class OWMCoordinary
    {
        [XmlAttribute("lon")]
        public string Longtitude { get; set; }
        [XmlAttribute("lat")]
        public string Latitude { get; set; }
    }
}
