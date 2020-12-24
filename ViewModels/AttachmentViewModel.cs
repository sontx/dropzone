using GalaSoft.MvvmLight;

namespace DropZone.ViewModels
{
    public class AttachmentViewModel : ViewModelBase
    {
        public string Name { get; set; }
        public bool IsFolder { get; set; }
        public string Path { get; set; }
        public string Size { get; set; }
    }
}