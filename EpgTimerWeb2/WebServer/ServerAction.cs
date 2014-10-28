using EpgTimerWeb2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EpgTimer
{
    public class ServerAction
    {
        static Regex r = new Regex(@"^\/api\/(.*)\/json\/(.*)\/run\/(.*)$");
        static Regex r1 = new Regex(@"^\/api\/(.*)\/run\/(.*)$");
        static Regex r2 = new Regex(@"^\/auth\/(.*)\=(.*)$");
        public static void Process(TcpClient Client)
        {
            var Info = new HttpContext()
            {
                Client = Client,
                IpAddress = ((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString()
            };
            Info.Request = HttpRequestParser.Parse(Info.HttpStream);
            if (Info.Request.Url == "/") Info.Request.Url = "/index.html";
            if (Info.Request.Url == "/ws") //WebSocket
            {
                WebSocket.HandshakeResponseSend(Info);
                SocketAction.Process(Info);
                Info.Close();
                return;
            }
            //Console.WriteLine("{0} & {1}", Info.Request.GetParam, Info.Request.PostParam);
            if (r.IsMatch(Info.Request.Url))
            {
                string Sess = r.Match(Info.Request.Url).Groups[3].Value;
                if (HttpSession.IsMatch(Sess, Info.IpAddress))
                {
                    string Json = Api.Call(
                        r.Match(Info.Request.Url).Groups[1].Value);
                    string cb = r.Match(Info.Request.Url).Groups[2].Value + "(";
                    byte[] Res = Encoding.UTF8.GetBytes(cb + Json + ");");
                    Info.Response.OutputStream.Write(Res, 0, Res.Length);
                    Info.Response.Headers["Content-Type"] = "application/javascript";
                }
                else
                {
                    Info.Response.StatusCode = 400;
                    Info.Response.StatusText = "Fail Auth";
                }
                HttpResponseGenerater.SendResponse(Info);
                Info.Close();
                return;
            }
            if (r1.IsMatch(Info.Request.Url))
            {
                string Sess = r1.Match(Info.Request.Url).Groups[2].Value;
                if (HttpSession.IsMatch(Sess, Info.IpAddress))
                {
                    string Json = Api.Call(
                        r1.Match(Info.Request.Url).Groups[1].Value);
                    byte[] Res = Encoding.UTF8.GetBytes(Json);
                    Info.Response.OutputStream.Write(Res, 0, Res.Length);
                }
                else
                {
                    Info.Response.StatusCode = 400;
                    Info.Response.StatusText = "Fail Auth";
                }
                HttpResponseGenerater.SendResponse(Info);
                Info.Close();
                return;
            }
            if (r2.IsMatch(Info.Request.Url))
            {
                var match = r2.Match(Info.Request.Url);
                string id=match.Groups[1].Value, pass = match.Groups[2].Value;
                var sess = new HttpSession(id, pass, Info.IpAddress);
                if (sess.CheckAuth(sess.SessionKey, Info.IpAddress))
                {
                    byte[] Res = Encoding.UTF8.GetBytes("{\"sess\":\"" + sess.SessionKey + "\", \"error\":false}");
                    Info.Response.OutputStream.Write(Res, 0, Res.Length);
                }
                else
                {
                    byte[] Res = Encoding.UTF8.GetBytes("{\"sess\":\"\", \"error\":true}");
                    Info.Response.OutputStream.Write(Res, 0, Res.Length);
                }
                HttpResponseGenerater.SendResponse(Info);
                Info.Close();
                return;
            }
            var Path = Info.Request.Url.Replace("/", "\\");
            try
            {
                if (!new HttpContent().RequestUrl(Info))
                {
                    byte[] NotFound = Encoding.UTF8.GetBytes(@"<html>
<body>
<h1>404 - Not found</h1>
<hr />
EpgTimerWeb(v2) by YUKI
</body>
</html>");
                    Info.Response.StatusCode = 404;
                    Info.Response.StatusText = "Not Found";
                    Info.Response.OutputStream.Write(NotFound, 0, NotFound.Length);
                    HttpResponseGenerater.SendResponse(Info);
                }
            }
            catch (Exception ex)
            {
                byte[] NotFound = Encoding.UTF8.GetBytes(@"<html>
<body>
<h1>500</h1><p>詳細: " + ex.Message + @"</p>
</body>
</html>");
                Info.Response.StatusCode = 500;
                Info.Response.StatusText = "Error";
                Info.Response.OutputStream.Write(NotFound, 0, NotFound.Length);
                HttpResponseGenerater.SendResponse(Info);
                Console.WriteLine("!!!! Exception !!!!");
            }
            if (Info.Request.Headers.ContainsKey("connection") &&
                Info.Request.Headers["connection"].ToLower() == "keep-alive") Process(Client); //KeepAlive対応
            Info.Close();
        }
        
    }
}
