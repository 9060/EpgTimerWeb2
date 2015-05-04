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
using System.Linq;

namespace EpgTimer
{
    public class PresetDb
    {
        private static PresetDb _instance;
        public static PresetDb Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PresetDb();
                return _instance;
            }
            set { _instance = value; }
        }
        public Dictionary<uint, KeyValuePair<RecPresetItem, RecSettingData>> Presets { set; get; }
        public PresetDb()
        {
            Presets = new Dictionary<uint, KeyValuePair<RecPresetItem, RecSettingData>>();
        }
        public UInt32 AddPreset(RecSettingData Data, string Name)
        {
            uint NextID = Presets.Count == 0 ? 0 : Presets.OrderByDescending(s => s.Key).Last().Key + 1;
            if (Presets.ContainsKey(NextID)) return 0xFFFFFFFF;
            RecPresetItem ItemInfo = new RecPresetItem()
            {
                DisplayName = Name,
                ID = NextID
            };
            Presets.Add(NextID, new KeyValuePair<RecPresetItem, RecSettingData>(ItemInfo, Data));
            return NextID;
        }
    }
}
