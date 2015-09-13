/*
 *  EpgTimerWeb2
 *  Copyright (C) 2015  YukiBoard 0X7hT.k8kU <yuki@yukiboard.tk>
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using CtrlCmdCLI.Def;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;

namespace EpgTimer
{
    public class EpgPage
    {
        private static DateTime ConvertDateTime(DateTime date)
        {
            return date
                .AddTicks(-(date.Ticks % TimeSpan.TicksPerSecond))
                .AddSeconds(-(UnixTime.ToUnixTime(date) % 3600));
        }
        public static string Generate(DateTime StartTime, int MaxHour, List<ulong> ServiceKeys = null, int MinSize = 5, bool SetBgColor = true, bool IncludeNotEpg = false, bool EpgCapOnly = true, EpgSearchKeyInfo Search = null)
        {
            DateTime Start = ConvertDateTime(StartTime.AddSeconds(StartTime.Second * -1).AddMinutes(StartTime.Minute * -1));
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
                if (a.Value.eventList.Count == 0) continue;
                if (ServiceKeys != null && ServiceKeys.Count != 0 && !ServiceKeys.Contains(a.Key)) continue;
                if (EpgCapOnly && (!ChSet5.Instance.ChList.ContainsKey(a.Key) || ChSet5.Instance.ChList[a.Key].EpgCapFlag != 1)) continue;
                if (Search == null)
                {
                    var Events = a.Value.eventList
                            .Where(x => x.ShortInfo != null)
                            .Where(b => b.start_time < End && b.start_time.AddSeconds(b.durationSec) > Start)
                            .OrderBy(d => d.start_time);
                    Debug.Print("{0}: {1}", a.Value.serviceInfo.service_name, Events.Count());
                    if (Events.Count() == 0) continue;
                    Out.Add(
                        a.Value.serviceInfo,
                            Events
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
            
            SortedList<long, DateTime> TimeList = new SortedList<long, DateTime>();
            foreach (var Item3 in Out.Values)
            {
                foreach (var Item2 in Item3)
                {
                    var TempDate = ConvertDateTime(Item2.Start);
                    var TempDate2 = ConvertDateTime(Item2.End);
                    if (TempDate < Start)
                        TempDate = Start;
                    if (TempDate2 > End)
                        TempDate2 = End;
                    for (; TempDate < TempDate2; )
                    {
                        long Unix = UnixTime.ToUnixTime(TempDate);
                        TimeList[Unix] = TempDate;
                        TempDate = TempDate.AddHours(1);
                    }
                }
            }

            sb.Append("</div>\n<div id=\"body\">\n");
            //Print Time
            sb.Append("<div id=\"timeline\" class=\"list\">\n");
            for (int i = 0; i < TimeList.Values.Count; i++)
            {
                DateTime Temp = TimeList.Values[i];
                string Text = (TimeList.Values.Count(s => s < Temp && s > Temp.AddHours(-1 * Temp.Hour)) == 0) ? "<p>" + Temp.Month + "/" + Temp.Day + "</p>" + Temp.Hour : Temp.Hour.ToString();
                sb.AppendFormat("<div style=\"height:{1}px;top:{2}px;\">{0}</div>\n", Text, MinSize * 60, MinSize * 60 * i);
            }
            sb.Append("</div>\n");

            //Print EPG
            foreach (var Item in Out)
            {
                StringBuilder sb2 = new StringBuilder();
                sb2.AppendFormat("<div class=\"list\" ts=\"{0}\" on=\"{1}\" s=\"{2}\">", Item.Key.TSID, Item.Key.ONID, Item.Key.SID);
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
                        long Size = 0;
                        OldTime = EventEnd;
                        if (EventStart < Start)
                            EventStart = Start;
                        if (EventEnd > End)
                            EventEnd = End;

                        Size = (UnixTime.ToUnixTime(EventEnd) - UnixTime.ToUnixTime(EventStart)) / 60 * MinSize;
                        if (Size <= 0) continue;
                        string StartTimeStr = Event.Start.ToString("HH:mm");
                        var EventName = String.Format("{0} <span title=\"{1}\">{1}</span>\n<p>{2}</p>\n", StartTimeStr,
                            HttpUtility.HtmlEncode(Event.Short.event_name),
                            HttpUtility.HtmlEncode(Event.Short.text_char));
                        DateTime Time1 = ConvertDateTime(EventStart);
                        long Top = 0;
                        foreach (var Time2 in TimeList.Values)
                        {
                            if (Time1 == ConvertDateTime(Time2)) break;
                            Top += MinSize * 60;
                        }
                        Top += ((UnixTime.ToUnixTime(EventStart) - UnixTime.ToUnixTime(Start)) % 3600) / 60 * MinSize;
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
                                sb2.AppendFormat("<div class=\"event{4} ct-{6}\" e=\"{0}\" style=\"top:{3}px;min-height:{2}px;max-height:{2}px;z-index:{5};\">{1}</div>\n", Event.EID, EventName, Size, Top, AddClass, ItemCount, Event.Content.nibbleList[0].content_nibble_level_1);
                            else
                                sb2.AppendFormat("<div class=\"event{4}\" e=\"{0}\" style=\"top:{3}px;min-height:{2}px;max-height:{2}px;z-index:{5};\">{1}</div>\n",
                                    Event.EID, EventName, Size, Top, AddClass, ItemCount);
                        }
                        else
                        {
                            sb2.AppendFormat("<div class=\"event{4}\" e=\"{0}\" style=\"top:{3}px;min-height:{2}px;max-height:{2}px;z-index:{5};\">{1}</div>\n",
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
