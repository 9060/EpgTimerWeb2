using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EpgTimerWeb2
{
    public class HttpSession
    {
        public static bool IsMatch(string SessionKey, string IpAddr)
        {
            foreach (HttpSession Tmp in PrivateSetting.Instance.Sessions)
            {
                if (Tmp.CheckAuth(SessionKey, IpAddr)) return true;
            }
            return false;
        }
        public static HttpSession Search(string SessionKey, string IpAddr)
        {
            foreach (HttpSession Tmp in PrivateSetting.Instance.Sessions)
            {
                if (Tmp.CheckAuth(SessionKey, IpAddr)) return Tmp;
            }
            foreach (HttpSession Tmp in PrivateSetting.Instance.Sessions.Where(s => s.IsExpired()))
            {
                Debug.Print("Expire: {0}", Tmp.SessionKey);
            }
            PrivateSetting.Instance.Sessions = PrivateSetting.Instance.Sessions.Where(s => !s.IsExpired()).ToList();
            return null;
        }
        private bool _IsAuth = false;
        private string _SessionKey = "";
        private string _SessionKey2 = "";
        public HttpSession(string UserId, string Password, string IpAddr)
        {
            if (Setting.Instance.LoginPassword == Password &&
                Setting.Instance.LoginUser == UserId) _IsAuth = true;
            if (_IsAuth)
            {
                LastTime = UnixTime.ToUnixTime(DateTime.Now);
                _SessionKey2 = BitConverter.ToString((new SHA1CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(Password + UserId + DateTime.Now.ToString())))).Replace("-", "");
                _SessionKey = BitConverter.ToString((new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(_SessionKey2 + IpAddr)))).Replace("-", "");
                PrivateSetting.Instance.Sessions.Add(this);
            }
        }
        public bool CheckAuth(string SessionKey, string IpAddr)
        {
            if (!IsExpired() && _IsAuth && 
                BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(SessionKey + IpAddr))).Replace("-", "") == _SessionKey)
            {
                LastTime = UnixTime.ToUnixTime(DateTime.Now);
                return true;
            }
            return false;
        }
        private bool IsExpired()
        {
            return LastTime < UnixTime.ToUnixTime(DateTime.Now) - Setting.Instance.SessionExpireSecond;
        }
        public string SessionKey { get { return _SessionKey2; } }
        public long LastTime { get; set; }
    }
}
