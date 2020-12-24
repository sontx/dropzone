﻿using DropZone.Protocol;
using DropZone.ViewModels;
using DropZone.Views;
using System;
using System.Collections.Generic;

namespace DropZone
{
    internal static class ChatWindowManager
    {
        private static readonly Dictionary<ChatViewModel, ChatWindow> _showingWindows = new Dictionary<ChatViewModel, ChatWindow>();

        private static ChatWindow GetShowingWindow(Station.Neighbor withNeighbor)
        {
            foreach (var pair in _showingWindows)
            {
                if (pair.Key.Neighbor.Address == withNeighbor.Address)
                {
                    return pair.Value;
                }
            }

            return null;
        }

        public static void Show(Station.Neighbor withNeighbor, bool active = false)
        {
            ThreadUtils.RunOnUiAndWait(() =>
            {
                var chatWindow = GetShowingWindow(withNeighbor);
                if (chatWindow == null)
                {
                    chatWindow = new ChatWindow();
                    var vm = new ChatViewModel(withNeighbor);
                    chatWindow.DataContext = vm;
                    chatWindow.Closed += ChatWindow_Closed;
                    chatWindow.Show();
                    _showingWindows.Add(vm, chatWindow);
                }

                if (active)
                {
                    chatWindow.Activate();
                }
            });
        }

        private static void ChatWindow_Closed(object sender, EventArgs e)
        {
            if (sender is ChatWindow chatWindow)
            {
                var vm = chatWindow.DataContext as ChatViewModel;
                _showingWindows.Remove(vm);
            }
        }
    }
}