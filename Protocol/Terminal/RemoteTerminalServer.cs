using System;
using System.IO;
using System.Net.Sockets;

namespace DropZone.Protocol.Terminal
{
    internal class RemoteTerminalServer : TcpServer
    {
        public Action<RemoteTerminalExecutor> ExecutorHandler { get; set; }

        public RemoteTerminalServer()
            : base(Constants.RemoteCommandPort)
        {
        }

        protected override async void OnAcceptClient(TcpClient client)
        {
            var reader = new StreamReader(client.GetStream());
            var command = await reader.ReadLineAsync();
            reader.DiscardBufferedData();

            var executor = new RemoteTerminalExecutor(client, command);
            ExecutorHandler?.Invoke(executor);
        }
    }
}