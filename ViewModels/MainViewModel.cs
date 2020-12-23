using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DropZone.Models;
using DropZone.Properties;
using DropZone.Protocol;
using DropZone.Views;
using GalaSoft.MvvmLight;

namespace DropZone.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly Master _master;
        private readonly Station _station;
        private bool _isInitializing;
        private string _currentScannedFile;
        private string _neighborsSummary;
        private bool _hasNeighbors;

        public string Title { get; }

        public string NeighborsSummary
        {
            get => _neighborsSummary;
            private set => Set(ref _neighborsSummary, value);
        }

        public bool HasNeighbors
        {
            get => _hasNeighbors;
            private set => Set(ref _hasNeighbors, value);
        }

        public ObservableCollection<NeighborMenuItemViewModel> NeighborMenuItems { get; } = new ObservableCollection<NeighborMenuItemViewModel>();

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
                _station = new Station(RandomNames.GetRandomName());
                _master = new Master(Constants.MASTER_PORT)
                {
                    ResolverHandler = HandleResolver
                };
                _master.Start();
                _station.Start();

                Title = $"Drop Zone [{_station.Name}]";

                UpdateNeighborsSummary();
                _station.PropertyChanged += _station_PropertyChanged;
            }
        }

        private void _station_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Station.Neighbors))
            {
                UpdateNeighborsSummary();
            }
        }

        private void UpdateNeighborsSummary()
        {
            ThreadUtils.RunOnUi(() =>
            {
                var neighbors = _station.Neighbors;
                NeighborsSummary = neighbors.Count > 0 ? $"Online neighbors ({neighbors.Count})" : string.Empty;
                HasNeighbors = neighbors.Count > 0;

                NeighborMenuItems.Clear();
                foreach (var neighbor in neighbors)
                {
                    NeighborMenuItems.Add(new NeighborMenuItemViewModel(this, neighbor));
                }
            });
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

        public async void SendFiles(string[] files, Station.Neighbor toNeighbor = null)
        {
            if (files == null || files.Length == 0)
                return;

            CurrentScannedFile = string.Empty;
            IsInitializing = true;

            await Task.Run(() =>
            {
                var fileModels = GetSendingModels(files);

                IsInitializing = false;

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

        private IEnumerable<SendFileModel> GetSendingModels(string[] files, string baseDir = null)
        {
            var ret = new LinkedList<SendFileModel>();
            foreach (var file in files)
            {
                ThreadUtils.RunOnUiAndWait(() =>
                {
                    var name = Path.GetFileName(file);
                    var dir = Path.GetDirectoryName(file);
                    var ts = dir;
                    var max = 16;
                    var shortenDir = ts.Length > max
                        ? ts.Substring(0, max) + "..." + ts.Substring((ts.Length - max), max)
                        : ts;
                    CurrentScannedFile = Path.Combine(shortenDir, name);
                });

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

        private async void RequestSendingFiles(IEnumerable<SendFileModel> files, Station.Neighbor sendTo)
        {
            var slaver = new Slaver(sendTo.Address, Constants.MASTER_PORT);
            var waitingOnPort = await slaver.RequestSendFilesAsync(files.Select(f => f.File));

            if (waitingOnPort > 0)
            {
                ThreadUtils.RunOnUi(() =>
                {
                    var transferWindow = new TransferWindow();
                    var sendingViewModel = new SendingViewModel(sendTo, waitingOnPort, _station.Name, files) { Title = "Sender" };
                    transferWindow.DataContext = sendingViewModel;
                    transferWindow.Show();
                    sendingViewModel.Start();
                });
            }
        }

        public void Close()
        {
            _master.Stop();
            _station.Stop();
        }
    }
}