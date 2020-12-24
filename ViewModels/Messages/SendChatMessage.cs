using DropZone.Protocol;

namespace DropZone.ViewModels.Messages
{
    internal class SendChatMessage
    {
        public string Text { get; }
        public Station.Neighbor To { get; }

        public SendChatMessage(string text, Station.Neighbor to)
        {
            To = to;
            Text = text;
        }
    }
}