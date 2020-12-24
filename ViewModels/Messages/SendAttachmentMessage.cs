using DropZone.Protocol;

namespace DropZone.ViewModels.Messages
{
    internal class SendAttachmentMessage
    {
        public string[] Files { get; }
        public Station.Neighbor To { get; }

        public SendAttachmentMessage(string[] files, Station.Neighbor to)
        {
            To = to;
            Files = files;
        }
    }
}