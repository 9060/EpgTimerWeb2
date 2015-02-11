using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpgTimer
{
    public class Util
    {
        public static byte[] ReadStream(Stream stream, int size)
        {
            var Buffer = new byte[1024];
            var BufferList = new List<byte>();
            int Size = 0, AllSize = 0, NextSize = Buffer.Length;
            if (NextSize > size) //size < 1024
                NextSize = size; //size のみ
            while ((Size = stream.Read(Buffer, 0, NextSize)) != 0)
            {
                Debug.Print("{0}byte中{1}byte 合計{2}byte", size, NextSize, AllSize);
                BufferList.AddRange(Buffer.Take(Size));
                AllSize += Size;
                if (NextSize > size - AllSize) //size - AllSize < 1024
                    NextSize = size - AllSize;
                if (AllSize >= size) break; //全て読んだ
            }
            return BufferList.ToArray();
        }
    }
}
