using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DropZone.Protocol
{
    public class Resolver : IDisposable
    {
        private readonly TcpClient _client;
        private readonly StreamWriter _writer;
        private readonly Stream _stream;

        public string RemoteAddress => (_client.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString();

        public Resolver(TcpClient client)
        {
            _client = client;
            _client.ConfigSocket();
            _stream = _client.GetStream();
            _writer = new StreamWriter(_client.GetStream());
        }

        public async Task<RequestData> WaitForRequestAsync()
        {
            var command = await ReadLineAsync();
            var stLength = await ReadLineAsync();
            var length = int.Parse(stLength);

            var buffer = new byte[length];
            var totalReadLength = 0;
            do
            {
                var readLength = await _stream.ReadAsync(buffer, totalReadLength, buffer.Length - totalReadLength);
                totalReadLength += readLength;
            } while (buffer.Length > totalReadLength);

            var data = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

            return new RequestData
            {
                Command = command,
                Data = data
            };
        }

        private Task<string> ReadLineAsync()
        {
            return Task.Run(() =>
            {
                var buffer = new byte[Constants.BUFFER_SIZE_SOCKET];
                var i = 0;
                for (; i < buffer.Length; i++)
                {
                    var readByte = _stream.ReadByte();
                    if (readByte < 0)
                    {
                        i -= 1;
                        break;
                    }

                    buffer[i] = (byte)readByte;

                    if (i > 0 && buffer[i - 1] == 13 && buffer[i] == 10)
                    {
                        buffer[i - 1] = 0;
                        buffer[i] = 0;
                        i -= 1;
                        break;
                    }
                }

                return Encoding.UTF8.GetString(buffer, 0, i);
            });
        }

        public Task Respond(string data)
        {
            return _writer.WriteLineAsync(data);
        }

        public void Dispose()
        {
            _writer?.Dispose();
            _stream?.Dispose();
            _client?.Dispose();
        }

        public class RequestData
        {
            public string Command { get; set; }
            public string Data { get; set; }
        }
    }
}