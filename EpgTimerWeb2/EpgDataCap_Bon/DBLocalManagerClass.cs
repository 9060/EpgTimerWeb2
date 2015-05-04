/*
 *  EpgTimerWeb2
 *  Copyright (C) 2015  YukiBoard 0X7hT.k8kU <yuki@yukiboard.tk>
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
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
            CommonManager.Instance.DB.ReloadRecFileInfo();
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
