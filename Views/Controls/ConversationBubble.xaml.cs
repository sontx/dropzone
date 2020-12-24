using System;
using DropZone.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DropZone.Views.Controls
{
    /// <summary>
    /// Interaction logic for ConversationBubble.xaml
    /// </summary>
    public partial class ConversationBubble : UserControl
    {
        public static readonly DependencyProperty AttachmentsProperty =
            DependencyProperty.Register("Attachments", typeof(IEnumerable<AttachmentViewModel>), typeof(ConversationBubble), new PropertyMetadata(null, OnAttachmentsChanged));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ConversationBubble), new PropertyMetadata("", OnTextChanged));

        public static readonly DependencyProperty BubbleColorProperty =
            DependencyProperty.Register("BubbleColor", typeof(Brush), typeof(ConversationBubble), new PropertyMetadata(null, OnBubbleColorChanged));

        private static void OnAttachmentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConversationBubble cb)
            {
                var attachments = (IEnumerable<AttachmentViewModel>)e.NewValue;
                cb.itemsControl.Visibility = attachments != null ? Visibility.Visible : Visibility.Collapsed;
                cb.textBlock.Visibility = attachments != null ? Visibility.Collapsed : Visibility.Visible;
                if (attachments != null)
                {
                    cb.itemsControl.ItemsSource = attachments;
                }
            }
        }

        private static void OnBubbleColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConversationBubble cb)
            {
                cb.border.Background = e.NewValue as Brush;
            }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConversationBubble cb)
            {
                cb.textBlock.Text = e.NewValue?.ToString();
            }
        }

        public IEnumerable<AttachmentViewModel> Attachments
        {
            get => (IEnumerable<AttachmentViewModel>)GetValue(AttachmentsProperty);
            set => SetValue(AttachmentsProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public Brush BubbleColor
        {
            get => (Brush)GetValue(BubbleColorProperty);
            set => SetValue(BubbleColorProperty, value);
        }

        public ConversationBubble()
        {
            InitializeComponent();
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            var text = Attachments != null
                ? string.Join(Environment.NewLine, Attachments.Select(attachment => attachment.Path))
                : textBlock.Text.Trim();

            if (!string.IsNullOrEmpty(text))
            {
                Clipboard.SetText(text);
            }
        }

        private void attachmentItem_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var element = sender as FrameworkElement;
                var vm = element.DataContext as AttachmentViewModel;
                Process.Start("explorer.exe", $"/select,\"{vm.Path}\"");
            }
        }
    }
}