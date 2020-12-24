using DropZone.Protocol;

namespace DropZone.ViewModels.Messages
{
    internal class ReceivedChatMessage
    {
        public string Text { get; }

        public Station.Neighbor Sender { get; }

        public ReceivedChatMessage(string text, Station.Neighbor sender)
        {
            Text = text;
            Sender = sender;
        }
    }
}