using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace EpgTimer
{
    public class HttpSession
    {
        public static bool IsMatch(string SessionKey, string IpAddr)
        {
            Optimize();
            return Search(SessionKey, IpAddr) != null;
        }
        public static HttpSession Search(string SessionKey, string IpAddr)
        {
            Optimize();
            foreach (HttpSession Tmp in PrivateSetting.Instance.Sessions)
            {
                if (Tmp.CheckAuth(SessionKey, IpAddr)) return Tmp;
            }
            return null;
        }
        public static void Optimize()
        {
            foreach (HttpSession Tmp in PrivateSetting.Instance.Sessions.Where(s => s.IsExpired()))
            {
                Debug.Print("Expire: {0}", Tmp.SessionKey);
            }
            PrivateSetting.Instance.Sessions = PrivateSetting.Instance.Sessions.Where(s => !s.IsExpired()).ToList();
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
        public void Logout()
        {
            PrivateSetting.Instance.Sessions.Remove(this);
        }
        public string SessionKey { get { return _SessionKey2; } }
        public long LastTime { get; set; }
    }
}
