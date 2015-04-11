using System;

namespace EpgTimer
{
    public class ContentKindInfo
    {
        public ContentKindInfo()
        {
        }
        public ContentKindInfo(string contentName, string subName, byte nibble1, byte nibble2)
        {
            this.ContentName = contentName;
            this.SubName = subName;
            this.Nibble1 = nibble1;
            this.Nibble2 = nibble2;
            this.ID = (ushort)(((ushort)nibble1) << 8 | nibble2);
        }
        public ushort ID
        {
            get;
            set;
        }
        public string ContentName
        {
            get;
            set;
        }
        public string SubName
        {
            get;
            set;
        }
        public byte Nibble1
        {
            get;
            set;
        }
        public byte Nibble2
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
        public string ToolTipView
        {
            get
            {
                string viewTip = "";
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
