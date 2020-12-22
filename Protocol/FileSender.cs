using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DropZone.Protocol
{
    internal class FileSender : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _name;
        private readonly TcpClient _client;

        public long TotalBytes { get; private set; }
        public long SentBytes { get; private set; }

        public string RemoteIdentify => (_client.Client.RemoteEndPoint as IPEndPoint)?.Address?.ToString();

        public FileSender(string host, int port, string name)
        {
            _host = host;
            _port = port;
            _name = name;
            _client = new TcpClient();
        }

        public void Connect()
        {
            _client.Connect(_host, _port);
        }

        public void Send(string file, string baseDir)
        {
            if (!File.Exists(file))
                return;

            TotalBytes = new FileInfo(file).Length;

            using (var stream = _client.GetStream())
            {
                SendHeader(file, baseDir, stream);
                SendBody(file, stream);
            }
        }

        // package1: length of header
        // package2: name | size
        private void SendHeader(string file, string baseDir, NetworkStream stream)
        {
            var fileInfo = new FileInfo(file);
            var name = fileInfo.Name;
            var size = fileInfo.Length;
            var combined = $"{_name}|{name}|{GetRelativeDir(file, baseDir)}|{size}";
            var combinedBytes = Encoding.UTF8.GetBytes(combined);

            var lengthBytes = BitConverter.GetBytes(combinedBytes.Length);
            stream.Write(lengthBytes, 0, 4);

            stream.Write(combinedBytes, 0, combinedBytes.Length);
        }

        private string GetRelativeDir(string file, string baseDir)
        {
            if (string.IsNullOrEmpty(baseDir))
                return null;

            if (!file.ToLower().StartsWith(baseDir.ToLower()))
                return null;

            var ret = file.Substring(baseDir.Length);
            if (ret.StartsWith("\\"))
                ret = ret.Substring(1);
            var name = Path.GetFileName(file);
            return ret != name ? ret.Substring(0, ret.Length - name.Length - 1) : null;
        }

        private void SendBody(string file, NetworkStream stream)
        {
#if DEBUG && LATENCY
            var random = new Random(DateTime.Now.Millisecond);
#endif
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var buffer = new byte[Constants.BUFFER_SIZE_SOCKET];
                do
                {
                    var readLength = fs.Read(buffer, 0, buffer.Length);
                    if (readLength <= 0) break;
                    stream.Write(buffer, 0, readLength);
                    SentBytes += readLength;
#if DEBUG && LATENCY
                    Thread.Sleep(random.Next(Constants.DEBUG_MIN_DELAY, Constants.DEBUG_MAX_DELAY));
#endif
                } while (true);

                stream.Flush();
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}