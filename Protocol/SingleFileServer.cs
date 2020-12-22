using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DropZone.Protocol
{
    internal class SingleFileServer : IDisposable
    {
        private readonly string _saveDir;
        private readonly TcpListener _listener;

        public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

        public SingleFileServer(string saveDir)
        {
            _saveDir = saveDir;
            _listener = new TcpListener(IPAddress.Any, 0);
        }

        public Task StartAsync()
        {
            return Task.Run(() => { _listener.Start(1); });
        }

        public FileReceiver AcceptReceiver()
        {
            var client = _listener.AcceptTcpClient();
            var receiver = new FileReceiver(client, _saveDir);
            return receiver;
        }

        public void Dispose()
        {
            _listener.Stop();
        }
    }
}