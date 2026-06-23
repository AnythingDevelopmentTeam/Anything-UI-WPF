namespace Anything_UI_WPF.Models;

public class FileSearchResult
{
    public string FullPath { get; set; } = string.Empty;
    public string Name => System.IO.Path.GetFileName(FullPath);
    public string DirectoryPath => System.IO.Path.GetDirectoryName(FullPath) ?? string.Empty;
}
