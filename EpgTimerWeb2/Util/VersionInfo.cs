using System.Windows.Forms;

namespace EpgTimer
{
    public class VersionInfo
    {
        private static VersionInfo _instance;
        public static VersionInfo Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new VersionInfo();
                return _instance;
            }
            set { _instance = value; }
        }
        public string AppName
        {
            get { return Application.ProductName; }
        }
        public string AppVersion
        {
            get { return Application.ProductVersion; }
        }
        public string Message
        {
            get { return "Hello,world"; }
        }
    }
}
