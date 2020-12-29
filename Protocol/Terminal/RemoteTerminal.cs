using DropZone.Utils;
using System;
using System.Threading.Tasks;

namespace DropZone.Protocol.Terminal
{
    public class RemoteTerminal : ReceivedThreadTcpClient
    {
        public Action<string> ReceivedOutput { get; set; }
        public Action<string> ReceivedError { get; set; }

        public RemoteTerminal(string address)
            : base(address, Constants.RemoteCommandPort)
        {
        }

        protected override bool OnReadNextIncomingMessage()
        {
            var line = ReadLine();

            if (line == null)
                return false;

            if (line.StartsWith(Constants.RemoteCommandOutputPrefix))
                ReceivedOutput?.Invoke(line.Substring(Constants.RemoteCommandOutputPrefix.Length + 1));
            else if (line.StartsWith(Constants.RemoteCommandErrorPrefix))
                ReceivedError?.Invoke(line.Substring(Constants.RemoteCommandErrorPrefix.Length + 1));

            return true;
        }

        public async Task<bool> CallAsync(string command)
        {
            try
            {
                await ConnectIfNeededAsync();
                Start();
                await SendLineAsync(command);

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
                SendLine(line);
            }
            catch (Exception ex)
            {
                Debugger.Log($"Error while send remote command: {line}");
                ReceivedError?.Invoke(ex.Message);
            }
        }
    }
}