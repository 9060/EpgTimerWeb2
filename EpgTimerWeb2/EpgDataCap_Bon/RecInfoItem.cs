using CtrlCmdCLI;
using CtrlCmdCLI.Def;
using System;
using System.Collections.Generic;

namespace EpgTimer
{
    
    public class RecInfoItem
    {
        public RecInfoItem(RecFileInfo item)
        {
            this.RecInfo = item;
        }
        
        private RecFileInfo RecInfo
        {
            get;
            set;
        }

        public uint ID
        {
            get { return RecInfo.ID; }
        }
        public string ProgramInfo
        {
            get
            {
                return RecInfo.ProgramInfo;
            }
        }

        public string ErrInfo
        {
            get
            {
                return RecInfo.ErrInfo;
            }
        }
        public ushort ONID
        {
            get { return RecInfo.OriginalNetworkID;  }
        }
        public ushort TSID
        {
            get { return RecInfo.TransportStreamID; }
        }
        public ushort SID
        {
            get { return RecInfo.ServiceID; }
        }
        public ushort EventID
        {
            get { return RecInfo.EventID; }
        }
        public ulong Key
        {
            get
            {
                return CommonManager.Create64PgKey(ONID, TSID, SID, EventID);
            }
        }
        public bool IsProtect
        {
            set
            {
                if (RecInfo != null)
                {
                    RecInfo.ProtectFlag = value;
                    List<RecFileInfo> list = new List<RecFileInfo>();
                    list.Add(RecInfo);
                    CtrlCmdUtil cmd = CommonManager.Instance.CtrlCmd;
                    cmd.SendChgProtectRecInfo(list);
                }
            }
            get
            {
                bool chk = false;
                if (RecInfo != null)
                {
                    chk = RecInfo.ProtectFlag;
                }
                return chk;
            }
        }

        public string EventName
        {
            get
            {
                if (RecInfo != null)
                    return RecInfo.Title;
                return "";
            }
        }

        public string ServiceName
        {
            get
            {
                if (RecInfo != null)
                    return RecInfo.ServiceName;
                return "";
            }
        }
        public uint DurationSecond
        {
            get { return RecInfo.DurationSecond;  }
        }
        public DateTime StartTime
        {
            get
            {
                if (RecInfo == null)
                    return DateTime.MinValue;
                return RecInfo.StartTime;
            }
        }

        public DateTime StartTimeEpg
        {
            get
            {
                if (RecInfo == null)
                    return DateTime.MinValue;
                return RecInfo.StartTimeEpg;
            }
        }

        public ulong Drops
        {
            get
            {
                if (RecInfo != null)
                    return RecInfo.Drops;
                return 0;
            }
        }

        public ulong Scrambles
        {
            get
            {
                if (RecInfo != null)
                    return RecInfo.Scrambles;
                return 0;
            }
        }

        public string Result
        {
            get
            {
                if (RecInfo != null)
                    return RecInfo.Comment;
                return "";
            }
        }
        
        public string NetworkName
        {
            get
            {
                if (RecInfo != null)
                {
                    return CommonManager.GetNetworkName(RecInfo.OriginalNetworkID);
                }
                return "";
            }
        }

        public string RecFilePath
        {
            get
            {
                if (RecInfo != null)
                    return RecInfo.RecFilePath;
                return "";
            }
        }
    }
}
