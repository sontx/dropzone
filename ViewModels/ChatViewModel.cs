using DropZone.Protocol;
using DropZone.Protocol.Chat;
using DropZone.Utils;
using DropZone.ViewModels.Messages;
using GalaSoft.MvvmLight;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace DropZone.ViewModels
{
    internal class ChatViewModel : ViewModelBase
    {
        private readonly ChatClient _chatClient;
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
            : this(null, neighbor)
        {
        }

        public ChatViewModel(ChatClient chatClient, Station.Neighbor neighbor)
        {
            _chatClient = chatClient ?? new ChatClient(neighbor.Address, Constants.ChatPort);
            _chatClient.ReceivedMessage = HandleReceivedChatMessage;
            _chatClient.Start();

            Neighbor = neighbor;
            Title = Neighbor.Name;
        }

        private void HandleReceivedChatMessage(ChatClient.Message msg)
        {
            Debugger.Log($"Received chat message \"{msg.Type}\" from {Neighbor}");

            ThreadUtils.RunOnUiAndWait(() =>
            {
                if (msg.Type == ChatClient.MessageType.Text)
                {
                    Bubbles.Add(new BubbleViewModel
                    {
                        Text = msg.Data,
                        IsLeft = true
                    });
                }
                else if (msg.Type == ChatClient.MessageType.Attachment)
                {
                    var files = msg.Data.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    ShowAttachments(files, true, false);
                }
            });
        }

        public async void SendMessage(string msg)
        {
            if (msg == null)
                return;

            msg = msg.Trim();

            if (string.IsNullOrEmpty(msg))
                return;

            Debugger.Log($"Send chat message \"{msg}\" to {Neighbor}");

            Bubbles.Add(new BubbleViewModel
            {
                Text = msg,
                IsLeft = false
            });

            await _chatClient.SendAsync(msg, ChatClient.MessageType.Text);
        }

        public async void SendAttachment(string[] files)
        {
            if (files == null || files.Length == 0)
                return;

            Debugger.Log($"Send {files.Length} attachments to {Neighbor}");

            ShowAttachments(files, false, true);

            MessengerInstance.Send(new SendAttachmentMessage(files, Neighbor));

            await _chatClient.SendAsync(string.Join("|", files.Select(Path.GetFileName)), ChatClient.MessageType.Attachment);
        }

        private void ShowAttachments(string[] files, bool isFromNeighbor, bool calculateSize)
        {
            Bubbles.Add(new BubbleViewModel
            {
                Attachments = files.Select(file => new AttachmentViewModel
                {
                    Path = file,
                    IsFolder = false,
                    Name = Path.GetFileName(file),
                    Size = calculateSize ? FileUtils.GetFriendlyFileSize(file) : string.Empty
                }),
                IsLeft = isFromNeighbor
            });
        }

        public override void Cleanup()
        {
            base.Cleanup();
            _chatClient.Stop();
        }
    }
}