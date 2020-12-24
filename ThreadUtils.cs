using System;
using System.Windows;
using System.Windows.Threading;

namespace DropZone
{
    internal static class ThreadUtils
    {
        private static Dispatcher _dispatcher;

        public static void Initialize()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public static void RunOnUi(Action action)
        {
            try
            {
                _dispatcher.BeginInvoke(action);
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        public static void RunOnUiAndWait(Action action)
        {
            try
            {
                _dispatcher.Invoke(action);
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        public static void ShowDebugMessage(object msg = null)
        {
            RunOnUi(() => MessageBox.Show(msg?.ToString()));
        }
    }
}