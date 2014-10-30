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
                Console.WriteLine("Usage: EpgTimerWeb2.EXE [-http=8080] [-host=127.0.0.1] [-port=4510] [-cb=4521] [-local] [-auth=file.txt]");
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
            Shell.Run();
        }
    }
}
