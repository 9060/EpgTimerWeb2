using System;

namespace EpgTimer
{
    public class ContentKindInfo
    {
        public ContentKindInfo()
        {
        }
        public ContentKindInfo(String contentName, String subName, Byte nibble1, Byte nibble2)
        {
            this.ContentName = contentName;
            this.SubName = subName;
            this.Nibble1 = nibble1;
            this.Nibble2 = nibble2;
            this.ID = (UInt16)(((UInt16)nibble1) << 8 | nibble2);
        }
        public UInt16 ID
        {
            get;
            set;
        }
        public String ContentName
        {
            get;
            set;
        }
        public String SubName
        {
            get;
            set;
        }
        public Byte Nibble1
        {
            get;
            set;
        }
        public Byte Nibble2
        {
            get;
            set;
        }
        public override string ToString()
        {
            if (Nibble2 == 0xFF)
            {
                return ContentName;
            }
            else
            {
                return "  " + SubName;
            }
        }
        public String ToolTipView
        {
            get
            {
                String viewTip = "";
                if (Nibble2 == 0xFF)
                {
                    viewTip = ContentName;
                }
                else
                {
                    viewTip = ContentName + " : " + SubName;
                }
                return viewTip;
            }
        }
    }
}
