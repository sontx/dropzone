using System;
using System.Net.Sockets;

namespace DropZone.Protocol.Chat
{
    internal class ChatServer : TcpServer
    {
        public Action<ChatClient> ChatHandler { get; set; }

        public ChatServer() : base(Constants.ChatPort)
        {
        }

        protected override void OnAcceptClient(TcpClient client)
        {
            ChatHandler?.Invoke(new ChatClient(client));
        }
    }
}