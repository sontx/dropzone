using System;
using System.Threading;

namespace DropZone.Utils
{
    internal class ThreadWrapper
    {
        private Thread _thread;
        private bool _isRunning;

        public Action OnStart { get; set; }
        public Action DoWork { get; set; }
        public Action OnExit { get; set; }
        public Action<Exception> OnError { get; set; }

        public void Start()
        {
            lock (this)
            {
                if (_isRunning)
                    return;
            }

            _thread = new Thread(RunOnBackground) { IsBackground = true };
            _thread.Start();
        }

        private void RunOnBackground()
        {
            lock (this)
            {
                _isRunning = true;
            }

            OnStart?.Invoke();

            try
            {
                DoWork?.Invoke();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
            finally
            {
                OnExit?.Invoke();

                lock (this)
                {
                    _isRunning = false;
                }
            }
        }
    }
}