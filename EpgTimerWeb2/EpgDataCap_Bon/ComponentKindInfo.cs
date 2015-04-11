using System;

namespace EpgTimer
{
    public class ComponentKindInfo
    {
        public ComponentKindInfo()
        {
        }
        public ComponentKindInfo(byte streamContent, byte componentType, string componentName)
        {
            StreamContent = streamContent;
            ComponentType = componentType;
            ComponentName = componentName;
        }
        public byte StreamContent
        {
            get;
            set;
        }
        public byte ComponentType
        {
            get;
            set;
        }
        public string ComponentName
        {
            get;
            set;
        }
        public override string ToString()
        {
            return ComponentName;
        }
    }
}
