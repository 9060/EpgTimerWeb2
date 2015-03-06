using System;
using System.Collections.Generic;
using System.Linq;

namespace EpgTimer
{
    public class Cookie : Dictionary<string, string>
    {
        private static string GetExpireHeader(DateTime Time)
        {
            return Time.ToString("R");
        }
        private static string[] SplitCookieKVPair(string Cookie)
        {
            return Cookie.Split(';').Select(s => Util.RemoveStartSpace(s)).ToArray();
        }
        public DateTime Expire { set; get; }
        public string Path { set; get; }
        public Cookie()
        {
            Expire = DateTime.Now.AddYears(1);
            Path = "/";
        }
        public static string Generate(Cookie cookie)
        {
            cookie["path"] = cookie.Path;
            cookie["expires"] = GetExpireHeader(cookie.Expire);
            string Cookie = "";
            foreach (var item in cookie)
            {
                string line = item.Key + "=" + item.Value;
                if (Cookie != "")
                    Cookie += "; ";
                Cookie += line;
            }
            return Cookie;
        }
        public static Cookie Parse(string Input)
        {
            Cookie cookie = new Cookie();
            foreach (string item in SplitCookieKVPair(Input))
            {
                if(item.IndexOf("=") < 0)continue;
                string name = item.Substring(0, item.IndexOf("="));
                string value = item.Substring(item.IndexOf("=") + 1);
                cookie[name] = value;
            }
            return cookie;
        }
    }
}
