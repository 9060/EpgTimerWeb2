﻿using EpgTimerWeb2;
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
        static Regex r = new Regex(@"^\/api\/(.*)\/json\/(.*)\/run\/(.*)$");
        static Regex r1 = new Regex(@"^\/api\/(.*)\/run\/(.*)$");
        static Regex r2 = new Regex(@"^\/auth\/(.*)\=(.*)$");
        public static void SetupProcess(HttpContext Context)
        {
            if (Context.Request.Url.StartsWith("/do"))
            {
                bool OK = false;
                var Param = HttpUtility.ParseQueryString(Context.Request.GetParam);
                byte[] Form;
                if (Param["code"] == PrivateSetting.Instance.SetupCode)
                {
                    try
                    {
                        if(Param["ctrlhost"] == null || Param["ctrlport"] == null ||
                           Param["cbport"] == null || Param["http"] == null ||
                           Param["authfile"] == null) throw new Exception("Bad Param");
                        string Host = Param["ctrlhost"];
                        int CtrlPort = int.Parse(Param["ctrlport"]);
                        int CbPort = int.Parse(Param["cbport"]);
                        int HttpPort = int.Parse(Param["http"]);
                        string Auth = Param["authfile"];
                        if (PrivateSetting.Instance.CmdConnect.StartConnect(Host, CbPort, CtrlPort) && File.Exists(Auth))
                        {
                            Setting.Instance.HttpPort = (uint)HttpPort;
                            Setting.Instance.CtrlHost = Host;
                            Setting.Instance.CtrlPort = (uint)CtrlPort;
                            Setting.Instance.CallbackPort = (uint)CbPort;
                            Setting.Instance.AuthFilePath = Auth;
                            Form = Encoding.UTF8.GetBytes("Success please restart");
                            OK = true;
                        }
                        else
                        {
                            throw new Exception("Bad Connect or Auth File Not Found");
                        }
                    }
                    catch (Exception ex)
                    {
                        Form = Encoding.Default.GetBytes(ex.Message);
                    }
                }
                else
                {
                    Form = Encoding.UTF8.GetBytes("Invalid Code");
                }
                Context.Response.OutputStream.Write(Form, 0, Form.Length);
                Context.Response.Headers["Content-Type"] = "text/html";
                Context.Response.Send();
                Context.Close();
                if (OK)
                {
                    PrivateSetting.Instance.CmdConnect.StopConnect();
                    Setting.SaveToXmlFile(PrivateSetting.Instance.ConfigPath);
                    PrivateSetting.Instance.SetupMode = false;
                }
            }
            else
            {
                byte[] Form = Encoding.UTF8.GetBytes(@"
<html>
 <head>
  <title>Setup</title>
 </head>
 <body>
  <h1>EpgTimerWeb2 Setup</h1>
  <form action='/do' method='get'>
   <p>EDCB Server:<input name='ctrlhost' placeholder='127.0.0.1' value='127.0.0.1' /></p>
   <p>EDCB Port:<input name='ctrlport' placeholder='4510' value='4510' /></p>
   <p>Callback Port:<input name='cbport' placeholder='4521' value='4521' /></p>
   <p>Auth File:<input name='authfile' placeholder='auth.txt'  /></p>
   <p>Http Port:<input name='http' placeholder='8080' value='8080' /></p>
   <p>Code:<input name='code' /></p>
   <p><input type='submit' value='SAVE' /></p>
  </form>
 </body>
</html>
".Replace("'", "\""));
                Context.Response.OutputStream.Write(Form, 0, Form.Length);
                Context.Response.Headers["Content-Type"] = "text/html";
                Context.Response.Send();
                Context.Close();
            }
        }
        public static void DoProcess(TcpClient Client)
        {
            var Info = new HttpContext(Client);
            if (Info.Request.Url == "/") Info.Request.Url = "/index.html";
            if (PrivateSetting.Instance.SetupMode)
            {
                SetupProcess(Info);
                return;
            }
            if (Info.Request.Url == "/ws") //WebSocket
            {
                WebSocket.HandshakeResponseSend(Info);
                SocketAction.Process(Info);
                Info.Close();
                return;
            }
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
                Info.Response.Send();
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
                Info.Response.Send(); //HttpResponseGenerater.SendResponse(Info);
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
                Info.Response.Send();
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
                    //HttpResponseGenerater.SendResponse(Info);
                    Info.Response.Send();
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
                Info.Response.StatusCode = 500;
                Info.Response.StatusText = "Error";
                Info.Response.OutputStream.Write(NotFound, 0, NotFound.Length);
                //HttpResponseGenerater.SendResponse(Info);
                Info.Response.Send();
                Console.WriteLine("\n!!!! Exception !!!!");
                
            }
            if (Info.Request.Headers.ContainsKey("connection") &&
                Info.Request.Headers["connection"].ToLower() == "keep-alive") DoProcess(Client); //KeepAlive対応(適当)
            Info.Close();
        }
        
    }
}
