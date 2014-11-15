using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Threading;
using EpgTimer;
using System.Runtime.Serialization.Json;
using CtrlCmdCLI.Def;
using System.Text.RegularExpressions;

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
            Console.WriteLine("Starting Web Server...");
            try
            {
                Listener.Start();
                Listener.BeginAcceptTcpClient(AcceptRequest, Listener);
                IsListen = true;
                Console.WriteLine("Started Web Server");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fail Start Web Server");
                Thread.Sleep(5000);
                Environment.Exit(1);
            }
        }
        public void Stop()
        {
            Console.WriteLine("Stopping Web Server...");
            if (Listener == null) return;
            IsListen = false;
            Listener.Stop();
            Listener = null;
            Console.WriteLine("Stopped Web Server");
        }
        
        private void AcceptRequest(IAsyncResult Result)
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
                Client.Close();
            }
            catch (Exception ex)
            {
                Client.Close();
                Console.WriteLine("Error: {0}", ex.Message);
            }
            Client = null;
        }
    }
}
