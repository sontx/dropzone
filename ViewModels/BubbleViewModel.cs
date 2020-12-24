using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace DropZone.ViewModels
{
    public class BubbleViewModel : ViewModelBase
    {
        public string Text { get; set; }

        public bool IsLeft { get; set; }
    }
}
