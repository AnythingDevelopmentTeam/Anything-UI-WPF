using System.IO;

namespace Anything_UI_WPF.Models;

public class FileSearchResult
{
    public string FullPath { get; set; } = string.Empty;
    public string Name => Path.GetFileName(FullPath);
    public string DirectoryPath => Path.GetDirectoryName(FullPath) ?? string.Empty;

    private long? _size;
    public long Size
    {
        get
        {
            if (_size == null)
            {
                try { _size = new FileInfo(FullPath).Length; }
                catch { _size = 0; }
            }
            return _size.Value;
        }
    }

    private DateTime? _lastModified;
    public DateTime LastModified
    {
        get
        {
            if (_lastModified == null)
            {
                try { _lastModified = File.GetLastWriteTime(FullPath); }
                catch { _lastModified = DateTime.MinValue; }
            }
            return _lastModified.Value;
        }
    }

    public string SizeFormatted => FormatSize(Size);
    public string LastModifiedFormatted => LastModified == DateTime.MinValue ? "" : LastModified.ToString("g");

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }
}
