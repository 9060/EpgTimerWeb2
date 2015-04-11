using CtrlCmdCLI.Def;
using System;

namespace EpgTimer
{
    public class ServiceItem
    {
        public EpgServiceInfo ServiceInfo
        {
            get;
            set;
        }
        public ulong ID
        {
            get { return CommonManager.Create64Key(ServiceInfo.ONID, ServiceInfo.TSID, ServiceInfo.SID); }
        }
        public string ServiceName
        {
            get { return ServiceInfo.service_name; }
        }
        public string NetworkName
        {
            get
            {
                return CommonManager.GetNetworkName(ServiceInfo.ONID);
            }
        }
    }
}
