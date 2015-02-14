using EpgTimerWeb2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EpgTimer
{
    public class SocketAction
    {
        static List<HttpContext> Sockets = new List<HttpContext>();
        static Regex r2 = new Regex(@"^RUNCMD (.*) (.*)$");
        static Regex r = new Regex(@"^LOGIN (.*) (.*)$");
        static Regex r3 = new Regex(@"^LOGIN (.*)$");
        public static void Process(HttpContext Info)
        {
            HttpSession _sess = null;
            Sockets.Add(Info);
            while (Info.Client.Connected)
            {
                byte[] UnMaskBuf = WebSocket.GetWebSocketUnMaskedFrame(Info);
                if (UnMaskBuf == null) continue;
                string UnMask = Encoding.UTF8.GetString(UnMaskBuf);
                Debug.Print(UnMask);
                if (r2.IsMatch(UnMask) && Info.IsAuth)
                {
                    var match = r2.Match(UnMask);
                    string Command = match.Groups[2].Value;
                    string Id = match.Groups[1].Value;
                    string JsonData = Api.Call(Command, false);
                    byte[] Response = WebSocket.WebSocketMask(
                                Encoding.UTF8.GetBytes("ERR No API"), 0x1);
                    if (JsonData != "")
                    {
                        Response = Encoding.UTF8.GetBytes("+OK" + Id + " " + JsonData);
                    }
                    HttpResponse.SendResponseBodyWS(Info, WebSocket.WebSocketMask(
                                    Response, 0x1));
                }
                else if (r.IsMatch(UnMask) && !Info.IsAuth)
                {
                    var match = r.Match(UnMask);
                    string Pass = match.Groups[2].Value;
                    string Id = match.Groups[1].Value;
                    byte[] Response = WebSocket.WebSocketMask(
                                    Encoding.UTF8.GetBytes("LERR No Auth"), 0x1);
                    var Sess = _sess = new HttpSession(Id, Pass, Info.IpAddress);
                    if ((Info.IsAuth = Sess.CheckAuth(Sess.SessionKey, Info.IpAddress)))
                    {
                        Response = WebSocket.WebSocketMask(
                                    Encoding.UTF8.GetBytes("+LOK " + Sess.SessionKey), 0x1);
                    }
                    HttpResponse.SendResponseBodyWS(Info, Response);
                }
                else if (r3.IsMatch(UnMask) && !Info.IsAuth)
                {
                    string Sess = r3.Match(UnMask).Groups[1].Value;
                    byte[] Response = WebSocket.WebSocketMask(
                                    Encoding.UTF8.GetBytes("LERR No Auth"), 0x1);
                    if (HttpSession.IsMatch(Sess, Info.IpAddress))
                    {
                        Response = WebSocket.WebSocketMask(
                                    Encoding.UTF8.GetBytes("+LOK " + Sess), 0x1);
                        _sess = HttpSession.Search(Sess, Info.IpAddress);
                        if(_sess != null) Info.IsAuth = true;
                    }
                    HttpResponse.SendResponseBodyWS(Info, Response);
                }
                else if (UnMask.StartsWith("LOGOUT"))
                {
                    if (_sess != null)
                        PrivateSetting.Instance.Sessions.Remove(_sess);
                    _sess = null;
                    Info.IsAuth = false;
                    byte[] Response = WebSocket.WebSocketMask(
                                Encoding.UTF8.GetBytes("-LOK"), 0x1);
                    HttpResponse.SendResponseBodyWS(Info, Response);
                }
                else if (UnMask.StartsWith("L-CHECK"))
                {
                    byte[] Response = WebSocket.WebSocketMask(
                        Encoding.UTF8.GetBytes(Setting.Instance.ReqAuth ? "-LA" : "-LN"), 0x1);
                    HttpResponse.SendResponseBodyWS(Info, Response);
                }
                else if (!Info.IsAuth)
                {
                    byte[] Response = WebSocket.WebSocketMask(
                                Encoding.UTF8.GetBytes("LERR"), 0x1);
                    HttpResponse.SendResponseBodyWS(Info, Response);
                }
                else
                {
                    byte[] Response = WebSocket.WebSocketMask(
                                Encoding.UTF8.GetBytes("ERR"), 0x1);
                    HttpResponse.SendResponseBodyWS(Info, Response);
                }
            }
            Sockets.Remove(Info);
        }
        public static void SendAllMessage(string Mes)
        {
            try
            {
                byte[] Response = WebSocket.WebSocketMask(
                                Encoding.UTF8.GetBytes(Mes), 0x1);
                var a = Sockets.ToArray();
                foreach (var Con in a)
                {
                    if (!Con.Client.Connected) Sockets.Remove(Con);
                    if (!Con.IsAuth) continue;
                    while (Con.IsWsSend) ;
                    HttpResponse.SendResponseBody(Con, Response);
                }
            }
            catch (Exception ex)
            {
                Debug.Print("SocketAction Error: {0}", ex.Message);
            }
        }
    }
}
