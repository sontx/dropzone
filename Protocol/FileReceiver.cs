using GalaSoft.MvvmLight;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DropZone.Protocol
{
    internal class FileReceiver : ObservableObject, IDisposable
    {
        private readonly TcpClient _client;
        private readonly string _saveDir;
        private string _fileName;
        private long _totalBytes;
        private string _relativeDir;
        private string _from;

        public Action AcceptedHeader { get; set; }

        public string FileName
        {
            get => _fileName;
            private set => Set(ref _fileName, value);
        }

        public string From
        {
            get => _from;
            private set => Set(ref _from, value);
        }

        public string RelativeDir
        {
            get => _relativeDir;
            private set => Set(ref _relativeDir, value);
        }

        public long TotalBytes
        {
            get => _totalBytes;
            private set => Set(ref _totalBytes, value);
        }

        public long ReceivedBytes { get; private set; }

        public string RemoteIdentify => (_client.Client.RemoteEndPoint as IPEndPoint)?.Address?.ToString();

        public FileReceiver(TcpClient client, string saveDir)
        {
            _client = client;
            _saveDir = saveDir;
        }

        public void Receive()
        {
            using (var stream = _client.GetStream())
            {
                ReceiveHeader(stream);
                AcceptedHeader?.Invoke();
                var file = PrepareSavingFile();
                ReceiveBody(file, stream);
                CleanUpIfNeeded(file);
            }
        }

        private string PrepareSavingFile()
        {
            var file = string.IsNullOrEmpty(RelativeDir)
                ? Path.Combine(_saveDir, FileName)
                : Path.Combine(_saveDir, RelativeDir, FileName);

            var dir = Path.GetDirectoryName(file);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return file;
        }

        private void ReceiveHeader(NetworkStream stream)
        {
            var buffer = new byte[Constants.BUFFER_SIZE_HEADER];
            var headerLengthBytes = stream.Read(buffer, 0, 4);
            if (headerLengthBytes != 4)
                throw new Exception("Header bytes length must be 4 bytes");

            var headerLength = BitConverter.ToInt32(buffer, 0);
            if (headerLength <= 0)
                throw new Exception("Header length must be greater than 0");

            var actualHeaderLength = stream.Read(buffer, 0, headerLength);
            if (actualHeaderLength != headerLength)
                throw new Exception($"Header length must be {headerLength} bytes but {actualHeaderLength} bytes");

            var combined = Encoding.UTF8.GetString(buffer, 0, headerLength);
            var parts = combined.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var offset = 0;
            From = parts[offset++];
            FileName = parts[offset++];
            if (parts.Length == 4)
                RelativeDir = parts[offset++];
            TotalBytes = long.Parse(parts[offset]);
        }

        private void ReceiveBody(string file, NetworkStream stream)
        {
            using (var fs = new FileStream(file, FileMode.Create))
            {
                var buffer = new byte[Constants.BUFFER_SIZE_SOCKET];
                while (ReceivedBytes < _totalBytes)
                {
                    var receivedLength = stream.Read(buffer, 0, buffer.Length);
                    if (receivedLength > 0)
                    {
                        fs.Write(buffer, 0, receivedLength);
                        ReceivedBytes += receivedLength;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void CleanUpIfNeeded(string file)
        {
            if (ReceivedBytes < TotalBytes && File.Exists(file))
                File.Delete(file);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}