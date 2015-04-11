using CtrlCmdCLI.Def;
using System;
using System.Collections.Generic;

namespace EpgTimer
{
    public class ReserveItem
    {
        public ReserveItem()
        {
        }

        public ReserveItem(ReserveData item)
        {
            this.ReserveInfo = item;
        }
        
        public uint ID
        {
            get
            {
                return ReserveInfo.ReserveID;
            }
        }

        private ReserveData ReserveInfo
        {
            get;
            set;
        }
        public string EventName
        {
            get
            {
                if (ReserveInfo != null)
                    return ReserveInfo.Title;
                return "";
            }
        }

        
        public string ServiceName
        {
            get
            {
                if (ReserveInfo != null)
                    return ReserveInfo.StationName;
                return "";
            }
        }

        public ushort ONID
        {
            get { return ReserveInfo.OriginalNetworkID; }
        }
        public ushort TSID
        {
            get { return ReserveInfo.TransportStreamID; }
        }
        public ushort SID
        {
            get { return ReserveInfo.ServiceID; }
        }
        public ushort EventID
        {
            get { return ReserveInfo.EventID; }
        }
        public ulong Key
        {
            get
            {
                return CommonManager.Create64PgKey(ONID, TSID, SID, EventID);
            }
        }
        public string NetworkName
        {
            get
            {
                if (ReserveInfo != null)
                    return CommonManager.GetNetworkName(ReserveInfo.OriginalNetworkID);
                return "";
            }
        }

        public uint DurationSecond
        {
            get { return ReserveInfo.DurationSecond;  }
        }
        
        public long StartTime
        {
            get
            {
                return UnixTime.ToUnixTime(ReserveInfo.StartTime);
            }
        }

        public long StartTimeEpg
        {
            get
            {
                return UnixTime.ToUnixTime(ReserveInfo.StartTimeEpg);
            }
        }
        
        public string Comment
        {
            get
            {
                if (ReserveInfo != null)
                {
                    return ReserveInfo.Comment;
                }
                return "";
            }
        }

        
        public string[] RecFileName
        {
            get
            {
                if (ReserveInfo != null)
                {
                    return ReserveInfo.RecFileNameList.ToArray();
                }
                return null;
            }
        }
        public RecSettingData Setting
        {
            get { return ReserveInfo.RecSetting; }
        }
    }
}
