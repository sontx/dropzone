using DropZone.Protocol.Terminal;
using System;

namespace DropZone.ViewModels
{
    public partial class MainViewModel
    {
        private RemoteTerminalServer _terminalServer;

        private void InitializeTerminal()
        {
            _terminalServer = new RemoteTerminalServer
            {
                ExecutorHandler = HandleTerminalExecutor
            };

            _terminalServer.Start();
        }

        private void CloseTerminal()
        {
            _terminalServer.Stop();
        }

        private void HandleTerminalExecutor(RemoteTerminalExecutor executor)
        {
            executor.Closed += TerminalExecutor_Closed;
            executor.Start();
        }

        private void TerminalExecutor_Closed(object sender, EventArgs e)
        {
            if (sender is RemoteTerminalExecutor executor)
            {
                executor.Stop();

                executor.Closed -= TerminalExecutor_Closed;
            }
        }
    }
}