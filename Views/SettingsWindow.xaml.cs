using System.Windows;
using DropZone.Utils;

namespace DropZone.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            var settings = SettingsUtils.Get<AppSettings>();
            propertyGrid.SelectedObject = settings;
            propertyGrid.HelpVisible = false;
            propertyGrid.ToolbarVisible = false;
        }

        private void btnClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}