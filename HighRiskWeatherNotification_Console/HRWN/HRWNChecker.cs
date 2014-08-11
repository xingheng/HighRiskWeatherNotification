using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using UTILITIES_HAN;

namespace HighRiskWeatherNotification
{
    class HRWNChecker
    {
        public HRWNReceiverMan ReceiverMan { get; set; }

        // PM25.in
        public PM25RequestKind RequestKind { get; set; }

        #region Historial feition post records
        const int capacity = 10;
        HRWNHistoryComparator<AQIRecord> aqiRecordHistoryList = new HRWNHistoryComparator<AQIRecord>(capacity);
        HRWNHistoryComparator<OWMCurWeather> curWeatherHistoryList = new HRWNHistoryComparator<OWMCurWeather>(capacity);
        HRWNHistoryComparator<OWMForecastItem> forecastHistoryList = new HRWNHistoryComparator<OWMForecastItem>(capacity);
        #endregion

        static int gCheckerCount = 0;
        static int gCheckerIndex = 0;

        public HRWNChecker()
        {
            gCheckerCount++;
            this.RequestKind = PM25RequestKind.AllMonitorAQI;

            InitTimer();
            aqiRecordHistoryList.ExpireTime = TimeSpan.FromHours(4);
        }

        public HRWNChecker(HRWNReceiverMan man)
            :this()
        {
            this.ReceiverMan = man;
        }

        ~HRWNChecker()
        {
            gCheckerIndex = gCheckerIndex % gCheckerCount;
            gCheckerCount--;
            gCheckerIndex = gCheckerIndex % gCheckerCount;
        }

        #region Timer
        Timer timer = new Timer();

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="interval">Minute(s)</param>
        private void InitTimer(int interval = 20)
        {
#if DEBUG
            timer.Interval = 5 * 60 * 1000;
#else
            timer.Interval = interval * 1000 * 60/* * 60 * 24*/;
#endif
            timer.Elapsed += timer_Elapsed;
        }

        public void Start()
        {
            int sleepTime = 5 * 1000 * 60 * (gCheckerIndex++ % gCheckerCount);
#if DEBUG
            sleepTime = 3 * 1000;
#endif
            System.Threading.Thread.Sleep(sleepTime); // To separate the different users' console log in global output.
            
            timer.Enabled = true;
        }

        /// <summary>
        /// Stop
        /// </summary>
        /// <param name="elapsedTime">Minute(s)</param>
        public void Stop(double elapsedTime = double.MaxValue)
        {
            timer.Enabled = false;
            if (elapsedTime != double.MaxValue)
            {
                CheckerLogWrite(string.Format("Now: {0}. Sleeping time begins, will elapse {1} minute(s)...", 
                    DateTime.Now.ToLongTimeString(), elapsedTime), true);
                System.Threading.Thread.Sleep(TimeSpan.FromMinutes(elapsedTime));
                CheckerLogWrite(string.Format("Now: {0}. Sleeping time ends, elapsed {1} minute(s)...", 
                    DateTime.Now.ToLongTimeString(), elapsedTime), true);
                this.Start();
            }
        }

        public void StartOnce()
        {
            // Start once for debugging? descrease the global checker count.
            gCheckerCount--;
            timer_Elapsed(null, null);
        }

        private void CheckSleepingTime()
        {
            const double hourStart = 0, hourEnd = 7.5;
            DateTime now = DateTime.Now;

            if (now.Hour >= hourStart && now.Hour < hourEnd)
            {
                DateTime nextValidTime = DateTime.Today.AddHours(hourEnd);
                TimeSpan tspan = nextValidTime - now;
                if (tspan.Ticks > 0)
                    this.Stop(tspan.TotalMinutes);
            }
        }

        private static void IsANewDay()
        {
            LogFile curLogFile = ConsoleMe.LogFileInstance;
            ConsoleMe.LogFileInstance.StartNewInstance();
            LogFile newLogFile = ConsoleMe.LogFileInstance;
            if (!curLogFile.Equals(newLogFile))
            {
                ConsoleMe.WriteLine("\n--------------------- Switch to a new day! ---------------------\n");
            }
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CheckSleepingTime();
            IsANewDay();

            CheckerLogWrite(string.Format("Checker: begin time: {0}", DateTime.Now.ToLongTimeString()));

            this.CheckCurrentWeatherInfo();
            this.CheckNextHourForecastInfo();
            this.CheckSpecialStationAQI();

            CheckerLogWrite(string.Format("Checker: end time: {0}\n\n", DateTime.Now.ToLongTimeString()));
            ConsoleMe.WriteToLogFile();
        }
        #endregion

