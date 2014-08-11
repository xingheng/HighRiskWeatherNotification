
//#define USE_ASYNC

using System;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace UTILITIES_HAN
{
    class WebStreamService
    {
        private static bool CheckStream(Stream stream)
        {
            if (stream.CanRead && stream.CanSeek && stream.Length > 0)
                return true;
            else
            {
                //MsgResult.WriteLine("The stream could not be seek." + stream.ToString());
                return false;
            }
        }

        public static string GetResponseStringFromURL(string strRequest)
        {
            string result = "";

            StreamReader resultStream = WebStreamService.GetStreamReaderFromURL(strRequest);
            if (resultStream != null)
            {
                result = resultStream.ReadToEnd();
                resultStream.Close();
            }

            return result;
        }

        public static StreamReader GetStreamReaderFromURL(string strRequest)
        {
            StreamReader resultStream = null;
            Stream rStream = GetStreamFromURL(strRequest);
            if (rStream != null)
            {
                resultStream = new StreamReader(rStream);
            }
            return resultStream;
        }

        public static Stream GetStreamFromURL(string strRequest)
        {
            Stream responseStream = null;
            try
            {
                if (!String.IsNullOrEmpty(strRequest))
                {
                    HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(strRequest);
                    myRequest.Method = "GET";
                    myRequest.Timeout = 30000;
                    myRequest.KeepAlive = false;
#if USE_ASYNC
                    HttpWebResponse myResponse = (HttpWebResponse)(await myRequest.GetResponseAsync());
#else
                    HttpWebResponse myResponse = (HttpWebResponse)(myRequest.GetResponse());
#endif

                    responseStream = myResponse.GetResponseStream();
                    //myResponse.Close();
                }
            }
            catch (Exception ex)
            {
                ConsoleMe.WriteLine(ex.ToString());
            }

            return responseStream;
        }
    }

}
