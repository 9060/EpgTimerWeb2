using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace EpgTimer
{
    public class ServiceItem2 : INotifyPropertyChanged
    {
        public ChSet5Item ServiceInfo
        {
            get;
            set;
        }
        public String ServiceName
        {
            get { return ServiceInfo.ServiceName; }
        }
        public String ToolTipView
        {
            get
            {
                String viewTip = "";

                if (ServiceInfo != null)
                {
                    viewTip =
                        "サービス名 : " + ServiceInfo.ServiceName + "\r\n" +
                        "サービスタイプ : " + ServiceInfo.ServiceType.ToString() + "(0x" + ServiceInfo.ServiceType.ToString("X2") + ")" + "\r\n" +
                        "OriginalNetworkId : " + ServiceInfo.ONID.ToString() + "(0x" + ServiceInfo.ONID.ToString("X4") + ")" + "\r\n" +
                        "Transport_Stream_Id : " + ServiceInfo.TSID.ToString() + "(0x" + ServiceInfo.TSID.ToString("X4") + ")" + "\r\n" +
                        "Service_Id : " + ServiceInfo.SID.ToString() + "(0x" + ServiceInfo.SID.ToString("X4") + ")" + "\r\n" +
                        "Partial_Reception : " + ServiceInfo.PartialFlag.ToString();
                }
                return viewTip;
            }
        }
    }
}
