using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DropZone.Utils;

namespace DropZone.Protocol
{
    internal class Requester : IDisposable
    {
        private readonly TcpClient _client;
        private readonly Stream _stream;
        private readonly StreamReader _reader;

        public Requester(TcpClient client)
        {
            _client = client;
            _client.ConfigSocket();

            _stream = _client.GetStream();
            _reader = new StreamReader(_client.GetStream());
        }

        public async Task SendCommand(string command, string data)
        {
            Debugger.Log($"Send command: {command}");

            var commandBytes = Encoding.UTF8.GetBytes(command + Environment.NewLine);
            await _stream.WriteAsync(commandBytes, 0, commandBytes.Length);

            var dataToSend = string.IsNullOrEmpty(data) ? Constants.COMMAND_DATA_NONE : data;
            var dataBytes = Encoding.UTF8.GetBytes(dataToSend);

            var lengthBytes = Encoding.UTF8.GetBytes(dataBytes.Length + Environment.NewLine);
            await _stream.WriteAsync(lengthBytes, 0, lengthBytes.Length);

            await _stream.WriteAsync(dataBytes, 0, dataBytes.Length);
            await _stream.FlushAsync();
        }

        public Task<string> WaitForResponseAsync()
        {
            return _reader.ReadLineAsync();
        }

        public void Dispose()
        {
            _reader?.Dispose();
            _stream?.Dispose();
            _client?.Dispose();
        }
    }
}