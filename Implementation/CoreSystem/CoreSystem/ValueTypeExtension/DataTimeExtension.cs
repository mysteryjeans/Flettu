using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreSystem.ValueTypeExtension
{
    public static class DataTimeExtension
    {
        private static readonly DateTime UnixRefereceDataTime = (new DateTime(1970, 1, 1, 0, 0, 0, 0)).ToUniversalTime();

        public static long GetUnixTime(this DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - UnixRefereceDataTime).TotalSeconds;
        }
    }
}
