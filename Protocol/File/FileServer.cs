using System;
using System.Net.Sockets;

namespace DropZone.Protocol.File
{
    internal class FileServer : TcpServer
    {
        public Action<ReceivingSessionHandler> OnSessionHandler { get; set; }

        public FileServer()
            : base(Constants.FileServerPort)
        {
        }

        protected override void OnAcceptClient(TcpClient client)
        {
            OnSessionHandler?.Invoke(new ReceivingSessionHandler(client));
        }
    }
}