using DropZone.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;

namespace DropZone.Views
{
    /// <summary>
    /// Interaction logic for SendingWindow.xaml
    /// </summary>
    public partial class TransferWindow : WindowBase
    {
        private volatile CloseReason _closeReason = CloseReason.UserClose;

        public TransferWindow()
        {
            InitializeComponent();
            DataContextChanged += TransferWindow_DataContextChanged;
        }

        private void TransferWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is TransferViewModel vm)
            {
                vm.Close += Vm_Close;
            }
        }

        private void Vm_Close(object sender, EventArgs e)
        {
            _closeReason = CloseReason.RequestClose;
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (Application.Current.MainWindow == null)
                _closeReason = CloseReason.AppClose;

            if (_closeReason == CloseReason.UserClose)
            {
                if (MessageBox.Show("Cancel current operation?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                    MessageBoxResult.No)
                {
                    e.Cancel = true;
                    base.OnClosing(e);
                    return;
                }
            }

            if (DataContext is TransferViewModel vm)
            {
                vm.Cancel();
            }

            base.OnClosing(e);
        }

        private void btnClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private enum CloseReason
        {
            RequestClose,
            UserClose,
            AppClose
        }
    }
}