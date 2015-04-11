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
        public Dictionary<ushort, ContentKindInfo> ContentKindDictionary { set; get; }
        public Dictionary<ushort, ContentKindInfo> ContentKindDictionary2 { set; get; }
        public Dictionary<ushort, ComponentKindInfo> ComponentKindDictionary { set; get; }
        public CommonManagerJson()
        {
            ContentKindDictionary = CommonManager.Instance.ContentKindDictionary;
            ContentKindDictionary2 = CommonManager.Instance.ContentKindDictionary2;
            ComponentKindDictionary = CommonManager.Instance.ComponentKindDictionary;
        }
    }
}
