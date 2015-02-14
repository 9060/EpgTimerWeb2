using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpgTimer
{
    public class Cookie
    {
        private static string GetExpireHeader(DateTime Time)
        {
            return Time.ToString("R");
        }

    }
}
