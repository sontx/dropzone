using DropZone.Utils;
using DropZone.ViewModels;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace DropZone.Views
{
    /// <summary>
    /// Interaction logic for TerminalWindow.xaml
    /// </summary>
    public partial class TerminalWindow : WindowBase
    {
        private List<string> _calledCommands = new List<string>();
        private int _currentBrowseCommandIndex;

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

        private void txtInput_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                _currentBrowseCommandIndex--;
            }
            else if (e.Key == Key.Down)
            {
                _currentBrowseCommandIndex++;
            }
            else
            {
                return;
            }

            if (_currentBrowseCommandIndex < 0)
                _currentBrowseCommandIndex = _calledCommands.Count - 1;
            else if (_currentBrowseCommandIndex >= _calledCommands.Count)
                _currentBrowseCommandIndex = 0;

            if (_calledCommands.Count > 0)
                txtInput.Text = _calledCommands[_currentBrowseCommandIndex];
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

                if (cmd.ToLower() == "cls")
                {
                    paragraph.Inlines.Clear();
                    vm.HasOutput = false;
                }
                else
                {
                    vm.HasOutput = true;
                    ReceivedOutput(cmd);
                }

                txtInput.Clear();

                _calledCommands.Add(cmd);
                _currentBrowseCommandIndex = _calledCommands.Count;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            txtInput.Focus();
        }

        private void content_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            txtInput.Focus();
        }
    }
}