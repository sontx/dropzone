using DropZone.Protocol;
using DropZone.Utils;
using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DropZone.ViewModels
{
    public class ReportViewModel : ViewModelBase
    {
        private readonly List<string> _receivedFiles;
        public string Title { get; set; }

        public List<ReportFileViewModel> Files { get; private set; }

        public ReportViewModel()
        {
            Files = new List<ReportFileViewModel>(new[]
            {
                new ReportFileViewModel {Path = "path/to/file1", Name = "file1", Size = "100KB"},
                new ReportFileViewModel {Path = "path/to/file2", Name = "file2", Size = "200KB"}
            });
        }

        public ReportViewModel(string from, List<string> receivedFiles)
        {
            _receivedFiles = receivedFiles;
            Title = $"Received from {from}";
        }

        public Task LoadAsync()
        {
            return Task.Run(() =>
            {
                Files = new List<ReportFileViewModel>(_receivedFiles.Count);
                foreach (var file in _receivedFiles)
                {
                    var size = FileUtils.GetFriendlyFileSize(file);
                    Files.Add(new ReportFileViewModel
                    {
                        Name = Path.GetFileName(file),
                        Size = size,
                        Path = file
                    });
                }
            });
        }
    }
}