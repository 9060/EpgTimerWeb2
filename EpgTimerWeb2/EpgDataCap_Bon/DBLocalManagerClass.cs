using CtrlCmdCLI.Def;
using System;
using System.Collections.Generic;
using System.IO;

namespace EpgTimer
{
    //ローカルモードでのみ使う。録画ファイルDL用
    public class DBLocalManager
    {
        private Dictionary<uint, RecFileInfo> _existRecFileInfo;
        private Dictionary<uint, RecFileInfo> _noExistRecFileInfo;

        public Dictionary<uint, RecFileInfo> ExistRecFileInfo { set; get; }
        public Dictionary<uint, RecFileInfo> NoExistRecFileInfo { set; get; }
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
            foreach (KeyValuePair<uint, RecFileInfo> Info in CommonManager.Instance.DB.RecFileInfo)
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
