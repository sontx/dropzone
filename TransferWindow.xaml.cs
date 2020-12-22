using System;
using System.ComponentModel;
using System.Windows;

namespace DropZone
{
    /// <summary>
    /// Interaction logic for SendingWindow.xaml
    /// </summary>
    public partial class TransferWindow : Window
    {
        private static int _instanceCount;

        public static bool _appExisting;

        private bool _windowExisting;

        public static int InstanceCount => _instanceCount;

        public TransferWindow()
        {
            InitializeComponent();
            _instanceCount++;
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
            _windowExisting = true;
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_appExisting && !_windowExisting)
            {
                if (MessageBox.Show("Cancel current process?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question) ==
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

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is TransferViewModel vm)
            {
                vm.CleanUp();
            }
            _instanceCount--;
            base.OnClosed(e);
        }

        private void btnClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}