using DropZone.Properties;
using DropZone.Protocol;
using DropZone.Utils;
using DropZone.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DropZone.ViewModels
{
    public class NeighborMenuItemViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly Station.Neighbor _neighbor;

        public string Header => _neighbor.Name;

        public List<SubMenuItem> MenuItems { get; }

        public NeighborMenuItemViewModel(MainViewModel mainViewModel, Station.Neighbor neighbor)
        {
            _mainViewModel = mainViewModel;
            _neighbor = neighbor;

            MenuItems = new List<SubMenuItem> {new SubMenuItem
            {
                Header = "Send Files",
                Command = new RelayCommand(HandleSendFiles),
                Icon = ImageUtils.ImageSourceFromBitmap(Resources.send_file)
            }, new SubMenuItem
            {
                Header = "Chat",
                Command = new RelayCommand(HandleSendMessage),
                Icon = ImageUtils.ImageSourceFromBitmap(Resources.chat)
            }, new SubMenuItem
            {
                Header = "Terminal",
                Command = new RelayCommand(HandleTerminal),
                Icon = ImageUtils.ImageSourceFromBitmap(Resources.terminal)
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
            ChatWindowManager.Show(_neighbor, null, true);
        }

        private void HandleTerminal()
        {
            var terminalWindow = new TerminalWindow { Owner = Application.Current.MainWindow };
            var terminalViewModel = new TerminalViewModel(_neighbor);
            terminalWindow.DataContext = terminalViewModel;
            terminalWindow.Show();
        }

        public class SubMenuItem
        {
            public string Header { get; set; }
            public ICommand Command { get; set; }
            public ImageSource Icon { get; set; }
        }
    }
}