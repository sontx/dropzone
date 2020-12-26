using System.Windows;
using DropZone.Utils;

namespace DropZone
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.EnableVisualStyles();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SettingsUtils.Save();
            base.OnExit(e);
        }
    }
}