        CheckerResult CheckSpecialStationAQI(int averageValue = int.MaxValue)
        {
            FeitionPoster poster = new FeitionPoster(this.ReceiverMan.FeitionAccount);
            List<AQIRecord> res = null;
            try
            {
                PM25WebRequest<AQIRecord> req = new PM25WebRequest<AQIRecord>(this.RequestKind, this.ReceiverMan.Location.PM25.CityName);
                res = req.GetResponse<AQIRecord>();
            }
            catch (Exception ex)
            {
                CheckerLogWrite(string.Format("CheckNextHourForecastInfo: {0}", ex.ToString()));
            }

            if (res == null)
            {
                CheckerLogWrite("PM25WebRequest:GetResponse returns null.");
                return CheckerResult.NetworkIssue;
            }

            var result = (from c in res
                          where c.Position_Name == this.ReceiverMan.Location.PM25.StationName
                          select c).ToList();

            AQIRecord aqiRecord;

            if (result != null && result.Count() > 0)
            {
                aqiRecord = result[0];

                if (this.ReceiverMan.Location.PM25.IsEnabled == 0)
                {
                    CheckerLogWrite("Disabled the AQI switcher.");
                    // Assume that the record is existing here.
                    return CheckerResult.Existing;
                }

                bool forceSend = false;
                if (averageValue != int.MaxValue)
                {
                    // Temporarily diabled. Should we really define another threshold value for AQI AGAIN?
                    //forceSend = Convert.ToInt32(aqiRecord.AQI) > averageValue;
                }

                if (forceSend || HRWNRiskDefinition.Instance.IsRiskAQI(aqiRecord))
                {
                    string str = aqiRecord.GetReadableText();
                    poster.AppendMessage(str);

                    bool fExist = aqiRecordHistoryList.Exists(aqiRecord);
                    if (fExist)
                    {
                        //ConsoleMe.WriteLine("Existing aqi: {0}.", aqiRecord.ToString());
                        CheckerLogWrite("Existing aqi.");
                        return CheckerResult.Existing;
                    }

                    bool status = poster.Send();
                    if (status)
                    {
                        aqiRecordHistoryList.Add(aqiRecord);
                    }
                    poster.ResetMessage();
                    return CheckerResult.Sent;
                }
                else
                {
                    CheckerLogWrite("AQIRecord: isn't a risk.");
                    return CheckerResult.IsNotRisk;
                }
            }
            else
                return CheckerResult.DataIssue;
        }

        CheckerResult CheckCurrentWeatherInfo()
        {
            FeitionPoster poster = new FeitionPoster(this.ReceiverMan.FeitionAccount);
            OWMCurWeather record = null;
            try
            {
                OWMWebRequest<OWMCurWeather> req = new OWMWebRequest<OWMCurWeather>(
                    OWMPublic.OWMRequest(OWMRequestKind.CurrentWeather),
                    this.ReceiverMan.Location.OWM.CityID,
                    this.ReceiverMan.Location.OWM.CityName);
                req.Coordinary = this.ReceiverMan.Location.OWM.Coordinary;
                req.Language = "zh_cn";
                record = req.GetResponse();
            }
            catch (Exception ex)
            {
                CheckerLogWrite(string.Format("CheckCurrentWeatherInfo: {0}", ex.ToString()));
            }

            if (record == null)
            {
                CheckerLogWrite("OWMWebRequest<OWMCurWeather>:GetResponse returns null.");
                return CheckerResult.NetworkIssue;
            }

            if (HRWNRiskDefinition.Instance.IsRiskWeather(record))
            {
                // Maybe the fog weather is fake if there is.
                CheckerResult cResult = CheckChineseSpecialFogWeather(record);
                switch (cResult)
                {
                    case CheckerResult.Sent:
                    case CheckerResult.Existing:
                    case CheckerResult.IsNotRisk:
                        {
                            CheckerLogWrite("Current Weather: This is a chinese fake fog, report AQI successfully instead.");
                            // and we just regard it as a good check process is done.
                            return CheckerResult.Sent;
                        }
                    case CheckerResult.NetworkIssue:
                    case CheckerResult.DataIssue:
                        {
                            CheckerLogWrite("Current Weather: Not sure whether this is a chinese fake fog as it failed to get AQI data.");
                            // and we just regard it as a good check process is done.
                            return CheckerResult.Sent;
                        }
                }

                string str = record.GetReadableText(this.ReceiverMan.Location.OWM.CityNickName);
                poster.AppendMessage(str);

                bool fExist = curWeatherHistoryList.Exists(record);
                if (fExist)
                {
                    //ConsoleMe.WriteLine("Existing current weather: {0}.", record.ToString());
                    CheckerLogWrite("Existing current weather.");
                    return CheckerResult.Existing;
                }

                bool status = poster.Send();
                if (!fExist && status)
                {
                    curWeatherHistoryList.Add(record);
                }
                poster.ResetMessage();
                return CheckerResult.Sent;
            }
            else
            {
                CheckerLogWrite("OWMCurWeather: isn't a risk.");
                return CheckerResult.IsNotRisk;
            }
        }

