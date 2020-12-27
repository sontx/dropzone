using DropZone.Utils;
using DropZone.ViewModels;
using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace DropZone.Views
{
    /// <summary>
    /// Interaction logic for TerminalWindow.xaml
    /// </summary>
    public partial class TerminalWindow : Window
    {
        public TerminalWindow()
        {
            InitializeComponent();
            DataContextChanged += RemoteExecutorWindow_DataContextChanged;
        }

        private void RemoteExecutorWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is TerminalViewModel vm)
            {
                vm.ReceivedError = msg => ThreadUtils.RunOnUi((() => ReceivedError(msg)));
                vm.ReceivedOutput = msg => ThreadUtils.RunOnUi((() => ReceivedOutput(msg)));
            }
        }

        private void ReceivedError(string msg)
        {
            AppendBreakLineIfNeeded();
            paragraph.Inlines.Add(new Run(msg) { Foreground = Brushes.Red });
            ScrollToBottomIfNeeded();
        }

        private void ReceivedOutput(string msg)
        {
            AppendBreakLineIfNeeded();
            paragraph.Inlines.Add(new Run(msg));
            ScrollToBottomIfNeeded();
        }

        private void AppendBreakLineIfNeeded()
        {
            if (paragraph.Inlines.Count > 0)
                paragraph.Inlines.Add(new LineBreak());
        }

        private void ScrollToBottomIfNeeded()
        {
            if (scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight)
                scrollViewer.ScrollToBottom();
        }

        private void txtInput_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (DataContext is TerminalViewModel vm)
            {
                var cmd = vm.SendCommand(txtInput.Text);
                if (string.IsNullOrEmpty(cmd))
                    return;

                vm.HasOutput = true;
                ReceivedOutput(cmd);
                txtInput.Clear();
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            txtInput.Focus();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (DataContext is TerminalViewModel vm)
            {
                vm.Cleanup();
            }
        }

        private void content_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            txtInput.Focus();
        }
    }
}