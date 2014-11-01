using CtrlCmdCLI.Def;
using EpgTimer;
using EpgTimerWeb2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace EpgTimer
{
    public class Api
    {
        private static RecSettingData GetPreset(Dictionary<string, string> Arg)
        {
            RecSettingData pInfo = new RecSettingData();
            pInfo.ServiceMode = (byte)(Arg.ContainsKey("savedata") || Arg.ContainsKey("savecaption") ? 1 : 0);
            if (Arg.ContainsKey("savecaption"))
                pInfo.ServiceMode |= 0x10;
            if (Arg.ContainsKey("savedata"))
                pInfo.ServiceMode |= 0x20;
            pInfo.PittariFlag = (byte)(Arg.ContainsKey("usepittari") ? 1 : 0);
            if (Arg.ContainsKey("suspendmode"))
            {
                if (byte.Parse(Arg["suspendmode"]) < 5 && byte.Parse(Arg["suspendmode"]) > 0)
                    pInfo.SuspendMode = byte.Parse(Arg["suspendmode"]);
            }
            pInfo.UseMargineFlag = (byte)(Arg.ContainsKey("marginestart") && Arg.ContainsKey("margineend") ? 1 : 0);
            if (Arg.ContainsKey("marginestart") && Arg.ContainsKey("margineend"))
            {
                int startSec = 0;
                int startMinus = 1;
                if (Arg["marginestart"].IndexOf("-") == 0)
                {
                    startMinus = -1;
                }
                string[] startArray = Arg["marginestart"].Split(':');
                if (startArray.Length == 2)
                {
                    startSec = Convert.ToInt32(startArray[0]) * 60;
                    startSec += Convert.ToInt32(startArray[1]) * startMinus;
                }
                else if (startArray.Length == 3)
                {
                    startSec = Convert.ToInt32(startArray[0]) * 60 * 60;
                    startSec += Convert.ToInt32(startArray[1]) * 60 * startMinus;
                    startSec += Convert.ToInt32(startArray[2]) * startMinus;
                }
                else
                {
                    startSec = Convert.ToInt32(startArray[0]);
                }

                int endSec = 0;
                int endMinus = 1;
                if (Arg["marginend"].IndexOf("-") == 0)
                {
                    endMinus = -1;
                }
                string[] endArray = Arg["margineend"].Split(':');
                if (endArray.Length == 2)
                {
                    endSec = Convert.ToInt32(endArray[0]) * 60;
                    endSec += Convert.ToInt32(endArray[1]) * endMinus;
                }
                else if (endArray.Length == 3)
                {
                    endSec = Convert.ToInt32(endArray[0]) * 60 * 60;
                    endSec += Convert.ToInt32(endArray[1]) * 60 * endMinus;
                    endSec += Convert.ToInt32(endArray[2]) * endMinus;
                }
                else
                {
                    endSec = Convert.ToInt32(endArray[0]);
                }

                pInfo.StartMargine = startSec;
                pInfo.EndMargine = endSec;
            }
            if (Arg.ContainsKey("usepartial") && Arg.ContainsKey("partialdir"))
            {
                pInfo.PartialRecFlag = 1;
                if (Arg["partialdir"].IndexOf("*") > 0)
                {
                    string[] s = Arg["partialdir"].Split('*');
                    pInfo.PartialRecFolder.AddRange(s.Select(p =>
                    {
                        if (p.Split(':').Length != 3 || p.Split(':').Length != 2) return null;
                        string[] k = p.Split(':');
                        if (!CommonManager.Instance.DB.WritePlugInList.ContainsValue(k[1])) return null;
                        if (k.Length == 3 && !CommonManager.Instance.DB.RecNamePlugInList.ContainsValue(k[2])) return null;
                        return new RecFileSetInfo()
                        {
                            RecNamePlugIn = k.Length == 3 ? k[2] : "",
                            WritePlugIn = k[1],
                            RecFolder = k[0]
                        };
                    }).Where(p => p != null));
                }
                else
                {
                    string p = Arg["partialdir"];
                    if (p.Split(':').Length != 3 || p.Split(':').Length != 2) throw new ArgumentException();
                    string[] k = p.Split(':');
                    if (!CommonManager.Instance.DB.WritePlugInList.ContainsValue(k[1])) throw new ArgumentException();
                    if (k.Length == 3 && !CommonManager.Instance.DB.RecNamePlugInList.ContainsValue(k[2])) throw new ArgumentException();
                    pInfo.PartialRecFolder.Add(new RecFileSetInfo()
                    {
                        RecNamePlugIn = k.Length == 3 ? k[2] : "",
                        WritePlugIn = k[1],
                        RecFolder = k[0]
                    });
                }
                if (pInfo.PartialRecFolder.Count == 0) pInfo.PartialRecFlag = 0;
            }
            if (Arg.ContainsKey("continuerec"))
                pInfo.ContinueRecFlag = 1;
            pInfo.TunerID = 0;
            if (Arg.ContainsKey("tuner") && CommonManager.Instance.DB.TunerReserveList.Values.Where(s => s.tunerID == uint.Parse(Arg["tuner"])).Count() > 0 && uint.Parse(Arg["tuner"]) != 0xFFFFFFFF)
                pInfo.TunerID = uint.Parse(Arg["tuner"]);
            pInfo.Priority = 2;
            if (Arg.ContainsKey("priority") && byte.Parse(Arg["priority"]) >= 1 && byte.Parse(Arg["priority"]) <= 5)
                pInfo.Priority = byte.Parse(Arg["priority"]);
            if (Arg.ContainsKey("recmode") && CommonManager.Instance.RecModeDictionary.Keys.Where(s => byte.Parse(Arg["recmode"]) == s).Count() > 0)
                pInfo.RecMode = byte.Parse(Arg["recmode"]);
            return pInfo;
        }
        public static EpgSearchKeyInfo GetEpgSKey(Dictionary<string, string> Arg)
        {
            var e = new EpgSearchKeyInfo();
            if (Arg["srvlist"].IndexOf(",") > 0)
            {
                e.serviceList = Arg["srvlist"].Split(',').Select(s => long.Parse(s)).ToList();
            }
            else
            {
                if (Arg["srvlist"] == "*")
                {
                    foreach (var ch in ChSet5.Instance.ChList)
                    {
                        e.serviceList.Add((long)ch.Value.Key);
                    }
                }
                else
                {
                    e.serviceList.Add(long.Parse(Arg["srvlist"]));
                }
            }
            if (Arg.ContainsKey("content") && Arg["content"].IndexOf(".") > 0)
            {
                if (Arg["content"].IndexOf(",") > 0)
                {
                    e.contentList.AddRange(Arg["content"].Split(',').Select(s =>
                    {
                        string[] c = s.Split('.');
                        if (c.Length != 4) return null;
                        return new EpgContentData()
                        {
                            content_nibble_level_1 = byte.Parse(c[0]),
                            content_nibble_level_2 = byte.Parse(c[1]),
                            user_nibble_1 = byte.Parse(c[2]),
                            user_nibble_2 = byte.Parse(c[3])
                        };
                    }).Where(s => s != null));
                }
                else
                {
                    string[] c = Arg["content"].Split('.');
                    if (c.Length == 4)
                    {
                        e.contentList.Add(new EpgContentData()
                        {
                            content_nibble_level_1 = byte.Parse(c[0]),
                            content_nibble_level_2 = byte.Parse(c[1]),
                            user_nibble_1 = byte.Parse(c[2]),
                            user_nibble_2 = byte.Parse(c[3])
                        });
                    }
                }
            }
            else
            {
                e.notContetFlag = 1;
            }
            if (Arg.ContainsKey("notcontent")) e.notContetFlag = 1;
            if (Arg.ContainsKey("useregex")) e.regExpFlag = 1;
            if (Arg.ContainsKey("useregex")) e.aimaiFlag = 0;
            if (Arg.ContainsKey("aimai")) e.aimaiFlag = 1;
            if (Arg.ContainsKey("aimai")) e.regExpFlag = 0;
            if (Arg.ContainsKey("tonly")) e.titleOnlyFlag = 1;
            if (Arg.ContainsKey("kw"))
            {
                e.andKey = Arg["kw"];
            }
            if (Arg.ContainsKey("notkw"))
            {
                e.notKey = Arg["notkw"];
            }
            if (Arg.ContainsKey("freeca"))
            {
                e.freeCAFlag = byte.Parse(Arg["freeca"]);
            }
            if (Arg.ContainsKey("date"))
            {
                if (Arg["date"].IndexOf(",") > 0)
                {
                    e.dateList.AddRange(Arg["date"].Split(',').Select(s =>
                    {
                        if (s.IndexOf("-") < 0) return null;
                        if (s.Split('-')[0].IndexOf(".") < 0 || s.Split('-')[1].IndexOf(".") < 0) return null;
                        string[] a = s.Split('-');
                        string[] b = a[0].Split('.');
                        string[] c = a[1].Split('.');
                        if (uint.Parse(a[1]) > 24 || uint.Parse(b[1]) > 24 || uint.Parse(a[2]) > 60 || uint.Parse(b[2]) > 60 || uint.Parse(a[0]) > 7 || uint.Parse(b[0]) > 7) return null;
                        return new EpgSearchDateInfo()
                        {
                            startDayOfWeek = byte.Parse(a[0]),
                            startHour = ushort.Parse(a[1]),
                            startMin = ushort.Parse(a[2]),
                            endDayOfWeek = byte.Parse(b[0]),
                            endHour = ushort.Parse(b[1]),
                            endMin = ushort.Parse(b[2])
                        };
                    }).Where(p => p != null));
                }
                else
                {
                    string s = Arg["date"];
                    if (s.IndexOf("-") < 0) return null;
                    if (s.Split('-')[0].IndexOf(".") < 0 || s.Split('-')[1].IndexOf(".") < 0) return null;
                    string[] a = s.Split('-');
                    string[] b = a[0].Split('.');
                    string[] c = a[1].Split('.');
                    if (uint.Parse(c[1]) > 24 || uint.Parse(b[1]) > 24 || uint.Parse(c[2]) > 60 || uint.Parse(b[2]) > 60 || uint.Parse(c[0]) > 7 || uint.Parse(b[0]) > 7) return null;
                    e.dateList.Add(new EpgSearchDateInfo()
                    {
                        startDayOfWeek = byte.Parse(b[0]),
                        startHour = ushort.Parse(b[1]),
                        startMin = ushort.Parse(b[2]),
                        endDayOfWeek = byte.Parse(c[0]),
                        endHour = ushort.Parse(c[1]),
                        endMin = ushort.Parse(c[2])
                    });
                    Debug.Print("OK");
                }
            }
            if (Arg.ContainsKey("notdate"))
            {
                e.notDateFlag = 1;
            }
            return e;
        }
        public static string Call(string Str)
        {
            string JsonData = "[]";
            try
            {
                string Command = Str;
                var Arg = new Dictionary<string, string>();
                if (Str.IndexOf("/") > 0)
                {
                    Command = Str.Split('/')[0];
                    var ArgStr = Str.Substring(Str.IndexOf("/") + 1);
                    foreach (var ArgTemp in ArgStr.Split('/'))
                    {
                        if (ArgTemp.IndexOf("=") > 0)
                        {
                            var Name = ArgTemp.Substring(0, ArgTemp.IndexOf("="));
                            var Val = ArgTemp.Substring(ArgTemp.IndexOf("=") + 1);
                            Arg[Name.ToLower()] = Uri.UnescapeDataString(Val);
                        }
                        else
                        {
                            Arg[ArgTemp.ToLower()] = "0";
                        }
                    }
                }

                if (Command == "EnumReserve")
                {
                    JsonData = JsonUtil.Serialize(CommonManager.Instance.DB.ReserveList.Values.Select(x => new ReserveItem(x)).ToList());
                }
                else if (Command == "EnumService")
                {
                    JsonData = JsonUtil.Serialize(ChSet5.Instance.ChList.Values);
                }
                else if (Command == "EnumRecFileInfo")
                {
                    JsonData = JsonUtil.Serialize(CommonManager.Instance.DB.RecFileInfo.Values.Select(x => new RecInfoItem(x)).ToList());
                }
                else if (Command == "EnumServiceEvent")
                {
                    int MaxHour = 6;
                    ulong Key = 0;
                    DateTime Start = DateTime.Now;
                    if (Arg.ContainsKey("maxhour"))
                    {
                        MaxHour = int.Parse(Arg["maxhour"]);
                    }
                    if (Arg.ContainsKey("key"))
                    {
                        Key = ulong.Parse(Arg["key"]);
                    }
                    if (Arg.ContainsKey("unixstart"))
                    {
                        Start = UnixTime.FromUnixTime(long.Parse(Arg["unixstart"]));
                        if (long.Parse(Arg["unixstart"]) < UnixTime.ToUnixTime(DateTime.Now) - (DateTime.Now.Minute * 60) - DateTime.Now.Second)
                        {
                            Start = DateTime.Now.AddMinutes(-1 * DateTime.Now.Minute).AddSeconds(-1 * DateTime.Now.Second); //変な時間対策
                        }
                    }
                    var Out = new Dictionary<ulong, List<EventInfoItem>>();
                    foreach (var a in CommonManager.Instance.DB.ServiceEventList)
                    {
                        if (Key != 0 && a.Key != Key) continue;
                        Out.Add(
                            a.Key,
                            //1: 指定された時間よりも前にある
                            //2: 終わっていない
                            a.Value.eventList.Where(b => b.start_time.AddSeconds(b.start_time.Second * -1) < Start.AddHours(MaxHour))
                                .Where(c => c.start_time.AddSeconds(c.durationSec - 1) > Start)
                                .OrderBy(d => d.start_time)
                                .Select(e => new EventInfoItem(e))
                                .ToList()
                            );
                    }
                    JsonData = JsonUtil.Serialize(Out);
                }
                else if (Command == "EnumContentKindList1")
                {
                    JsonData = JsonUtil.Serialize(CommonManager.Instance.ContentKindDictionary);
                }
                else if (Command == "EnumContentKindList2")
                {
                    JsonData = JsonUtil.Serialize(CommonManager.Instance.ContentKindDictionary2);
                }
                else if (Command == "EnumWritePlugInList")
                {
                    JsonData = JsonUtil.Serialize(CommonManager.Instance.DB.WritePlugInList);
                }
                else if (Command == "EnumRecNamePlugInList")
                {
                    JsonData = JsonUtil.Serialize(CommonManager.Instance.DB.RecNamePlugInList);
                }
                else if (Command == "EnumEpgAutoAddList")
                {
                    JsonData = JsonUtil.Serialize(CommonManager.Instance.DB.EpgAutoAddList);
                }
                else if (Command == "EpgCapNow")
                {
                    CommonManager.Instance.CtrlCmd.SendEpgCapNow();
                }
                else if (Command == "EpgReload")
                {
                    CommonManager.Instance.CtrlCmd.SendReloadEpg();
                    Thread.Sleep(500);
                    CommonManager.Instance.DB.ReloadEpgData();
                }
                else if (Command == "EnumTunerReserve")
                {
                    List<TunerReserveInfo> reserves = new List<TunerReserveInfo>();
                    if (CommonManager.Instance.CtrlCmd.SendEnumTunerReserve(ref reserves)
                         == (uint)ErrCode.CMD_SUCCESS)
                    {
                        JsonData = JsonUtil.Serialize(reserves);
                    }
                }
                else if (Command == "GetSetting")
                {
                    JsonData = JsonUtil.Serialize(Setting.Instance);
                }
                else if (Command == "GetCommonManager")
                {
                    JsonData = JsonUtil.Serialize(CommonManagerJson.Instance);
                }
                else if (Command == "AddReserve")
                {
                    if (Arg.ContainsKey("eventname") && Arg.ContainsKey("starttime")
                        && Arg.ContainsKey("durationsec") && Arg.ContainsKey("tsid")
                        && Arg.ContainsKey("onid") && Arg.ContainsKey("sid")
                        && Arg.ContainsKey("eid") && Arg.ContainsKey("preset")) //最低限必要
                    {
                        ushort ONID = ushort.Parse(Arg["onid"]);
                        ushort SID = ushort.Parse(Arg["sid"]);
                        ushort TSID = ushort.Parse(Arg["tsid"]);
                        ushort EventID = ushort.Parse(Arg["eid"]);
                        ulong Key = CommonManager.Create64Key(ONID, TSID, SID);
                        ReserveData rd = new ReserveData()
                        {
                            StartTime = UnixTime.FromUnixTime(long.Parse(Arg["starttime"])),
                            StartTimeEpg = UnixTime.FromUnixTime(long.Parse(Arg["starttime"])),
                            OriginalNetworkID = ONID,
                            ServiceID = SID,
                            TransportStreamID = TSID,
                            EventID = EventID,
                            DurationSecond = 10 * 60
                        };
                        if (ChSet5.Instance.ChList.ContainsKey(Key)) rd.StationName = ChSet5.Instance.ChList[Key].ServiceName;
                        if (Arg["eventname"] != "") rd.Title = Arg["eventname"];
                        if (Arg["durationsec"] != "") rd.DurationSecond = uint.Parse(Arg["durationsec"]);
                        RecSettingData setInfo = new RecSettingData();
                    }
                }
                else if (Command == "EnumPresets")
                {
                    JsonData = JsonUtil.Serialize(PresetDb.Instance.Presets);
                }
                else if (Command == "AddPreset")
                {
                    if (!Arg.ContainsKey("name")) return JsonData;
                    uint ID = PresetDb.Instance.AddPreset(GetPreset(Arg), Arg["name"]);
                    JsonData = JsonUtil.Serialize(PresetDb.Instance.Presets[ID]);
                }
                else if (Command == "EpgSearch")
                {
                    if (!Arg.ContainsKey("srvlist")) return JsonData;
                    List<EpgEventInfo> EpgResult = new List<EpgEventInfo>();
                    List<EpgSearchKeyInfo> Search = new List<EpgSearchKeyInfo>();
                    Search.Add(GetEpgSKey(Arg));
                    CommonManager.Instance.CtrlCmd.SendSearchPg(Search, ref EpgResult);
                    JsonData = JsonUtil.Serialize(EpgResult);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return JsonData;
        }
    }
}
