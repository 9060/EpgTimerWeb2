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
                string text = "";
                if (NotifyInfo != null)
                {
                    switch ((UpdateNotifyItem)NotifyInfo.notifyID)
                    {
                        case UpdateNotifyItem.PreRecStart:
                            {
                                text = "予約録画開始準備";
                            }
                            break;
                        case UpdateNotifyItem.RecStart:
                            {
                                text = "録画開始";
                            }
                            break;
                        case UpdateNotifyItem.RecEnd:
                            {
                                text = "録画終了";
                            }
                            break;
                        case UpdateNotifyItem.RecTuijyu:
                            {
                                text = "追従発生";
                            }
                            break;
                        case UpdateNotifyItem.ChgTuijyu:
                            {
                                text = "番組変更";
                            }
                            break;
                        case UpdateNotifyItem.PreEpgCapStart:
                            {
                                text = "EPG取得";
                            }
                            break;
                        case UpdateNotifyItem.EpgCapStart:
                            {
                                text = "EPG取得";
                            }
                            break;
                        case UpdateNotifyItem.EpgCapEnd:
                            {
                                text = "EPG取得";
                            }
                            break;
                        default:
                            text = "";
                            break;
                    }
                }
                return text;
            }
        }

        public string LogText
        {
            get
            {
                string text = "";
                if (NotifyInfo != null)
                {
                    switch ((UpdateNotifyItem)NotifyInfo.notifyID)
                    {
                        case UpdateNotifyItem.PreRecStart:
                            {
                                text = NotifyInfo.param4;
                            }
                            break;
                        case UpdateNotifyItem.RecStart:
                            {
                                text = NotifyInfo.param4;
                            }
                            break;
                        case UpdateNotifyItem.RecEnd:
                            {
                                text = NotifyInfo.param4;
                            }
                            break;
                        case UpdateNotifyItem.RecTuijyu:
                            {
                                text = NotifyInfo.param4;
                            }
                            break;
                        case UpdateNotifyItem.ChgTuijyu:
                            {
                                text = NotifyInfo.param4;
                            }
                            break;
                        case UpdateNotifyItem.PreEpgCapStart:
                            {
                                text = NotifyInfo.param4;
                            }
                            break;
                        case UpdateNotifyItem.EpgCapStart:
                            {
                                text = "開始";
                            }
                            break;
                        case UpdateNotifyItem.EpgCapEnd:
                            {
                                text = "終了";
                            }
                            break;
                        default:
                            text = "";
                            break;
                    }
                }
                return text;
            }
        }

        public string FileLogText
        {
            get
            {
                string text = "";
                if (NotifyInfo != null)
                {
                    text = NotifyInfo.time.ToString("yyyy/MM/dd HH:mm:ss.fff");

                    text += " [";
                    switch ((UpdateNotifyItem)NotifyInfo.notifyID)
                    {
                        case UpdateNotifyItem.PreRecStart:
                            {
                                text += "予約録画開始準備";
                            }
                            break;
                        case UpdateNotifyItem.RecStart:
                            {
                                text += "録画開始";
                            }
                            break;
                        case UpdateNotifyItem.RecEnd:
                            {
                                text += "録画終了";
                            }
                            break;
                        case UpdateNotifyItem.RecTuijyu:
                            {
                                text += "追従発生";
                            }
                            break;
                        case UpdateNotifyItem.ChgTuijyu:
                            {
                                text += "番組変更";
                            }
                            break;
                        case UpdateNotifyItem.PreEpgCapStart:
                            {
                                text += "EPG取得";
                            }
                            break;
                        case UpdateNotifyItem.EpgCapStart:
                            {
                                text += "EPG取得";
                            }
                            break;
                        case UpdateNotifyItem.EpgCapEnd:
                            {
                                text += "EPG取得";
                            }
                            break;
                        default:
                            text += NotifyInfo.notifyID.ToString();
                            break;
                    }
                    text += "] ";

                    switch ((UpdateNotifyItem)NotifyInfo.notifyID)
                    {
                        case UpdateNotifyItem.PreRecStart:
                            {
                                text += NotifyInfo.param4;
                            }
                            break;
                        case UpdateNotifyItem.RecStart:
                            {
                                text += NotifyInfo.param4;
                            }
                            break;
                        case UpdateNotifyItem.RecEnd:
                            {
                                text += NotifyInfo.param4;
                            }
                            break;
                        case UpdateNotifyItem.RecTuijyu:
                            {
                                text += NotifyInfo.param4;
                            }
                            break;
                        case UpdateNotifyItem.ChgTuijyu:
                            {
                                text += NotifyInfo.param4;
                            }
                            break;
                        case UpdateNotifyItem.PreEpgCapStart:
                            {
                                text += NotifyInfo.param4;
                            }
                            break;
                        case UpdateNotifyItem.EpgCapStart:
                            {
                                text += "開始";
                            }
                            break;
                        case UpdateNotifyItem.EpgCapEnd:
                            {
                                text += "終了";
                            }
                            break;
                        default:
                            text += NotifyInfo.notifyID.ToString();
                            break;
                    }
                    text += "\r\n";
                }
                return text;
            }
        }
    }
}
