using DropZone.Utils;
using System;
using System.Net.Sockets;

namespace DropZone.Protocol
{
    public abstract class ReceivedThreadTcpClient : ReadWriteTcpClient
    {
        private ThreadWrapper _receivingThread;
        private bool _disposing;

        protected ReceivedThreadTcpClient(TcpClient client)
            : base(client)
        {
            InitializeThread();
        }

        protected ReceivedThreadTcpClient(string host, int port)
            : base(host, port)
        {
            InitializeThread();
        }

        private void InitializeThread()
        {
            _receivingThread = new ThreadWrapper
            {
                DoWork = WaitForIncomingMessage,
                OnExit = OnExitIncomingMessageLoop,
                OnError = HandleError
            };
        }

        private void HandleError(Exception ex)
        {
            if (!_disposing)
                OnErrorWhileWaitingIncomingMessage(ex);
        }

        private void WaitForIncomingMessage()
        {
            while (true)
            {
                if (!OnReadNextIncomingMessage())
                    break;
            }
        }

        protected abstract bool OnReadNextIncomingMessage();

        protected virtual void OnExitIncomingMessageLoop()
        {
        }

        protected virtual void OnErrorWhileWaitingIncomingMessage(Exception ex)
        {
            Debugger.Log(ex);
        }

        public virtual void Start()
        {
            _receivingThread.Start();
        }

        public virtual void Stop()
        {
            Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            _disposing = true;
            base.Dispose(disposing);
        }
    }
}