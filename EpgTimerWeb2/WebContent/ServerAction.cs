using EpgTimerWeb2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
namespace EpgTimer
{
    public class ServerAction
    {
        private static string HTTPAPIRequest(string CallStr, string Sess, string IP)
        {
            if (HttpSession.IsMatch(Sess, IP) || !Setting.Instance.ReqAuth)
            {
                string Json = Api.Call(CallStr);
                if (Json != "")
                {
                    return Json;
                }
                return "\x00";
            }
            return "\x11";
        }
        //                                   API         CB     SESS
        static Regex r = new Regex(@"^\/api\/(.*)\/json\/(.*)\/;?(.*)$");
        //                                    API    SESS
        static Regex r1 = new Regex(@"^\/api\/(.*)\/;?(.*)$");
        //                                      ID    PW
        static Regex r2 = new Regex(@"^\/auth\/;(.*)\=(.*)$");
        public static void DoProcess(TcpClient Client)
        {
            var Info = new HttpContext(Client);
            try
            {
                if (Info.Request.Url == "/") Info.Request.Url = "/index.html";
                if (PrivateSetting.Instance.SetupMode)
                {
                    Setup.SetupProcess(Info);
                    return;
                }
                if (Info.Request.Url == "/ws") //WebSocket
                {
                    WebSocket.HandshakeResponseSend(Info);
                    SocketAction.Process(Info);
                    Info.Close();
                    return;
                }
                else if (r.IsMatch(Info.Request.Url))
                {
                    string Sess = r.Match(Info.Request.Url).Groups[3].Value;
                    string Result = HTTPAPIRequest(r.Match(Info.Request.Url).Groups[1].Value, Sess, Info.IpAddress);
                    if (Result == "\x11")
                    {
                        Info.Response.SetStatus(401, "Unauthorixed");
                    }
                    else if (Result == "\x00")
                    {
                        Info.Response.SetStatus(404, "Not Found");
                    }
                    else
                    {
                        string cb = r.Match(Info.Request.Url).Groups[2].Value + "(";
                        byte[] Res = Encoding.UTF8.GetBytes(cb + Result + ");");
                        Info.Response.Headers["Content-Type"] = "application/javascript; charset=utf8";
                        Info.Response.OutputStream.Write(Res, 0, Res.Length);
                    }
                    Info.Response.Send();
                    Info.Close();
                    return;
                }
                else if (r1.IsMatch(Info.Request.Url))
                {
                    string Sess = r1.Match(Info.Request.Url).Groups[2].Value;
                    string Result = HTTPAPIRequest(r1.Match(Info.Request.Url).Groups[1].Value, Sess, Info.IpAddress);
                    if (Result == "\x11")
                    {
                        Info.Response.SetStatus(401, "Unauthorixed");
                    }
                    else if (Result == "\x00")
                    {
                        Info.Response.SetStatus(404, "Not Found");
                    }
                    else
                    {
                        byte[] Res = Encoding.UTF8.GetBytes(Result);
                        Info.Response.Headers["Content-Type"] = "application/javascript; charset=utf8";
                        Info.Response.OutputStream.Write(Res, 0, Res.Length);
                    }
                    Info.Response.Send();
                    Info.Close();
                    return;
                }
                else if (r2.IsMatch(Info.Request.Url))
                {
                    var match = r2.Match(Info.Request.Url);
                    string id = match.Groups[1].Value, pass = match.Groups[2].Value;
                    var sess = new HttpSession(id, pass, Info.IpAddress);
                    Info.Response.Headers["Content-Type"] = "application/javascript; charset=utf8";
                    if (sess.CheckAuth(sess.SessionKey, Info.IpAddress))
                    {
                        byte[] Res = Encoding.UTF8.GetBytes("{\"sess\":\"" + sess.SessionKey + "\", \"error\":false}");
                        Info.Response.OutputStream.Write(Res, 0, Res.Length);
                    }
                    else
                    {
                        byte[] Res = Encoding.UTF8.GetBytes("{\"sess\":\"\", \"error\":true}");
                        Info.Response.OutputStream.Write(Res, 0, Res.Length);
                        Info.Response.SetStatus(401, "Unauthorixed");
                    }
                    Info.Response.Send();
                    Info.Close();
                    return;
                }
                if (!new HttpContent().RequestUrl(Info))
                {
                    HttpResponseGenerater.NotFound(Info);
                }
            }
            catch (Exception ex)
            {
                byte[] NotFound = Encoding.UTF8.GetBytes(@"<html>
<body>
<h1>500</h1><p>詳細: " + ex.Message + @"</p>
<hr />
EpgTimerWeb(v2) by YUKI
</body>
</html>");
                Info.Response.SetStatus(500, "Internal Server Error");
                Info.Response.OutputStream.Write(NotFound, 0, NotFound.Length);
                Info.Response.Send();
                Console.WriteLine("\n!!!! Exception !!!!");
            }
            if (Info.Request.Headers.ContainsKey("connection") &&
                Info.Request.Headers["connection"].ToLower() == "keep-alive") DoProcess(Client); //KeepAlive対応(適当)
            Info.Close();
        }

    }
}
