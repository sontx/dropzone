using GalaSoft.MvvmLight;
using System;
using System.Windows;

namespace DropZone.Views
{
    public abstract class WindowBase : Window
    {
        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is ViewModelBase vm)
            {
                vm.Cleanup();
            }

            base.OnClosed(e);
        }
    }
}