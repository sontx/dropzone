using DropZone.Protocol;
using DropZone.Utils;
using DropZone.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DropZone.ViewModels
{
    internal class ReceivingViewModel : TransferViewModel
    {
        private readonly SingleFileServer _fileServer;
        private readonly List<string> _receivingFiles;
        private readonly Thread _thread;
        private readonly Timer _timer;
        private FileReceiver _currentReceiver;

        public ReceivingViewModel(SingleFileServer fileServer, IEnumerable<string> receivingFiles)
        {
            _fileServer = fileServer;
            _receivingFiles = receivingFiles.ToList();
            _thread = new Thread(DoInBackground) { IsBackground = true };
            _timer = new Timer(UpdateProgress);
        }

        private void DoInBackground()
        {
            _timer.Change(0, 1000);
            List<string> savedFiles = null;

            try
            {
                Debugger.Log($"Start receiving {_receivingFiles.Count} file(s)");
                savedFiles = ReceiveFiles();
            }
            catch (Exception ex)
            {
                if (!_canceled)
                {
                    Debugger.Log($"Receiving was aborted: {ex.Message}");
                    ShowError($"Error while receiving files{Environment.NewLine}Detail: {ex.Message}");
                }
                else
                {
                    Debugger.Log($"Error while receiving file(s): {ex.Message}");
                }
            }

            try
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            catch
            {
                // ignored
            }

            if (!_canceled)
            {
                ThreadUtils.RunOnUiAndWait(() =>
                {
                    Status = "Done!";
                    Percent = 100;

                    if (savedFiles != null)
                        OnReceivedSuccessfully(savedFiles);
                });

                Thread.Sleep(300);
            }

            _currentReceiver = null;

            CloseWindow();
        }

        private List<string> ReceiveFiles()
        {
            var ret = new List<string>(_receivingFiles.Count);

            for (var i = 0; i < _receivingFiles.Count; i++)
            {
                lock (this)
                {
                    if (_canceled)
                        break;
                }

                using (var receiver = _fileServer.AcceptReceiver())
                {
                    _currentReceiver = receiver;

                    var receivingCount = i + 1;
                    ThreadUtils.RunOnUiAndWait(() =>
                    {
                        Title = "Receiver";
                        Status = $"Receiving {receivingCount}/{_receivingFiles.Count}";
                        Percent = 0;
                    });

                    receiver.AcceptedHeader = () =>
                    {
                        ThreadUtils.RunOnUiAndWait(() =>
                        {
                            Title = $"Receiving from {receiver.From} [{receiver.RemoteIdentify}]";
                            CurrentFileName = $"{receiver.FileName} ({FileUtils.BytesToString(receiver.TotalBytes)})";
                        });
                    };

                    Debugger.Log($"Receiving {receiver.FileName} from {receiver.RemoteIdentify}");
                    receiver.Receive();
                    Debugger.Log($"Sent {receiver.FileName}: " + (receiver.ReceivedBytes < receiver.TotalBytes ? "FAIL" : "OK"));

                    if (receiver.ReceivedBytes < receiver.TotalBytes)
                        throw new Exception("Operation was aborted by sender");

                    ThreadUtils.RunOnUiAndWait(() => Percent = 100);

                    ret.Add(receiver.SavedPath);
                }
            }

            return ret;
        }

        private void UpdateProgress(object state)
        {
            ThreadUtils.RunOnUi(() =>
            {
                var receiver = _currentReceiver;
                if (receiver != null && receiver.TotalBytes > 0)
                {
                    Percent = (int)((double)receiver.ReceivedBytes / (double)receiver.TotalBytes * 100D);
                }
            });
        }

        private void OnReceivedSuccessfully(List<string> savedFiles)
        {
            Debugger.Log($"Sent {_receivingFiles.Count} file(s) successfully");

            var settings = SettingsUtils.Get<AppSettings>();
            var currentReceiver = _currentReceiver;

            if (!settings.IsShowNotification || currentReceiver == null)
                return;

            var notificationViewModel = new NotificationViewModel(currentReceiver.From, savedFiles, currentReceiver.SaveDir);
            var notificationWindow = new NotificationWindow { DataContext = notificationViewModel };
            notificationWindow.Show();
        }

        protected override void OnStart()
        {
            _thread.Start();
        }

        protected override void OnCancel()
        {
            OnCleanUp();
        }

        protected override void OnCleanUp()
        {
            _timer.Dispose();
            var currentReceiver = _currentReceiver;
            currentReceiver?.Dispose();
            _fileServer.Dispose();
        }
    }
}