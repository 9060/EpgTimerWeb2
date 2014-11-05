using EpgTimerWeb2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EpgTimer
{
    public class HttpContext
    {
        public bool IsWsSend { set; get; }
        public HttpContext(TcpClient client)
        {
            Client = client;
            Request = new HttpRequest();
            Response = new HttpResponse(this);
            IpAddress = "0.0.0.0";
            IsWsSend = false;
            IsAuth = PrivateSetting.Instance.Passwords == null;
            Request = HttpRequestParser.Parse(this.HttpStream);
            IpAddress = ((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString();
        }
        public HttpRequest Request { set; get; }
        public HttpResponse Response { set; get; }
        public string IpAddress { set; get; }
        public bool IsAuth { set; get; }
        private TcpClient _Client;
        public TcpClient Client
        {
            get
            {
                return _Client;
            }
            set
            {
                _Client = value;
                HttpStream = _Client.GetStream();
            }
        }
        public Stream HttpStream {set;get;}

        public void Close()
        {
            HttpStream.Flush();
            HttpStream.Close();
            Request = null;
            Response = null;
            Client.Close();
        }
    }
}
