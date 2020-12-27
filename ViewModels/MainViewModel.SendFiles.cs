using DropZone.Models;
using DropZone.Protocol;
using DropZone.Utils;
using DropZone.Views;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DropZone.ViewModels
{
    public partial class MainViewModel
    {
        private bool _isReadyToSend;
        private string _currentScannedFile;
        private bool _isListingSendingFiles;

        public bool IsReadyToSend
        {
            get => _isReadyToSend;
            set => Set(ref _isReadyToSend, value);
        }

        public bool IsListingSendingFiles
        {
            get => _isListingSendingFiles;
            private set => Set(ref _isListingSendingFiles, value);
        }

        public string CurrentScannedFile
        {
            get => _currentScannedFile;
            set => Set(ref _currentScannedFile, value);
        }

        private async Task HandleSendFilesAsync(Resolver resolver, IEnumerable<string> receivingFiles)
        {
            var settings = SettingsUtils.Get<AppSettings>();
            var fileSever = new SingleFileServer(settings.ReceivedFilesDir);
            await fileSever.StartAsync();
            ThreadUtils.RunOnUiAndWait(() =>
            {
                var transferWindow = new TransferWindow();
                var receivingViewModel = new ReceivingViewModel(fileSever, receivingFiles) { Title = "Receiver" };
                transferWindow.DataContext = receivingViewModel;
                transferWindow.Show();
                receivingViewModel.Start();
            });
            await resolver.Respond(fileSever.Port.ToString());
        }

        public async void SendFiles(string[] files, Station.Neighbor toNeighbor = null)
        {
            if (files == null || files.Length == 0)
                return;

            CurrentScannedFile = string.Empty;

            IsReadyToSend = false;
            IsListingSendingFiles = true;

            await Task.Run(() =>
            {
                var fileModels = ListFilesWillBeSent(files);

                IsListingSendingFiles = false;
                IsReadyToSend = HasNeighbors;

                if (toNeighbor == null)
                {
                    var neighbors = _station.Neighbors;
                    foreach (var neighbor in neighbors)
                    {
                        RequestSendingFiles(fileModels, neighbor);
                    }
                }
                else
                {
                    RequestSendingFiles(fileModels, toNeighbor);
                }
            });
        }

        private IEnumerable<SendFileModel> ListFilesWillBeSent(string[] files, string baseDir = null)
        {
            var ret = new LinkedList<SendFileModel>();
            foreach (var file in files)
            {
                CurrentScannedFile = Path.GetFileName(file);

                if (!FileUtils.IsFile(file))
                {
                    var dir = file;
                    var allFiles = Directory.GetFileSystemEntries(dir, "*", SearchOption.AllDirectories);
                    var models = ListFilesWillBeSent(allFiles, dir);
                    foreach (var model in models)
                    {
                        ret.AddLast(model);
                    }
                }
                else
                {
                    ret.AddLast(new SendFileModel
                    {
                        File = file,
                        BaseDir = baseDir
                    });
                }
            }

            return ret;
        }

        private async void RequestSendingFiles(IEnumerable<SendFileModel> files, Station.Neighbor sendTo)
        {
            var slaver = Slaver.ConnectToMaster(sendTo.Address);
            var waitingOnPort = await slaver.RequestSendFilesAsync(files.Select(f => f.File));

            if (waitingOnPort > 0)
            {
                ThreadUtils.RunOnUi(() =>
                {
                    var transferWindow = new TransferWindow();
                    var sendingViewModel = new SendingViewModel(sendTo, waitingOnPort, _station.Name, files)
                    { Title = "Sender" };
                    transferWindow.DataContext = sendingViewModel;
                    transferWindow.Show();
                    sendingViewModel.Start();
                });
            }
        }
    }
}