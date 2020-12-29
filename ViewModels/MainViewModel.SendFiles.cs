using DropZone.Models;
using DropZone.Protocol;
using DropZone.Protocol.File;
using DropZone.Utils;
using DropZone.Views;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DropZone.ViewModels
{
    public partial class MainViewModel
    {
        private FileServer _fileServer;
        private bool _isReadyToSend;
        private string _currentScannedFile;
        private bool _isListingSendingFiles;
        private int _processingTaskCount;

        public int ProcessingTaskCount => _processingTaskCount;

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

        private void InitializeSendFiles()
        {
            _fileServer = new FileServer
            {
                OnSessionHandler = HandleReceivingSessionHandler
            };
            _fileServer.Start();
        }

        private void CloseSendFiles()
        {
            _fileServer.Stop();
        }

        private async void HandleReceivingSessionHandler(ReceivingSessionHandler sessionHandler)
        {
            var session = await sessionHandler.AcceptSessionAsync();
            ThreadUtils.RunOnUiAndWait(() =>
            {
                var settings = SettingsUtils.Get<AppSettings>();
                session.SaveDir = settings.ReceivedFilesDir;
                var transferWindow = new TransferWindow();
                var receivingViewModel = new ReceivingViewModel(session, _station) { Title = "Receiver" };

                transferWindow.DataContext = receivingViewModel;
                transferWindow.Closed += TransferWindow_Closed;
                transferWindow.Show();

                Interlocked.Increment(ref _processingTaskCount);
                receivingViewModel.Start();

            });
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

        private void RequestSendingFiles(IEnumerable<SendFileModel> files, Station.Neighbor sendTo)
        {
            ThreadUtils.RunOnUi(() =>
            {
                var transferWindow = new TransferWindow();
                var sendingViewModel = new SendingViewModel(sendTo, files)
                {
                    Title = "Sender"
                };

                transferWindow.Closed += TransferWindow_Closed;
                transferWindow.DataContext = sendingViewModel;
                transferWindow.Show();

                Interlocked.Increment(ref _processingTaskCount);
                sendingViewModel.Start();
            });
        }

        private void TransferWindow_Closed(object sender, System.EventArgs e)
        {
            Interlocked.Decrement(ref _processingTaskCount);
        }
    }
}