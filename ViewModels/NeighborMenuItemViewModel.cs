using DropZone.Protocol;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows.Input;

namespace DropZone.ViewModels
{
    public class NeighborMenuItemViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly Station.Neighbor _neighbor;

        public string Header => $"{_neighbor.Name} [{_neighbor.Address}]";

        public List<SubMenuItem> MenuItems { get; }

        public NeighborMenuItemViewModel(MainViewModel mainViewModel, Station.Neighbor neighbor)
        {
            _mainViewModel = mainViewModel;
            _neighbor = neighbor;

            MenuItems = new List<SubMenuItem> {new SubMenuItem
            {
                Header = "Send Files",
                Command = new RelayCommand(HandleSendFiles)
            }, new SubMenuItem
            {
                Header = "Chat",
                Command = new RelayCommand(HandleSendMessage)
            }};
        }

        private void HandleSendFiles()
        {
            var openFileDialog = new OpenFileDialog { Multiselect = true };
            if (openFileDialog.ShowDialog() == true)
            {
                _mainViewModel.SendFiles(openFileDialog.FileNames, _neighbor);
            }
        }

        private void HandleSendMessage()
        {
            ChatWindowManager.Show(_neighbor, true);
        }

        public class SubMenuItem
        {
            public string Header { get; set; }
            public ICommand Command { get; set; }
        }
    }
}