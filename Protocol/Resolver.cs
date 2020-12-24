using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DropZone.Protocol
{
    public class Resolver : IDisposable
    {
        private readonly TcpClient _client;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;

        public string RemoteAddress => (_client.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString();

        public Resolver(TcpClient client)
        {
            _client = client;
            _client.ConfigSocket();
            _reader = new StreamReader(_client.GetStream());
            _writer = new StreamWriter(_client.GetStream());
        }

        public async Task<RequestData> WaitForRequestAsync()
        {
            var command = await _reader.ReadLineAsync();

            var buffer = new char[Constants.BUFFER_SIZE_SOCKET];
            var length = await _reader.ReadAsync(buffer, 0, buffer.Length);
            var data = new string(buffer, 0, length);

            return new RequestData
            {
                Command = command,
                Data = data
            };
        }

        public Task Respond(string data)
        {
            return _writer.WriteLineAsync(data);
        }

        public void Dispose()
        {
            _writer?.Dispose();
            _reader?.Dispose();
            _client?.Dispose();
        }

        public class RequestData
        {
            public string Command { get; set; }
            public string Data { get; set; }
        }
    }
}