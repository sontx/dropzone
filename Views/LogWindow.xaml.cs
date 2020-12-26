using System;
using System.Threading;
using System.Windows;

namespace DropZone.Views
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        private readonly int _mainThreadId;

        public LogWindow()
        {
            InitializeComponent();
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public void AppendLine(string st)
        {
            if (_mainThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Dispatcher.Invoke(() => AppendLine(st));
            }
            else
            {
                textBox.AppendText(st + Environment.NewLine);
            }
        }
    }
}