using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UTILITIES_HAN
{
    public class DateTimeUtil
    {
        public static void TestEntry()
        {
            var a = DateTimeFormatString();



            int ia = 0;
        }

        public static string DateTimeFormatString(DateTime dTime = default(DateTime))
        {
            DateTime dt = dTime == default(DateTime) ? DateTime.Now : dTime;

            string res = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                dt.Year.ToString().PadLeft(4, '0'),
                dt.Month.ToString().PadLeft(2,'0'),
                dt.Day.ToString().PadLeft(2, '0'),
                dt.Hour.ToString().PadLeft(2, '0'),
                dt.Minute.ToString().PadLeft(2, '0'),
                dt.Second.ToString().PadLeft(2, '0'),
                dt.Millisecond.ToString().PadLeft(3,'0')
                );

            return res;
        }
    }
}
