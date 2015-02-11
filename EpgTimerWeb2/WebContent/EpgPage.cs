using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CtrlCmdCLI.Def;
using System.Drawing;
using EpgTimerWeb2;
using System.Diagnostics;

namespace EpgTimer
{
    public class EpgPage
    {
        public static string Generate(DateTime StartTime, int MaxHour, List<ulong> ServiceKeys = null, int MinSize = 5, bool SetBgColor = true, bool IncludeNotEpg = false, bool EpgCapOnly = true, EpgSearchKeyInfo Search = null)
        {
            DateTime Start = StartTime.AddSeconds(StartTime.Second * -1).AddMinutes(StartTime.Minute * -1);
            DateTime End = Start.AddHours(MaxHour);
            StringBuilder sb = new StringBuilder();
            StringBuilder sb1 = new StringBuilder();
            sb1.Append("<div id=\"epg\">\n");
            sb1.Append("<div id=\"header\">\n");
            sb1.Append("<div class=\"time\">時間</div>\n");
            //Select Item
            var Out = new Dictionary<EpgServiceInfo, List<EventInfoItem>>();
            var List = new Dictionary<ulong, EpgServiceEventInfo>();
            List<EpgServiceEventInfo> list = new List<EpgServiceEventInfo>();
            ErrCode ret = (ErrCode)CommonManager.Instance.CtrlCmd.SendEnumPgAll(ref list);
            if (ret == ErrCode.CMD_SUCCESS)
            {
                foreach (EpgServiceEventInfo info in list)
                {
                    UInt64 id = CommonManager.Create64Key(
                        info.serviceInfo.ONID,
                        info.serviceInfo.TSID,
                        info.serviceInfo.SID);
                    List.Add(id, info);
                }
            }
            else
            {
                return "";
            }
            list.Clear();
            list = null;

            if (Search != null)
            {
                Start = DateTime.MinValue;
                End = DateTime.MaxValue;
                List<EpgEventInfo> Events = new List<EpgEventInfo>();
                var Res = CommonManager.Instance.CtrlCmd.SendSearchPg(new List<EpgSearchKeyInfo>() { Search }, ref Events);
                if ((ErrCode)Res != ErrCode.CMD_SUCCESS || Events.Count == 0) return "";
                foreach (var item in List)
                {
                    item.Value.eventList.Clear();
                }
                foreach (var item in Events)
                {
                    var Key = CommonManager.Create64Key(item.original_network_id, item.transport_stream_id, item.service_id);
                    if (List.ContainsKey(Key))
                    {
                        List[Key].eventList.Add(item);
                    }
                    else
                    {
                        if (!CommonManager.Instance.DB.ServiceEventList.ContainsKey(Key)) return "";
                        List[Key] = new EpgServiceEventInfo();
                        List[Key].serviceInfo = CommonManager.Instance.DB.ServiceEventList[Key].serviceInfo;
                        List[Key].eventList = new List<EpgEventInfo>();
                        List[Key].eventList.Add(item);
                    }
                }
            }
            foreach (var a in List)
            {
                if (ServiceKeys != null && ServiceKeys.Count != 0 && !ServiceKeys.Contains(a.Key)) continue;
                if (EpgCapOnly && (!ChSet5.Instance.ChList.ContainsKey(a.Key) || ChSet5.Instance.ChList[a.Key].EpgCapFlag != 1)) continue;
                if (Search == null)
                {
                    Out.Add(
                        a.Value.serviceInfo,
                        a.Value.eventList
                            .Where(b => b.start_time < End && b.start_time.AddSeconds(b.durationSec) > Start)
                            .OrderBy(d => d.start_time)
                            .Select(e => new EventInfoItem(e))
                            .ToList()
                        );
                }
                else
                {
                    Out.Add(
                        a.Value.serviceInfo,
                        a.Value.eventList
                            .Select(e => new EventInfoItem(e))
                            .ToList()
                        );
                }
            }
            SortedList<DateTime, DateTime> TimeList = new SortedList<DateTime, DateTime>(); ;
            foreach (var Item3 in Out.Values)
            {
                foreach (var Item2 in Item3)
                {
                    var TempDate = Item2.Start;
                    var TempDate2 = Item2.End;
                    if (TempDate < Start)
                        TempDate = Start;
                    if (TempDate2 > End)
                        TempDate2 = End;
                    if (UnixTime.ToUnixTime(TempDate) % 3600 != 0)
                        TempDate = TempDate.AddSeconds(-1 * (UnixTime.ToUnixTime(TempDate) % 3600));
                    for (; TempDate < TempDate2; )
                    {
                        TimeList[TempDate] = TempDate;
                        TempDate = TempDate.AddHours(1);
                    }
                }
            }

            sb.Append("</div><div id=\"body\">\n");
            //Print Time
            sb.Append("<div id=\"timeline\" class=\"list\">\n");
            for (int i = 0; i < TimeList.Values.Count; i++)
            {
                DateTime Temp = TimeList.Values[i];
                string Text = (TimeList.Values.Count(s => s < Temp && s > Temp.AddHours(-1 * Temp.Hour)) == 0) ? "<p>" + Temp.Month + "/" + Temp.Day + "</p>" + Temp.Hour : Temp.Hour.ToString();
                sb.AppendFormat("<div style=\"height: {1}px;top: {2}px;\">{0}</div>\n", Text, MinSize * 60, MinSize * 60 * i);
                Debug.Print(Temp.ToString());
            }
            sb.Append("</div>\n");
            /*
            DateTime Temp = StartTime.AddSeconds(StartTime.Second * -1).AddMinutes(StartTime.Minute * -1);
            for (int i = 0; i < MaxHour; i++)
            {
                string Text = (i == 0 || Temp.Hour == 0) ? "<p>" + Temp.Month + "/" + Temp.Day + "</p>" + Temp.Hour : Temp.Hour.ToString();
                sb.AppendFormat("<div style=\"height: {1}px;top: {2}px;\">{0}</div>", Text, MinSize * 60, MinSize * 60 * i);
                Temp = Temp.AddHours(1);
            }
            sb.Append("</div>");
            */
            //Print EPG
            foreach (var Item in Out)
            {
                StringBuilder sb2 = new StringBuilder();
                sb2.AppendFormat("<div class=\"list\" data-t=\"{0}\" data-o=\"{1}\" data-s=\"{2}\">", Item.Key.TSID, Item.Key.ONID, Item.Key.SID);
                if (Item.Value == null || Item.Value.Count == 0)
                {
                    if (!IncludeNotEpg) continue;
                }
                else
                {
                    DateTime OldTime = Start;
                    int ItemCount = 0;
                    foreach (var Event in Item.Value)
                    {
                        //Debug.Print("EPG Header {0}", Item.Key.service_name);
                        if (Event.Short == null || !Event.StartFlg) continue;
                        DateTime EventStart = Event.Start, EventEnd = Event.End;
                        EventStart = EventStart.AddSeconds(-1 * EventStart.Second); // 19:00:03 => 19:00:00
                        EventEnd = EventEnd.AddSeconds(-1 * EventEnd.Second);
                        long Size = 0;
                        OldTime = EventEnd;
                        if (EventStart < Start)
                            EventStart = Start;
                        if (EventEnd > End)
                            EventEnd = End;

                        Size = (UnixTime.ToUnixTime(EventEnd) - UnixTime.ToUnixTime(EventStart)) / 60 * MinSize;
                        if (Size <= 0) continue;
                        string StartTimeStr = Event.Start.ToString("HH:mm");
                        var EventName = String.Format("{0} <span title=\"{1}\">{1}</span><p>{2}</p>", StartTimeStr,
                            HttpUtility.HtmlEncode(Event.Short.event_name),
                            HttpUtility.HtmlEncode(Event.Short.text_char));
                        DateTime Time1 = EventStart;
                        long Top = 0;
                        if (UnixTime.ToUnixTime(Time1) % 3600 != 0)
                            Time1 = Time1.AddSeconds(-1 * (UnixTime.ToUnixTime(Time1) % 3600));
                        foreach (var Time2 in TimeList.Values)
                        {
                            if (Time1 == Time2) break;
                            Top += MinSize * 60;
                        }
                        Top += EventStart.Minute * MinSize;
                        //long Top = (UnixTime.ToUnixTime(EventStart) - UnixTime.ToUnixTime(Start)) / 60 * MinSize;
                        var Reserve = CommonManager.Instance.DB.ReserveList.Values.Where(s =>
                            s.EventID == Event.EID &&
                            s.ServiceID == Event.SID &&
                            s.TransportStreamID == Event.TSID &&
                            s.OriginalNetworkID == Event.ONID);
                        string AddClass = (Reserve.Count() > 0) ? (Reserve.First().RecSetting.RecMode == 5 ? " disable-reserve" : " reserved") : "";
                        if (SetBgColor)
                        {
                            if (Event.Content != null && Event.Content.nibbleList != null && Event.Content.nibbleList.Count != 0 &&
                                Setting.Instance.ContentToColorTable
                                        .Count(s => s.ContentLevel1 == Event.Content.nibbleList[0].content_nibble_level_1) > 0)
                                sb2.AppendFormat("<div class=\"event{5}\" data-e=\"{0}\" style=\"background: {2};top: {4}px;min-height: {3}px;max-height: {3}px;z-index: {6};\">{1}</div>", Event.EID, EventName,
                                    Setting.Instance.ContentToColorTable
                                        .Where(s => s.ContentLevel1 == Event.Content.nibbleList[0].content_nibble_level_1).First().Color, Size, Top, AddClass, ItemCount);
                            else
                                sb2.AppendFormat("<div class=\"event{4}\" data-e=\"{0}\" style=\"top: {3}px;min-height: {2}px;max-height: {2}px;z-index: {5};\">{1}</div>",
                                    Event.EID, EventName, Size, Top, AddClass, ItemCount);
                        }
                        else
                        {
                            sb2.AppendFormat("<div class=\"event {4}\" data-e=\"{0}\" style=\"top: {3}px;min-height: {2}px;max-height: {2}px;z-index: {5};\">{1}</div>",
                                Event.EID, EventName, Size, Top, AddClass, ItemCount);
                        }
                        ItemCount++;
                    }
                    sb.Append(sb2.ToString());
                    if (Item.Key.remote_control_key_id == 0)
                        sb1.AppendFormat("<div>{0}<p>{1}</p></div>", Item.Key.service_name, Item.Key.network_name + " " + Item.Key.SID);
                    else
                        sb1.AppendFormat("<div>{0}<p>{1}</p></div>", Item.Key.service_name, Item.Key.remote_control_key_id);
                }
                sb.Append("</div>");
            }
            sb.Append("</div>");

            return sb1.ToString() + sb.ToString();
        }
    }
}
