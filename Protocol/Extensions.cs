using System.Net.Sockets;

namespace DropZone.Protocol
{
    internal static class Extensions
    {
        public static void ConfigSocket(this TcpClient client)
        {
            client.ReceiveBufferSize = Constants.BufferSizeSocket;
            client.SendBufferSize = Constants.BufferSizeSocket;
        }
    }
}