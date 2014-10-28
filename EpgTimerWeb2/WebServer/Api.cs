using CtrlCmdCLI.Def;
using EpgTimer;
using EpgTimerWeb2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EpgTimer
{
    public class Api
    {
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
                    if(Arg.ContainsKey("eventname") && Arg.ContainsKey("starttime") 
                        && Arg.ContainsKey("durationsec") && Arg.ContainsKey("tsid")
                        && Arg.ContainsKey("onid") && Arg.ContainsKey("sid")
                        && Arg.ContainsKey("eid")) //最低限必要
                    {
                        ushort ONID = ushort.Parse(Arg["onid"]);
                        ushort SID = ushort.Parse(Arg["sid"]);
                        ushort TSID = ushort.Parse(Arg["tsid"]);
                        ushort EventID = ushort.Parse(Arg["eid"]);
                        ulong Key = CommonManager.Create64Key(ONID, TSID,SID);
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return JsonData;
        }
    }
}
