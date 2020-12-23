using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DropZone.Protocol;

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

            try
            {
                ReceiveFiles();
            }
            catch (Exception ex)
            {
                if (!_canceled)
                    ShowError($"Error while receiving files{Environment.NewLine}Detail: {ex.Message}");
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
                });

                Thread.Sleep(300);
            }

            _currentReceiver = null;

            CloseWindow();
        }

        private void ReceiveFiles()
        {
            for (var i = 0; i < _receivingFiles.Count; i++)
            {
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

                    receiver.Receive();

                    if (receiver.ReceivedBytes < receiver.TotalBytes)
                        throw new Exception("Operation was aborted by sender");

                    ThreadUtils.RunOnUiAndWait(() => Percent = 100);
                }
            }
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