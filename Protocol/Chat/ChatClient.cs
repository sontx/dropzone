using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DropZone.Protocol.Chat
{
    internal class ChatClient : ReceivedThreadTcpClient
    {
        public Action<Message> ReceivedMessage { get; set; }

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
            var stMessageType = ReadLine();
            if (stMessageType == null)
                return false;

            var messageType = (MessageType)Enum.Parse(typeof(MessageType), stMessageType);
            var data = ReadLine();
            var decodedData = WebUtility.UrlDecode(data);
            ReceivedMessage?.Invoke(new Message
            {
                Data = decodedData,
                Type = messageType
            });

            return true;
        }

        public override async void Start()
        {
            await ConnectIfNeededAsync();
            base.Start();
        }

        public async Task SendAsync(string msg, MessageType type)
        {
            if (string.IsNullOrWhiteSpace(msg))
                return;

            await ConnectIfNeededAsync();

            await SendLineAsync(type.ToString());
            var encoded = WebUtility.UrlEncode(msg.Trim());
            await SendLineAsync(encoded);
        }

        public enum MessageType
        {
            Text,
            Attachment
        }

        public class Message
        {
            public string Data { get; set; }
            public MessageType Type { get; set; }
        }
    }
}