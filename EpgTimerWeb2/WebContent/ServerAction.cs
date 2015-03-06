using EpgTimerWeb2.Properties;
using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
namespace EpgTimer
{
    public class ServerAction
    {
        private static string HTTPAPIRequest(string CallStr)
        {
            string Json = Api.Call(CallStr);
            if (Json != "")
            {
                return Json;
            }
            return "\x00";
        }
        private static void LoginURL(HttpContext Info)
        {
            Info.Response.Headers["Cache-Control"] = "no-cache";
            if (CheckCookie(Info))
            {
                HttpContext.Redirect(Info, "/");
                return;
            }
            Info.Response.Headers["Content-Type"] = "text/html";
            Info.Response.Headers["Cache-Control"] = "no-cache";
            HttpContext.SendResponse(Info, Resources.Login);
        }
        private static void DoLoginURL(HttpContext Info)
        {
            Info.Response.Headers["Cache-Control"] = "no-cache";
            if (CheckCookie(Info))
            {
                HttpContext.Redirect(Info, "/");
                return;
            }
            var Param = HttpUtility.ParseQueryString(Info.Request.PostString);
            string UserName = Param["user"];
            string Password = Param["pass"];
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
            {
                HttpContext.Redirect(Info, "/login");
                return;
            }
            HttpSession Session = new HttpSession(UserName, Password, Info.IpAddress);
            if (!Session.CheckAuth(Session.SessionKey, Info.IpAddress))
            {
                HttpContext.Redirect(Info, "/login");
                return;
            }
            Info.Response.Headers["Set-Cookie"] = Cookie.Generate(new Cookie(){
                {"session", Session.SessionKey}
            });
            HttpContext.Redirect(Info, "/");
        }
        private static bool CheckCookie(HttpContext Info)
        {
            if (!Setting.Instance.ReqAuth || (Info.Request.Cookie.ContainsKey("session") && HttpSession.IsMatch(Info.Request.Cookie["session"], Info.IpAddress)))
                return true;
            else
                return false;
        }
        private static void LogoutURL(HttpContext Info)
        {
            Info.Response.Headers["Cache-Control"] = "no-cache";
            Cookie removeCookie = new Cookie(){
                {"session", "delete"}
            };
            removeCookie.Expire = DateTime.Now.AddYears(-5);
            Info.Response.Headers["Set-Cookie"] = Cookie.Generate(removeCookie);
            HttpContext.Redirect(Info, "/");
        }
        //                                   API         CB
        static Regex r = new Regex(@"^\/api\/(.*)\/json\/(.*)\/$");
        //                                    API
        static Regex r1 = new Regex(@"^\/api\/(.*)\/$");
        public static void DoProcess(TcpClient Client)
        {
            var Info = new HttpContext(Client);
            try
            {
                if (Info.Request.Url == "/index.htm") Info.Request.Url = "/index.html";
                if (Info.Request.Url == "/") Info.Request.Url = "/index.html";
                if (PrivateSetting.Instance.SetupMode)
                {
                    Setup.SetupProcess(Info);
                    return;
                }
                if (Info.Request.Url.ToLower() == "/ws") //WebSocket
                {
                    Info.Response.Headers["Cache-Control"] = "no-cache";
                    if (CheckCookie(Info))
                    {
                        WebSocket.HandshakeResponseSend(Info);
                        SocketAction.Process(Info);
                    }
                    else
                    {
                        Info.Response.SetStatus(401, "Unauthorixed");
                        Info.Response.Send();
                    }
                    Info.Close();
                    return;
                }
                if (Info.Request.Url.ToLower() == "/logout")
                {
                    LogoutURL(Info);
                }
                if (Info.Request.Url.ToLower() == "/login")
                {
                    LoginURL(Info);
                }
                if (Info.Request.Url.ToLower() == "/dologin")
                {
                    DoLoginURL(Info);
                }
                if (r.IsMatch(Info.Request.Url))
                {
                    Info.Response.Headers["Cache-Control"] = "no-cache";
                    if (CheckCookie(Info))
                    {
                        string Result = HTTPAPIRequest(r.Match(Info.Request.Url).Groups[1].Value);
                        if (Result == "\x00")
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
                    }
                    else
                    {
                        Info.Response.SetStatus(401, "Unauthorixed");
                    }
                    Info.Response.Send();
                    Info.Close();
                    return;
                }
                else if (r1.IsMatch(Info.Request.Url))
                {
                    Info.Response.Headers["Cache-Control"] = "no-cache";
                    if (CheckCookie(Info))
                    {
                        string Result = HTTPAPIRequest(r1.Match(Info.Request.Url).Groups[1].Value);
                        if (Result == "\x00")
                        {
                            Info.Response.SetStatus(404, "Not Found");
                        }
                        else
                        {
                            byte[] Res = Encoding.UTF8.GetBytes(Result);
                            Info.Response.Headers["Content-Type"] = "application/javascript; charset=utf8";
                            Info.Response.OutputStream.Write(Res, 0, Res.Length);
                        }
                    }
                    else
                    {
                        Info.Response.SetStatus(401, "Unauthorixed");
                    }
                    Info.Response.Send();
                    Info.Close();
                    return;
                }
                if (Info.Request.Url == "/index.html" && !CheckCookie(Info))
                {
                    Info.Response.Headers["Cache-Control"] = "no-cache";
                    HttpContext.Redirect(Info, "/login");
                }
                else if (!new HttpContent().RequestUrl(Info))
                {
                    HttpResponse.NotFound(Info);
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
            
            Info.Close();
        }

    }
}
