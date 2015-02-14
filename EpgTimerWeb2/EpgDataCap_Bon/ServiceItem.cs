using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using CtrlCmdCLI;
using CtrlCmdCLI.Def;

namespace EpgTimer
{
    public class ServiceItem
    {
        public EpgServiceInfo ServiceInfo
        {
            get;
            set;
        }
        public UInt64 ID
        {
            get { return CommonManager.Create64Key(ServiceInfo.ONID, ServiceInfo.TSID, ServiceInfo.SID); }
        }
        public String ServiceName
        {
            get { return ServiceInfo.service_name; }
        }
        public String NetworkName
        {
            get
            {
                String name = "Other";
                if (ServiceInfo.ONID == 0x0004)
                {
                    name = "BS";
                }
                else if (ServiceInfo.ONID == 0x0006 || ServiceInfo.ONID == 0x0007)
                {
                    name = "CS";
                }
                else if (0x7880 <= ServiceInfo.ONID && ServiceInfo.ONID <= 0x7FE8)
                {
                    name = "Digital";
                }
                return name;
            }
        }
    }
}
