using GalaSoft.MvvmLight;

namespace DropZone.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        public string Title { get; }

        public MainViewModel()
        {
            if (IsInDesignMode)
                return;

            InitializeTerminal();
            InitializeStation();
            InitializeSendFiles();
            InitializeChat();

            Title = $"Drop Zone [{_station.Name}]";
        }

        public void Close()
        {
            CloseStation();
            CloseTerminal();
            CloseSendFiles();
            CloseChat();
        }
    }
}