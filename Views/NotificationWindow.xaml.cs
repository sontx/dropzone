using DropZone.Utils;
using DropZone.ViewModels;
using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace DropZone.Views
{
    /// <summary>
    /// Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        public NotificationWindow()
        {
            InitializeComponent();

            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                var workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
                var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
                var corner = transform.Transform(new Point(workingArea.Right, workingArea.Bottom));

                this.Left = corner.X - this.ActualWidth;
                this.Top = corner.Y - this.ActualHeight;
            }));

            Loaded += NotificationWindow_Loaded;
            MouseLeftButtonDown += NotificationWindow_MouseLeftButtonDown;
        }

        private void NotificationWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1 && DataContext is NotificationViewModel vm)
            {
                vm.OpenReport();
                Close();
            }
        }

        private void NotificationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= NotificationWindow_Loaded;
            PlayNotificationSound();
            ScheduleClose();
        }

        private async void ScheduleClose()
        {
            await Task.Delay(5000);
            Close();
        }

        private void btnClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PlayNotificationSound()
        {
            using (var theSound = new SoundPlayer(new MemoryStream(Properties.Resources.notification_sound)))
            {
                theSound.Play();
            }
        }

        private void saveLocation_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1 && DataContext is NotificationViewModel vm)
            {
                e.Handled = true;
                FileUtils.OpenInExplorer(vm.SaveLocation);
            }
        }
    }
}