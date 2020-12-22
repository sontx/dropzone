using System;
using System.Net.Sockets;

namespace DropZone.Protocol
{
    internal class Master : TcpServer
    {
        public Action<Resolver> ResolverHandler { get; set; }

        public Master(int port) : base(port)
        {
        }

        protected override void OnAcceptClient(TcpClient client)
        {
            var resolver = new Resolver(client);
            ResolverHandler?.Invoke(resolver);
        }
    }
}