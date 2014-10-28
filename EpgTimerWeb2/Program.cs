using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CtrlCmdCLI;
using CtrlCmdCLI.Def;
using EpgTimer;
using System.Threading;
using System.Text.RegularExpressions;
namespace EpgTimerWeb2
{
    class Program
    {
        public static ConsoleColor Default = ConsoleColor.DarkGray;
        static void Main(string[] args)
        {
            Default = Console.ForegroundColor;
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: EpgTimerWeb2 [-http=8080] [-host=127.0.0.1] [-port=4510] [-cb=4521] [-local] [-auth=file.txt]");
                Console.WriteLine(" -auth Option: File Format -> user:pass");
                Environment.Exit(1);
                return;
            }
            Console.ForegroundColor = ConsoleColor.Red;
            try
            {
                foreach (string arg in args)
                {
                    if (!arg.StartsWith("-"))
                    {
                        Console.WriteLine("Invalid Paramatar");
                        Environment.Exit(1);
                    }
                    string varg = arg.Substring(1);
                    if (varg.IndexOf("=") > 0)
                    {
                        string val = varg.Substring(varg.IndexOf("=") + 1);
                        switch (varg.Substring(0, varg.IndexOf("=")))
                        {
                            case "http":
                                Setting.Instance.HttpPort = UInt32.Parse(val);
                                Console.WriteLine("HttpPort: {0}", val);
                                break;
                            case "host":
                                Setting.Instance.CtrlHost = val;
                                Console.WriteLine("CtrlHost: {0}", val);
                                break;
                            case "port":
                                Setting.Instance.CtrlPort = UInt32.Parse(val);
                                Console.WriteLine("CtrlPort: {0}", val);
                                break;
                            case "cb":
                                Setting.Instance.CallbackPort = UInt32.Parse(val);
                                Console.WriteLine("CallbackPort: {0}", val);
                                break;
                            case "auth":
                                PrivateSetting.Instance.AuthFilePath = val;
                                Console.WriteLine("Auth File: {0}", val);
                                break;
                            default:
                                Console.WriteLine("Invalid Paramatar");
                                Environment.Exit(1);
                                break;
                        }
                    }
                    else
                    {
                        switch (varg)
                        {
                            case "local":
                                Setting.Instance.LocalMode = true;
                                Console.WriteLine("Use LocalMode(LocationFree Server)");
                                break;
                            default:
                                Console.WriteLine("Invalid Paramatar");
                                Environment.Exit(1);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invalid Paramatar");
                Environment.Exit(1);
            }
            Console.ForegroundColor = Default;
            Console.WriteLine("Connecting...");
            Setting.Instance.CmdConnect = new CtrlCmdConnect();
            if (!Setting.Instance.CmdConnect.StartConnect(Setting.Instance.CtrlHost, (int)Setting.Instance.CallbackPort, (int)Setting.Instance.CtrlPort))
            {
                Console.WriteLine("Server {0}:{1} ({2}) Connect Failed", Setting.Instance.CtrlHost, Setting.Instance.CtrlPort, Setting.Instance.CallbackPort);
                Environment.Exit(1);
            }
            Console.WriteLine("Loading Data...");
            CommonManager.Instance.DB.ClearAllDB();
            CommonManager.Instance.DB.ReloadEpgAutoAddInfo();
            CommonManager.Instance.DB.ReloadEpgData();
            CommonManager.Instance.DB.ReloadManualAutoAddInfo();
            CommonManager.Instance.DB.ReloadPlugInFile();
            CommonManager.Instance.DB.ReloadrecFileInfo();
            CommonManager.Instance.DB.ReloadReserveInfo();
            Console.WriteLine("Loaded Data");
            if (PrivateSetting.Instance.AuthFilePath != "")
            {
                PrivateSetting.Instance.Passwords = PasswordPair.LoadFile(PrivateSetting.Instance.AuthFilePath);
                if (PrivateSetting.Instance.Passwords == null)
                {
                    Console.WriteLine("Invalid Auth Data");
                    Environment.Exit(1);
                }
            }
            Setting.Instance.Server = new WebServer((int)Setting.Instance.HttpPort);
            Setting.Instance.Server.Start();
            Console.CancelKeyPress += (a, b) =>
            {
                Setting.Instance.Server.Stop();
                Console.WriteLine("Disconecting EpgTimer...");
                if (Setting.Instance.CmdConnect.StopConnect())
                    Console.WriteLine("Disconected EpgTimer");
                else
                    Console.WriteLine("Error Disconect EpgTimer");
            };
            var r = new Regex("^ShowPassword (.*)$");
            var r2 = new Regex("^Login (.*) (.*)$");
            HttpSession sess = null;
            while (true)
            {
                string CmdText = ((sess != null && HttpSession.IsMatch(sess.SessionKey, "127.0.0.1")) ? sess.SessionKey.Substring(0, 8) + "@127.0.0.1~# " : "unknown@127.0.0.1~$ ");
                if (HttpSession.IsMatch("", "")) CmdText = "user@127.0.0.1~# ";
                Console.Write(CmdText);
                string ApiStr = Console.ReadLine();
                if (ApiStr == null) return;
                if (r2.IsMatch(ApiStr) && PrivateSetting.Instance.Passwords != null && sess == null)
                {
                    var match = r2.Match(ApiStr);
                    string Pass = match.Groups[2].Value;
                    string Id = match.Groups[1].Value;
                    sess = new HttpSession(Id, Pass, "127.0.0.1");
                    if (sess.CheckAuth(sess.SessionKey, "127.0.0.1"))
                    {
                        Console.WriteLine("OK {0}", sess.SessionKey);
                        continue;
                    }
                }
                if (r2.IsMatch(ApiStr) && sess != null)
                {
                    Console.WriteLine("....");
                    continue;
                }
                if (ApiStr == "Logout" && PrivateSetting.Instance.Passwords != null)
                {
                    if(PrivateSetting.Instance.Sessions.Remove(sess) && !HttpSession.IsMatch(sess.SessionKey, "127.0.0.1"))
                        Console.WriteLine("OK {0}....", sess.SessionKey.Substring(0, 12));
                    sess = null;
                    continue;
                }
                if (ApiStr == "Exit")
                {
                    Setting.Instance.Server.Stop();
                    Console.WriteLine("Disconecting EpgTimer...");
                    if (Setting.Instance.CmdConnect.StopConnect())
                        Console.WriteLine("Disconected EpgTimer");
                    else
                        Console.WriteLine("Error Disconect EpgTimer");
                    Environment.Exit(0);
                }
                if ((sess != null && HttpSession.IsMatch(sess.SessionKey, "127.0.0.1")) || HttpSession.IsMatch("", ""))
                {
                    if ((ApiStr == "ShowSession" || ApiStr == "ShowPassword") && PrivateSetting.Instance.Passwords == null) continue;
                    if (ApiStr == "ShowSession")
                    {
                        foreach (var Sess in PrivateSetting.Instance.Sessions)
                        {
                            Console.WriteLine("+ {0}", Sess.SessionKey);
                        }
                        continue;
                    }
                    if (ApiStr == "ShowPassword")
                    {
                        foreach (var Pass in PrivateSetting.Instance.Passwords)
                        {
                            Console.WriteLine("+ {0} : {1}", Pass.UserId, Pass.Password);
                        }
                        continue;
                    }
                    Console.WriteLine(Api.Call(ApiStr));
                }
                else
                {
                    Console.WriteLine("Error: No auth");
                }
            }
        }
    }
}
