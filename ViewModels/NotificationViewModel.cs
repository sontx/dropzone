using DropZone.Protocol;
using DropZone.Views;
using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Windows;

namespace DropZone.ViewModels
{
    public class NotificationViewModel : ViewModelBase
    {
        private readonly string _from;
        private readonly List<string> _receivedFiles;
        public string Message { get; set; }

        public string SaveLocation { get; set; }

        public NotificationViewModel()
        {
            Message = "Received 30 files from ABC";
            SaveLocation = "C:\\path\\to\\folder";
        }

        public NotificationViewModel(string from, List<string> receivedFiles, string saveDir)
        {
            _from = @from;
            _receivedFiles = receivedFiles;
            Message = $"Received {receivedFiles.Count} file(s) from {from}";
            SaveLocation = saveDir;
        }

        public async void OpenReport()
        {
            var reportViewModel = new ReportViewModel(_from, _receivedFiles);
            await reportViewModel.LoadAsync();
            var reportWindow = new ReportWindow { DataContext = reportViewModel, Owner = Application.Current.MainWindow };
            reportWindow.Show();
        }
    }
}