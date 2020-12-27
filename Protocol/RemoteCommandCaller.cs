using DropZone.Utils;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DropZone.Protocol
{
    public class RemoteCommandCaller : IDisposable
    {
        private readonly Station.Neighbor _neighbor;
        private readonly TcpClient _client;
        private readonly Thread _outputThread;
        private StreamWriter _writer;
        private StreamReader _reader;

        public Action<string> ReceivedOutput { get; set; }
        public Action<string> ReceivedError { get; set; }

        public RemoteCommandCaller(Station.Neighbor neighbor)
        {
            _neighbor = neighbor;
            _client = new TcpClient();
            _outputThread = new Thread(WaitForRemoteOutput) { IsBackground = true };
        }

        private void WaitForRemoteOutput()
        {
            try
            {
                while (true)
                {
                    var line = _reader.ReadLine();

                    if (line == null)
                        break;

                    if (line.StartsWith(Constants.REMOTE_COMMAND_OUTPUT_PREFIX))
                        ReceivedOutput?.Invoke(line.Substring(Constants.REMOTE_COMMAND_OUTPUT_PREFIX.Length + 1));
                    else if (line.StartsWith(Constants.REMOTE_COMMAND_ERROR_PREFIX))
                        ReceivedError?.Invoke(line.Substring(Constants.REMOTE_COMMAND_ERROR_PREFIX.Length + 1));
                }
            }
            catch
            {
                // ignored
            }
        }

        public async Task<bool> CallAsync(string command)
        {
            try
            {
                await _client.ConnectAsync(_neighbor.Address, Constants.REMOTE_COMMAND_PORT);
                _writer = new StreamWriter(_client.GetStream()) { AutoFlush = true };
                _reader = new StreamReader(_client.GetStream());
                _outputThread.Start();

                await _writer.WriteLineAsync(command);

                return true;
            }
            catch (Exception ex)
            {
                Debugger.Log($"Error while call remote command: {command}");
                ReceivedError?.Invoke(ex.Message);
            }

            return false;
        }

        public void SendInput(string line)
        {
            try
            {
                _writer?.WriteLine(line);
            }
            catch (Exception ex)
            {
                Debugger.Log($"Error while send remote command: {line}");
                ReceivedError?.Invoke(ex.Message);
            }
        }

        public void Dispose()
        {
            _reader?.Dispose();
            _writer?.Dispose();
            _client?.Close();
        }
    }
}