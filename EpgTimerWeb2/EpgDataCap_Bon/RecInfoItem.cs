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
        public String ProgramInfo
        {
            get
            {
                return RecInfo.ProgramInfo;
            }
        }

        public String ErrInfo
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
        public string KeyS
        {
            get
            {
                return Key.ToString();
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
        
        public String EventName
        {
            get
            {
                String view = "";
                if (RecInfo != null)
                {
                    view = RecInfo.Title;
                }
                return view;
            }
        }
        
        public String ServiceName
        {
            get
            {
                String view = "";
                if (RecInfo != null)
                {
                    view = RecInfo.ServiceName;
                }
                return view;
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
                    return DateTime.Now;
                return RecInfo.StartTime;
            }
        }

        public DateTime StartTimeEpg
        {
            get
            {
                if (RecInfo == null)
                    return DateTime.Now;
                return RecInfo.StartTimeEpg;
            }
        }
        
        public String Drops
        {
            get
            {
                String view = "";
                if (RecInfo != null)
                {
                    view = RecInfo.Drops.ToString();
                }
                return view;
            }
        }
        
        public String Scrambles
        {
            get
            {
                String view = "";
                if (RecInfo != null)
                {
                    view = RecInfo.Scrambles.ToString();
                }
                return view;
            }
        }
        
        public String Result
        {
            get
            {
                String view = "";
                if (RecInfo != null)
                {
                    view = RecInfo.Comment;
                }
                return view;
            }
        }
        
        public String NetworkName
        {
            get
            {
                String view = "";
                if (RecInfo != null)
                {
                    if (0x7880 <= RecInfo.OriginalNetworkID && RecInfo.OriginalNetworkID <= 0x7FE8)
                    {
                        view = "Digital";
                    }
                    else if (RecInfo.OriginalNetworkID == 0x0004)
                    {
                        view = "BS";
                    }
                    else if (RecInfo.OriginalNetworkID == 0x0006)
                    {
                        view = "CS1";
                    }
                    else if (RecInfo.OriginalNetworkID == 0x0007)
                    {
                        view = "CS2";
                    }
                    else
                    {
                        view = "その他";
                    }

                }
                return view;
            }
        }
        
        public String RecFilePath
        {
            get
            {
                String view = "";
                if (RecInfo != null)
                {
                    view = RecInfo.RecFilePath;
                }
                return view;
            }
        }
        
        public String TextView
        {
            get
            {
                String view = "";
                if (RecInfo != null)
                {
                    view = RecInfo.StartTime.ToString("yyyy/MM/dd(ddd) HH:mm:ss ～ ");
                    DateTime endTime = RecInfo.StartTime + TimeSpan.FromSeconds(RecInfo.DurationSecond);
                    view += endTime.ToString("yyyy/MM/dd(ddd) HH:mm:ss") + "\r\n";

                    view += ServiceName;
                    if (0x7880 <= RecInfo.OriginalNetworkID && RecInfo.OriginalNetworkID <= 0x7FE8)
                    {
                        view += " (地デジ)";
                    }
                    else if (RecInfo.OriginalNetworkID == 0x0004)
                    {
                        view += " (BS)";
                    }
                    else if (RecInfo.OriginalNetworkID == 0x0006)
                    {
                        view += " (CS1)";
                    }
                    else if (RecInfo.OriginalNetworkID == 0x0007)
                    {
                        view += " (CS2)";
                    }
                    else
                    {
                        view += " (その他)";
                    }
                    view += "\r\n";
                    view += EventName + "\r\n";
                    view += "\r\n";
                    view += "録画結果 : " + RecInfo.Comment + "\r\n";
                    view += "録画ファイルパス : " + RecInfo.RecFilePath;
                }

                return view;
            }
        }
    }
}
