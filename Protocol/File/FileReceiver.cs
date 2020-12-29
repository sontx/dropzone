using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace DropZone.Protocol.File
{
    internal class FileReceiver : TcpClientBase
    {
        public string SaveDir { get; }
        public Action AcceptedHeader { get; set; }

        public string FileName { get; private set; }

        public string RelativeDir { get; private set; }

        public long TotalBytes { get; private set; }

        public long ReceivedBytes { get; private set; }

        public string SavedPath { get; private set; }

        public FileReceiver(TcpClient client, string saveDir)
            : base(client)
        {
            SaveDir = saveDir;
        }

        public void Receive()
        {
            using (var stream = Stream)
            {
                ReceiveHeader(stream);
                AcceptedHeader?.Invoke();
                SavedPath = PrepareSavingFile();
                ReceiveBody(SavedPath, stream);
                CleanUpIfNeeded(SavedPath);
            }
        }

        private string PrepareSavingFile()
        {
            var file = string.IsNullOrEmpty(RelativeDir)
                ? Path.Combine(SaveDir, FileName)
                : Path.Combine(SaveDir, RelativeDir, FileName);

            var dir = Path.GetDirectoryName(file);
            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return file;
        }

        private void ReceiveHeader(NetworkStream stream)
        {
            var buffer = new byte[Constants.BufferSizeHeader];
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
            FileName = parts[offset++];
            if (parts.Length == 3)
                RelativeDir = parts[offset++];
            TotalBytes = long.Parse(parts[offset]);
        }

        private void ReceiveBody(string file, NetworkStream stream)
        {
            using (var fs = new FileStream(file, FileMode.Create))
            {
                var buffer = new byte[Constants.BufferSizeSocket];
                while (ReceivedBytes < TotalBytes)
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
            if (ReceivedBytes < TotalBytes && System.IO.File.Exists(file))
                System.IO.File.Delete(file);
        }
    }
}