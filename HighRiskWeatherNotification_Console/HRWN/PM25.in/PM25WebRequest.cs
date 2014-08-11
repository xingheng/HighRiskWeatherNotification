using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UTILITIES_HAN;
using Newtonsoft.Json;

// Link: http://www.pm25.in/api_doc

namespace HighRiskWeatherNotification
{
    class PM25WebRequest<T>
    {
        public string RequestPrefix;
        public string CityName;     // city：城市名称
        public bool Average = true; // avg：是否返回一个城市所有监测点数据均值的标识，可选参数，默认是true，不需要均值时传这个参数并设置为false
        public bool Station = true; // stations：是否只返回一个城市均值的标识，可选参数，默认是yes，不需要监测点信息时传这个参数并设置为no
        public string StationCode;

        public PM25WebRequest()
        {
        }

        public PM25WebRequest(PM25RequestKind requestKind, string cityName = "", string stationCode = "", bool station = true, bool avg = true) 
            : this (PM25Public.PM25Request(requestKind), cityName, stationCode, station, avg)
        {
        }

        public PM25WebRequest(string requestPrefix, string cityName = "", string stationCode = "", bool station = true, bool avg = true)
        {
            this.RequestPrefix = requestPrefix;
            this.CityName = cityName;
            this.StationCode = stationCode;
            this.Station = station;
            this.Average = avg;
        }

        private string GetRequestURL()
        {
            string request = RequestPrefix;

            if (!string.IsNullOrEmpty(CityName))
                request += "&city=" + CityName;
            if (!string.IsNullOrEmpty(StationCode))
                request += "&station_code=" + StationCode;
            if (!Average)
                request += "&avg=false";
            if (!Station)
                request += "&stations=no";

            return request;
        }

        public List<T> GetResponse<T>()
        {
            string request = GetRequestURL();
            string res = WebStreamService.GetResponseStringFromURL(request);

            List<T> obj = JsonConvert.DeserializeObject<List<T>>(res);

            return obj;
        }

        public T GetResponse()
        {
            string request = GetRequestURL();
            string res = WebStreamService.GetResponseStringFromURL(request);

            T obj = JsonConvert.DeserializeObject<T>(res);

            return obj;
        }
    }
}
