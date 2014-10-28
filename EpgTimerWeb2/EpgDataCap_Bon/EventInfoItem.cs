using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CtrlCmdCLI.Def;
namespace EpgTimer
{
    public class EventInfoItem
    {
        private EpgEventInfo EventInfo = null;
        public EventInfoItem(EpgEventInfo EventInfo)
        {
            this.EventInfo = EventInfo;
        }

        public DateTime StartTime
        {
            get
            {
                if (EventInfo.StartTimeFlag != 1) return DateTime.MinValue;
                return EventInfo.start_time;
            }
        }

        public DateTime EndTime
        {
            get
            {
                if (EventInfo.StartTimeFlag != 1 || EventInfo.DurationFlag != 1) return DateTime.MinValue;
                return StartTime.AddSeconds(EventInfo.durationSec);
            }
        }
         
        public ushort ONID
        {
            get
            {
                return EventInfo.original_network_id;
            }
        }

        public ushort TSID
        {
            get
            {
                return EventInfo.transport_stream_id;
            }
        }

        public ushort SID
        {
            get
            {
                return EventInfo.service_id;
            }
        }

        public ushort EventID
        {
            get
            {
                return EventInfo.event_id;
            }
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
            get { return Key.ToString(); }
        }
        public ulong ServiceKey
        {
            get
            {
                return CommonManager.Create64Key(ONID, TSID, SID);
            }
        }
        public string ServiceKeyS
        {
            get { return ServiceKey.ToString(); }
        }
        public bool IsScramble
        {
            get
            {
                return EventInfo.FreeCAFlag != 1;
            }
        }

        public EpgComponentInfo ComponentInfo
        {
            get
            {
                return EventInfo.ComponentInfo;
            }
        }

        public EpgAudioComponentInfo AudioInfo
        {
            get
            {
                return EventInfo.AudioInfo;
            }
        }

        public EpgContentInfo ContentInfo
        {
            get
            {
                return EventInfo.ContentInfo;
            }
        }

        public EpgEventGroupInfo EventGroupInfo
        {
            get
            {
                return EventInfo.EventGroupInfo;
            }
        }

        public EpgEventGroupInfo EventRelayInfo
        {
            get
            {
                return EventInfo.EventRelayInfo;
            }
        }

        public EpgExtendedEventInfo ExtInfo
        {
            get
            {
                return EventInfo.ExtInfo;
            }
        }

        public EpgShortEventInfo ShortInfo
        {
            get
            {
                return EventInfo.ShortInfo;
            }
        }

        public string TextViewAll
        {
            get
            {
                return CommonManager.Instance.ConvertProgramText(EventInfo, EventInfoTextMode.All);
            }
        }

        public string TextViewBasic
        {
            get
            {
                return CommonManager.Instance.ConvertProgramText(EventInfo, EventInfoTextMode.BasicOnly);
            }
        }

        public string TextViewExt
        {
            get
            {
                return CommonManager.Instance.ConvertProgramText(EventInfo, EventInfoTextMode.ExtOnly);
            }
        }
    }
}
