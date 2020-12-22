using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DropZone.Protocol
{
    internal class Requester : IDisposable
    {
        private readonly TcpClient _client;
        private readonly StreamWriter _writer;
        private readonly StreamReader _reader;

        public Requester(TcpClient client)
        {
            _client = client;
            _writer = new StreamWriter(_client.GetStream()) { AutoFlush = true };
            _reader = new StreamReader(_client.GetStream());
        }

        public async Task SendCommand(string command, string data)
        {
            await _writer.WriteLineAsync(command);

            var dataToSend = string.IsNullOrEmpty(data) ? Constants.COMMAND_DATA_NONE : data;
            await _writer.WriteLineAsync(dataToSend);
        }

        public Task<string> WaitForResponseAsync()
        {
            return _reader.ReadLineAsync();
        }

        public void Dispose()
        {
            _reader?.Dispose();
            _writer?.Dispose();
            _client?.Dispose();
        }
    }
}