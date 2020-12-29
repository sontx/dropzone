using System.ComponentModel;
using System.IO;
using DropZone.Protocol;

namespace DropZone
{
    public class AppSettings
    {
        [Editor(typeof(System.Windows.Forms.Design.FolderNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DisplayName("Received Files Location")]
        public string ReceivedFilesDir { get; set; }

        [DisplayName("Show Notification")]
        public bool IsShowNotification { get; set; }

        [DisplayName("Show Logs")]
        public bool IsEnabledDebugger { get; set; }

        public AppSettings()
        {
            ReceivedFilesDir = Path.GetFullPath(Constants.DefaultSaveDir);
            IsShowNotification = true;
        }
    }
}