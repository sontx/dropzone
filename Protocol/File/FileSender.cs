using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DropZone.Protocol.File
{
    internal class FileSender : TcpClientBase
    {
        public long TotalBytes { get; private set; }
        public long SentBytes { get; private set; }

        public FileSender(string host, int port)
            : base(host, port)
        {
        }

        public void Connect()
        {
            ConnectIfNeededAsync().Wait();
        }

        public void Send(string file, string baseDir)
        {
            if (!System.IO.File.Exists(file))
                return;

            TotalBytes = new FileInfo(file).Length;

            using (var stream = Stream)
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
            var combined = $"{name}|{GetRelativeDir(file, baseDir)}|{size}";
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
                var buffer = new byte[Constants.BufferSizeSocket];
                do
                {
                    var readLength = fs.Read(buffer, 0, buffer.Length);
                    if (readLength <= 0) break;
                    stream.Write(buffer, 0, readLength);
                    SentBytes += readLength;
#if DEBUG && LATENCY
                    Thread.Sleep(random.Next(Constants.DebugMinDelay, Constants.DebugMaxDelay));
#endif
                } while (true);

                stream.Flush();
            }
        }
    }
}