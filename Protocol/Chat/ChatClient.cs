using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DropZone.Protocol.Chat
{
    internal class ChatClient : ReceivedThreadTcpClient
    {
        public Action<string> ReceivedMessage { get; set; }

        public ChatClient(TcpClient client)
            : base(client)
        {
        }

        public ChatClient(string host, int port)
            : base(host, port)
        {
        }

        protected override void OnExitIncomingMessageLoop()
        {
            Stop();
            base.OnExitIncomingMessageLoop();
        }

        protected override bool OnReadNextIncomingMessage()
        {
            var line = ReadLine();
            if (line == null)
                return false;

            line = WebUtility.UrlDecode(line);
            ReceivedMessage?.Invoke(line);

            return true;
        }

        public override async void Start()
        {
            await ConnectIfNeededAsync();
            base.Start();
        }

        public async Task SendAsync(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
                return;

            await ConnectIfNeededAsync();

            var encoded = WebUtility.UrlEncode(msg.Trim());
            await SendLineAsync(encoded);
        }
    }
}