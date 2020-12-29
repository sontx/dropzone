using System;

namespace DropZone.Protocol
{
    internal static class Constants
    {
        public static readonly string DefaultSaveDir = "Received Files";

        public static readonly string RemoteCommandOutputPrefix = "OUTPUT";
        public static readonly string RemoteCommandErrorPrefix = "ERROR";

        public static readonly string SendingFileSessionHeader = "SEND_FILES";

        public static readonly ushort ChatPort = 22041;
        public static readonly ushort FileServerPort = 22042;
        public static readonly ushort RemoteCommandPort = 22043;
        public static readonly ushort StationPort = 22044;

        public static readonly int BufferSizeSocket = 8000;
        public static readonly int BufferSizeHeader = 2000;

        public static readonly int DebugMaxDelay = 10;
        public static readonly int DebugMinDelay = 1;

        public static readonly TimeSpan ShortestOnlineTime = TimeSpan.FromSeconds(3);
    }
}