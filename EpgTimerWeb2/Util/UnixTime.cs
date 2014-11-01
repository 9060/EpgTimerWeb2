﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpgTimer
{
    public class UnixTime
    {
        public readonly static DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
        public static long ToUnixTime(DateTime dateTime)
        {
            //dateTime = dateTime.ToUniversalTime();
            return (long)dateTime.Subtract(UnixEpoch).TotalSeconds;
        }

        public static DateTime FromUnixTime(long unixTime)
        {
            return UnixEpoch.AddSeconds(unixTime).ToLocalTime();
        }
    }
}