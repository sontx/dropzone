using DropZone.Protocol;
using System;

namespace DropZone.ViewModels
{
    public partial class MainViewModel
    {
        private readonly RemoteCommandServer _commandServer;

        private void HandleRemoteExecutor(RemoteCommandExecutor executor)
        {
            executor.Closed += Executor_Closed;
            executor.Start();
        }

        private void Executor_Closed(object sender, EventArgs e)
        {
            if (sender is RemoteCommandExecutor executor)
            {
                executor.Stop();

                executor.Closed -= Executor_Closed;
            }
        }
    }
}