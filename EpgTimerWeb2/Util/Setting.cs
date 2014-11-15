using EpgTimer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            CmdConnect = new CtrlCmdConnect();
            Server = null;
            SetupMode = false;
            ConfigPath = "EpgTimerWeb2.xml";
            SetupCode = "";
        }
        public List<PasswordPair> Passwords { set; get; }
        public List<HttpSession> Sessions { set; get; }
        public string AuthFilePath { get { return Setting.Instance.AuthFilePath; } }
        public CtrlCmdConnect CmdConnect { get; set; }
        public WebServer Server { get; set; }
        public bool SetupMode { set; get; }
        public string ConfigPath { set; get; }
        public string SetupCode { set; get; }
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
            AuthFilePath = "";
        }
        public string CtrlHost { get; set; }
        public UInt32 CtrlPort { get; set; }
        public UInt32 CallbackPort { get; set; }
        public UInt32 HttpPort { get; set; }
        public bool LocalMode { get; set; }
        public string AuthFilePath { set; get; }
        public bool ReqAuth
        {
            get
            {
                return PrivateSetting.Instance.Passwords != null;
            }
        }

        public static void SaveToXmlFile(string path)
        {
            try
            {
                if (System.IO.File.Exists(path) == true)
                {
                    string backPath = path + ".back";
                    System.IO.File.Copy(path, backPath, true);
                }

                FileStream fs = new FileStream(path,
                    FileMode.Create,
                    FileAccess.Write, FileShare.None);
                System.Xml.Serialization.XmlSerializer xs =
                    new System.Xml.Serialization.XmlSerializer(
                    typeof(Setting));
                //シリアル化して書き込む
                xs.Serialize(fs, Instance);
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }
        public static void LoadFromXmlFile(string path)
        {
            try
            {
                FileStream fs = new FileStream(path,
                    FileMode.Open,
                    FileAccess.Read);
                System.Xml.Serialization.XmlSerializer xs =
                    new System.Xml.Serialization.XmlSerializer(
                        typeof(Setting));
                //読み込んで逆シリアル化する
                object obj = xs.Deserialize(fs);
                fs.Close();
                Instance = (Setting)obj;
            }
            catch (Exception ex)
            {
            }
        }
    }
}
