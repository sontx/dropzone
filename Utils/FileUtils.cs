using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace DropZone.Utils
{
    internal static class FileUtils
    {
        public static string BytesToString(long byteCount)
        {
            var suf = new[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString(CultureInfo.InvariantCulture) + suf[place];
        }

        public static bool IsFile(string path)
        {
            return !File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }

        public static string GetFriendlyFileSize(string path)
        {
            var sizeInBytes = new FileInfo(path).Length;
            return BytesToString(sizeInBytes);
        }

        public static void OpenInExplorer(string path)
        {
            var args = IsFile(path)
                ? $"/select,\"{path}\""
                : $"\"{path}\"";
            Process.Start("explorer.exe", args);
        }
    }
}