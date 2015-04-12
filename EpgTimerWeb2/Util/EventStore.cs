using System.Collections.Generic;

namespace EpgTimer
{
    public class EventStore
    {
        private static EventStore _instance;
        public static EventStore Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EventStore();
                return _instance;
            }
            set { _instance = value; }
        }
        private List<NotifySrvInfoItem> _events = null;
        public List<NotifySrvInfoItem> Events { get { return _events; } }
        public EventStore()
        {
            _events = new List<NotifySrvInfoItem>();
        }
        public void AddMessage(NotifySrvInfoItem item)
        {
            _events.Add(item);
        }
    }
}
