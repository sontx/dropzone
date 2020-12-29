using DropZone.Protocol;
using DropZone.Protocol.File;
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
        private readonly ReceivingSessionHandler.Session _session;
        private readonly Station _station;
        private readonly ThreadWrapper _thread;
        private List<string> _receivedFiles;
        private FileReceiver _currentReceiver;

        public ReceivingViewModel(ReceivingSessionHandler.Session session, Station station)
        {
            _session = session;
            _station = station;
            _thread = new ThreadWrapper
            {
                DoWork = ReceiveFiles,
                OnError = OnErrorWhileReceivingFiles,
                OnExit = OnFinishReceivingFiles
            };
        }

        private void OnErrorWhileReceivingFiles(Exception ex)
        {
            lock (this)
            {
                if (!Canceled)
                {
                    Debugger.Log($"Receiving was aborted: {ex.Message}");
                    ShowError($"Error while receiving files{Environment.NewLine}Detail: {ex.Message}");
                }
                else
                {
                    Debugger.Log($"Error while receiving file(s): {ex.Message}");
                }
            }
        }

        private void OnFinishReceivingFiles()
        {
            StopTimer();

            lock (this)
            {
                if (!Canceled)
                {
                    ThreadUtils.RunOnUiAndWait(() =>
                    {
                        Status = "Done!";
                        Percent = 100;

                        if (_receivedFiles != null && _receivedFiles.Count == _session.WillReceiveFiles.Count)
                            OnReceivedSuccessfully(_receivedFiles);
                    });

                    Thread.Sleep(300);
                }
            }

            _currentReceiver = null;

            CloseUi();
        }

        private void ReceiveFiles()
        {
            var receivingFiles = _session.WillReceiveFiles;
            _receivedFiles = new List<string>(receivingFiles.Count);

            for (var i = 0; i < receivingFiles.Count; i++)
            {
                lock (this)
                {
                    if (Canceled)
                        break;
                }

                using (var receiver = _session.AcceptReceiverAsync().Result)
                {
                    _currentReceiver = receiver;

                    var receivingCount = i + 1;
                    ThreadUtils.RunOnUiAndWait(() =>
                    {
                        Title = "Receiver";
                        Status = $"Receiving {receivingCount}/{receivingFiles.Count}";
                        Percent = 0;
                    });

                    receiver.AcceptedHeader = () =>
                    {
                        ThreadUtils.RunOnUiAndWait(() =>
                        {
                            var neighbor = _station.GetNeighbor(receiver.RemoteAddress);
                            Title = $"Receiving from {neighbor}";
                            CurrentFileName = $"{receiver.FileName} ({FileUtils.BytesToString(receiver.TotalBytes)})";
                        });
                    };

                    Debugger.Log($"Receiving {receiver.FileName} from {receiver.RemoteAddress}");
                    receiver.Receive();
                    Debugger.Log($"Sent {receiver.FileName}: " + (receiver.ReceivedBytes < receiver.TotalBytes ? "FAIL" : "OK"));

                    if (receiver.ReceivedBytes < receiver.TotalBytes)
                        throw new Exception("Operation was aborted by sender");

                    ThreadUtils.RunOnUiAndWait(() => Percent = 100);

                    _receivedFiles.Add(receiver.SavedPath);
                }
            }
        }

        protected override void OnTimerCallback(object state)
        {
            base.OnTimerCallback(state);
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
            var receivingFiles = _session.WillReceiveFiles;
            Debugger.Log($"Sent {receivingFiles.Count} file(s) successfully");

            var settings = SettingsUtils.Get<AppSettings>();
            var currentReceiver = _currentReceiver;

            if (!settings.IsShowNotification || currentReceiver == null)
                return;

            var neighbor = _station.GetNeighbor(currentReceiver.RemoteAddress);

            var notificationViewModel = new NotificationViewModel(neighbor.ToString(), savedFiles, currentReceiver.SaveDir);
            var notificationWindow = new NotificationWindow { DataContext = notificationViewModel };
            notificationWindow.Show();
        }

        protected override void OnStart()
        {
            _thread.Start();
            StartTimer(1000);
        }

        protected override void OnCancel()
        {
            OnCleanUp();
        }

        protected override void OnCleanUp()
        {
            var currentReceiver = _currentReceiver;
            currentReceiver?.Dispose();
            _session.Dispose();
        }
    }
}