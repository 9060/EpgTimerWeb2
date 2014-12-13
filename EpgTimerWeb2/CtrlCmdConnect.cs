using CtrlCmdCLI.Def;
using EpgTimer;
using EpgTimerWeb2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace EpgTimer
{
    public class Reader
    {
        private static Thread inputThread;
        private static AutoResetEvent getInput, gotInput;
        private static string input;

        public Reader()
        {
            getInput = new AutoResetEvent(false);
            gotInput = new AutoResetEvent(false);
            inputThread = new Thread(reader);
            inputThread.IsBackground = true;
            inputThread.Start();
        }

        private static void reader()
        {
            while (true)
            {
                getInput.WaitOne();
                input = Console.ReadLine();
                gotInput.Set();
            }
        }

        public string ReadLine(int timeOutMillisecs)
        {
            getInput.Set();
            bool success = gotInput.WaitOne(timeOutMillisecs);
            if (success)
                return input;
            else
                return null;
        }
    }
    public class CtrlCmdConnect
    {
        public static void Connect()
        {

            Console.WriteLine("Connecting...");
            if (!PrivateSetting.Instance.CmdConnect.
                StartConnect(Setting.Instance.CtrlHost, (int)Setting.Instance.CallbackPort, (int)Setting.Instance.CtrlPort))
            {
                Console.WriteLine("Server {0}:{1} ({2}) Connect Failed", Setting.Instance.CtrlHost, Setting.Instance.CtrlPort, Setting.Instance.CallbackPort);
                Environment.Exit(1);
            }
            Console.WriteLine("Loading Data...");
            CommonManager.Instance.DB.ClearAllDB();
            CommonManager.Instance.DB.ReloadEpgAutoAddInfo();
            CommonManager.Instance.DB.ReloadEpgData();
            CommonManager.Instance.DB.ReloadManualAutoAddInfo();
            CommonManager.Instance.DB.ReloadPlugInFile();
            CommonManager.Instance.DB.ReloadrecFileInfo();
            CommonManager.Instance.DB.ReloadReserveInfo();
            Console.WriteLine("Loaded Data");
        }
        private int OutsideCmdCallback(object pParam, CMD_STREAM pCmdParam, ref CMD_STREAM pResParam)
        {
            Trace.WriteLine((CtrlCmd)pCmdParam.uiParam);
            switch ((CtrlCmd)pCmdParam.uiParam)
            {
                case CtrlCmd.CMD_TIMER_GUI_SHOW_DLG:
                    {
                        pResParam.uiParam = (uint)ErrCode.CMD_SUCCESS;
                    }
                    break;
                case CtrlCmd.CMD_TIMER_GUI_UPDATE_RESERVE:
                    {
                        pResParam.uiParam = (uint)ErrCode.CMD_SUCCESS;
                        CommonManager.Instance.DB.SetUpdateNotify((UInt32)UpdateNotifyItem.ReserveInfo);
                        CommonManager.Instance.DB.SetUpdateNotify((UInt32)UpdateNotifyItem.RecInfo);
                        CommonManager.Instance.DB.SetUpdateNotify((UInt32)UpdateNotifyItem.AutoAddEpgInfo);
                        CommonManager.Instance.DB.SetUpdateNotify((UInt32)UpdateNotifyItem.AutoAddManualInfo);

                        CommonManager.Instance.DB.ReloadReserveInfo();
                        ReserveData item = new ReserveData();
                        if (CommonManager.Instance.DB.GetNextReserve(ref item) == true)
                        {
                            String timeView = item.StartTime.ToString("yyyy/MM/dd(ddd) HH:mm:ss ～ ");
                            DateTime endTime = item.StartTime + TimeSpan.FromSeconds(item.DurationSecond);
                            timeView += endTime.ToString("HH:mm:ss");
                        }
                    }
                    break;
                case CtrlCmd.CMD_TIMER_GUI_UPDATE_EPGDATA:
                    {
                        pResParam.uiParam = (uint)ErrCode.CMD_SUCCESS;
                        CommonManager.Instance.DB.SetUpdateNotify((UInt32)UpdateNotifyItem.EpgData);
                    }
                    break;
                case CtrlCmd.CMD_TIMER_GUI_VIEW_EXECUTE:
                    {
                        pResParam.uiParam = (uint)ErrCode.CMD_SUCCESS;
                        String exeCmd = "";
                        CmdStreamUtil.ReadStreamData(ref exeCmd, pCmdParam);
                        try
                        {
                            string[] cmd = exeCmd.Split('\"');
                            Process process;
                            if (cmd.Length >= 3)
                            {
                                process = Process.Start(cmd[1], cmd[2]);
                            }
                            else if (cmd.Length >= 2)
                            {
                                process = Process.Start(cmd[1]);
                            }
                            else
                            {
                                process = Process.Start(cmd[0]);
                            }
                            CmdStreamUtil.CreateStreamData(process.Id, ref pResParam);
                        }
                        catch
                        {
                        }
                    }
                    break;
                case CtrlCmd.CMD_TIMER_GUI_QUERY_SUSPEND:
                    {
                        pResParam.uiParam = (uint)ErrCode.CMD_SUCCESS;
                    }
                    break;
                case CtrlCmd.CMD_TIMER_GUI_QUERY_REBOOT:
                    {
                        pResParam.uiParam = (uint)ErrCode.CMD_SUCCESS;
                    }
                    break;
                case CtrlCmd.CMD_TIMER_GUI_SRV_STATUS_CHG:
                    {
                        pResParam.uiParam = (uint)ErrCode.CMD_SUCCESS;
                        UInt16 status = 0;
                        CmdStreamUtil.ReadStreamData(ref status, pCmdParam);

                        if (status == 1) //Rec
                        {
                        }
                        else if (status == 2) //EPG
                        {
                        }
                        else
                        {

                        }

                    }
                    break;
                case CtrlCmd.CMD_TIMER_GUI_SRV_STATUS_NOTIFY2:
                    {
                        pResParam.uiParam = (uint)ErrCode.CMD_SUCCESS;
                        var Status = new NotifySrvInfo();
                        CmdStreamUtil.ReadStreamData(ref Status, pCmdParam);
                        var Notify = new NotifySrvInfoItem();
                        Notify.NotifyInfo = Status;
                        if (Notify.Title != "")
                        {
                            SocketAction.SendAllMessage("EVENT " + JsonUtil.Serialize(Notify));
                            //Console.WriteLine("\n" + (Notify.Title + Notify.LogText).Replace("\n", ""));
                            EventStore.Instance.AddMessage(Notify);
                        }

                        //Console.WriteLine("Sending Msg...");
                    }
                    break;
                default:
                    pResParam.uiParam = (uint)ErrCode.CMD_NON_SUPPORT;
                    break;
            }
            return 0;
        }
        public bool StopConnect()
        {
            if (!CommonManager.Instance.NW.IsConnected) return true;
            if ((ErrCode)CommonManager.Instance.CtrlCmd.SendUnRegistTCP(CommonManager.Instance.NW.CallbackPort) != ErrCode.CMD_SUCCESS &&
            CommonManager.Instance.NW.StopTCPServer()) return false;
            return true;
        }
        public bool StartConnect(string IP, int TcpPort, int CtrlPort)
        {
            var ping = new Ping();
            if (ping.Send(IP).Status != IPStatus.Success)
            {
                Console.Write("Ping can not be sent. Do you want to connect? \nCancel after 10 seconds.(y/n):");
                string Res = new Reader().ReadLine(10000);
                if(Res == null || !Res.ToLower().StartsWith("y"))
                    return false;
            }
            CommonManager.Instance.NWMode = true;
            if (!CommonManager.Instance.NW.ConnectServer(IP, (uint)CtrlPort
                , (uint)TcpPort, OutsideCmdCallback, this)) return false;
            byte[] binData;
            if (CommonManager.Instance.CtrlCmd.SendFileCopy("ChSet5.txt", out binData) == 1)
            {
                string filePath = @".\Setting";
                Directory.CreateDirectory(filePath);
                filePath += @"\ChSet5.txt";
                using (BinaryWriter w = new BinaryWriter(File.Create(filePath)))
                {
                    w.Write(binData);
                    w.Close();
                }
                ChSet5.LoadFile();
                return true;
            }
            return false;
        }
    }
}
