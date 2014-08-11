using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UTILITIES_HAN;

namespace HighRiskWeatherNotification
{
    class HRWNHistoryComparator<T> where T : class
    {
        int Capacity = 0;
        Dictionary<DateTime,T> Container = null;

        public TimeSpan ExpireTime { get; set; }

        public HRWNHistoryComparator(int capacity)
        {
            this.Capacity = capacity;
            this.ExpireTime = TimeSpan.FromMinutes(180);     // By default, two hours is the expire time!
            this.Container = new Dictionary<DateTime, T>(this.Capacity);
        }

        public void Add(T obj)
        {
            if (this.Container.Count == this.Capacity)
            {
                var list = this.Container.Keys.OrderBy(a => a).ToList();
                DateTime first = list[0];
                this.Container.Remove(first);
            }

            DateTime dtime = DateTime.Now;
            do
            {
                if (this.Container.Keys.Contains(dtime))
                    dtime = dtime.AddMilliseconds(1);
                else
                    break;
            } while (true);

            this.Container.Add(dtime, obj);
        }

        //public bool Exists(T obj)
        //{
        //    return this.Container.ContainsValue(obj);
        //}

        public bool Exists(T obj)
        {
            var existing = (from c in this.Container
                            where c.Value.Equals(obj)
                                && DateTime.Now.Subtract(c.Key) < this.ExpireTime
                            select c).ToList();
#if DEBUG
            ConsoleMe.WriteLine("------------------------DEBUG LOG----------------------------");
            ConsoleMe.WriteLine("T is " + obj.ToString());
            ConsoleMe.WriteLine("There are {0} objects in array: ", this.Container.Count);
            foreach (var item in this.Container)
            {
                ConsoleMe.WriteLine(string.Format("Key: {0}, value: {1}",
                    item.Key.ToLongTimeString(), item.Value.ToString()));
            }
            ConsoleMe.WriteLine("------------------------END LOG----------------------------");
#endif
            if (existing != null && existing.Count > 0)
                return true;

            return false;
        }

        public override string ToString()
        {
            string result = "";
            foreach (var item in this.Container)
            {
                result += string.Format("DateTime: {0}, Value: {1}.", item.Key.ToLongTimeString(), item.Value.ToString());
                result += "\n";
            }
            return result;
        }
    }
}
