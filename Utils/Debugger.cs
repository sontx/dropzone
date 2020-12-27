using DropZone.Views;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DropZone.Utils
{
    internal static class Debugger
    {
        private static readonly object _lock = new object();
        private static LogWindow _logWindow;
        private static bool _isEnabledLog;

        public static bool IsEnabledLog
        {
            get => _isEnabledLog;
            set
            {
                _isEnabledLog = value;
                if (_isEnabledLog) return;

                lock (_lock)
                {
                    if (_logWindow != null)
                    {
                        _logWindow.Close();
                        _logWindow = null;
                    }
                }
            }
        }

        public static void Init()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Log(e.Exception);
        }

        private static void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            Log(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log(e.ExceptionObject);
        }

        public static void Log(object msg)
        {
            if (msg == null || !IsEnabledLog)
                return;

            if (msg is Exception ex)
            {
                LogImpl(ex.Message);
                LogImpl(ex.StackTrace);
            }
            else
            {
                LogImpl(msg.ToString());
            }
        }

        private static void LogImpl(string msg)
        {
            lock (_lock)
            {
                if (_logWindow == null)
                {
                    ThreadUtils.RunOnUiAndWait(() =>
                    {
                        _logWindow = new LogWindow { Owner = Application.Current.MainWindow };
                        _logWindow.Closed += _logWindow_Closed;
                        _logWindow.Show();
                    });
                }
            }

            _logWindow?.AppendLine(msg);
        }

        private static void _logWindow_Closed(object sender, EventArgs e)
        {
            lock (_lock)
            {
                _logWindow = null;
            }
        }
    }
}