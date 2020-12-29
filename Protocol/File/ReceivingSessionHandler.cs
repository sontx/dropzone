using DropZone.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DropZone.Protocol.File
{
    internal class ReceivingSessionHandler : ReadWriteTcpClient
    {
        public ReceivingSessionHandler(TcpClient client)
            : base(client)
        {
        }

        public async Task<Session> AcceptSessionAsync()
        {
            var line = await ReadLineAsync();
            if (line == null)
                return null;

            if (line != Constants.SendingFileSessionHeader)
                return null;

            var willReceiveFiles = ReadRequest();

            var listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();
            var port = (listener.LocalEndpoint as IPEndPoint)?.Port;

            // respond to requester
            await SendLineAsync(port?.ToString());

            return new Session(listener, willReceiveFiles);
        }

        private List<string> ReadRequest()
        {
            var stLength = ReadLine();
            var length = int.Parse(stLength);
            var willReceiveFiles = new List<string>(length);
            for (var i = 0; i < length; i++)
            {
                var file = ReadLine();
                willReceiveFiles.Add(file);
            }

            return willReceiveFiles;
        }

        public sealed class Session : IDisposable
        {
            private readonly TcpListener _fileServer;
            private int _createdReceiverCount;

            public List<string> WillReceiveFiles { get; }

            public string SaveDir { get; set; }

            public Session(TcpListener fileServer, List<string> willReceiveFiles)
            {
                _fileServer = fileServer;
                WillReceiveFiles = willReceiveFiles;
            }

            public async Task<FileReceiver> AcceptReceiverAsync()
            {
                if (_createdReceiverCount >= WillReceiveFiles.Count)
                    return null;

                var client = await _fileServer.AcceptTcpClientAsync();

                var willReceiveFile = WillReceiveFiles[_createdReceiverCount++];
                Debugger.Log($"Created receiver for receiving {willReceiveFile}");

                return new FileReceiver(client, SaveDir);
            }

            public void Dispose()
            {
                _fileServer.Stop();
            }
        }
    }
}