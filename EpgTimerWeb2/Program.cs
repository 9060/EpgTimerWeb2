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
using System.IO;
namespace EpgTimerWeb2
{
    class Program
    {
        public static ConsoleColor Default = ConsoleColor.DarkGray;
        public static void Main(string[] args)
        {
            Default = Console.ForegroundColor;

            Console.WriteLine("Usage: EpgTimerWeb2.EXE [-cfg=EpgTimerWeb2.xml]");
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
                            case "cfg":
                                PrivateSetting.Instance.ConfigPath = val;
                                Console.WriteLine("Config File: {0}", val);
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
            if (File.Exists(PrivateSetting.Instance.ConfigPath))
            {
                Setting.LoadFromXmlFile(PrivateSetting.Instance.ConfigPath);
            }
            else
            {
                Console.WriteLine("Not found config file!");
                PrivateSetting.Instance.SetupMode = true;
            }
            Console.ForegroundColor = Default;
            if (!PrivateSetting.Instance.SetupMode)
            {
                Console.WriteLine("Connecting...");
                if (!PrivateSetting.Instance.CmdConnect.
                    StartConnect(Setting.Instance.CtrlHost, (int)Setting.Instance.CallbackPort, (int)Setting.Instance.CtrlPort))
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
            }
            var r = new Random(new Random().Next(100));
            for (int i = 0; i < 5; i++)
            {
                PrivateSetting.Instance.SetupCode += r.Next(0 + r.Next(0, 5), 9).ToString();
            }
            PrivateSetting.Instance.Server = new WebServer((int)Setting.Instance.HttpPort);
            PrivateSetting.Instance.Server.Start();
            Console.CancelKeyPress += (a, b) =>
            {
                PrivateSetting.Instance.Server.Stop();
                if (!PrivateSetting.Instance.SetupMode)
                {
                    Console.WriteLine("Disconecting EpgTimer...");
                    if (PrivateSetting.Instance.CmdConnect.StopConnect())
                        Console.WriteLine("Disconected EpgTimer");
                    else
                        Console.WriteLine("Error Disconect EpgTimer");
                }
                if (!PrivateSetting.Instance.SetupMode) Setting.SaveToXmlFile(PrivateSetting.Instance.ConfigPath);
            };
            if (PrivateSetting.Instance.SetupMode)
            {
                Console.WriteLine("Please access to http://localhost:" + Setting.Instance.HttpPort);
                Console.WriteLine("Code: {0}", PrivateSetting.Instance.SetupCode);
                while (true) Thread.Sleep(1);
            }
            else
            {
                Shell.Run();
            }
        }
    }
}
