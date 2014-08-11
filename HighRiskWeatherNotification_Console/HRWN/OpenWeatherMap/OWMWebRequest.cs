using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UTILITIES_HAN;
using System.Web;
using System.Xml.Serialization;
using System.IO;

namespace HighRiskWeatherNotification
{
    class OWMWebRequest<T> where T : class, IOWMEntity
    {
        public string RequestPrefix;

        public string CityID { get; set; }
        public string CityName { get; set; }
        public OWMCoordinary Coordinary { get; set; }

        public string Mode { get; private set; }
        public string Language { get; set; }

        public OWMWebRequest()
        {
            this.Mode = "xml";
            this.Language = "en_us";
        }

        public OWMWebRequest(string request, string cityID = "", string cityName = "")
            : this()
        {
            this.RequestPrefix = request;
            this.CityID = cityID;
            this.CityName = cityName;
        }

        public OWMWebRequest(string request, double longtitude, double latitude)
            : this()
        {
            this.RequestPrefix = request;
            OWMCoordinary coor = new OWMCoordinary();
            coor.Longtitude = longtitude.ToString();
            coor.Latitude = latitude.ToString();
            this.Coordinary = coor;
        }

        private string GetRequestURL()
        {
            string result = "";

            if (this.Coordinary != null && !string.IsNullOrEmpty(this.Coordinary.Longtitude) && !string.IsNullOrEmpty(this.Coordinary.Latitude))
            {
                result = string.Format("{0}&lat={1}&lon={2}", this.RequestPrefix, this.Coordinary.Latitude, this.Coordinary.Longtitude);
            }
            else if (!string.IsNullOrEmpty(this.CityID))
            {
                result = string.Format("{0}&id={1}", this.RequestPrefix, this.CityID);
            }
            else if (!string.IsNullOrEmpty(this.CityName))
            {
                result = string.Format("{0}&q={1}", this.RequestPrefix, this.CityName);
            }

            result += string.Format("&mode={0}", this.Mode);
            result += string.Format("&lang={0}", this.Language);

            return result;
        }

        public T GetResponse()
        {
            T record = default(T);

            string request = GetRequestURL();

            using (Stream stream = WebStreamService.GetStreamFromURL(request))
            {
                if (stream != null)
                {
                    var serializer = new XmlSerializer(typeof(T));

                    object resultObj;
                    resultObj = serializer.Deserialize(stream);
                    if (resultObj != null)
                    {
                        record = resultObj as T;
                    }
                }
            }

            return record;
        }

        /*
        /// <summary>
        /// The caller should release the returned stream object, recommand using block for the stream.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        */
    }
}
