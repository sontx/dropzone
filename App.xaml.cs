using DropZone.Properties;
using DropZone.Protocol;
using System.IO;
using System.Windows;

namespace DropZone
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            InitializeSettings();
            base.OnStartup(e);
        }

        private static void InitializeSettings()
        {
            if (string.IsNullOrEmpty(Settings.Default.SaveDir))
                Settings.Default.SaveDir = Path.GetFullPath(Constants.DEFAULT_SAVE_DIR);
        }
    }
}