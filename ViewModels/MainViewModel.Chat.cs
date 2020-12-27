using DropZone.Protocol;
using DropZone.ViewModels.Messages;
using System.Threading.Tasks;

namespace DropZone.ViewModels
{
    public partial class MainViewModel
    {
        private Task HandleChatMessageAsync(Resolver resolver, string message)
        {
            return Task.Run(() =>
            {
                var neighbors = _station.Neighbors;
                var remoteAddress = resolver.RemoteAddress;
                var found = neighbors.Find(item => item.Address == remoteAddress) ??
                            new Station.Neighbor { Address = remoteAddress };

                ChatWindowManager.Show(found);
                MessengerInstance.Send(new ReceivedChatMessage(message, found));
            });
        }

        private async void HandleSendChatMessage(SendChatMessage msg)
        {
            var slaver = Slaver.ConnectToMaster(msg.To.Address);
            await slaver.SendChatAsync(msg.Text);
        }

        private void HandleSendAttachmentMessage(SendAttachmentMessage msg)
        {
            SendFiles(msg.Files, msg.To);
        }
    }
}