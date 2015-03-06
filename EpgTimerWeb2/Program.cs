using EpgTimer;
using System;
using System.IO;
using System.Threading;
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
                        Console.WriteLine("コマンドラインがおかしいです");
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
                                Console.WriteLine("設定ファイル: {0}", val);
                                break;
                            default:
                                Console.WriteLine("コマンドラインがおかしいです");
                                Environment.Exit(1);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("コマンドラインがおかしいです({0})", ex.Message);
                Environment.Exit(1);
            }
            if (File.Exists(PrivateSetting.Instance.ConfigPath))
            {
                Setting.LoadFromXmlFile(PrivateSetting.Instance.ConfigPath);
            }
            else
            {
                Console.WriteLine("設定ファイルがありません");
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
                    Console.WriteLine("EpgTimerから切断...");
                    if (PrivateSetting.Instance.CmdConnect.StopConnect())
                        Console.WriteLine("EpgTimerから切断済み");
                    else
                        Console.WriteLine("EpgTimerから切断できませんでした");
                }
                if (!PrivateSetting.Instance.SetupMode) Setting.SaveToXmlFile(PrivateSetting.Instance.ConfigPath);
            };
            if (PrivateSetting.Instance.SetupMode)
            {
                Console.WriteLine("初期設定を行います。\nのURLに接続してください。http://localhost:" + Setting.Instance.HttpPort);
                Console.WriteLine("PINコード: {0}", PrivateSetting.Instance.SetupCode);
                while (PrivateSetting.Instance.SetupMode) Thread.Sleep(1);
            }
            while (true) Thread.Sleep(100);
        }
    }
}
