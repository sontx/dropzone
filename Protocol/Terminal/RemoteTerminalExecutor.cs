using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DropZone.Protocol.Terminal
{
    public class RemoteTerminalExecutor : ReceivedThreadTcpClient
    {
        public event EventHandler Closed;

        private readonly string _command;
        private readonly Process _process;

        public RemoteTerminalExecutor(TcpClient client, string command)
            : base(client)
        {
            _command = command;
            _process = new Process();
        }

        protected override bool OnReadNextIncomingMessage()
        {
            var line = ReadLine();
            if (line == null)
                return false;

            _process.StandardInput.WriteLine(line);
            _process.StandardInput.Flush();

            return true;
        }

        protected override void OnErrorWhileWaitingIncomingMessage(Exception ex)
        {
            base.OnErrorWhileWaitingIncomingMessage(ex);

            if (ex is InvalidOperationException)
            {
                SendError(ex.Message);
            }

            Closed?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnExitIncomingMessageLoop()
        {
            base.OnExitIncomingMessageLoop();

            try
            {
                _process.StandardInput.Close();
                _process.CloseMainWindow();
                _process.Close();
                _process?.Kill();
            }
            catch
            {
                // ignored
            }
        }

        public override void Start()
        {
            var startInfo = new ProcessStartInfo("cmd", $"/K {_command}")
            {
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            try
            {
                _process.OutputDataReceived += _process_OutputDataReceived;
                _process.ErrorDataReceived += _process_ErrorDataReceived;
                _process.StartInfo = startInfo;
                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                SendError(ex.Message);
                return;
            }

            base.Start();
        }

        private async void _process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            await SendBackToCaller($"{Constants.RemoteCommandOutputPrefix}:{e.Data}");
        }

        private void _process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            SendError(e.Data);
        }

        private async void SendError(string err)
        {
            await SendBackToCaller($"{Constants.RemoteCommandErrorPrefix}:{err}");
        }

        private Task SendBackToCaller(string msg)
        {
            return Task.Run(() =>
            {
                lock (this)
                {
                    try
                    {
                        SendLine(msg);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            });
        }

        public override void Stop()
        {
            _process.OutputDataReceived -= _process_OutputDataReceived;
            _process.ErrorDataReceived -= _process_ErrorDataReceived;

            base.Stop();
        }
    }
}