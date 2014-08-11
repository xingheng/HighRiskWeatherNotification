#if DEBUG
//#define         FeitionPoster   // In debug mode, comment/uncomment this line to open/close the switcher.
#else
#define         FeitionPoster
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UTILITIES_HAN;

namespace HighRiskWeatherNotification
{
    class FeitionPoster
    {
        public string Receiver { get; set; }
        public string Message { get; private set; }
        public string AppSuffixer { get; set; }
        private const int MaxLength = 255;

        public FeitionPoster(string receiver)
        {
            this.Receiver = receiver;
            this.Message = "";
            this.AppSuffixer = FeitionPublic.AppSuffix;
        }

        public override string ToString()
        {
            return string.Format("Receiver: '{0}'\nMessage: '{1}'.", this.Receiver, this.Message);
        }

        public void AppendMessage(string msg)
        {
            this.Message += msg;
            this.Message += "\n";
        }

        public void ResetMessage()
        {
            this.Message = string.Empty;
        }

        bool AppendSuffix()
        {
            this.Message = this.Message.Trim();
            if (string.IsNullOrEmpty(this.Message))
                return false;

            int originalLen = this.Message.Length;
            const string placeHolder = "....";

            if (originalLen > MaxLength - AppSuffixer.Length)
            {
                this.Message = this.Message.Substring(0, MaxLength - placeHolder.Length - AppSuffixer.Length) + 
                    placeHolder + this.AppSuffixer;
            }
            else if (originalLen > MaxLength - AppSuffixer.Length - placeHolder.Length)
            {
                this.Message = this.Message.Substring(0, MaxLength - AppSuffixer.Length) + this.AppSuffixer;
            }
            else
            {
                this.Message = this.Message + this.AppSuffixer;
            }
            return true;
        }

        public bool Send()
        {
            // Before sending message, clean up the message body.
            if (!AppendSuffix())
            {
                ConsoleMe.WriteLine("The message body is empty.");
                return false;
            }

#if FeitionPoster
            int FailedTimes = 0;
            const int MaxFailedTimes = 5;

        LStart:
            FeitionWebRequest request = new FeitionWebRequest(
                FeitionPublic.UserName,
                FeitionPublic.Password,
                this.Receiver,
                this.Message);

            string strOut;
            bool status = request.Send(out strOut);
            ConsoleMe.WriteLine("Feition Poster: Send time: " + DateTime.Now.ToLongTimeString());
            ConsoleMe.WriteLine("Result: " + strOut);

            if (!status && FailedTimes < MaxFailedTimes)
            {
                FailedTimes++;
                ConsoleMe.WriteLine(string.Format("Failed to send message '{0}', failed times: {1}", this.Message, FailedTimes));
                ConsoleMe.WriteLine(string.Format("Try to resend to receiver {0}....", this.Receiver));
                goto LStart;
            }
#else
            ConsoleMe.WriteLine("\n(Use ConsoleMe instead of Feition Poster).");
#endif
            ConsoleMe.WriteLine(this.ToString() + "\n");

            return true;
        }
    }
}
