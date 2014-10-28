using CtrlCmdCLI.Def;
using EpgTimerWeb2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpgTimer
{
    //ローカルモードでのみ使う。録画ファイルDL用
    public class DBLocalManager
    {
        private bool updateExistRecFileInfo = true;
        private bool updateNoExistRecFileInfo = true;
        private Dictionary<UInt32, RecFileInfo> _existRecFileInfo;
        private Dictionary<UInt32, RecFileInfo> _noExistRecFileInfo;

        public Dictionary<UInt32, RecFileInfo> ExistRecFileInfo { set; get; }
        public Dictionary<UInt32, RecFileInfo> NoExistRecFileInfo { set; get; }
        public DBLocalManager()
        {
            ClearAllDB();
        }
        public void ClearAllDB()
        {
            _existRecFileInfo = new Dictionary<uint, RecFileInfo>();
            _noExistRecFileInfo = new Dictionary<uint, RecFileInfo>();
        }
        public void ReloadRecFileInfo()
        {
            if (!Setting.Instance.LocalMode) return;
            CommonManager.Instance.DB.ReloadrecFileInfo();
            ClearAllDB();
            foreach (KeyValuePair<UInt32, RecFileInfo> Info in CommonManager.Instance.DB.RecFileInfo)
            {
                if (File.Exists(Info.Value.RecFilePath))
                {
                    _existRecFileInfo.Add(Info.Key, Info.Value);
                }
                else
                {
                    _noExistRecFileInfo.Add(Info.Key, Info.Value);
                }
            }
        }
    }
}
