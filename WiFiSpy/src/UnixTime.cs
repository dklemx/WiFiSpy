using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFiSpy.src
{
    public class UnixTime
    {
        private static DateTime dateTime = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime GetTimeFromMsec(long time)
        {
            return dateTime.AddMilliseconds(time);
        }
    }
}