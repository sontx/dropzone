using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DropZone.Protocol.File
{
    internal class SendingSessionFactory : ReadWriteTcpClient
    {
        private readonly string _host;
        private readonly IEnumerable<string> _filesWillBeSent;

        public SendingSessionFactory(string host, IEnumerable<string> filesWillBeSent)
            : base(host, Constants.FileServerPort)
        {
            _host = host;
            _filesWillBeSent = filesWillBeSent;
        }

        public async Task<Session> CreateSessionAsync()
        {
            await ConnectIfNeededAsync();
            await SendLineAsync(Constants.SendingFileSessionHeader);
            await SendLineAsync(_filesWillBeSent.Count().ToString());
            foreach (var file in _filesWillBeSent)
            {
                await SendLineAsync(file);
            }

            var response = await ReadLineAsync();
            return new Session(_host, int.Parse(response));
        }

        public class Session
        {
            private readonly string _remoteHost;
            private readonly int _remotePort;

            public Session(string remoteHost, int remotePort)
            {
                _remoteHost = remoteHost;
                _remotePort = remotePort;
            }

            public FileSender CreateSender()
            {
                return new FileSender(_remoteHost, _remotePort);
            }
        }
    }
}