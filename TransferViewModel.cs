using GalaSoft.MvvmLight;
using System;
using System.Windows;

namespace DropZone
{
    public abstract class TransferViewModel : ViewModelBase
    {
        public event EventHandler Close;

        private string _title;
        private string _currentFileName;
        private int _percent;
        private string _status;

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        public string CurrentFileName
        {
            get => _currentFileName;
            set => Set(ref _currentFileName, value);
        }

        public int Percent
        {
            get => _percent;
            set => Set(ref _percent, value);
        }

        public string Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        public void Cancel()
        {
            try
            {
                OnCancel();
            }
            catch
            {
                // ignored
            }
        }

        public void CleanUp()
        {
            try
            {
                OnCleanUp();
            }
            catch
            {
                // ignored
            }
        }

        protected abstract void OnCancel();

        protected abstract void OnCleanUp();

        protected void CloseWindow()
        {
            ThreadUtils.RunOnUiAndWait(() =>
            {
                Close?.Invoke(this, EventArgs.Empty);
            });
        }

        public void Start()
        {
            OnStart();
        }

        protected void ShowError(string msg)
        {
            ThreadUtils.RunOnUiAndWait(() =>
            {
                MessageBox.Show(msg, Title, MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
            });
        }

        protected abstract void OnStart();
    }
}