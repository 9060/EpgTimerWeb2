using EpgTimer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpgTimerWeb2
{
    public class PrivateSetting
    {
        private static PrivateSetting _instance;
        public static PrivateSetting Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PrivateSetting();
                return _instance;
            }
            set { _instance = value; }
        }
        public PrivateSetting()
        {
            Passwords = null;
            Sessions = new List<HttpSession>();
            AuthFilePath = "";
        }
        public List<PasswordPair> Passwords { set; get; }
        public List<HttpSession> Sessions { set; get; }
        public string AuthFilePath { set; get; }
    }
    public class Setting
    {
        private static Setting _instance;
        public static Setting Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Setting();
                return _instance;
            }
            set { _instance = value; }
        }
        public Setting()
        {
            CtrlHost = "127.0.0.1";
            CtrlPort = 4510;
            CallbackPort = 4521;
            HttpPort = 8080;
            LocalMode = false;
            CmdConnect = null;
            Server = null;
        }
        public string CtrlHost { get; set; }
        public UInt32 CtrlPort { get; set; }
        public UInt32 CallbackPort { get; set; }
        public UInt32 HttpPort { get; set; }
        public bool LocalMode { get; set; }
        public CtrlCmdConnect CmdConnect { get; set; }
        public WebServer Server { get; set; }
        public bool ReqAuth
        {
            get
            {
                return PrivateSetting.Instance.Passwords != null;
            }
        }
    }
}
