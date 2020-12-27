using DropZone.Utils;
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
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.EnableVisualStyles();
            Debugger.Init();
            Debugger.IsEnabledLog = SettingsUtils.Get<AppSettings>().IsEnabledDebugger;

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SettingsUtils.Save();
            base.OnExit(e);
        }
    }
}