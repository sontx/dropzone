using DropZone.Utils;
using System.Net;
using System.Net.Sockets;

namespace DropZone.Protocol
{
    internal abstract class TcpServer
    {
        private readonly TcpListener _listener;
        private readonly ThreadWrapper _threadWrapper;

        public int Port { get; }

        protected TcpServer(int port)
        {
            Port = port;
            _listener = new TcpListener(IPAddress.Any, port);
            _threadWrapper = new ThreadWrapper
            {
                DoWork = WaitForConnections
            };
        }

        private void WaitForConnections()
        {
            while (true)
            {
                var client = _listener.AcceptTcpClient();
                OnAcceptClient(client);
            }
        }

        protected abstract void OnAcceptClient(TcpClient client);

        public void Start()
        {
            _listener.Start();
            _threadWrapper.Start();
        }

        public void Stop()
        {
            _listener.Stop();
        }
    }
}