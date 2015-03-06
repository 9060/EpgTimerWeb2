using System;
using System.IO;
using System.Threading;

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
                if (Next == -1)
                {
                    Thread.Sleep(1);
                    To++;
                    if (To > 1000) throw new TimeoutException();
                    continue;
                }
                Data += Convert.ToChar(Next);
            }
            return Data;
        }
    }
}
