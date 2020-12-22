using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace DropZone
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

        private void DropArea_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.IsInitializing)
            {
                return;
            }

            var openFileDialog = new OpenFileDialog { Multiselect = true };
            if (openFileDialog.ShowDialog() == true)
            {
                HandleFiles(openFileDialog.FileNames);
            }
        }
    }
}