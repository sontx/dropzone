using DropZone.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DropZone
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
            _thread = new Thread(DoInBackground);
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
                ShowError($"Error while receiving files{Environment.NewLine}Detail: ${ex.Message}");
            }

            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            ThreadUtils.RunOnUiAndWait(() =>
            {
                Status = "Done!";
                Percent = 100;
            });

            Thread.Sleep(500);

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
                        Title = $"Receiving from {receiver.RemoteIdentify}";
                        Status = $"Receiving {receivingCount}/{_receivingFiles.Count}";
                        Percent = 0;
                    });

                    receiver.AcceptedHeader = () =>
                    {
                        ThreadUtils.RunOnUiAndWait(() =>
                        {
                            CurrentFileName = $"{receiver.FileName} ({FileUtils.BytesToString(receiver.TotalBytes)})";
                        });
                    };

                    receiver.Receive();

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
            _fileServer.Dispose();
        }
    }
}