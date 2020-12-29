using DropZone.Protocol;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Concurrent;
using System.Threading;
using DropZone.Protocol.Terminal;

namespace DropZone.ViewModels
{
    internal class TerminalViewModel : ViewModelBase
    {
        private readonly RemoteTerminal _commandCaller;
        private readonly BlockingCollection<string> _inputCommands = new BlockingCollection<string>();
        private readonly Thread _readCommandThread;
        private bool _init;
        private bool _hasOutput;

        public Action<string> ReceivedOutput { get; set; }
        public Action<string> ReceivedError { get; set; }

        public string Title { get; set; }

        public bool HasOutput
        {
            get => _hasOutput;
            set => Set(ref _hasOutput, value);
        }

        public TerminalViewModel()
        {
            Title = "Terminal [ABC]";
        }

        public TerminalViewModel(Station.Neighbor neighbor)
        {
            _commandCaller = new RemoteTerminal(neighbor.Address)
            {
                ReceivedOutput = HandleReceivedOutput,
                ReceivedError = HandleReceivedError
            };
            Title = $"Terminal {neighbor}";
            _readCommandThread = new Thread(ReadCommand) { IsBackground = true };
        }

        private void HandleReceivedError(string msg)
        {
            ReceivedError?.Invoke(msg);
        }

        private void HandleReceivedOutput(string msg)
        {
            ReceivedOutput?.Invoke(msg);
        }

        private void ReadCommand()
        {
            try
            {
                while (true)
                {
                    var command = _inputCommands.Take();
                    _commandCaller.SendInput(command);
                }
            }
            catch
            {
                // ignored
            }
        }

        public string SendCommand(string cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd))
                return string.Empty;

            cmd = cmd.Trim();

            if (!_init)
            {
                _init = true;
                Initialize(cmd);
            }
            else
            {
                _inputCommands.Add(cmd);
            }

            return cmd;
        }

        private async void Initialize(string cmd)
        {
            if (await _commandCaller.CallAsync(cmd))
                _readCommandThread.Start();
        }

        public override void Cleanup()
        {
            base.Cleanup();
            _inputCommands.Dispose();
            _commandCaller.Dispose();
        }
    }
}