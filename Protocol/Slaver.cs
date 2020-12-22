using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DropZone.Protocol
{
    internal class Slaver
    {
        private readonly string _ipAddress;
        private readonly int _port;

        public Slaver(string ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
        }

        public async Task<int> RequestSendFilesAsync(IEnumerable<string> files)
        {
            var client = new TcpClient();
            await client.ConnectAsync(_ipAddress, _port);
            using (var requester = new Requester(client))
            {
                return await requester.RequestSendFiles(files);
            }
        }
    }
}