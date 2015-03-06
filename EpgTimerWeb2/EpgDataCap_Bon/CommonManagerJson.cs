using System;
using System.Collections.Generic;

namespace EpgTimer
{
    public class CommonManagerJson
    {
        private static CommonManagerJson _instance;
        public static CommonManagerJson Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CommonManagerJson();
                return _instance;
            }
            set { _instance = value; }
        }
        public Dictionary<UInt16, ContentKindInfo> ContentKindDictionary { set; get; }
        public Dictionary<UInt16, ContentKindInfo> ContentKindDictionary2 { set; get; }
        public Dictionary<UInt16, ComponentKindInfo> ComponentKindDictionary { set; get; }
        public Dictionary<byte, DayOfWeekInfo> DayOfWeekDictionary { set; get; }
        public Dictionary<UInt16, UInt16> HourDictionary { set; get; }
        public Dictionary<UInt16, UInt16> MinDictionary { set; get; }
        public Dictionary<byte, RecModeInfo> RecModeDictionary { set; get; }
        public Dictionary<byte, YesNoInfo> YesNoDictionary { set; get; }
        public Dictionary<byte, PriorityInfo> PriorityDictionary { set; get; }
        public CommonManagerJson()
        {
            ContentKindDictionary = CommonManager.Instance.ContentKindDictionary;
            ContentKindDictionary2 = CommonManager.Instance.ContentKindDictionary2;
            ComponentKindDictionary = CommonManager.Instance.ComponentKindDictionary;
            DayOfWeekDictionary = CommonManager.Instance.DayOfWeekDictionary;
            HourDictionary = CommonManager.Instance.HourDictionary;
            MinDictionary = CommonManager.Instance.MinDictionary;
            RecModeDictionary = CommonManager.Instance.RecModeDictionary;
            YesNoDictionary = CommonManager.Instance.YesNoDictionary;
            PriorityDictionary = CommonManager.Instance.PriorityDictionary;
        }
    }
}
