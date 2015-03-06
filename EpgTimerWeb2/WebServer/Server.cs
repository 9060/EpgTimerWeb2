using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EpgTimer
{
    public class WebServer
    {
        private TcpListener Listener = null;
        private bool IsListen = false;
        private int Port = 8080;
        
        public WebServer(int P)
        {
            Port = P;
        }
        
        public void Start()
        {
            if (Listener != null)
                return;

            var EndPoint = new IPEndPoint(IPAddress.Any, Port);
            Listener = new TcpListener(EndPoint);
            Console.WriteLine("Webサーバ起動中...");
            try
            {
                Listener.Start();
                Listener.BeginAcceptTcpClient(AcceptRequest, Listener);
                IsListen = true;
                Console.WriteLine("Webサーバ起動完了");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Webサーバ起動失敗: {0}", ex.Message);
                Thread.Sleep(5000);
                Environment.Exit(1);
            }
        }
        public void Stop()
        {
            Console.WriteLine("Webサーバ停止中");
            if (Listener == null) return;
            IsListen = false;
            Listener.Stop();
            Listener = null;
            Console.WriteLine("Webサーバ停止完了");
        }
        
        private void AcceptRequest(IAsyncResult Result)
        {
            try
            {
                var RequsetListener = (TcpListener)Result.AsyncState;
                if (!IsListen) return;
                RequsetListener.BeginAcceptTcpClient(AcceptRequest, RequsetListener);
                var Client = RequsetListener.EndAcceptTcpClient(Result);
                var IP = ((IPEndPoint)Client.Client.RemoteEndPoint).Address;
                try
                {
                    ServerAction.DoProcess(Client);
                }
                catch (TimeoutException to)
                {
                    Debug.Print("Timeout: {0}", to.Message);
                }
                Client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("エラー: {0}", ex.Message);
            }
        }
    }
}
