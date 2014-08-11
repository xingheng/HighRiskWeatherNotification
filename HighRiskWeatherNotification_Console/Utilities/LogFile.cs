using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UTILITIES_HAN
{
    /// <summary>
    /// --------------------------------- PLEASE READ ---------------------------------
    /// DO NOT create or initialize any instances of this class directly!
    /// 
    /// We have a Single Instance in class ConsoleMe, you should omit this class completely for your all common usages.
    /// </summary>
    public class LogFile
    {
        private static LogFile instance;
        public static LogFile Instance
        {
            get
            {
                if (instance == null)
                    instance = new LogFile();
                return instance;
            }
        }

        private string FileID = "";
        public string Message { get; private set; }

        const int MaxMsgLength = 512;

        private int _MsgLength;
        private int MsgLength 
        {
            get
            {
                return _MsgLength;
            }
            set
            {
                _MsgLength = value;
                if (_MsgLength > MaxMsgLength)
                {
                    if (!this.Write())
                    {
                        this.AppendLog(string.Format("Failed to write log to file {0}.", this.FileID));
                        this.Write(true);
                    }
                }
            }
        }

        private LogFile(string suffixName = "")
        {
            this.FileID = string.Format("{0}{1}", DateTimeUtil.DateTimeFormatString(DateTime.Today).Substring(0, 8),
                string.IsNullOrEmpty(suffixName) ? "" : "_" + suffixName.Trim());
        }

        public void StartNewInstance(string suffixName = "")
        {
            if (instance != null && !string.IsNullOrEmpty(instance.Message.Trim()))
            {
                instance.Write(true);
            }
            instance = new LogFile(suffixName);
        }

        public override bool Equals(object obj)
        {
            LogFile newObj = obj as LogFile;
            if (newObj != null)
            {
                return newObj.FileID == this.FileID;
            }
            return base.Equals(obj);
        }

        public void AppendLog(string log)
        {
            // Maybe there are some '\r\n' units in log string, let's do a clean up completely.
            // then replace all the '\n' units to LFCR.
            log = log.Replace("\r\n", "\n");
            log = log.Replace("\n", "\r\n");

            this.Message += log;
            this.MsgLength += log.Length;
        }

        static object locker = new object();

        public bool Write(bool fForceWrite = false)
        {
            lock (locker)
            {
                bool result = false;

                string folderName = "Logs";
                DirectoryInfo dirInfo = new DirectoryInfo(folderName);
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }

                string fileName = string.Format("{0}\\{1}.txt", dirInfo.FullName, this.FileID);

                if (fForceWrite)
                {
                    try
                    {
                        File.AppendAllText(fileName, this.Message);
                        result = true;
                    }
                    finally
                    {
                        if (result)
                            this.Message = string.Empty;
                    }
                }
                else if (this.Message.Length > MaxMsgLength)
                {
                    string trimMessage = this.Message.Substring(0, MaxMsgLength);
                    try
                    {
                        File.AppendAllText(fileName, trimMessage);
                        result = true;
                    }
                    finally
                    {
                        if (result)
                        {
                            int newLen = this.Message.Length - MaxMsgLength;
                            if (newLen > 0)
                            {
                                this.Message = this.Message.Substring(MaxMsgLength, newLen);
                            }
                        }
                    }
                }
                else
                {
                    // No force write and the message body length is too 'short', let's do a fake operation here
                    // Assume the operation result is successful.
                    result = true;
                }

                return result;
            }
        }
    }
}
