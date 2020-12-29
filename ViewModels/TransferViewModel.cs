using DropZone.Utils;
using GalaSoft.MvvmLight;
using System;
using System.Threading;
using System.Windows;

namespace DropZone.ViewModels
{
    public abstract class TransferViewModel : ViewModelBase
    {
        public event EventHandler Close;

        protected volatile bool Canceled;

        private string _title;
        private string _currentFileName;
        private int _percent;
        private string _status;
        private readonly Timer _timer;

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

        protected TransferViewModel()
        {
            _timer = new Timer(OnTimerCallback);
        }

        protected void StartTimer(int intervalMillis)
        {
            try
            {
                _timer.Change(0, intervalMillis);
            }
            catch
            {
                // ignored
            }
        }

        protected void StopTimer()
        {
            try
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            catch
            {
                // ignored
            }
        }

        protected virtual void OnTimerCallback(object state)
        {
        }

        public void Cancel()
        {
            lock (this)
            {
                if (Canceled)
                    return;
                Canceled = true;
            }

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
                _timer.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        protected abstract void OnCancel();

        protected abstract void OnCleanUp();

        protected void CloseUi()
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