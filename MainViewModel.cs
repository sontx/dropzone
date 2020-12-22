using DropZone.Properties;
using DropZone.Protocol;
using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DropZone
{
    public class MainViewModel : ViewModelBase
    {
        private readonly Master _master;
        private readonly NeighborHelper _neighborHelper;
        private bool _isInitializing;
        private string _currentScannedFile;

        public bool IsInitializing
        {
            get => _isInitializing;
            set => Set(ref _isInitializing, value);
        }

        public string CurrentScannedFile
        {
            get => _currentScannedFile;
            set => Set(ref _currentScannedFile, value);
        }

        public MainViewModel()
        {
            if (!IsInDesignMode)
            {
                _neighborHelper = new NeighborHelper();
                _master = new Master(Constants.MASTER_PORT)
                {
                    ResolverHandler = HandleResolver
                };
                _master.Start();
            }
        }

        private async void HandleResolver(Resolver resolver)
        {
            try
            {
                var receivingFiles = await resolver.WaitForFilesAsync();
                var fileSever = new SingleFileServer(Settings.Default.SaveDir);
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
            finally
            {
                resolver.Dispose();
            }
        }

        public async void SendFiles(string[] files)
        {
            if (files == null || files.Length == 0)
                return;

            CurrentScannedFile = string.Empty;
            IsInitializing = true;

            await _neighborHelper.RefreshAsync();
            var neighbors = _neighborHelper.Neighbors;

            await Task.Run(() =>
            {
                var fileModels = GetSendingModels(files);

                IsInitializing = false;

                foreach (var neighbor in neighbors)
                {
                    RequestSendingFiles(fileModels, neighbor);
                }
            });
        }

        private IEnumerable<SendFileModel> GetSendingModels(string[] files, string baseDir = null)
        {
            var ret = new LinkedList<SendFileModel>();
            foreach (var file in files)
            {
                ThreadUtils.RunOnUiAndWait(() => CurrentScannedFile = file);

                if (File.GetAttributes(file).HasFlag(FileAttributes.Directory))
                {
                    var dir = file;
                    var allFiles = Directory.GetFileSystemEntries(dir, "*", SearchOption.AllDirectories);
                    var models = GetSendingModels(allFiles, dir);
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

        private async void RequestSendingFiles(IEnumerable<SendFileModel> files, string requestTo)
        {
            var slaver = new Slaver(requestTo, Constants.MASTER_PORT);
            var waitingOnPort = await slaver.RequestSendFilesAsync(files.Select(f => f.File));

            if (waitingOnPort > 0)
            {
                ThreadUtils.RunOnUi(() =>
                {
                    var transferWindow = new TransferWindow();
                    var sendingViewModel = new SendingViewModel(requestTo, waitingOnPort, files){Title = "Sender"};
                    transferWindow.DataContext = sendingViewModel;
                    transferWindow.Show();
                    sendingViewModel.Start();
                });
            }
        }

        public void Close()
        {
            _master.Stop();
        }
    }
}