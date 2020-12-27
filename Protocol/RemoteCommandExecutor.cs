using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DropZone.Protocol
{
    public class RemoteCommandExecutor
    {
        public event EventHandler Closed;

        private readonly TcpClient _client;
        private readonly string _command;
        private readonly Thread _inputThread;
        private readonly Process _process;
        private readonly StreamWriter _writer;
        private readonly StreamReader _reader;

        public RemoteCommandExecutor(TcpClient client, string command)
        {
            _client = client;
            _command = command;
            _inputThread = new Thread(WaitForRemoteInput) { IsBackground = true };
            _process = new Process();
            _writer = new StreamWriter(_client.GetStream()) { AutoFlush = true };
            _reader = new StreamReader(_client.GetStream());
        }

        private void WaitForRemoteInput()
        {
            try
            {
                while (true)
                {
                    var line = _reader.ReadLine();
                    if (line == null)
                        break;
                    _process.StandardInput.WriteLine(line);
                    _process.StandardInput.Flush();
                }
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException)
                {
                    SendError(ex.Message);
                }

                Closed?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
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
        }

        public void Start()
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

            _inputThread.Start();
        }

        private async void _process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            await SendBackToCaller($"{Constants.REMOTE_COMMAND_OUTPUT_PREFIX}:{e.Data}");
        }

        private void _process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            SendError(e.Data);
        }

        private async void SendError(string err)
        {
            await SendBackToCaller($"{Constants.REMOTE_COMMAND_ERROR_PREFIX}:{err}");
        }

        private Task SendBackToCaller(string msg)
        {
            return Task.Run(() =>
            {
                lock (this)
                {
                    try
                    {
                        _writer?.WriteLine(msg);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            });
        }

        public void Stop()
        {
            _process.OutputDataReceived -= _process_OutputDataReceived;
            _process.ErrorDataReceived -= _process_ErrorDataReceived;

            _reader.Dispose();
            _writer.Dispose();
            _process.Dispose();
            _client.Dispose();
        }
    }
}