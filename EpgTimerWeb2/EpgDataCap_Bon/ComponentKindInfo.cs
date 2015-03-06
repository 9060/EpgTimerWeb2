using System;

namespace EpgTimer
{
    public class ComponentKindInfo
    {
        public ComponentKindInfo()
        {
        }
        public ComponentKindInfo(Byte streamContent, Byte componentType, String componentName)
        {
            StreamContent = streamContent;
            ComponentType = componentType;
            ComponentName = componentName;
        }
        public Byte StreamContent
        {
            get;
            set;
        }
        public Byte ComponentType
        {
            get;
            set;
        }
        public String ComponentName
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
