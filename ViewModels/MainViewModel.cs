using DropZone.Protocol;
using DropZone.ViewModels.Messages;
using GalaSoft.MvvmLight;
using System;

namespace DropZone.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly Master _master;

        public string Title { get; }

        public MainViewModel()
        {
            if (IsInDesignMode) return;

            _station = new Station(Environment.MachineName);
            _master = new Master(Constants.MASTER_PORT)
            {
                ResolverHandler = HandleResolver
            };
            _commandServer = new RemoteCommandServer
            {
                ExecutorHandler = HandleRemoteExecutor
            };
            _master.Start();
            _station.Start();
            _commandServer.Start();

            Title = $"Drop Zone [{_station.Name}]";

            UpdateNeighborsSummary();
            _station.PropertyChanged += _station_PropertyChanged;

            MessengerInstance.Register<SendChatMessage>(this, HandleSendChatMessage);
            MessengerInstance.Register<SendAttachmentMessage>(this, HandleSendAttachmentMessage);
        }

        private async void HandleResolver(Resolver resolver)
        {
            try
            {
                await resolver.HandleNextRequestAsync(new CommandHandler
                {
                    HandleSendFilesAsync = HandleSendFilesAsync,
                    HandleChatMessageAsync = HandleChatMessageAsync
                });
            }
            finally
            {
                resolver.Dispose();
            }
        }

        public void Close()
        {
            _master.Stop();
            _station.Stop();
            _commandServer.Stop();
        }
    }
}