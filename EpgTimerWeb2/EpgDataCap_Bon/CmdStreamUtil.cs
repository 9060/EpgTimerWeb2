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
using System;
using System.Text;

namespace EpgTimer
{
    public class NotifySrvInfo
    {
        public uint notifyID;
        public DateTime time;
        public uint param1;
        public uint param2;
        public uint param3;
        public string param4;
        public string param5;
        public string param6;

        public NotifySrvInfo()
        {
            notifyID = 0;
            time = new DateTime();
            param1 = 0;
            param2 = 0;
            param3 = 0;
            param4 = "";
            param5 = "";
            param6 = "";
        }
    }

    public class CmdStreamUtil
    {
        private static bool ReadData(ushort ver, ref string val, byte[] buff, ref int startIndex)
        {
            try
            {
                int iStrSize = 0;
                Encoding uniEnc = Encoding.GetEncoding("unicode");

                iStrSize = BitConverter.ToInt32(buff, startIndex);
                startIndex += sizeof(int);
                iStrSize -= sizeof(int)+2;
                if (iStrSize > 0)
                {
                    val = uniEnc.GetString(buff, startIndex, iStrSize);
                }
                startIndex += iStrSize + 2;
            }
            catch
            {
                return false;
            }
            return true;
        }
        private static bool ReadData(ushort ver, ref uint val, byte[] buff, ref int startIndex)
        {
            try
            {
                val = BitConverter.ToUInt32(buff, startIndex);
                startIndex += sizeof(uint);
            }
            catch
            {
                return false;
            }
            return true;
        }
        private static bool ReadData(ushort ver, ref DateTime val, byte[] buff, ref int startIndex)
        {
            try
            {
                short yy = BitConverter.ToInt16(buff, startIndex);
                startIndex += sizeof(short);
                short mm = BitConverter.ToInt16(buff, startIndex);
                startIndex += sizeof(short);
                //wDayOfWeek
                startIndex += sizeof(short);
                short dd = BitConverter.ToInt16(buff, startIndex);
                startIndex += sizeof(short);
                short h = BitConverter.ToInt16(buff, startIndex);
                startIndex += sizeof(short);
                short m = BitConverter.ToInt16(buff, startIndex);
                startIndex += sizeof(short);
                short s = BitConverter.ToInt16(buff, startIndex);
                startIndex += sizeof(short);
                short ms = BitConverter.ToInt16(buff, startIndex);
                startIndex += sizeof(short);
                val = new DateTime(yy, mm, dd, h, m, s, ms, DateTimeKind.Utc);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool ReadStreamData(ref string value, CMD_STREAM cmd)
        {
            int iPos = 0;
            int iStrSize = 0;
            Encoding uniEnc = Encoding.GetEncoding("unicode");

            if (cmd.uiSize != cmd.bData.Length)
            {
                return false;
            }

            iStrSize = BitConverter.ToInt32(cmd.bData, iPos);
            iPos += sizeof(int);
            iStrSize -= sizeof(int) + 2;

            value = uniEnc.GetString(cmd.bData, iPos, iStrSize);
            iPos += iStrSize;

            return true;
        }

        public static bool ReadStreamData(ref ushort value, CMD_STREAM cmd)
        {
            int iPos = 0;

            if (cmd.uiSize != cmd.bData.Length)
            {
                return false;
            }

            value = BitConverter.ToUInt16(cmd.bData, iPos);
            iPos += sizeof(ushort);

            return true;
        }

        public static bool ReadStreamData(ref NotifySrvInfo value, CMD_STREAM cmd)
        {
            try
            {
                int iPos = 0;

                ushort ver = BitConverter.ToUInt16(cmd.bData, iPos);
                iPos += sizeof(ushort);
                uint size = BitConverter.ToUInt32(cmd.bData, iPos);
                iPos += sizeof(uint);

                if (size > cmd.bData.Length - 2)
                    return false;

                if (ReadData(ver, ref value.notifyID, cmd.bData, ref iPos) == false)
                    return false;
                if (ReadData(ver, ref value.time, cmd.bData, ref iPos) == false)
                    return false;
                if (ReadData(ver, ref value.param1, cmd.bData, ref iPos) == false)
                    return false;
                if (ReadData(ver, ref value.param2, cmd.bData, ref iPos) == false)
                    return false;
                if (ReadData(ver, ref value.param3, cmd.bData, ref iPos) == false)
                    return false;
                if (ReadData(ver, ref value.param4, cmd.bData, ref iPos) == false)
                    return false;
                if (ReadData(ver, ref value.param5, cmd.bData, ref iPos) == false)
                    return false;
                if (ReadData(ver, ref value.param6, cmd.bData, ref iPos) == false)
                    return false;
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool CreateStreamData(int value, ref CMD_STREAM cmd)
        {
            cmd.uiSize = sizeof(uint);
            cmd.bData = new byte[cmd.uiSize];

            int iPos = 0;

            Array.Copy(BitConverter.GetBytes(value), 0, cmd.bData, iPos, sizeof(uint));
            iPos += sizeof(uint);

            return true;
        }
    }
}
