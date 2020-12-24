using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DropZone.Protocol
{
    internal class Slaver
    {
        private readonly string _ipAddress;
        private readonly int _port;

        public static Slaver ConnectToMaster(string address)
        {
            return new Slaver(address, Constants.MASTER_PORT);
        }

        public Slaver(string ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
        }

        public async Task<int> RequestSendFilesAsync(IEnumerable<string> files)
        {
            var client = await ConnectAsync();
            using (var requester = new Requester(client))
            {
                return await requester.RequestSendFilesAsync(files);
            }
        }

        public async Task SendChatAsync(string message)
        {
            var client = await ConnectAsync();
            using (var requester = new Requester(client))
            {
                await requester.SendChatAsync(message);
            }
        }

        private async Task<TcpClient> ConnectAsync()
        {
            var client = new TcpClient();
            await client.ConnectAsync(_ipAddress, _port);
            return client;
        }
    }
}