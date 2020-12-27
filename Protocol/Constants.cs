using System;

namespace DropZone.Protocol
{
    internal static class Constants
    {
        public static readonly string COMMAND_SEND_FILES = "SEND_FILES";
        public static readonly string COMMAND_CHAT_MESSAGE = "CHAT_MESSAGE";
        public static readonly string COMMAND_DATA_NONE = "NONE";
        public static readonly string DEFAULT_SAVE_DIR = "Received Files";
        public static readonly string REMOTE_COMMAND_OUTPUT_PREFIX = "OUTPUT";
        public static readonly string REMOTE_COMMAND_ERROR_PREFIX = "ERROR";
        public static readonly ushort MASTER_PORT = 22496;
        public static readonly ushort FILE_SERVER_PORT = 30393;
        public static readonly ushort REMOTE_COMMAND_PORT = 22049;
        public static readonly ushort STATION_PORT = 30394;
        public static readonly int BUFFER_SIZE_SOCKET = 8000;
        public static readonly int BUFFER_SIZE_HEADER = 2000;
        public static readonly int DEBUG_MAX_DELAY = 10;
        public static readonly int DEBUG_MIN_DELAY = 1;
        public static readonly TimeSpan SHORTEST_ONLINE_TIME = TimeSpan.FromSeconds(3);
    }
}