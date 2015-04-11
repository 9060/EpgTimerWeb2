using System;


namespace EpgTimer
{
    public class RecPresetItem
    {
        public RecPresetItem()
        {
        }
        public string DisplayName
        {
            get;
            set;
        }
        public uint ID
        {
            get;
            set;
        }
        public override string ToString()
        {
            return DisplayName;
        }
    }
}
