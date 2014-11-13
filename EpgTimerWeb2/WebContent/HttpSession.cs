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
    public class PasswordPair
    {
        public PasswordPair(string User, string Pass)
        {
            UserId = User;
            Password = Pass;
        }
        public string UserId { set; get; }
        public string Password { set; get; }
        public static List<PasswordPair> LoadFile(string Path)
        {
            if (!File.Exists(Path)) return null;
            List<PasswordPair> Ret = new List<PasswordPair>();
            using (StreamReader sr = new StreamReader(Path, Encoding.UTF8))
            {
                string Line = "";
                while ((Line = sr.ReadLine()) != null)
                {
                    if (Line.IndexOf("/") > 0) return null;
                    if (Line.IndexOf(":") < 0) return null;
                    string UserId = Line.Substring(0, Line.IndexOf(":"));
                    string Password = Line.Substring(Line.IndexOf(":") + 1).Replace("\r", "").Replace("\n", "");
                    Ret.Add(new PasswordPair(UserId, Password));
                }
            }
            return Ret;
        }
    }
    public class HttpSession
    {
        public static bool IsMatch(string SessionKey, string IpAddr)
        {
            if (PrivateSetting.Instance.Passwords == null) return true;
            foreach (HttpSession Tmp in PrivateSetting.Instance.Sessions)
            {
                if (Tmp.CheckAuth(SessionKey, IpAddr)) return true;
            }
            return false;
        }
        public static HttpSession Search(string SessionKey, string IpAddr)
        {
            if (PrivateSetting.Instance.Passwords == null) return null;
            foreach (HttpSession Tmp in PrivateSetting.Instance.Sessions)
            {
                if (Tmp.CheckAuth(SessionKey, IpAddr)) return Tmp;
            }
            return null;
        }
        private bool _IsAuth = false;
        private string _SessionKey = "";
        private string _SessionKey2 = "";
        public HttpSession(string UserId, string Password, string IpAddr)
        {
            if (PrivateSetting.Instance.Passwords.Count == 0) _IsAuth = true;
            foreach (PasswordPair Pair in PrivateSetting.Instance.Passwords)
            {
                if (Pair.Password == Password && Pair.UserId == UserId) _IsAuth = true;
            }
            if (_IsAuth)
            {
                _SessionKey2 = BitConverter.ToString((new SHA512CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(Password + UserId + DateTime.Now.ToString())))).Replace("-", "");
                _SessionKey = BitConverter.ToString((new SHA1CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(_SessionKey2 + IpAddr)))).Replace("-", "");
                PrivateSetting.Instance.Sessions.Add(this);
            }
        }
        public bool CheckAuth(string SessionKey, string IpAddr)
        {
            if (_IsAuth && BitConverter.ToString(new SHA1CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(SessionKey + IpAddr))).Replace("-", "") == _SessionKey) return true;
            return false;
        }
        public string SessionKey { get { return _SessionKey2; } }
        private long _time = 0;
        public long Time { get { return _time; } }
    }
}
