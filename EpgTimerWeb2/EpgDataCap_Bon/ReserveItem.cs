using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;


using CtrlCmdCLI;
using CtrlCmdCLI.Def;

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

        public String TextView
        {
            get
            {
                return CommonManager.Instance.ConvertReserveText(ReserveInfo);
            }
        }

        public String EventName
        {
            get
            {
                String view = "";
                if (ReserveInfo != null)
                {
                    view = ReserveInfo.Title;
                }
                return view;
            }
        }

        
        public String ServiceName
        {
            get
            {
                String view = "";
                if (ReserveInfo != null)
                {
                    view = ReserveInfo.StationName;
                }
                return view;
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
        public string KeyS
        {
            get
            {
                return Key.ToString();
            }
        }
        public ulong Key
        {
            get
            {
                return CommonManager.Create64PgKey(ONID, TSID, SID, EventID);
            }
        }
        public String NetworkName
        {
            get
            {
                String view = "";
                if (ReserveInfo != null)
                {
                    if (0x7880 <= ReserveInfo.OriginalNetworkID && ReserveInfo.OriginalNetworkID <= 0x7FE8)
                    {
                        view = "Digital";
                    }
                    else if (ReserveInfo.OriginalNetworkID == 0x0004)
                    {
                        view = "BS";
                    }
                    else if (ReserveInfo.OriginalNetworkID == 0x0006)
                    {
                        view = "CS1";
                    }
                    else if (ReserveInfo.OriginalNetworkID == 0x0007)
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

        
        public String RecMode
        {
            get
            {
                String view = "";
                if (ReserveInfo != null)
                {
                    switch (ReserveInfo.RecSetting.RecMode)
                    {
                        case 0:
                            view = "全サービス";
                            break;
                        case 1:
                            view = "指定サービス";
                            break;
                        case 2:
                            view = "全サービス（デコード処理なし）";
                            break;
                        case 3:
                            view = "指定サービス（デコード処理なし）";
                            break;
                        case 4:
                            view = "視聴";
                            break;
                        case 5:
                            view = "無効";
                            break;
                        default:
                            break;
                    }
                }
                return view;
            }
        }

        
        public String Priority
        {
            get
            {
                String view = "";
                if (ReserveInfo != null)
                {
                    view = ReserveInfo.RecSetting.Priority.ToString();
                }
                return view;
            }
        }

        
        public String Tuijyu
        {
            get
            {
                String view = "";
                if (ReserveInfo != null)
                {
                    if (ReserveInfo.RecSetting.TuijyuuFlag == 0)
                    {
                        view = "しない";
                    }
                    else if (ReserveInfo.RecSetting.TuijyuuFlag == 1)
                    {
                        view = "する";
                    }
                }
                return view;
            }
        }

        
        public String Pittari
        {
            get
            {
                String view = "";
                if (ReserveInfo != null)
                {
                    if (ReserveInfo.RecSetting.PittariFlag == 0)
                    {
                        view = "しない";
                    }
                    else if (ReserveInfo.RecSetting.PittariFlag == 1)
                    {
                        view = "する";
                    }
                }
                return view;
            }
        }

        
        public String Comment
        {
            get
            {
                String view = "";
                if (ReserveInfo != null)
                {
                    view = ReserveInfo.Comment.ToString();
                }
                return view;
            }
        }

        
        public String[] RecFileName
        {
            get
            {
                List<String> list = new List<String>();
                if (ReserveInfo != null)
                {
                    list = ReserveInfo.RecFileNameList;
                }
                return list.ToArray();
            }
        }
    }
}
