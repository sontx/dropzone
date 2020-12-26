using GalaSoft.MvvmLight;

namespace DropZone.ViewModels
{
    public class ReportFileViewModel : ViewModelBase
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
    }
}