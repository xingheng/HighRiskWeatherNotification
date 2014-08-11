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
    class HRWNReceiver
    {
        private static HRWNReceiver instance;
        public static HRWNReceiver Instance
        {
            get
            {
                if (instance == null)
                    instance = new HRWNReceiver();
                return instance;
            }
        }

        static HRWNReceiverConfig ReceiverConfig = null;
        static object locker = new object();

        private HRWNReceiver()
        {
            lock (locker)
            {
                if (ReceiverConfig == null)
                    LoadReceiverConfigFile();
            }
        }

        private bool LoadReceiverConfigFile()
        {
            bool result = false;

            string filename = "";
            string value = ConfigurationManager.AppSettings["HRWNReceiverConfig"];
            if (!string.IsNullOrEmpty(value) && File.Exists(value))
                filename = value;

            if (!string.IsNullOrEmpty(filename))
            {
                using (FileStream stream = new FileStream(filename, FileMode.Open))
                {
                    var serializer = new XmlSerializer(typeof(HRWNReceiverConfig));

                    object resultObj;
                    resultObj = serializer.Deserialize(stream);
                    if (resultObj != null)
                    {
                        ReceiverConfig = resultObj as HRWNReceiverConfig;
                        result = true;
                    }
                }
            }

            if (!result)
            {
                ConsoleMe.WriteLine("Failed to load HRWN receiver config file {0}.", value ?? "");
                ConsoleMe.ReadyToExit();
            }

            return result;
        }

        public List<HRWNReceiverMan> GetReceivers()
        {
            if (ReceiverConfig != null)
                return ReceiverConfig.Receivers;
            return null;
        }
    }

    [XmlRoot("HRWN")]
    public class HRWNReceiverConfig
    {
        [XmlElementAttribute("Receiver")]
        public List<HRWNReceiverMan> Receivers { get; set; }
    }

    public class HRWNReceiverMan
    {
        //[XmlElement("UserName")]
        public string UserName { get; set; }
        //[XmlElement("NickName")]
        public string NickName { get; set; }
        //[XmlElement("FeitionAccount")]
        public string FeitionAccount { get; set; }
        //[XmlElement("Location")]
        public HRWNReceiverLocation Location { get; set; }

        public void Format()
        {
            this.UserName = this.UserName.Trim();
            this.NickName = this.NickName.Trim();
            this.FeitionAccount = this.FeitionAccount.Trim();
            this.Location.Format();
        }

        public class HRWNReceiverLocation
        {
            //[XmlElement("PM25")]
            public HRWNReceiverLocationPM25 PM25 { get; set; }
            //[XmlElement("OWM")]
            public HRWNReceiverLocationOWM OWM { get; set; }

            public void Format()
            {
                this.PM25.Format();
                this.OWM.Format();
            }

            public class HRWNReceiverLocationPM25
            {
                [XmlAttribute]
                public int IsEnabled { get; set; }
                public string CityName { get; set; }
                public string StationName { get; set; }

                public void Format()
                {
                    this.CityName = this.CityName.Trim();
                    this.StationName = this.StationName.Trim();
                }
            }

            public class HRWNReceiverLocationOWM
            {
                public string CityID { get; set; }
                public string CityName { get; set; }
                public string CityNickName { get; set; }
                public OWMCoordinary Coordinary { get; set; }

                public void Format()
                {
                    this.CityID = this.CityID.Trim();
                    this.CityName = this.CityName.Trim();
                    this.CityNickName = this.CityNickName.Trim();
                }
            }
        }
    }
}
