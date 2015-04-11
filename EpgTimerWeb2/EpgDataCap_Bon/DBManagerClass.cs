using CtrlCmdCLI;
using CtrlCmdCLI.Def;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

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

        private Dictionary<ulong, EpgServiceEventInfo> serviceEventList = null;
        private Dictionary<uint, ReserveData> reserveList = null;
        private Dictionary<uint, TunerReserveInfo> tunerReserveList = null;
        private Dictionary<uint, RecFileInfo> recFileInfo = null;
        private Dictionary<int, string> writePlugInList = null;
        private Dictionary<int, string> recNamePlugInList = null;
        private Dictionary<uint, ManualAutoAddData> manualAutoAddList = null;
        private Dictionary<uint, EpgAutoAddData> epgAutoAddList = null;

        public Dictionary<ulong, EpgServiceEventInfo> ServiceEventList
        {
            get { return serviceEventList; }
        }
        public Dictionary<uint, ReserveData> ReserveList
        {
            get { return reserveList; }
        }
        public Dictionary<uint, TunerReserveInfo> TunerReserveList
        {
            get { return tunerReserveList; }
        }
        public Dictionary<uint, RecFileInfo> RecFileInfo
        {
            get { return recFileInfo; }
        }
        public Dictionary<int, string> WritePlugInList
        {
            get { return writePlugInList; }
        }
        public Dictionary<int, string> RecNamePlugInList
        {
            get { return recNamePlugInList; }
        }
        public Dictionary<uint, ManualAutoAddData> ManualAutoAddList
        {
            get { return manualAutoAddList; }
        }
        public Dictionary<uint, EpgAutoAddData> EpgAutoAddList
        {
            get { return epgAutoAddList; }
        }

        public DBManager(CtrlCmdUtil ctrlCmd)
        {
            cmd = ctrlCmd;
            serviceEventList = new Dictionary<ulong, EpgServiceEventInfo>();
            reserveList = new Dictionary<uint, ReserveData>();
            tunerReserveList = new Dictionary<uint, TunerReserveInfo>();
            recFileInfo = new Dictionary<uint, RecFileInfo>();
            writePlugInList = new Dictionary<int, string>();
            recNamePlugInList = new Dictionary<int, string>();
            manualAutoAddList = new Dictionary<uint, ManualAutoAddData>();
            epgAutoAddList = new Dictionary<uint, EpgAutoAddData>();
        }

        public void ClearAllDB()
        {
            serviceEventList.Clear();
            reserveList.Clear();
            tunerReserveList.Clear();
            recFileInfo.Clear();
            writePlugInList.Clear();
            recNamePlugInList.Clear();
            manualAutoAddList.Clear();
            epgAutoAddList.Clear();
        }

        /// <summary>
        /// EPGデータの自動取得を行うかどうか（NW用）
        /// </summary>
        /// <param name="noReload"></param>
        public void SetNoAutoReloadEPG(bool noReload)
        {
            noAutoReloadEpg = noReload;
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
                            serviceEventList.Clear();
                            serviceEventList = null;
                            serviceEventList = new Dictionary<ulong, EpgServiceEventInfo>();
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
                                    serviceEventList.Add(id, info);
                                }
                                updateEpgData = false;
                                oneTimeReloadEpg = false;
                            }
                            list.Clear();
                            list = null;
                            ContentCache.Instance.ClearAll();
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
                            serviceEventList.Clear();
                            serviceEventList = null;
                            serviceEventList = new Dictionary<ulong,EpgServiceEventInfo>();
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
                                    serviceEventList.Add(id, info);
                                }
                                updateEpgData = false;
                            }
                            list.Clear();
                            list = null;
                            ContentCache.Instance.ClearAll();
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
                        reserveList.Clear();
                        reserveList = null;
                        reserveList = new Dictionary<uint, ReserveData>();
                        tunerReserveList.Clear();
                        tunerReserveList = null;
                        tunerReserveList = new Dictionary<uint, TunerReserveInfo>();
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
                                    reserveList.Add(info.ReserveID, info);
                                }
                                foreach (TunerReserveInfo info in list2)
                                {
                                    tunerReserveList.Add(info.tunerID, info);
                                }
                                updateReserveInfo = false;
                            }
                        }
                        list.Clear();
                        list2.Clear();
                        list = null;
                        list2 = null;
                        ContentCache.Instance.ClearAll();
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
        public ErrCode ReloadrecFileInfo()
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
                        recFileInfo.Clear();
                        recFileInfo = null;
                        recFileInfo = new Dictionary<uint, CtrlCmdCLI.Def.RecFileInfo>();
                        List<RecFileInfo> list = new List<RecFileInfo>();
                        ret = (ErrCode)cmd.SendEnumRecInfo(ref list);
                        if (ret == ErrCode.CMD_SUCCESS)
                        {
                            foreach (RecFileInfo info in list)
                            {
                                recFileInfo.Add(info.ID, info);
                            }
                            updateRecInfo = false;
                        }
                        list.Clear();
                        list = null;
                        ContentCache.Instance.ClearAll();
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
                        writePlugInList.Clear();
                        writePlugInList = null;
                        writePlugInList = new Dictionary<int, string>();
                        recNamePlugInList.Clear();
                        recNamePlugInList = null;
                        recNamePlugInList = new Dictionary<int, string>();
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
                                    writePlugInList.Add(writePlugInList.Count, info);
                                }
                                foreach (string info in recNameList)
                                {
                                    recNamePlugInList.Add(recNamePlugInList.Count, info);
                                }
                                updatePlugInFile = false;
                            }
                        }
                        writeList.Clear();
                        recNameList.Clear();
                        writeList = null;
                        recNameList = null;
                        ContentCache.Instance.ClearAll();
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
                        epgAutoAddList.Clear();
                        epgAutoAddList = null;
                        epgAutoAddList = new Dictionary<uint, EpgAutoAddData>();
                        List<EpgAutoAddData> list = new List<EpgAutoAddData>();
                        ret = (ErrCode)cmd.SendEnumEpgAutoAdd(ref list);
                        if (ret == ErrCode.CMD_SUCCESS)
                        {
                            foreach (EpgAutoAddData info in list)
                            {
                                epgAutoAddList.Add(info.dataID, info);
                            }
                            updateAutoAddEpgInfo = false;
                        }
                        list.Clear();
                        list = null;
                        ContentCache.Instance.ClearAll();
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
                        manualAutoAddList.Clear();
                        manualAutoAddList = null;
                        manualAutoAddList = new Dictionary<uint, ManualAutoAddData>();
                        List<ManualAutoAddData> list = new List<ManualAutoAddData>();
                        ret = (ErrCode)cmd.SendEnumManualAdd(ref list);
                        if (ret == ErrCode.CMD_SUCCESS)
                        {
                            foreach (ManualAutoAddData info in list)
                            {
                                manualAutoAddList.Add(info.dataID, info);
                            }
                            updateAutoAddManualInfo = false;
                        }
                        list.Clear();
                        list = null;
                        ContentCache.Instance.ClearAll();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
            return ret;
        }

        public bool GetNextReserve(ref ReserveData info)
        {
            bool ret = false;
            
            SortedList<string, ReserveData> sortList = new SortedList<string, ReserveData>();
            foreach (ReserveData resInfo in reserveList.Values)
            {
                if (resInfo.RecSetting.RecMode != 5)
                {
                    string key = resInfo.StartTime.ToString("yyyyMMddHHmmss");
                    key += resInfo.ReserveID.ToString("X8");
                    sortList.Add(key, resInfo);
                }
            }

            foreach (ReserveData resInfo in sortList.Values)
            {
                if (resInfo.StartTime > DateTime.Now)
                {
                    info = resInfo;
                    ret = true;
                    break;
                }
            }
            sortList.Clear();
            sortList = null;
            return ret;
        }
    }
}
