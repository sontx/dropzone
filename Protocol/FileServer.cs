using System;
using System.Net.Sockets;

namespace DropZone.Protocol
{
    internal class FileServer : TcpServer
    {
        public Action<FileReceiver> FileReceiverHandler { get; set; }

        public string SaveDir { get; set; }

        public FileServer(int port) : base(port)
        {
        }

        protected override void OnAcceptClient(TcpClient client)
        {
            var receiver = new FileReceiver(client, SaveDir);
            FileReceiverHandler?.Invoke(receiver);
        }
    }
}