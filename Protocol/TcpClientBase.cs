using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DropZone.Protocol
{
    public abstract class TcpClientBase : IDisposable
    {
        protected readonly object SyncObj = new object();

        private readonly string _host;
        private readonly int _port;
        private TcpClient _client;

        protected NetworkStream Stream
        {
            get
            {
                lock (SyncObj)
                {
                    return _client?.GetStream();
                }
            }
        }

        public string RemoteAddress { get; private set; }

        protected TcpClientBase(TcpClient client)
        {
            _client = client;
            _client.ConfigSocket();
            RemoteAddress =(_client.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString();
        }

        protected TcpClientBase(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public Task ConnectIfNeededAsync()
        {
            return Task.Run(() =>
            {
                lock (SyncObj)
                {
                    if (_client != null)
                        return;

                    _client = new TcpClient();
                    _client.Connect(_host, _port);
                    _client.ConfigSocket();

                    RemoteAddress = (_client.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString();

                    OnConnected();
                }
            });
        }

        protected virtual void OnConnected()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (SyncObj)
                {
                    _client?.Dispose();
                    _client = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}