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
using System.Diagnostics;
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
                CtrlCmdConnect.Connect();
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
                    Console.WriteLine("Disconnecting EpgTimer...");
                    if (PrivateSetting.Instance.CmdConnect.StopConnect())
                        Console.WriteLine("Disconnected EpgTimer");
                    else
                        Console.WriteLine("Error Disconnect EpgTimer");
                }
                if (!PrivateSetting.Instance.SetupMode) Setting.SaveToXmlFile(PrivateSetting.Instance.ConfigPath);
            };
            if (PrivateSetting.Instance.SetupMode)
            {
                Console.WriteLine("Please access to http://localhost:" + Setting.Instance.HttpPort);
                Console.WriteLine("Code: {0}", PrivateSetting.Instance.SetupCode);
                while (PrivateSetting.Instance.SetupMode) Thread.Sleep(1);
            }
            Shell.Run();
        }
    }
}
