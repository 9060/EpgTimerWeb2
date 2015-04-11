using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EpgTimer
{
    public class ChSet5
    {
        public Dictionary<ulong, ChSet5Item> ChList
        {
            get;
            set;
        }

        private static ChSet5 _instance;
        public static ChSet5 Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ChSet5();
                return _instance;
            }
            set { _instance = value; }
        }

        public ChSet5()
        {
            ChList = new Dictionary<ulong, ChSet5Item>();
        }
        public static bool LoadFile()
        {
            try
            {
                if (Instance.ChList == null)
                {
                    Instance.ChList = new Dictionary<ulong, ChSet5Item>();
                }
                else
                {
                    Instance.ChList.Clear();
                }
                String filePath = @".\Setting\ChSet5.txt";
                StreamReader reader = new StreamReader(filePath, Encoding.Default);
                while (reader.Peek() >= 0)
                {
                    string buff = reader.ReadLine();
                    if (!buff.StartsWith(";"))
                    {
                        string[] list = buff.Split('\t');
                        ChSet5Item item = new ChSet5Item();
                        try
                        {
                            item.ServiceName = list[0];
                            item.NetworkName = list[1];
                            item.ONID = ushort.Parse(list[2]);
                            item.TSID = ushort.Parse(list[3]);
                            item.SID = ushort.Parse(list[4]);
                            item.ServiceType = ushort.Parse(list[5]);
                            item.PartialFlag = byte.Parse(list[6]);
                            item.EpgCapFlag = byte.Parse(list[7]);
                            item.SearchFlag = byte.Parse(list[8]);
                        }
                        finally
                        {
                            ulong key = ((ulong)item.ONID) << 32 | ((ulong)item.TSID) << 16 | ((ulong)item.SID);
                            Instance.ChList.Add(key, item);
                        }
                    }
                }

                reader.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }
        public static bool SaveFile()
        {
            try
            {
                string filePath = @".\Config\ChSet5.txt";
                StreamWriter writer = new StreamWriter(filePath, false, Encoding.Default);
                if (Instance.ChList != null)
                {
                    foreach (ChSet5Item info in Instance.ChList.Values)
                    {
                        writer.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}",
                            info.ServiceName,
                            info.NetworkName,
                            info.ONID,
                            info.TSID,
                            info.SID,
                            info.ServiceType,
                            info.PartialFlag,
                            info.EpgCapFlag,
                            info.SearchFlag);
                    }
                }
                writer.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }
    }

    public class ChSet5Item
    {
        public ChSet5Item()
        {
        }

        public ulong Key
        {
            get
            {
                return ((ulong)ONID) << 32 | ((ulong)TSID) << 16 | (ulong)SID;
            }
        }

        public ushort ONID
        {
            get;
            set;
        }
        public ushort TSID
        {
            get;
            set;
        }

        public ushort SID
        {
            get;
            set;
        }
        public ushort ServiceType
        {
            get;
            set;
        }
        public byte PartialFlag
        {
            get;
            set;
        }
        public string ServiceName
        {
            get;
            set;
        }
        public string NetworkName
        {
            get;
            set;
        }
        public byte EpgCapFlag
        {
            get;
            set;
        }
        public byte SearchFlag
        {
            get;
            set;
        }
        public byte RemoconID
        {
            get;
            set;
        }

        public override string ToString()
        {
            return ServiceName;
        }
    }

}
