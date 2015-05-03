using System;

namespace EpgTimer
{
    public class NotifySrvInfoItem
    {
        public NotifySrvInfo NotifyInfo
        {
            get;
            set;
        }
        public DateTime Time
        {
            get
            {
                return NotifyInfo.time;
            }
        }
        public string Title
        {
            get
            {
                if (NotifyInfo != null)
                {
                    switch ((UpdateNotifyItem)NotifyInfo.notifyID)
                    {
                        case UpdateNotifyItem.PreRecStart:
                            return "予約録画開始準備";
                        case UpdateNotifyItem.RecStart:
                            return "録画開始";
                        case UpdateNotifyItem.RecEnd:
                            return "録画終了";
                        case UpdateNotifyItem.RecTuijyu:
                            return "追従発生";
                        case UpdateNotifyItem.ChgTuijyu:
                            return "番組変更";
                        case UpdateNotifyItem.PreEpgCapStart:
                            return "EPG取得";
                        case UpdateNotifyItem.EpgCapStart:
                            return "EPG取得";
                        case UpdateNotifyItem.EpgCapEnd:
                            return "EPG取得";
                        default:
                            return "";
                    }
                }
                return "";
            }
        }

        public string LogText
        {
            get
            {
                if (NotifyInfo != null)
                {
                    switch ((UpdateNotifyItem)NotifyInfo.notifyID)
                    {
                        case UpdateNotifyItem.PreRecStart:
                            return NotifyInfo.param4;
                        case UpdateNotifyItem.RecStart:
                            return NotifyInfo.param4;
                        case UpdateNotifyItem.RecEnd:
                            return NotifyInfo.param4;
                        case UpdateNotifyItem.RecTuijyu:
                            return NotifyInfo.param4;
                        case UpdateNotifyItem.ChgTuijyu:
                            return NotifyInfo.param4;
                        case UpdateNotifyItem.PreEpgCapStart:
                            return NotifyInfo.param4;
                        case UpdateNotifyItem.EpgCapStart:
                            return "開始";
                        case UpdateNotifyItem.EpgCapEnd:
                            return "終了";
                        default:
                            return NotifyInfo.notifyID.ToString();
                    }
                }
                return "";
            }
        }

        public string FileLogText
        {
            get
            {
                if (NotifyInfo != null)
                {
                    return NotifyInfo.time.ToString("yyyy/MM/dd HH:mm:ss.fff") + 
                        " [" + Title + "] " + LogText + "\n";
                }
                return "";
            }
        }
    }
}
