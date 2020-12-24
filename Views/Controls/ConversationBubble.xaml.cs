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
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ConversationBubble), new PropertyMetadata("", OnTextChanged));

        public static readonly DependencyProperty BubbleColorProperty =
            DependencyProperty.Register("BubbleColor", typeof(Brush), typeof(ConversationBubble), new PropertyMetadata(null, OnBubbleColorChanged));

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
            var text = textBlock.Text.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                Clipboard.SetText(text);
            }
        }
    }
}