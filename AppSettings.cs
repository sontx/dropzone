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

        public AppSettings()
        {
            ReceivedFilesDir = Path.GetFullPath(Constants.DEFAULT_SAVE_DIR);
            IsShowNotification = true;
        }
    }
}