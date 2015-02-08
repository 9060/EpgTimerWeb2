using EpgTimer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
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
            Sessions = new List<HttpSession>();
            CmdConnect = new CtrlCmdConnect();
            Server = null;
            SetupMode = false;
            ConfigPath = "EpgTimerWeb2.xml";
            SetupCode = "";
        }
        public List<HttpSession> Sessions { set; get; }
        public CtrlCmdConnect CmdConnect { get; set; }
        public WebServer Server { get; set; }
        public bool SetupMode { set; get; }
        public string ConfigPath { set; get; }
        public string SetupCode { set; get; }
    }
    [DataContract]
    public class ContentColorItem
    {
        [DataMember]
        public uint ContentLevel1 { set; get; }
        [DataMember]
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
    [DataContract]
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
            LoginUser = "";
            LoginPassword = "";
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
            MaxUploadSize = 1024 * 1024 * 3;
        }
        [DataMember]
        public string CtrlHost { get; set; }
        [DataMember]
        public UInt32 CtrlPort { get; set; }
        [DataMember]
        public UInt32 CallbackPort { get; set; }
        [DataMember]
        public UInt32 HttpPort { get; set; }
        [DataMember]
        public bool LocalMode { get; set; }
        [DataMember]
        public string LoginUser { set; get; }
        [DataMember]
        public string LoginPassword { set; get; }
        [DataMember]
        public List<ContentColorItem> ContentToColorTable;
        [DataMember]
        public long MaxUploadSize { set; get; }
        public bool ReqAuth
        {
            get
            {
                return Setting.Instance.LoginPassword != "";
            }
        }

        public static void SaveToXmlFile(string path)
        {
            try
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(Setting));
                XmlWriter writer = XmlWriter.Create(path, new XmlWriterSettings() { Encoding = Encoding.UTF8 });
                serializer.WriteObject(writer, Instance);
                writer.Close();
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
                if (!File.Exists(path)) return;
                DataContractSerializer serializer = new DataContractSerializer(typeof(Setting));
                XmlReader reader = XmlReader.Create(path);
                Instance = (Setting)serializer.ReadObject(reader);
                reader.Close();
            }
            catch (Exception ex)
            {
            }
        }
    }
}
