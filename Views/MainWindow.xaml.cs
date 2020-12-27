using DropZone.ViewModels;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DropZone.Utils;
using Debugger = DropZone.Utils.Debugger;

namespace DropZone.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ThreadUtils.Initialize();
            DataContext = new MainViewModel();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (TransferWindow.InstanceCount > 0 && MessageBox.Show("Close this window will cancel all processes?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                TransferWindow._appExisting = true;
                Application.Current.Shutdown();
            }

            base.OnClosing(e);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            HandleFiles(files);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.Close();
                Application.Current.Shutdown();
            }

            base.OnClosed(e);
        }

        private void HandleFiles(string[] files)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.SendFiles(files);
            }
        }

        private void DropArea_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2)
            {
                return;
            }

            if (DataContext is MainViewModel vm && !vm.IsReadyToSend)
            {
                return;
            }

            var openFileDialog = new OpenFileDialog { Multiselect = true };
            if (openFileDialog.ShowDialog() == true)
            {
                HandleFiles(openFileDialog.FileNames);
            }
        }

        private void NeighborsLink_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.NeighborMenuItems.Count > 0)
            {
                if (vm.NeighborMenuItems.Count == 1)
                {
                    var items = vm.NeighborMenuItems[0].MenuItems;
                    var cm = new ContextMenu();
                    foreach (var item in items)
                    {
                        cm.Items.Add(new MenuItem
                        {
                            Header = item.Header,
                            Command = item.Command,
                            Icon = new Image { Source = item.Icon }
                        });
                    }
                    cm.PlacementTarget = sender as UIElement;
                    cm.IsOpen = true;
                }
                else
                {
                    var cm = FindResource("NeighborsMenu") as ContextMenu;
                    cm.PlacementTarget = sender as UIElement;
                    cm.DataContext = vm;
                    cm.IsOpen = true;
                }
            }
        }

        private void btnSettings_OnClick(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow {Owner = this};
            settingsWindow.ShowDialog();
            Debugger.IsEnabledLog = SettingsUtils.Get<AppSettings>().IsEnabledDebugger;
        }

        private void btnAbout_OnClick(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow{Owner = this};
            aboutWindow.ShowDialog();
        }
    }
}