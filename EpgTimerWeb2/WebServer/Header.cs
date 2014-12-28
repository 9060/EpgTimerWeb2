using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpgTimer
{
    public class HttpHeader
    {
        public static string Generate(IDictionary<string, string> Input)
        {
            StringBuilder Ret = new StringBuilder();
            foreach (KeyValuePair<string, string> Item in Input)
            {
                Ret.AppendFormat("{0}: {1}\r\n", Item.Key, Item.Value);
            }
            return Ret.ToString();
        }
        public static Dictionary<string, string> Parse(Stream Input)
        {
            string Line = "";
            Dictionary<string, string> Dict = new Dictionary<string, string>();
            while ((Line = HttpCommon.StreamReadLine(Input)) != null)
            {
                if (Line == "") return Dict;
                int Separator = Line.IndexOf(":");
                if (Separator == -1) throw new Exception("Invalid Http Header " + Line);
                var Name = Line.Substring(0, Separator);
                int Pos = Separator + 1;
                while ((Pos < Line.Length) && (Line[Pos] == ' ')) Pos++;

                Dict[Name.ToLower()] = Line.Substring(Pos);
            }
            return Dict;
        }
    }
}
