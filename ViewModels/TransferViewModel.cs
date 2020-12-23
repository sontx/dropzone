using System;
using System.Windows;
using GalaSoft.MvvmLight;

namespace DropZone.ViewModels
{
    public abstract class TransferViewModel : ViewModelBase
    {
        public event EventHandler Close;

        private string _title;
        private string _currentFileName;
        private int _percent;
        private string _status;
        protected bool _canceled;

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
            if (_canceled)
                return;

            _canceled = true;

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
                MessageBox.Show(msg, Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            });
        }

        protected abstract void OnStart();
    }
}