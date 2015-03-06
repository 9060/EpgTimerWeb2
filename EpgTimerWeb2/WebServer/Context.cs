﻿using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EpgTimer
{
    public class HttpContext
    {
        public bool IsWsSend { set; get; }
        public HttpContext(TcpClient client)
        {
            Client = client;
            Response = new HttpResponse(this);
            IpAddress = "0.0.0.0";
            IsWsSend = false;
            Request = HttpRequest.Parse(this.HttpStream);
            IpAddress = ((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString();
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
        public static void SendResponse(HttpContext Context, byte[] Bytes)
        {
            if (!Context.Response.Headers.ContainsKey("Content-Length"))
            {
                Context.Response.Headers.Add("Content-Length", Bytes.LongLength.ToString());
            }
            Context.Response.OutputStream.Write(Bytes, 0, Bytes.Length);
            Context.Response.Send();
        }
        public static void SendResponse(HttpContext Context, string Str)
        {
            SendResponse(Context, Encoding.UTF8.GetBytes(Str));
        }
        public static void Redirect(HttpContext Context, string Url)
        {
            var Domain = "localhost:8080";
            if (Context.Request.Headers.ContainsKey("host"))
            {
                Domain = Context.Request.Headers["host"];
            }
            Context.Response.StatusCode = 301;
            Context.Response.StatusText = "Moved Permanently";
            Context.Response.Headers["Location"] = "http://" + Domain + Url;
            Context.Response.Send();
        }
    }
}
