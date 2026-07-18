using System;
using System.Collections.Generic;
using System.IO;

namespace TinyTable
{
    public static class DesktopItemClassifier
    {
        public const string Folders = "文件夹";
        public const string Shortcuts = "快捷方式";
        public const string Images = "图片";
        public const string Documents = "文档";
        public const string Archives = "压缩包";
        public const string Media = "音视频";
        public const string Other = "其他";
        public const string SystemIcons = "系统图标";

        public static readonly string[] CategoryOrder =
        {
            Folders, Shortcuts, Images, Documents,
            Archives, Media, Other, SystemIcons
        };

        private static readonly HashSet<string> ImageExtensions = Set(
            ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp", ".tif",
            ".tiff", ".svg", ".ico", ".psd", ".heic", ".raw");
        private static readonly HashSet<string> DocumentExtensions = Set(
            ".txt", ".md", ".pdf", ".doc", ".docx", ".xls", ".xlsx",
            ".ppt", ".pptx", ".csv", ".rtf", ".odt", ".ods", ".wps",
            ".et", ".tex", ".html", ".htm", ".json", ".xml", ".key");
        private static readonly HashSet<string> ArchiveExtensions = Set(
            ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz", ".iso");
        private static readonly HashSet<string> MediaExtensions = Set(
            ".mp3", ".wav", ".flac", ".aac", ".m4a", ".ogg", ".mp4",
            ".mkv", ".avi", ".mov", ".wmv", ".webm", ".flv");

        public static string Classify(string path, bool isVirtual)
        {
            if (isVirtual) return SystemIcons;
            if (Directory.Exists(path)) return Folders;
            string extension = Path.GetExtension(path).ToLowerInvariant();
            if (extension == ".lnk" || extension == ".url") return Shortcuts;
            if (ImageExtensions.Contains(extension)) return Images;
            if (DocumentExtensions.Contains(extension)) return Documents;
            if (ArchiveExtensions.Contains(extension)) return Archives;
            if (MediaExtensions.Contains(extension)) return Media;
            return Other;
        }

        public static DateTime GetWriteTimeUtc(string path)
        {
            if (string.IsNullOrEmpty(path))
                return DateTime.MinValue;
            try
            {
                return Directory.Exists(path)
                    ? Directory.GetLastWriteTimeUtc(path)
                    : File.GetLastWriteTimeUtc(path);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        private static HashSet<string> Set(params string[] values)
        {
            return new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
        }
    }
}
