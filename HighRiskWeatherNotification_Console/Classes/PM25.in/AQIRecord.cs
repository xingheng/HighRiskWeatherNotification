using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace HighRiskWeatherNotification
{

    // Link: http://www.pm25.in/api_doc

    interface IAQIRecord
    { 
    }

    [DataContract]
    public class AQIRecord : IAQIRecord
    {
        [DataMember]
        public string AQI { get; set; }
        [DataMember]
        public string Area { get; set; }
        [DataMember]
        public string Position_Name { get; set; }
        [DataMember]
        public string Primary_Pollutant { get; set; }
        [DataMember]
        public string Quality { get; set; }
        [DataMember]
        public string Station_Code { get; set; }
        [DataMember]
        public string Time_Point { get; set; }

        public override bool Equals(object obj)
        {
            AQIRecord aqiObj = obj as AQIRecord;
            if (aqiObj != null)
            {
                return this.AQIRoughlyEqual(aqiObj) && 
                    this.Area == aqiObj.Area && 
                    this.Position_Name == aqiObj.Position_Name;
            }
            return base.Equals(obj);
        }

        public bool AQIRoughlyEqual(AQIRecord newObj)
        {
            int thisAQI = 0, newAQI = 0;
            int.TryParse(this.AQI, out thisAQI);
            int.TryParse(newObj.AQI, out newAQI);

            int roughValue = 50;
            return (thisAQI >= (newAQI - roughValue)) && (thisAQI <= (newAQI + roughValue));
        }

        public override string ToString()
        {
            return string.Format("AQI: {0}. Area: {1}. Position name: {2}. Primary pollutant: {3}. Quality: {4}. Station code: {5}. Time point: {6}.",
                this.AQI, this.Area, this.Position_Name, this.Primary_Pollutant, this.Quality, this.Station_Code, this.Time_Point);
        }

        public string GetReadableText()
        {
            int aqi = -1;
            int.TryParse(this.AQI, out aqi);
            return AQITextConverter.Instance.GetReadableTextByAQI(aqi);
        }
    }

    [DataContract]
    public class AQIRecordDetail : AQIRecord
    {
        [DataMember]
        public string CO { get; set; }
        [DataMember]
        public string CO_24h { get; set; }
        [DataMember]
        public string NO2 { get; set; }
        [DataMember]
        public string NO2_24h { get; set; }
        [DataMember]
        public string O3 { get; set; }
        [DataMember]
        public string O3_24h { get; set; }
        [DataMember]
        public string O3_8h { get; set; }
        [DataMember]
        public string O3_8h_24h { get; set; }
        [DataMember]
        public string PM10 { get; set; }
        [DataMember]
        public string PM10_24h { get; set; }
        [DataMember]
        public string PM2_5 { get; set; }
        [DataMember]
        public string PM2_5_24h { get; set; }
        [DataMember]
        public string SO2 { get; set; }
        [DataMember]
        public string SO2_24h { get; set; }

        public override bool Equals(object obj)
        {
            AQIRecordDetail aqiObj = obj as AQIRecordDetail;
            if (aqiObj != null)
            {
                return this.Time_Point == aqiObj.Time_Point && this.AQI == aqiObj.AQI && 
                    this.Area == aqiObj.Area && this.Position_Name == aqiObj.Position_Name;
            }
            return base.Equals(obj);
        }

        public override string ToString()
        {
            string baseResult = base.ToString();
            return string.Format("{0} Details: CO/24h: {1}. NO2/24h: {2}. O3/24h: {3}. PM10/24h: {4}. PM2.5/24h: {5}. SO2/24h: {6}.",
               baseResult, this.CO_24h, this.NO2_24h, this.O3_24h, this.PM10_24h, this.PM2_5_24h, this.SO2_24h);
        }
    }
}