        CheckerResult CheckNextHourForecastInfo()
        {
            FeitionPoster poster = new FeitionPoster(this.ReceiverMan.FeitionAccount);
            OWMForecast record = null;
            try
            {
                OWMWebRequest<OWMForecast> req = new OWMWebRequest<OWMForecast>(
                    OWMPublic.OWMRequest(OWMRequestKind.HourlyForecast), 
                    this.ReceiverMan.Location.OWM.CityID,
                    this.ReceiverMan.Location.OWM.CityName);
                req.Coordinary = this.ReceiverMan.Location.OWM.Coordinary;
                req.Language = "zh_cn";
                record = req.GetResponse();
            }
            catch (Exception ex)
            {
                CheckerLogWrite(string.Format("CheckNextHourForecastInfo: {0}", ex.ToString()));
                return CheckerResult.NetworkIssue;
            }

            if (record == null)
            {
                CheckerLogWrite("OWMWebRequest<OWMForecast>:GetResponse returns null.");
                return CheckerResult.NetworkIssue;
            }

            if (record.Forecasts.TimeArray.Count() > 0)
            {
                var res = (from c in record.Forecasts.TimeArray
                           orderby c.FromDate
                           select c).ToList();

                if (res != null && res.Count > 0)
                {
                    OWMForecastItem forecastItem = res[0];
                    if (HRWNRiskDefinition.Instance.IsRiskWeather(forecastItem))
                    {
                        if (IsChineseFog(forecastItem))
                        {
                            CheckerLogWrite("Forecast: Maybe this is a chinese fake fog, let's omit it for foreast.");
                            return CheckerResult.DataIssue;
                        }

                        poster.AppendMessage(forecastItem.ToString());

                        bool fExist = forecastHistoryList.Exists(forecastItem);
                        if (fExist)
                        {
                            //ConsoleMe.WriteLine("Existing forecast: {0}.", forecastItem.ToString());
                            CheckerLogWrite("Existing forecast.");
                            return CheckerResult.Existing;
                        }

                        bool status = poster.Send();
                        if (!fExist && status)
                        {
                            forecastHistoryList.Add(forecastItem);
                        }
                        poster.ResetMessage();
                        return CheckerResult.Sent;
                    }
                    else
                    {
                        CheckerLogWrite("OWMForecastItem: isn't a risk.");
                        return CheckerResult.IsNotRisk;
                    }
                }
                else
                    return CheckerResult.DataIssue;
            }
            else
            {
                CheckerLogWrite("No data returned for OWMForecastItem.");
                return CheckerResult.DataIssue;
            }
        }

        bool IsChineseFog(OWMForecastItem obj)
        {
            return obj.Symbol.Var.Contains("雾");
        }

        CheckerResult CheckChineseSpecialFogWeather(OWMCurWeather obj)
        {
            if (!obj.Weather.Value.Contains("雾"))
                return CheckerResult.Init;

            const int FogThreadholdVal = 140;
            return CheckSpecialStationAQI(FogThreadholdVal);
        }

        void CheckerLogWrite(string value, bool fWrite = false)
        {
            ConsoleMe.WriteLine(string.Format("Receiver: {0}\t{1}", this.ReceiverMan.NickName, value));
            if (fWrite)
            {
                ConsoleMe.WriteToLogFile();
            }
        }

        enum CheckerResult
        {
            Init,
            NetworkIssue,
            DataIssue,
            IsNotRisk,
            Existing,
            Sent
        }
    }
}
