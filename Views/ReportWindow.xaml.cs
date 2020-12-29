using DropZone.Utils;
using DropZone.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace DropZone.Views
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : WindowBase
    {
        public ReportWindow()
        {
            InitializeComponent();
            var reportViewModel = new ReportViewModel();
            DataContext = reportViewModel;
        }

        private void btnClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void listView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((FrameworkElement)e.OriginalSource).DataContext is ReportFileViewModel item)
            {
                FileUtils.OpenInExplorer(item.Path);
            }
        }
    }
}