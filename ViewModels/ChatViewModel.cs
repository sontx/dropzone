using DropZone.Protocol;
using DropZone.ViewModels.Messages;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace DropZone.ViewModels
{
    public class ChatViewModel : ViewModelBase
    {
        public Station.Neighbor Neighbor { get; }

        public string Title { get; }

        public ObservableCollection<BubbleViewModel> Bubbles { get; } = new ObservableCollection<BubbleViewModel>();

        public ChatViewModel()
        {
            if (IsInDesignMode)
            {
                Bubbles.Add(new BubbleViewModel
                {
                    IsLeft = true,
                    Text = "Recap of all previous answers, different ways to convert a string to a Color or Brush"
                });
                Bubbles.Add(new BubbleViewModel
                {
                    IsLeft = false,
                    Text = "Recap of all previous answers, different ways to convert a string to a Color or Brush"
                });
                Bubbles.Add(new BubbleViewModel
                {
                    IsLeft = true,
                    Text = "Recap of all previous answers, different ways to convert a string to a Color or Brush"
                });
                Bubbles.Add(new BubbleViewModel
                {
                    IsLeft = true,
                    Text = "Recap of all previous answers, different ways to convert a string to a Color or Brush"
                });
                Bubbles.Add(new BubbleViewModel
                {
                    IsLeft = false,
                    Text = "Recap of all previous answers, different ways to convert a string to a Color or Brush"
                });
            }
        }

        public ChatViewModel(Station.Neighbor neighbor)
        {
            Neighbor = neighbor;
            Title = Neighbor.ToString();
            MessengerInstance.Register<ReceivedChatMessage>(this, HandleReceivedChatMessage);
        }

        private void HandleReceivedChatMessage(ReceivedChatMessage msg)
        {
            if (msg.Sender.Address != Neighbor.Address)
                return;

            ThreadUtils.RunOnUiAndWait(() =>
            {
                Bubbles.Add(new BubbleViewModel
                {
                    Text = msg.Text,
                    IsLeft = true
                });
            });
        }

        public void SendMessage(string msg)
        {
            if (msg == null)
                return;

            msg = msg.Trim();

            if (string.IsNullOrEmpty(msg))
                return;

            Bubbles.Add(new BubbleViewModel
            {
                Text = msg,
                IsLeft = false
            });

            MessengerInstance.Send(new SendChatMessage(msg, Neighbor));
        }

        public void SendAttachment(string[] files)
        {
            if (files == null || files.Length == 0)
                return;

            Bubbles.Add(new BubbleViewModel
            {
                Attachments = files.Select(file =>
                {
                    var isFolder = !FileUtils.IsFile(file);
                    return new AttachmentViewModel
                    {
                        Path = file,
                        IsFolder = isFolder,
                        Name = Path.GetFileName(file),
                        Size = isFolder ? string.Empty : FileUtils.GetFriendlyFileSize(file)
                    };
                }),
                IsLeft = false
            });

            MessengerInstance.Send(new SendAttachmentMessage(files, Neighbor));
        }
    }
}