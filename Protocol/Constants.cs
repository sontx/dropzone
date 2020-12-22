namespace DropZone.Protocol
{
    internal static class Constants
    {
        public static readonly string COMMAND_SEND_FILES = "SEND_FILES";
        public static readonly string COMMAND_DATA_NONE = "NONE";
        public static readonly string DEFAULT_SAVE_DIR = "Received Files";
        public static readonly ushort MASTER_PORT = 22496;
        public static readonly ushort FILE_SERVER_PORT = 30393;
        public static readonly int BUFFER_SIZE_SOCKET = 8000;
        public static readonly int BUFFER_SIZE_HEADER = 2000;
    }
}