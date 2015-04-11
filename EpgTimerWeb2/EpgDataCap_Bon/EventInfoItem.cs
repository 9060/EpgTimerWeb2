using CtrlCmdCLI.Def;
using System;
namespace EpgTimer
{
    public class EventInfoItem
    {
        private EpgEventInfo EventInfo = null;
        public EventInfoItem(EpgEventInfo EventInfo)
        {
            this.EventInfo = EventInfo;
        }
        public bool StartFlg
        {
            get { return EventInfo.StartTimeFlag == 1; }
        }
        public bool DurFlg
        {
            get { return EventInfo.DurationFlag == 1; }
        }
        public DateTime Start
        {
            get
            {
                if (EventInfo.StartTimeFlag != 1) return DateTime.MinValue;
                return EventInfo.start_time;
            }
        }

        public DateTime End
        {
            get
            {
                if (EventInfo.StartTimeFlag != 1 || EventInfo.DurationFlag != 1) return DateTime.MinValue;
                return Start.AddSeconds(EventInfo.durationSec);
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

        public ushort EID
        {
            get
            {
                return EventInfo.event_id;
            }
        }

        public string Key
        {
            get { return CommonManager.Create64PgKey(ONID, TSID, SID, EID).ToString(); }
        }


        public string ServiceKeyS
        {
            get { return CommonManager.Create64Key(ONID, TSID, SID).ToString(); }
        }
        public byte FreeCA
        {
            get
            {
                return EventInfo.FreeCAFlag;
            }
        }

        public EpgComponentInfo Component
        {
            get
            {
                return EventInfo.ComponentInfo;
            }
        }

        public EpgAudioComponentInfo Audio
        {
            get
            {
                return EventInfo.AudioInfo;
            }
        }

        public EpgContentInfo Content
        {
            get
            {
                return EventInfo.ContentInfo;
            }
        }

        public EpgEventGroupInfo EventGroup
        {
            get
            {
                return EventInfo.EventGroupInfo;
            }
        }

        public EpgEventGroupInfo EventRelay
        {
            get
            {
                return EventInfo.EventRelayInfo;
            }
        }

        public EpgExtendedEventInfo Ext
        {
            get
            {
                return EventInfo.ExtInfo;
            }
        }

        public EpgShortEventInfo Short
        {
            get
            {
                return EventInfo.ShortInfo;
            }
        }
    }
}
