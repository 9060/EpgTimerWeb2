using EpgTimer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

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
    public class ContentColorItem
    {
        [XmlAttribute("id")]
        public uint ContentLevel1 { set; get; }
        [XmlAttribute("color")]
        public string Color { set; get; }
        public ContentColorItem()
        {
            ContentLevel1 = 15;
            Color = "#f0f0f0";
        }
        public ContentColorItem(uint Content, string color)
        {
            Color = color;
            ContentLevel1 = Content;
        }
    }
    [XmlRoot("setting")]
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
            ContentToColorTable = new List<ContentColorItem>(){
                new ContentColorItem(0, "#ffffe0"),
                new ContentColorItem(1, "#e0e0ff"),
                new ContentColorItem(2, "#ffe0f0"),
                new ContentColorItem(3, "#ffe0e0"),
                new ContentColorItem(4, "#e0ffe0"),
                new ContentColorItem(5, "#e0ffff"),
                new ContentColorItem(6, "#fff0e0"),
                new ContentColorItem(7, "#ffe0ff"),
                new ContentColorItem(8, "#ffffe0"),
                new ContentColorItem(9, "#fff0e0"),
                new ContentColorItem(10, "#e0f0ff"),
                new ContentColorItem(11, "#e0f0ff"),
                new ContentColorItem(15, "#f0f0f0")
            };
        }
        [XmlElement("host")]
        public string CtrlHost { get; set; }
        [XmlAttribute("port")]
        public UInt32 CtrlPort { get; set; }
        [XmlAttribute("callback")]
        public UInt32 CallbackPort { get; set; }
        [XmlAttribute("http")]
        public UInt32 HttpPort { get; set; }
        [XmlAttribute("local")]
        public bool LocalMode { get; set; }
        [XmlElement("auth")]
        public string AuthFilePath { set; get; }
        public List<ContentColorItem> ContentToColorTable { set; get; }
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
                
                if (File.Exists(path) == true)
                {
                    string backPath = path + ".back";
                    File.Copy(path, backPath, true);
                    File.Delete(path);
                }

                FileStream fs = new FileStream(path,
                    FileMode.Create,
                    FileAccess.Write, FileShare.None);
                XmlSerializer xs = new XmlSerializer(typeof(Setting));
                //シリアル化して書き込む
                xs.Serialize(fs, Instance);
                fs.Close();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
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
