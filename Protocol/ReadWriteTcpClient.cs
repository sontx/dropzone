using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DropZone.Protocol
{
    public abstract class ReadWriteTcpClient : TcpClientBase
    {
        private StreamWriter _writer;
        private StreamReader _reader;

        protected ReadWriteTcpClient(TcpClient client)
            : base(client)
        {
            InitializeStreams();
        }

        protected ReadWriteTcpClient(string host, int port)
            : base(host, port)
        {
        }

        protected override void OnConnected()
        {
            InitializeStreams();
            base.OnConnected();
        }

        private void InitializeStreams()
        {
            lock (SyncObj)
            {
                _reader = new StreamReader(Stream);
                _writer = new StreamWriter(Stream) { AutoFlush = true };
            }
        }

        protected Task<string> ReadLineAsync()
        {
            return _reader?.ReadLineAsync();
        }

        protected string ReadLine()
        {
            return _reader?.ReadLine();
        }

        protected Task SendLineAsync(string msg)
        {
            return _writer?.WriteLineAsync(msg);
        }

        protected void SendLine(string msg)
        {
            _writer?.WriteLine(msg);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (SyncObj)
                {
                    _writer?.Dispose();
                    _reader?.Dispose();

                    _writer = null;
                    _reader = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}