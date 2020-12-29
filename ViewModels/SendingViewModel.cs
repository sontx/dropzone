using DropZone.Models;
using DropZone.Protocol;
using DropZone.Protocol.File;
using DropZone.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DropZone.ViewModels
{
    public class SendingViewModel : TransferViewModel
    {
        private readonly Station.Neighbor _neighbor;
        private readonly List<SendFileModel> _sendingFiles;
        private readonly ThreadWrapper _thread;
        private FileSender _currentSender;

        public SendingViewModel()
        {
        }

        public SendingViewModel(Station.Neighbor neighbor, IEnumerable<SendFileModel> sendingFiles)
        {
            _neighbor = neighbor;
            _sendingFiles = sendingFiles.ToList();
            _thread = new ThreadWrapper
            {
                DoWork = SendFiles,
                OnError = OnErrorWhileSendingFiles,
                OnExit = OnFinishSendingFiles
            };
        }

        private void OnErrorWhileSendingFiles(Exception ex)
        {
            lock (this)
            {
                if (!Canceled)
                {
                    Debugger.Log($"Sending was aborted: {ex.Message}");
                    ShowError(ex is IOException
                        ? "Operation was aborted by receiver"
                        : $"Error while sending files{Environment.NewLine}Detail: {ex.Message}");
                }
                else
                {
                    Debugger.Log($"Error while sending file(s): {ex.Message}");
                }
            }
        }

        private void OnFinishSendingFiles()
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
                    });

                    Thread.Sleep(300);
                }
            }

            _currentSender = null;

            CloseUi();
        }

        private void SendFiles()
        {
            Debugger.Log($"Start sending {_sendingFiles.Count} file(s) to ${_neighbor}");

            using (var factory = new SendingSessionFactory(_neighbor.Address, _sendingFiles.Select(file => file.File)))
            {
                var session = factory.CreateSessionAsync().Result;

                for (var i = 0; i < _sendingFiles.Count; i++)
                {
                    lock (this)
                    {
                        if (Canceled)
                            return;
                    }

                    var sendingFile = _sendingFiles[i];

                    using (var sender = session.CreateSender())
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

                        ThreadUtils.RunOnUiAndWait(() => { Title = $"Sending to {_neighbor.Name} [{sender.RemoteAddress}]"; });

                        Debugger.Log($"Sending {sendingFile.File} to {sender.RemoteAddress}");
                        sender.Send(sendingFile.File, sendingFile.BaseDir);
                        Debugger.Log($"Sent {sendingFile.File}: " + (sender.SentBytes < sender.TotalBytes ? "FAIL" : "OK"));

                        if (sender.SentBytes < sender.TotalBytes)
                            throw new Exception("Operation was aborted by receiver");

                        ThreadUtils.RunOnUiAndWait(() => Percent = 100);
                    }
                }
            }

            Debugger.Log($"Sent {_sendingFiles.Count} file(s) successfully");
        }

        protected override void OnTimerCallback(object state)
        {
            base.OnTimerCallback(state);
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
        }

        protected override void OnStart()
        {
            _thread.Start();
            StartTimer(1000);
        }
    }
}