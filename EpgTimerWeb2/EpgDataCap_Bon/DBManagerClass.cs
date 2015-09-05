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
using CtrlCmdCLI;
using CtrlCmdCLI.Def;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

namespace EpgTimer
{
    public class DBManager
    {
        private CtrlCmdUtil cmd = null;
        private bool updateEpgData = true;
        private bool updateReserveInfo = true;
        private bool updateRecInfo = true;
        private bool updateAutoAddEpgInfo = true;
        private bool updateAutoAddManualInfo = true;
        private bool updatePlugInFile = true;
        private bool noAutoReloadEpg = false;
        private bool oneTimeReloadEpg = false;

        public Dictionary<ulong, EpgServiceEventInfo> ServiceEventList { set; get; }
        public Dictionary<uint, ReserveData> ReserveList { set; get; }
        public Dictionary<uint, TunerReserveInfo> TunerReserveList { set; get; }
        public Dictionary<uint, RecFileInfo> RecFileInfo { set; get; }
        public Dictionary<int, string> WritePlugInList { set; get; }
        public Dictionary<int, string> RecNamePlugInList { set; get; }
        public Dictionary<uint, ManualAutoAddData> ManualAutoAddList { set; get; }
        public Dictionary<uint, EpgAutoAddData> EpgAutoAddList { set; get; }

        public DBManager(CtrlCmdUtil ctrlCmd)
        {
            cmd = ctrlCmd;
            ServiceEventList = new Dictionary<ulong, EpgServiceEventInfo>();
            ReserveList = new Dictionary<uint, ReserveData>();
            TunerReserveList = new Dictionary<uint, TunerReserveInfo>();
            RecFileInfo = new Dictionary<uint, RecFileInfo>();
            WritePlugInList = new Dictionary<int, string>();
            RecNamePlugInList = new Dictionary<int, string>();
            ManualAutoAddList = new Dictionary<uint, ManualAutoAddData>();
            EpgAutoAddList = new Dictionary<uint, EpgAutoAddData>();
        }

        public void ClearAllDB()
        {
            ServiceEventList.Clear();
            ReserveList.Clear();
            TunerReserveList.Clear();
            RecFileInfo.Clear();
            WritePlugInList.Clear();
            RecNamePlugInList.Clear();
            ManualAutoAddList.Clear();
            EpgAutoAddList.Clear();
        }

        /// <summary>
        /// EPGデータの自動取得を行うかどうか（NW用）
        /// </summary>
        /// <param name="noReload"></param>
        public void SetNoAutoReloadEPG(bool noReload)
        {
            noAutoReloadEpg = noReload;
        }

        public void ReloadAll(bool force = false)
        {
            updateEpgData = force;
            updateReserveInfo = force;
            updateRecInfo = force;
            updateAutoAddEpgInfo = force;
            updateAutoAddManualInfo = force;
            updatePlugInFile = force;
            this.ClearAllDB();
            this.ReloadEpgAutoAddInfo();
            this.ReloadEpgData();
            this.ReloadManualAutoAddInfo();
            this.ReloadPlugInFile();
            this.ReloadRecFileInfo();
            this.ReloadReserveInfo();
        }

        public void SetOneTimeReloadEpg()
        {
            oneTimeReloadEpg = true;
        }

        /// <summary>
        /// データの更新があったことを通知
        /// </summary>
        /// <param name="updateInfo">[IN]更新のあったデータのフラグ</param>
        public void SetUpdateNotify(uint updateInfo)
        {
            if (updateInfo == (uint)UpdateNotifyItem.EpgData)
            {
                updateEpgData = true;
            }
            if (updateInfo == (uint)UpdateNotifyItem.ReserveInfo)
            {
                updateReserveInfo = true;
            }
            if (updateInfo == (uint)UpdateNotifyItem.RecInfo)
            {
                updateRecInfo = true;
            }
            if (updateInfo == (uint)UpdateNotifyItem.AutoAddEpgInfo)
            {
                updateAutoAddEpgInfo = true;
            }
            if (updateInfo == (uint)UpdateNotifyItem.AutoAddManualInfo)
            {
                updateAutoAddManualInfo = true;
            }
            if (updateInfo == (uint)UpdateNotifyItem.PlugInFile)
            {
                updatePlugInFile = true;
            }
        }

