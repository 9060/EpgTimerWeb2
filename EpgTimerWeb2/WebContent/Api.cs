using CtrlCmdCLI.Def;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace EpgTimer
{
    public class Api
    {
        private static RecSettingData GetPreset(Dictionary<string, string> Arg, RecSettingData Old)
        {
            RecSettingData pInfo = Old;
            pInfo.ServiceMode = (byte)((Arg.ContainsKey("savedata") || Arg.ContainsKey("savecaption")) || Arg.ContainsKey("nousedata") ? 1 : 0);
            if (Arg.ContainsKey("savecaption"))
                pInfo.ServiceMode |= 0x10;
            if (Arg.ContainsKey("savedata"))
                pInfo.ServiceMode |= 0x20;
            pInfo.PittariFlag = (byte)(Arg.ContainsKey("usepittari") ? 1 : 0);
            pInfo.TuijyuuFlag = (byte)(Arg.ContainsKey("usetuijyuu") ? 1 : 0);
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
                    startSec = int.Parse(startArray[0]) * 60;
                    startSec += int.Parse(startArray[1]) * startMinus;
                }
                else if (startArray.Length == 3)
                {
                    startSec = int.Parse(startArray[0]) * 60 * 60;
                    startSec += int.Parse(startArray[1]) * 60 * startMinus;
                    startSec += int.Parse(startArray[2]) * startMinus;
                }
                else
                {
                    startSec = int.Parse(startArray[0]);
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
                    endSec = int.Parse(endArray[0]) * 60;
                    endSec += int.Parse(endArray[1]) * endMinus;
                }
                else if (endArray.Length == 3)
                {
                    endSec = int.Parse(endArray[0]) * 60 * 60;
                    endSec += int.Parse(endArray[1]) * 60 * endMinus;
                    endSec += int.Parse(endArray[2]) * endMinus;
                }
                else
                {
                    endSec = int.Parse(endArray[0]);
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
            pInfo.RecMode = 1;
            if (Arg.ContainsKey("recmode") && byte.Parse(Arg["recmode"]) >= 0 && byte.Parse(Arg["recmode"]) <= 5)
                pInfo.RecMode = byte.Parse(Arg["recmode"]);
            return pInfo;
        }
        public static EpgSearchKeyInfo GetEpgSKey(Dictionary<string, string> Arg)
        {
            var e = new EpgSearchKeyInfo();
            if (Arg.ContainsKey("srvlist"))
            {
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
            }
            else
            {
                foreach (var ch in ChSet5.Instance.ChList)
                {
                    e.serviceList.Add((long)ch.Value.Key);
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
                        if (uint.Parse(c[1]) > 24 || uint.Parse(b[1]) > 24 || uint.Parse(c[2]) > 60 || uint.Parse(b[2]) > 60 || uint.Parse(c[0]) > 7 || uint.Parse(b[0]) > 7) return null;
                        return new EpgSearchDateInfo()
                        {
                            startDayOfWeek = byte.Parse(b[0]),
                            startHour = ushort.Parse(b[1]),
                            startMin = ushort.Parse(b[2]),
                            endDayOfWeek = byte.Parse(c[0]),
                            endHour = ushort.Parse(c[1]),
                            endMin = ushort.Parse(c[2])
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
                    //Debug.Print("OK");
                }
            }
            if (Arg.ContainsKey("notdate"))
            {
                e.notDateFlag = 1;
            }
            return e;
        }
        private static Dictionary<string, string> ParseArgs(string Command)
        {
            var Arg = new Dictionary<string, string>();
            var ArgStr = Command.Substring(Command.IndexOf("/") + 1);
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
                    Arg[ArgTemp.ToLower()] = "";
                }
            }
            return Arg;
        }
        public static string Call(string Str, bool Indent = true, bool NotUseCache = false)
        {
            string JsonData = "";
            JsonResult Data = new JsonResult(null, false, ErrCode.CMD_NO_ARG);
            try
            {
                string Command = Str;
                var Arg = new Dictionary<string, string>();
                if (Str.IndexOf("/") > 0)
                {
                    Command = Str.Split('/')[0];
                    Arg = ParseArgs(Str);
                }

                if (Command == "EnumReserve")
                {
                    Data = new JsonResult(CommonManager.Instance.DB.ReserveList.Values);
                }
                else if (Command == "EnumService")
                {
                    Data = new JsonResult(ChSet5.Instance.ChList.Values);
                }
                else if (Command == "EnumRecFileInfo")
                {
                    Data = new JsonResult(CommonManager.Instance.DB.RecFileInfo.Values);
                }
                else if (Command == "GenerateEpgHTML")
                {
                    int MaxHour = 6;
                    DateTime Start = DateTime.Now;
                    List<ulong> ServiceKeys = null;
                    int MinSize = 5;
                    bool EpgCapOnly = false;
                    bool SetBgColor = false;
                    EpgSearchKeyInfo search = null;
                    if (Arg.ContainsKey("maxhour"))
                    {
                        MaxHour = int.Parse(Arg["maxhour"]);
                    }
                    if (Arg.ContainsKey("unixstart"))
                    {
                        Start = UnixTime.FromUnixTime(long.Parse(Arg["unixstart"]));
                        if (Start < DateTime.Now.AddMinutes(DateTime.Now.Minute * -1).AddSeconds(DateTime.Now.Second * -1).AddSeconds(-1))
                        {
                            Start = DateTime.Now.AddMinutes(-1 * DateTime.Now.Minute).AddSeconds(-1 * DateTime.Now.Second); //変な時間対策
                            Debug.Print("変な時間発動");
                        }
                    }
                    if (Arg.ContainsKey("services"))
                    {
                        ServiceKeys = new List<ulong>();
                        if (Arg["services"].IndexOf(",") > 0)
                        {
                            ServiceKeys.AddRange(Arg["services"].Split(',').Select(s => ulong.Parse(s)));
                        }
                        else
                        {
                            ServiceKeys.Add(ulong.Parse(Arg["services"]));
                        }
                    }
                    if (Arg.ContainsKey("minsize"))
                    {
                        MinSize = int.Parse(Arg["minsize"]);
                    }
                    if (Arg.ContainsKey("epgcaponly"))
                        EpgCapOnly = true;
                    if (Arg.ContainsKey("setbg"))
                        SetBgColor = true;
                    if (Arg.ContainsKey("search"))
                        search = GetEpgSKey(Arg);
                    string Page = EpgPage.Generate(Start, MaxHour, ServiceKeys, MinSize, EpgCapOnly: EpgCapOnly, SetBgColor: SetBgColor, Search: search);
                    Data = new JsonResult(Page, Page != null || Page != "");
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
                        if (Start < DateTime.Now.AddMinutes(DateTime.Now.Minute * -1).AddSeconds(DateTime.Now.Second * -1).AddSeconds(-1))
                        {
                            Start = DateTime.Now.AddMinutes(-1 * DateTime.Now.Minute).AddSeconds(-1 * DateTime.Now.Second); //変な時間対策
                            Debug.Print("変な時間発動");
                        }
                    }
                    DateTime End = Start.AddHours(MaxHour);
                    var Out = new Dictionary<string, List<EventInfoItem>>();
                    foreach (var a in CommonManager.Instance.DB.ServiceEventList)
                    {
                        if (Key != 0 && a.Key != Key) continue;
                        Out.Add(
                            a.Key.ToString(),
                            //1: 指定された時間よりも前にある
                            //2: 終わっていない
                            a.Value.eventList.Where(b => b.start_time.AddSeconds(b.start_time.Second * -1) < Start.AddHours(MaxHour))
                                .Where(c => c.start_time.AddSeconds(c.durationSec) > Start)
                                .OrderBy(d => d.start_time)
                                .Select(e => new EventInfoItem(e))
                                .ToList()
                            );
                    }
                    Data = new JsonResult(Out);
                }
                else if (Command == "GetEpgEvent")
                {
                    ulong Key = 0;
                    if (Arg.ContainsKey("key"))
                    {
                        Key = ulong.Parse(Arg["key"]);
                    }
                    else if (Arg.ContainsKey("o") && Arg.ContainsKey("t")
                        && Arg.ContainsKey("s") && Arg.ContainsKey("e"))
                    {
                        Key = CommonManager.Create64PgKey(ushort.Parse(Arg["o"]), ushort.Parse(Arg["t"]),
                            ushort.Parse(Arg["s"]), ushort.Parse(Arg["e"]));
                    }
                    if (Key == 0) return "";
                    EpgEventInfo Event = null;
                    foreach (var service in CommonManager.Instance.DB.ServiceEventList.Values)
                    {
                        foreach (var _event in service.eventList)
                        {
                            if (CommonManager.Create64PgKey(_event.original_network_id, _event.transport_stream_id, _event.service_id, _event.event_id) == Key)
                            {
                                Event = _event;
                                break;
                            }
                        }
                        if (Event != null) break;
                    }
                    if (Event == null)
                        Data = new JsonResult(null, false, ErrCode.CMD_NO_RES);
                    else
                        Data = new JsonResult(new EventInfoItem(Event));
                }
                else if (Command == "EnumContentKindList1")
                {
                    Data = new JsonResult(CommonManager.Instance.ContentKindDictionary);
                }
                else if (Command == "EnumContentKindList2")
                {
                    Data = new JsonResult(CommonManager.Instance.ContentKindDictionary2);
                }
                else if (Command == "EnumWritePlugInList")
                {
                    Data = new JsonResult(CommonManager.Instance.DB.WritePlugInList);
                }
                else if (Command == "EnumRecNamePlugInList")
                {
                    Data = new JsonResult(CommonManager.Instance.DB.RecNamePlugInList);
                }
                else if (Command == "EnumEpgAutoAddList")
                {
                    Data = new JsonResult(CommonManager.Instance.DB.EpgAutoAddList);
                }
                else if (Command == "EpgCapNow")
                {
                    ErrCode Err = (ErrCode)CommonManager.Instance.CtrlCmd.SendEpgCapNow();
                    Data = new JsonResult(null, Err);
                }
                else if (Command == "EpgReload")
                {
                    ErrCode Err = (ErrCode)CommonManager.Instance.CtrlCmd.SendReloadEpg();
                    Data = new JsonResult(null, Err);
                }
                else if (Command == "EnumTunerReserve")
                {
                    List<TunerReserveInfo> reserves = new List<TunerReserveInfo>();
                    ErrCode Err = (ErrCode)CommonManager.Instance.CtrlCmd.SendEnumTunerReserve(ref reserves);
                    if (Err == ErrCode.CMD_SUCCESS)
                    {
                        Data = new JsonResult(reserves, true, Err);
                    }
                    else
                    {
                        Data = new JsonResult(null, false, Err);
                    }
                }
                else if (Command == "GetSetting")
                {
                    Data = new JsonResult(Setting.Instance);
                }
                else if (Command == "GetCommonManager")
                {
                    Data = new JsonResult(CommonManagerJson.Instance);
                }
                else if (Command == "AddReserve")
                {
                    if (Arg.ContainsKey("tsid")
                        && Arg.ContainsKey("onid") && Arg.ContainsKey("sid")
                        && Arg.ContainsKey("eid")) //最低限必要
                    {
                        ushort ONID = ushort.Parse(Arg["onid"]);
                        ushort SID = ushort.Parse(Arg["sid"]);
                        ushort TSID = ushort.Parse(Arg["tsid"]);
                        ushort EventID = ushort.Parse(Arg["eid"]);
                        ulong Key = CommonManager.Create64Key(ONID, TSID, SID);
                        ulong PGKey = CommonManager.Create64PgKey(ONID, TSID, SID, EventID);
                        EpgEventInfo Event = null;
                        if (!CommonManager.Instance.DB.ServiceEventList.ContainsKey(Key)) JsonData = "{\"result\":false}";
                        if (CommonManager.Instance.DB.ServiceEventList[Key].eventList.Count(e => e.event_id == EventID) == 1)
                        {
                            Event = CommonManager.Instance.DB.ServiceEventList[Key].eventList.First(e => e.event_id == EventID);
                        }
                        else
                        {
                            CommonManager.Instance.CtrlCmd.SendGetPgInfo(PGKey, ref Event);
                        }
                        if (Event != null)
                        {
                            ReserveData Reserve = new ReserveData();
                            if (Event.ShortInfo != null) Reserve.Title = Event.ShortInfo.event_name;
                            Reserve.StartTime = Event.start_time;
                            Reserve.StartTimeEpg = Event.start_time;
                            if (Event.DurationFlag == 0)
                            {
                                Reserve.DurationSecond = 10 * 60;
                            }
                            else
                            {
                                Reserve.DurationSecond = Event.durationSec;
                            }
                            if (ChSet5.Instance.ChList.ContainsKey(Key)) Reserve.StationName = ChSet5.Instance.ChList[Key].ServiceName;
                            Reserve.OriginalNetworkID = Event.original_network_id;
                            Reserve.TransportStreamID = Event.transport_stream_id;
                            Reserve.ServiceID = Event.service_id;
                            Reserve.EventID = Event.event_id;
                            RecSettingData Setting = GetPreset(Arg, new RecSettingData());
                            Reserve.RecSetting = Setting;
                            ErrCode err = (ErrCode)CommonManager.Instance.CtrlCmd.SendAddReserve(new List<ReserveData> { Reserve });
                            Data = new JsonResult(Reserve, err);
                        }
                        else
                        {
                            Data = new JsonResult(null, false, ErrCode.CMD_NO_RES);
                        }
                    }
                    else
                    {
                        Data = new JsonResult(null, false, ErrCode.CMD_NO_ARG);
                    }
                }

                else if (Command == "AddAutoReserve")
                {
                    var Preset = GetPreset(Arg, new RecSettingData());
                    var Search = GetEpgSKey(Arg);
                    if (Arg.ContainsKey("overlap_check") && Arg.ContainsKey("overlap_day"))
                    {
                        Search.chkRecDay = ushort.Parse(Arg["overlap_day"]);
                        Search.chkRecEnd = 1;
                    }

                    ErrCode err = (ErrCode)CommonManager.Instance.CtrlCmd.SendAddEpgAutoAdd(new List<EpgAutoAddData>{
                        new EpgAutoAddData(){
                            searchInfo = Search,
                            recSetting = Preset
                        }
                    });
                    Data = new JsonResult(new EpgAutoAddData()
                    {
                        searchInfo = Search,
                        recSetting = Preset
                    }, err);
                }
                else if (Command == "EnumPresets")
                {
                    Data = new JsonResult(PresetDb.Instance.Presets);
                }
                else if (Command == "AddPreset")
                {
                    if (!Arg.ContainsKey("name")) return JsonData;
                    uint ID = PresetDb.Instance.AddPreset(GetPreset(Arg, new RecSettingData()), Arg["name"]);
                    Data = new JsonResult(PresetDb.Instance.Presets[ID]);
                }
                else if (Command == "EpgSearch")
                {
                    if (Arg.ContainsKey("srvlist"))
                    {
                        List<EpgEventInfo> EpgResult = new List<EpgEventInfo>();
                        ErrCode code = (ErrCode)CommonManager.Instance.CtrlCmd.SendSearchPg(new List<EpgSearchKeyInfo> { GetEpgSKey(Arg) }, ref EpgResult);
                        Data = new JsonResult(EpgResult.Select(s => new EventInfoItem(s)), code);
                    }
                    else
                    {
                        Data = new JsonResult(null, false, ErrCode.CMD_NO_ARG);
                    }
                }
                else if (Command == "EnumEvents")
                {
                    Data = new JsonResult(EventStore.Instance.Events);
                }
                else if (Command == "GetContentColorTable")
                {
                    Data = new JsonResult(Setting.Instance.ContentToColorTable);
                }
                else if (Command == "SetContentColorTable")
                {
                    Regex ColorRegex = new Regex(@"[0-9a-fA-F]{6}");
                    if (Arg.ContainsKey("id") && Arg.ContainsKey("color") && uint.Parse(Arg["id"]) >= 0 && ColorRegex.IsMatch(Arg["color"]))
                    {
                        if (Setting.Instance.ContentToColorTable.Count(s => s.ContentLevel1 == uint.Parse(Arg["id"])) == 0)
                        {
                            Setting.Instance.ContentToColorTable.Add(new ContentColorItem(uint.Parse(Arg["id"]), "#" + Arg["color"]));
                        }
                        else
                        {
                            Setting.Instance.ContentToColorTable.RemoveAll(s => s.ContentLevel1 == uint.Parse(Arg["id"]));
                            Setting.Instance.ContentToColorTable.Add(new ContentColorItem(uint.Parse(Arg["id"]), "#" + Arg["color"]));
                        }

                        Data = new JsonResult(null);
                    }
                }
                else if (Command == "RemoveReserve")
                {
                    if (Arg.ContainsKey("id"))
                    {
                        ErrCode err = (ErrCode)CommonManager.Instance.CtrlCmd.SendDelReserve(new List<uint> { uint.Parse(Arg["id"]) });
                        Data = new JsonResult(null, err);
                    }
                }
                else if (Command == "RemoveAutoReserve")
                {
                    if (Arg.ContainsKey("id"))
                    {
                        ErrCode err = (ErrCode)CommonManager.Instance.CtrlCmd.SendDelEpgAutoAdd(new List<uint> { uint.Parse(Arg["id"]) });
                        Data = new JsonResult(null, err);
                    }
                }
                else if (Command == "RemoveManualReserve")
                {
                    if (Arg.ContainsKey("id"))
                    {
                        ErrCode err = (ErrCode)CommonManager.Instance.CtrlCmd.SendDelManualAdd(new List<uint> { uint.Parse(Arg["id"]) });
                        Data = new JsonResult(null, err);
                    }
                }
                else if (Command == "RemoveRecFile")
                {
                    if (Arg.ContainsKey("id"))
                    {
                        ErrCode err = (ErrCode)CommonManager.Instance.CtrlCmd.SendDelRecInfo(new List<uint> { uint.Parse(Arg["id"]) });
                        Data = new JsonResult(null, err);
                    }
                }
                else if (Command == "UpdateReserve")
                {
                    if (Arg.ContainsKey("id") &&
                        CommonManager.Instance.DB.ReserveList.ContainsKey(uint.Parse(Arg["id"])))
                    {
                        ReserveData Target = CommonManager.Instance.DB.ReserveList[uint.Parse(Arg["id"])];
                        Target.RecSetting = GetPreset(Arg, Target.RecSetting);
                        ErrCode err = (ErrCode)CommonManager.Instance.CtrlCmd.SendChgReserve(new List<ReserveData> { Target });
                        Data = new JsonResult(Target, err);
                    }
                }
                else if (Command == "Hello")
                {
                    Data = new JsonResult(VersionInfo.Instance);
                }
                JsonData = JsonUtil.Serialize(Data, Indent);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
            return JsonData;
        }
    }
}
