using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EpgTimer
{
    public class HttpContext
    {
        public bool IsWsSend = false;
        public HttpContext()
        {
            Request = new HttpRequest();
            Response = new HttpResponse();
            IpAddress = "0000";
        }
        public HttpRequest Request { set; get; }
        public HttpResponse Response { set; get; }
        public string IpAddress { set; get; }
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
