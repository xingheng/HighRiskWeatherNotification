using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighRiskWeatherNotification
{
    class CityStation
    {
        public string City { get; set; }
        public List<Station> Stations { get; set; }
    }

    class Station
    {
        public string Station_Name { get; set; }
        public string Station_Code { get; set; }
    }
}
