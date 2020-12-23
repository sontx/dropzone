using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DropZone.Models;
using DropZone.Protocol;

namespace DropZone.ViewModels
{
    public class SendingViewModel : TransferViewModel
    {
        private readonly Station.Neighbor _neighbor;
        private readonly int _port;
        private readonly string _name;
        private readonly List<SendFileModel> _sendingFiles;
        private readonly Thread _thread;
        private readonly Timer _timer;
        private FileSender _currentSender;

        public SendingViewModel(Station.Neighbor neighbor, int port, string name, IEnumerable<SendFileModel> sendingFiles)
        {
            _neighbor = neighbor;
            _port = port;
            _name = name;
            _sendingFiles = sendingFiles.ToList();
            _thread = new Thread(DoInBackground) { IsBackground = true };
            _timer = new Timer(UpdateProgress);
        }

        private void DoInBackground()
        {
            _timer.Change(0, 1000);

            try
            {
                SendFiles();
            }
            catch (Exception ex)
            {
                if (!_canceled)
                {
                    ShowError(ex is IOException
                        ? "Operation was aborted by receiver"
                        : $"Error while sending files{Environment.NewLine}Detail: {ex.Message}");
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
                });

                Thread.Sleep(300);
            }

            _currentSender = null;

            CloseWindow();
        }

        private void SendFiles()
        {
            for (var i = 0; i < _sendingFiles.Count; i++)
            {
                var sendingFile = _sendingFiles[i];
                using (var sender = new FileSender(_neighbor.Address, _port, _name))
                {
                    _currentSender = sender;

                    var sendingCount = i + 1;
                    ThreadUtils.RunOnUiAndWait(() =>
                    {
                        Status = $"Sending {sendingCount}/{_sendingFiles.Count}";
                        var fileInfo = new FileInfo(sendingFile.File);
                        CurrentFileName = $"{fileInfo.Name} ({FileUtils.BytesToString(fileInfo.Length)})";
                        Percent = 0;
                    });

                    sender.Connect();

                    ThreadUtils.RunOnUiAndWait(() => { Title = $"Sending to {_neighbor.Name} [{sender.RemoteIdentify}]"; });

                    sender.Send(sendingFile.File, sendingFile.BaseDir);

                    if (sender.SentBytes < sender.TotalBytes)
                        throw new Exception("Operation was aborted by receiver");

                    ThreadUtils.RunOnUiAndWait(() => Percent = 100);
                }
            }
        }

        private void UpdateProgress(object state)
        {
            ThreadUtils.RunOnUi(() =>
            {
                var sender = _currentSender;
                if (sender != null && sender.TotalBytes > 0)
                {
                    Percent = (int)((double)sender.SentBytes / (double)sender.TotalBytes * 100D);
                }
            });
        }

        protected override void OnCancel()
        {
            OnCleanUp();
        }

        protected override void OnCleanUp()
        {
            var currentSender = _currentSender;
            currentSender?.Dispose();
            _timer.Dispose();
        }

        protected override void OnStart()
        {
            _thread.Start();
        }
    }
}