        /// <summary>
        /// EPGデータの更新があれば再読み込みする
        /// </summary>
        /// <returns></returns>
        public ErrCode ReloadEpgData()
        {
            ErrCode ret = ErrCode.CMD_SUCCESS;
            try
            {
                if (noAutoReloadEpg == true)
                {
                    if (updateEpgData == true && oneTimeReloadEpg == true)
                    {
                        if (cmd == null)
                        {
                            ret = ErrCode.CMD_ERR;
                        }
                        else
                        {
                            ServiceEventList.Clear();
                            ServiceEventList = null;
                            ServiceEventList = new Dictionary<ulong, EpgServiceEventInfo>();
                            List<EpgServiceEventInfo> list = new List<EpgServiceEventInfo>();
                            ret = (ErrCode)cmd.SendEnumPgAll(ref list);
                            if (ret == ErrCode.CMD_SUCCESS)
                            {
                                foreach (EpgServiceEventInfo info in list)
                                {
                                    ulong id = CommonManager.Create64Key(
                                        info.serviceInfo.ONID,
                                        info.serviceInfo.TSID,
                                        info.serviceInfo.SID);
                                    ServiceEventList.Add(id, info);
                                }
                                updateEpgData = false;
                                oneTimeReloadEpg = false;
                            }
                            list.Clear();
                            list = null;
                        }
                    }
                }
                else
                {
                    if (updateEpgData == true)
                    {
                        if (cmd == null)
                        {
                            ret = ErrCode.CMD_ERR;
                        }
                        else
                        {
                            ServiceEventList.Clear();
                            ServiceEventList = null;
                            ServiceEventList = new Dictionary<ulong, EpgServiceEventInfo>();
                            List<EpgServiceEventInfo> list = new List<EpgServiceEventInfo>();
                            ret = (ErrCode)cmd.SendEnumPgAll(ref list);
                            if (ret == ErrCode.CMD_SUCCESS)
                            {
                                foreach (EpgServiceEventInfo info in list)
                                {
                                    ulong id = CommonManager.Create64Key(
                                        info.serviceInfo.ONID,
                                        info.serviceInfo.TSID,
                                        info.serviceInfo.SID);
                                    ServiceEventList.Add(id, info);
                                }
                                updateEpgData = false;
                            }
                            list.Clear();
                            list = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
            return ret;
        }

        /// <summary>
        /// 予約情報の更新があれば再読み込みする
        /// </summary>
        /// <returns></returns>
        public ErrCode ReloadReserveInfo()
        {
            ErrCode ret = ErrCode.CMD_SUCCESS;
            try
            {
                if (updateReserveInfo == true)
                {
                    if (cmd == null)
                    {
                        ret = ErrCode.CMD_ERR;
                    }
                    else
                    {
                        ReserveList.Clear();
                        ReserveList = null;
                        ReserveList = new Dictionary<uint, ReserveData>();
                        TunerReserveList.Clear();
                        TunerReserveList = null;
                        TunerReserveList = new Dictionary<uint, TunerReserveInfo>();
                        List<ReserveData> list = new List<ReserveData>();
                        List<TunerReserveInfo> list2 = new List<TunerReserveInfo>();
                        ret = (ErrCode)cmd.SendEnumReserve(ref list);
                        if (ret == ErrCode.CMD_SUCCESS)
                        {
                            ret = (ErrCode)cmd.SendEnumTunerReserve(ref list2);
                            if (ret == ErrCode.CMD_SUCCESS)
                            {
                                foreach (ReserveData info in list)
                                {
                                    ReserveList.Add(info.ReserveID, info);
                                }
                                foreach (TunerReserveInfo info in list2)
                                {
                                    TunerReserveList.Add(info.tunerID, info);
                                }
                                updateReserveInfo = false;
                            }
                        }
                        list.Clear();
                        list2.Clear();
                        list = null;
                        list2 = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
            return ret;
        }

        /// <summary>
        /// 録画済み情報の更新があれば再読み込みする
        /// </summary>
        /// <returns></returns>
        public ErrCode ReloadRecFileInfo()
        {
            ErrCode ret = ErrCode.CMD_SUCCESS;
            try
            {
                if (updateRecInfo == true)
                {
                    if (cmd == null)
                    {
                        ret = ErrCode.CMD_ERR;
                    }
                    else
                    {
                        RecFileInfo.Clear();
                        RecFileInfo = null;
                        RecFileInfo = new Dictionary<uint, CtrlCmdCLI.Def.RecFileInfo>();
                        List<RecFileInfo> list = new List<RecFileInfo>();
                        ret = (ErrCode)cmd.SendEnumRecInfo(ref list);
                        if (ret == ErrCode.CMD_SUCCESS)
                        {
                            foreach (RecFileInfo info in list)
                            {
                                RecFileInfo.Add(info.ID, info);
                            }
                            updateRecInfo = false;
                        }
                        list.Clear();
                        list = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
            return ret;
        }

        /// <summary>
        /// PlugInFileの再読み込み指定があればする
        /// </summary>
        /// <returns></returns>
        public ErrCode ReloadPlugInFile()
        {
            ErrCode ret = ErrCode.CMD_SUCCESS;
            try
            {
                if (updatePlugInFile == true)
                {
                    if (cmd == null)
                    {
                        ret = ErrCode.CMD_ERR;
                    }
                    else
                    {
                        WritePlugInList.Clear();
                        WritePlugInList = null;
                        WritePlugInList = new Dictionary<int, string>();
                        RecNamePlugInList.Clear();
                        RecNamePlugInList = null;
                        RecNamePlugInList = new Dictionary<int, string>();
                        List<string> writeList = new List<string>();
                        List<string> recNameList = new List<string>();
                        ret = (ErrCode)cmd.SendEnumPlugIn(2, ref writeList);
                        if (ret == ErrCode.CMD_SUCCESS)
                        {
                            ret = (ErrCode)cmd.SendEnumPlugIn(1, ref recNameList);
                            if (ret == ErrCode.CMD_SUCCESS)
                            {
                                foreach (string info in writeList)
                                {
                                    WritePlugInList.Add(WritePlugInList.Count, info);
                                }
                                foreach (string info in recNameList)
                                {
                                    RecNamePlugInList.Add(RecNamePlugInList.Count, info);
                                }
                                updatePlugInFile = false;
                            }
                        }
                        writeList.Clear();
                        recNameList.Clear();
                        writeList = null;
                        recNameList = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
            return ret;
        }

        /// <summary>
        /// EPG自動予約登録情報の更新があれば再読み込みする
        /// </summary>
        /// <returns></returns>
        public ErrCode ReloadEpgAutoAddInfo()
        {
            ErrCode ret = ErrCode.CMD_SUCCESS;
            try
            {
                if (updateAutoAddEpgInfo == true)
                {
                    if (cmd == null)
                    {
                        ret = ErrCode.CMD_ERR;
                    }
                    else
                    {
                        EpgAutoAddList.Clear();
                        EpgAutoAddList = null;
                        EpgAutoAddList = new Dictionary<uint, EpgAutoAddData>();
                        List<EpgAutoAddData> list = new List<EpgAutoAddData>();
                        ret = (ErrCode)cmd.SendEnumEpgAutoAdd(ref list);
                        if (ret == ErrCode.CMD_SUCCESS)
                        {
                            foreach (EpgAutoAddData info in list)
                            {
                                EpgAutoAddList.Add(info.dataID, info);
                            }
                            updateAutoAddEpgInfo = false;
                        }
                        list.Clear();
                        list = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
            return ret;
        }


        /// <summary>
        /// 自動予約登録情報の更新があれば再読み込みする
        /// </summary>
        /// <returns></returns>
        public ErrCode ReloadManualAutoAddInfo()
        {
            ErrCode ret = ErrCode.CMD_SUCCESS;
            try
            {
                if (updateAutoAddManualInfo == true)
                {
                    if (cmd == null)
                    {
                        ret = ErrCode.CMD_ERR;
                    }
                    else
                    {
                        ManualAutoAddList.Clear();
                        ManualAutoAddList = null;
                        ManualAutoAddList = new Dictionary<uint, ManualAutoAddData>();
                        List<ManualAutoAddData> list = new List<ManualAutoAddData>();
                        ret = (ErrCode)cmd.SendEnumManualAdd(ref list);
                        if (ret == ErrCode.CMD_SUCCESS)
                        {
                            foreach (ManualAutoAddData info in list)
                            {
                                ManualAutoAddList.Add(info.dataID, info);
                            }
                            updateAutoAddManualInfo = false;
                        }
                        list.Clear();
                        list = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
            return ret;
        }

        public ReserveData GetNextReserve()
        {
            return ReserveList.Values.First(s => s.RecSetting.RecMode != 5 && s.StartTime > DateTime.Now);
        }
    }
}
