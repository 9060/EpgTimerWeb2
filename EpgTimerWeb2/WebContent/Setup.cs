using System;
using System.Text;
using System.Web;

namespace EpgTimer
{
    public class Setup
    {
        public static void SetupProcess(HttpContext Context)
        {
            if (Context.Request.Url.StartsWith("/update"))
            {
                bool OK = false;
                var Param = HttpUtility.ParseQueryString(Context.Request.PostString);
                byte[] Form;
                if (Param["code"] == PrivateSetting.Instance.SetupCode)
                {
                    try
                    {
                        if (Param["ctrlhost"] == null || Param["ctrlport"] == null ||
                            Param["cbport"] == null || Param["http"] == null ||
                            Param["user"] == null || Param["pass"] == null)
                            throw new Exception("Bad Param");
                        string Host = Param["ctrlhost"];
                        int CtrlPort = int.Parse(Param["ctrlport"]);
                        int CbPort = int.Parse(Param["cbport"]);
                        int HttpPort = int.Parse(Param["http"]);
                        if (PrivateSetting.Instance.CmdConnect.StartConnect(Host, CbPort, CtrlPort))
                        {
                            Setting.Instance.HttpPort = (uint)HttpPort;
                            Setting.Instance.CtrlHost = Host;
                            Setting.Instance.CtrlPort = (uint)CtrlPort;
                            Setting.Instance.CallbackPort = (uint)CbPort;
                            if (Param["user"] != null && Param["pass"] != null)
                            {
                                Setting.Instance.LoginUser = Param["user"];
                                Setting.Instance.LoginPassword = Param["pass"];
                            }
                            Form = Encoding.UTF8.GetBytes("<html>\n<head></head>\n<body onload=\"setTimeout(function(){location.href = 'http://' + location.hostname + ':" + HttpPort + "\';}, 1500);\">\nplease wait....\n</body>\n</html>");
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
                if (OK)
                {
                    Setting.SaveToXmlFile(PrivateSetting.Instance.ConfigPath);
                    PrivateSetting.Instance.CmdConnect.StopConnect();
                    CtrlCmdConnect.Connect();
                    Context.Response.OutputStream.Write(Form, 0, Form.Length);
                    Context.Response.Headers["Content-Type"] = "text/html";
                    Context.Response.Send();
                    Context.Close();
                    PrivateSetting.Instance.SetupMode = false;
                    PrivateSetting.Instance.Server.Stop();
                    PrivateSetting.Instance.Server = new WebServer((int)Setting.Instance.HttpPort);
                    PrivateSetting.Instance.Server.Start();
                }
                else
                {
                    Context.Response.OutputStream.Write(Form, 0, Form.Length);
                    Context.Response.Headers["Content-Type"] = "text/html";
                    Context.Response.Send();
                    Context.Close();
                }
            }
            else
            {
                byte[] Form = Encoding.UTF8.GetBytes(@"
<html>
\t<head>
\t\t<title>Setup</title>
\t</head>
\t<body>
\t\t<h1>EpgTimerWeb2 Configure</h1>
\t\t<form action='/update' method='post'>
\t\t\t<p>EDCB Server:<input name='ctrlhost' placeholder='127.0.0.1' value='127.0.0.1' /></p>
\t\t\t<p>EDCB Port:<input name='ctrlport' placeholder='4510' value='4510' /></p>
\t\t\t<p>Callback Port:<input name='cbport' placeholder='4521' value='4521' /></p>
\t\t\t<p>Username:<input name='user' placeholder='user'  /></p>
\t\t\t<p>Password:<input name='pass' placeholder='pass'  /></p>
\t\t\t<p>Http Port:<input name='http' placeholder='8080' value='8080' /></p>
\t\t\t<p>Pin Code:<input name='code' /></p>
\t\t\t<p><input type='submit' value='Update config' /></p>
\t\t</form>
\t</body>
</html>
".Replace("'", "\""));
                Context.Response.OutputStream.Write(Form, 0, Form.Length);
                Context.Response.Headers["Content-Type"] = "text/html";
                Context.Response.Headers["Cache-Control"] = "no-cache";
                Context.Response.Send();
                Context.Close();
            }
        }
    }
}
