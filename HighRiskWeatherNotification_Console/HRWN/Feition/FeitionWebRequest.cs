using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using UTILITIES_HAN;

namespace HighRiskWeatherNotification
{
    class FeitionWebRequest
    {
        // Open API: http://quanapi.sinaapp.com/fetion.php?u=飞信登录手机号&p=飞信登录密码&to=接收飞信的手机号&m=飞信内容

        // SAE News: sinaapp.com is inaccessible for domain name provider, use vipsinaapp.com instead.
        //const string RequestPrefix = "http://quanapi.sinaapp.com/fetion.php?";
        const string RequestPrefix = "http://quanapi.vipsinaapp.com/fetion.php?";

        public string UserName { get; set; }
        public string Password { get; set; }
        public string Receiver { get; set; }
        public string Message { get; set; }


        public FeitionWebRequest()
        {
        }

        public FeitionWebRequest(string user, string password, string receiver, string message)
        {
            this.UserName = user;
            this.Password = password;
            this.Receiver = receiver;
            this.Message = message;
        }

        private string GetRequestURL()
        {
            string result = "";

            if (!string.IsNullOrEmpty(this.UserName) && !string.IsNullOrEmpty(this.Password) &&
                !string.IsNullOrEmpty(this.Receiver) && !string.IsNullOrEmpty(this.Message))
            {
                result = string.Format("{0}u={1}&p={2}&to={3}&m={4}", RequestPrefix, this.UserName, this.Password, this.Receiver, this.Message);
            }

            return result;
        }

        public bool Send(out string strOut)
        {
            bool result = false;

            string strRequest = GetRequestURL();
            if (!string.IsNullOrEmpty(strRequest))
            {
                string res = null;
                FeitionReturnCode obj = null;
                try
                {
                    res = WebStreamService.GetResponseStringFromURL(strRequest);
                    obj = JsonConvert.DeserializeObject<FeitionReturnCode>(res);
                }
                catch
                {
                    ConsoleMe.WriteLine("FeitionWebRequest: send request failed.");
                }

                if (obj != null)
                {
                    strOut = obj.ToString();
                    if (obj.Result.Trim() == "0")
                    {
                        result = true;
                    }
                    else
                    {
                        strOut += string.Format(" Result code: {0}", obj.Result);
                        ConsoleMe.WriteLine();
                    }
                }
                else
                {
                    // Deserialized failed? check the string result directly.
                    if (res.Contains("{\"result\":0"))
                        result = true;

                    strOut = res;
                }
            }
            else
                strOut = "Invalid Feition request.";

            return result;
        }

    }

    // {"result":-2,"message":"\u53d1\u9001\u5931\u8d25"}
    // {"result":-1,"message":"\u63a5\u6536\u65b9\u53f7\u7801\u9519\u8bef"}
    // {"result":0,"message":"\u53d1\u9001\u6210\u529f"}
    [DataContract]
    public class FeitionReturnCode
    {
        [DataMember]
        public string Result { get; set; }
        [DataMember]
        public string Message { get; set; }

        public override string ToString()
        {
            return this.Message.ToString();
        }
    }
}
