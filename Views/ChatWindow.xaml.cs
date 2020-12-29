using DropZone.ViewModels;
using Microsoft.Win32;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;

namespace DropZone.Views
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : WindowBase
    {
        public ChatWindow()
        {
            InitializeComponent();
            DataContextChanged += ChatWindow_DataContextChanged;
        }

        private void ChatWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ChatViewModel vm)
            {
                vm.Bubbles.CollectionChanged += Bubbles_CollectionChanged;
            }
        }

        private void Bubbles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                scrollViewer.ScrollToBottom();
            }
        }

        private void txtInput_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                return;

            if (DataContext is ChatViewModel vm)
            {
                vm.SendMessage(txtInput.Text);
                txtInput.Text = string.Empty;
                e.Handled = true;
            }
        }

        private void btnAttachment_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is ChatViewModel vm)
            {
                var openFileDialog = new OpenFileDialog
                {
                    Multiselect = true,
                    ValidateNames = false,
                    CheckFileExists = false,
                    CheckPathExists = false
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    vm.SendAttachment(openFileDialog.FileNames);
                }
            }
        }
    }
}