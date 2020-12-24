﻿using DropZone.ViewModels;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;

namespace DropZone.Views
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
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
                vm.Send(txtInput.Text);
                txtInput.Text = string.Empty;
                e.Handled = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is ChatViewModel vm)
            {
                vm.Cleanup();
            }

            base.OnClosed(e);
        }
    }
}