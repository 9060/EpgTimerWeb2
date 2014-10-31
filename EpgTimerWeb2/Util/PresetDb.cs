using CtrlCmdCLI.Def;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public Dictionary<UInt32, KeyValuePair<RecPresetItem, RecSettingData>> Presets { set; get; }
        public PresetDb()
        {
            Presets = new Dictionary<uint, KeyValuePair<RecPresetItem, RecSettingData>>();
        }
        public UInt32 AddPreset(RecSettingData Data, string Name)
        {
            UInt32 NextID = Presets.OrderByDescending(s => s.Key).Last().Key + 1;
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
