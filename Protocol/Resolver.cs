using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DropZone.Protocol
{
    internal class Resolver : IDisposable
    {
        private readonly TcpClient _client;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;

        public Resolver(TcpClient client)
        {
            _client = client;
            _reader = new StreamReader(_client.GetStream());
            _writer = new StreamWriter(_client.GetStream());
        }

        public async Task<RequestData> WaitForRequestAsync()
        {
            var command = await _reader.ReadLineAsync();
            var data = await _reader.ReadLineAsync();
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