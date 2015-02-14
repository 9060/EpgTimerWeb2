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
        public static void Process(HttpContext Info)
        {
            Sockets.Add(Info);
            while (Info.Client.Connected)
            {
                byte[] UnMaskBuf = WebSocket.GetWebSocketUnMaskedFrame(Info);
                if (UnMaskBuf == null) continue;
                string UnMask = Encoding.UTF8.GetString(UnMaskBuf);
                Debug.Print(UnMask);
                if (r2.IsMatch(UnMask))
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
