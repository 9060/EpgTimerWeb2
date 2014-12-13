﻿using EpgTimer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EpgTimerWeb2
{
    public class Shell
    {
        public static void Run()
        {
            var r2 = new Regex("^Login (.*) (.*)$");
            HttpSession sess = null;
            while (true)
            {
                string CmdText = ((sess != null && HttpSession.IsMatch(sess.SessionKey, "127.0.0.1")) ? sess.SessionKey.Substring(0, 8) + "@127.0.0.1~# " : "unknown@127.0.0.1~$ ");
                if (HttpSession.IsMatch("", "")) CmdText = "root@127.0.0.1~# ";
                Console.Write(CmdText);
                string ApiStr = Console.ReadLine();
                if (ApiStr == null) return;
                if (r2.IsMatch(ApiStr) && sess == null)
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
                    sess = null;
                }
                if (sess == null && Setting.Instance.LoginPassword == "")
                {
                    sess = new HttpSession("", "", "127.0.0.1");
                    if (!sess.CheckAuth(sess.SessionKey, "127.0.0.1"))
                    {
                        Console.WriteLine("\nError Session \n");
                        sess = null;
                        continue;
                    }
                }
                if (r2.IsMatch(ApiStr) && sess != null)
                {
                    Console.WriteLine("....");
                    continue;
                }
                if (ApiStr == "Logout")
                {
                    if (PrivateSetting.Instance.Sessions.Remove(sess) && !HttpSession.IsMatch(sess.SessionKey, "127.0.0.1"))
                        Console.WriteLine("OK {0}....", sess.SessionKey.Substring(0, 12));
                    sess = null;
                    continue;
                }
                if (ApiStr == "Exit")
                {
                    PrivateSetting.Instance.Server.Stop();
                    Console.WriteLine("Disconecting EpgTimer...");
                    if (PrivateSetting.Instance.CmdConnect.StopConnect())
                        Console.WriteLine("Disconected EpgTimer");
                    else
                        Console.WriteLine("Error Disconect EpgTimer");
                    Environment.Exit(0);
                }
                if ((sess != null && HttpSession.IsMatch(sess.SessionKey, "127.0.0.1")))
                {
                    if (ApiStr == "ShowSession")
                    {
                        foreach (var Sess in PrivateSetting.Instance.Sessions)
                        {
                            Console.WriteLine("+ {0}", Sess.SessionKey);
                        }
                        continue;
                    }
                    if (ApiStr == "ShowCacheList")
                    {
                        foreach (var cache in ContentCache.Instance.Cache)
                        {
                            Console.WriteLine("{0}: {1}bytes {2}", cache.Key, cache.Value.Value.Length, cache.Value.ExpireDate.ToString());
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
