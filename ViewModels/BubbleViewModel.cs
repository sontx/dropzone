using System.Collections.Generic;
using GalaSoft.MvvmLight;

namespace DropZone.ViewModels
{
    public class BubbleViewModel : ViewModelBase
    {
        public string Text { get; set; }

        public bool IsLeft { get; set; }

        public IEnumerable<AttachmentViewModel> Attachments { get; set; }
    }
}