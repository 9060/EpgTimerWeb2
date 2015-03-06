using System;

namespace EpgTimer
{
    public class PriorityInfo
    {
        public PriorityInfo()
        {
        }
        public PriorityInfo(String displayName, Byte value)
        {
            this.DisplayName = displayName;
            this.Value = value;
        }
        public String DisplayName
        {
            get;
            set;
        }
        public Byte Value
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
