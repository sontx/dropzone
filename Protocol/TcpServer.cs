using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DropZone.Protocol
{
    internal abstract class TcpServer
    {
        private readonly TcpListener _listener;
        private readonly Thread _thread;

        public int Port { get; }

        protected TcpServer(int port)
        {
            Port = port;
            _listener = new TcpListener(IPAddress.Any, port);
            _thread = new Thread(WaitForConnections) { IsBackground = true };
        }

        private void WaitForConnections()
        {
            try
            {
                while (true)
                {
                    var client = _listener.AcceptTcpClient();
                    OnAcceptClient(client);
                }
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        protected abstract void OnAcceptClient(TcpClient client);

        public void Start()
        {
            _listener.Start();
            _thread.Start();
        }

        public void Stop()
        {
            _listener.Stop();
        }
    }
}