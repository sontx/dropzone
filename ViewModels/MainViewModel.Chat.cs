using DropZone.Protocol.Chat;
using DropZone.ViewModels.Messages;

namespace DropZone.ViewModels
{
    public partial class MainViewModel
    {
        private ChatServer _chatServer;

        private void InitializeChat()
        {
            _chatServer = new ChatServer
            {
                ChatHandler = HandleChatClient
            };
            _chatServer.Start();

            MessengerInstance.Register<SendAttachmentMessage>(this, HandleSendAttachmentMessage);
        }

        private void CloseChat()
        {
            _chatServer?.Stop();
        }

        private void HandleChatClient(ChatClient chatClient)
        {
            var neighbor = _station.GetNeighbor(chatClient.RemoteAddress);
            ChatWindowManager.Show(neighbor, chatClient);
        }

        private void HandleSendAttachmentMessage(SendAttachmentMessage msg)
        {
            SendFiles(msg.Files, msg.To);
        }
    }
}