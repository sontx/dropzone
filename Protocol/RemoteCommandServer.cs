using System;
using System.IO;
using System.Net.Sockets;

namespace DropZone.Protocol
{
    internal class RemoteCommandServer : TcpServer
    {
        public Action<RemoteCommandExecutor> ExecutorHandler { get; set; }

        public RemoteCommandServer()
            : base(Constants.REMOTE_COMMAND_PORT)
        {
        }

        protected override async void OnAcceptClient(TcpClient client)
        {
            var reader = new StreamReader(client.GetStream());
            var command = await reader.ReadLineAsync();
            reader.DiscardBufferedData();

            var executor = new RemoteCommandExecutor(client, command);
            ExecutorHandler?.Invoke(executor);
        }
    }
}