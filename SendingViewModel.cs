using DropZone.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DropZone
{
    public class SendingViewModel : TransferViewModel
    {
        private readonly string _host;
        private readonly int _port;
        private readonly List<SendFileModel> _sendingFiles;
        private readonly Thread _thread;
        private readonly Timer _timer;
        private FileSender _currentSender;

        public SendingViewModel(string host, int port, IEnumerable<SendFileModel> sendingFiles)
        {
            _host = host;
            _port = port;
            _sendingFiles = sendingFiles.ToList();
            _thread = new Thread(DoInBackground);
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
                ShowError($"Error while sending files{Environment.NewLine}Detail: {ex.Message}");
            }

            try
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            catch
            {
                // ignored
            }

            ThreadUtils.RunOnUiAndWait(() =>
            {
                Status = "Done!";
                Percent = 100;
            });

            Thread.Sleep(500);

            _currentSender = null;

            CloseWindow();
        }

        private void SendFiles()
        {
            for (var i = 0; i < _sendingFiles.Count; i++)
            {
                var sendingFile = _sendingFiles[i];
                using (var sender = new FileSender(_host, _port))
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

                    ThreadUtils.RunOnUiAndWait(() => { Title = $"Sending to {sender.RemoteIdentify}"; });

                    sender.Send(sendingFile.File, sendingFile.BaseDir);

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