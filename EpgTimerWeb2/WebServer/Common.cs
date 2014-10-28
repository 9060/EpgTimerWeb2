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
            while (true)
            {
                Next = Input.ReadByte();
                if (Next == '\n') break;
                if (Next == '\r') { continue; }
                if (Next == -1) { Thread.Sleep(1); continue; }
                Data += Convert.ToChar(Next);
            }
            return Data;
        }
    }
}
