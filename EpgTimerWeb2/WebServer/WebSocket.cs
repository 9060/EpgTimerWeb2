using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Threading;

namespace EpgTimer
{
    public class WebSocket
    {

        public static byte[] GetWebSocketUnMaskedFrame(HttpContext Context)
        {
            while (Context.Client.Available < 2) Thread.Sleep(10);
            UInt64 DataSize = 0;
            UInt64 NowSize = 0;
            UInt64 HdrSize = 2;
            byte[] Buffer = new byte[1024];
            int Size;
            int TotalSize = 0;
            List<byte> MaskBuffer = new List<byte>();
            while (TotalSize <= 2)
            {
                Size = Context.HttpStream.Read(Buffer, 0, (int)HdrSize);
                MaskBuffer.AddRange(Buffer.Take(Size));
                TotalSize += Size;
            }
            HdrSize = WebSocketGetHdrLen(MaskBuffer.ToArray()) - (ulong)TotalSize;
            while (HdrSize != 0)
            {
                Size = Context.HttpStream.Read(Buffer, 0, (int)HdrSize);
                MaskBuffer.AddRange(Buffer.Take(Size));
                TotalSize += Size;
                HdrSize -= (ulong)Size;
            }
            DataSize = WebSocketGetLen(MaskBuffer.ToArray());
            NowSize = DataSize - (ulong)TotalSize;
            while (NowSize > 0)
            {
                if(NowSize < (ulong)Buffer.Length){
                    Size = Context.HttpStream.Read(Buffer, 0, (int)NowSize);
                }
                else
                {
                    Size = Context.HttpStream.Read(Buffer, 0, Buffer.Length);
                }
                MaskBuffer.AddRange(Buffer.Take(Size));
                NowSize -= (ulong)Size;
            }
            DataSize = 0;
            if ((byte)(MaskBuffer[0] & 0x0f) == 0x8)
            {
                Context.Close();
                return null;
            }
            else if ((byte)(MaskBuffer[0] & 0x0f) == 0x9)
            {
                var SendFrame = MaskBuffer.ToArray();
                SendFrame[0] = 0x8A;
                HttpResponseGenerater.SendResponseBody(Context, SendFrame);
                return null;
            }
            return WebSocketUnMask(MaskBuffer.ToArray());
        }
        public static void HandshakeResponseSend(HttpContext Context)
        {
            if (!Context.Request.Headers.ContainsKey("upgrade") && Context.Request.Headers.ContainsKey("sec-websocket-key")) return;
            if (Context.Request.Headers["upgrade"] == "websocket" && Context.Request.Headers["sec-websocket-key"] != null)
            {
                var Accept = GenerateAccept(Context.Request.Headers["sec-websocket-key"]);               
                
                Context.Response.Headers.Add("Connection", "Upgrade");
                Context.Response.Headers.Add("Upgrade", "websocket");
                Context.Response.Headers.Add("Sec-WebSocket-Accept", Accept);
                Context.Response.StatusCode = 101;
                Context.Response.StatusText = "Switching Protocols";
            }
            HttpResponseGenerater.SendResponseCode(Context);
            HttpResponseGenerater.SendResponseHeader(Context, Context.Response.Headers);
        }
        private const string ACCEPT_KEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        /// <summary>
        /// Sec-WebSocket-Acceptで使用するキーを作成する
        /// </summary>
        /// <param name="Key">Sec-WebSocket-Key</param>
        /// <returns>Sec-WebSocket-Accept</returns>
        public static string GenerateAccept(string Key)
        {
            return Convert.ToBase64String(
                new SHA1CryptoServiceProvider().
                ComputeHash(
                    Encoding.UTF8.GetBytes(Key + ACCEPT_KEY)
                ));
        }
        public static UInt64 WebSocketGetHdrLen(byte[] Frame)
        {
            var PayloadLen = (UInt64)(Frame[1] & 0x7f);
            UInt64 Len = 2;
            if (PayloadLen == 126) Len += 2;
            if (PayloadLen == 127) Len += 8;
            if ((Frame[1] & 0x80) == 0x80) Len += 4;
            return Len;
        }
        public static UInt64 WebSocketGetLen(byte[] Frame)
        {
            var PayloadLen = (UInt64)(Frame[1] & 0x7f);
            int Offset = 2;
            if (PayloadLen == 126)
            {
                var LenArray = new byte[2];
                Array.Copy(Frame, Offset, LenArray, 0, 2);
                Array.Reverse(LenArray);
                PayloadLen = BitConverter.ToUInt16(LenArray, 0);
                Offset += 2;
            }
            else if (PayloadLen == 127)
            {
                var RevArray = new byte[8];
                Array.Copy(Frame, Offset, RevArray, 0, 8);
                Array.Reverse(RevArray);
                PayloadLen = BitConverter.ToUInt64(RevArray, 0);
                Offset += 8;
            }
            if ((Frame[1] & 0x80) == 0x80)
            {
                Offset += 4;
            }
            return PayloadLen + (UInt64)Offset;
        }
        public static byte[] WebSocketUnMask(byte[] WebSocketFrame)
        {
            var Fin = (WebSocketFrame[0] & 0x80) == 0x80;
            var OpCode = (byte)(WebSocketFrame[0] & 0x0f);
            var Mask = (WebSocketFrame[1] & 0x80) == 0x80;
            var PayloadLen = (UInt64)(WebSocketFrame[1] & 0x7f);
            int Offset = 2;
            if (PayloadLen == 126)
            {
                var LenArray = new byte[2];
                Array.Copy(WebSocketFrame, Offset, LenArray, 0, 2);
                Array.Reverse(LenArray);
                PayloadLen = BitConverter.ToUInt16(LenArray, 0);
                Offset += 2;
            }
            else if (PayloadLen == 127)
            {
                var RevArray = new byte[8];
                Array.Copy(WebSocketFrame, Offset, RevArray, 0, 8);
                Array.Reverse(RevArray);
                PayloadLen = BitConverter.ToUInt64(RevArray, 0);
                Offset += 8;
            }
            var MaskKey = new byte[4];
            if (Mask)
            {
                Array.Copy(WebSocketFrame, Offset, MaskKey, 0, 4);
                Offset += 4;
            }
            var PayloadData = new byte[PayloadLen];
            Array.Copy(WebSocketFrame, Offset, PayloadData, 0, (long)PayloadLen);
            if (Mask)
            {
                for (ulong i = 0; i < PayloadLen; i++)
                {
                    PayloadData[i] = (byte)(PayloadData[i] ^ MaskKey[i % 4]);
                }
            }
            return PayloadData;
        }
        public static byte[] WebSocketMask(byte[] Data, byte OpCode)
        {
            byte[] WebSocketFrame;
            byte[] Hd;
            if (Data.Length <= 125)
            {
                Hd = new byte[2];
                Hd[1] = (byte)Data.Length;
            }
            else if (Data.Length <= 65535)
            {
                Hd = new byte[4];
                Hd[1] = (byte)126;
                byte[] lenData = BitConverter.GetBytes((UInt16)Data.Length);
                Array.Reverse(lenData);
                Array.Copy(lenData, 0, Hd, 2, 2);
            }
            else
            {
                Hd = new byte[10];
                Hd[1] = (byte)127;
                byte[] lenData = BitConverter.GetBytes((UInt64)Data.Length);
                Array.Reverse(lenData);
                Array.Copy(lenData, 0, Hd, 2, 8);
            }
            Hd[0] = (byte)(0x80 | OpCode);
            WebSocketFrame = new byte[Hd.Length + Data.Length];
            Buffer.BlockCopy(Hd, 0, WebSocketFrame, 0, Hd.Length);
            Buffer.BlockCopy(Data, 0, WebSocketFrame, Hd.Length, Data.Length);
            return WebSocketFrame;
        }
    }
}
