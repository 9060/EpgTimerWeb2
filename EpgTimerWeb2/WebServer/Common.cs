using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EpgTimer
{
    public class HttpCommon
    {
        public static string StreamReadLine(Stream Input)
        {
            int Next;
            string Data = "";
            int To = 0;
            while (true)
            {
                Next = Input.ReadByte();
                if (Next == '\n') break;
                if (Next == '\r') { continue; }
                if (Next == -1) { Thread.Sleep(1); To++; if (To > 300) throw new TimeoutException(); continue; }
                Data += Convert.ToChar(Next);
            }
            return Data;
        }
    }
}
