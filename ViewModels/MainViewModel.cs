using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Anything_UI_WPF.Models;
using Anything_UI_WPF.Services;
using Wpf.Ui.Appearance;

namespace Anything_UI_WPF.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly SearchService _search = new();
    private readonly LanguageService _lang;
    private CancellationTokenSource? _indexCts;
    private string _searchQuery = string.Empty;
    private string _statusText = string.Empty;
    private bool _isIndexing;
    private bool _isDarkTheme;
    private ulong _indexSize;

    public LanguageService Lang => _lang;

    public ObservableCollection<FileSearchResult> Results { get; } = new();

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
                _ = SearchAsync(value);
        }
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public bool IsIndexing
    {
        get => _isIndexing;
        set => SetProperty(ref _isIndexing, value);
    }

    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set
        {
            if (SetProperty(ref _isDarkTheme, value))
            {
                ApplyTheme(value);
                OnPropertyChanged(nameof(ThemeTooltip));
            }
        }
    }

    public ulong IndexSize
    {
        get => _indexSize;
        set => SetProperty(ref _indexSize, value);
    }

    public string WindowTitle => _lang["window_title"];
    public string LangSettings => _lang["settings"];
    public string LangAbout => _lang["about"];
    public string LangRebuildIndex => _lang["rebuild_index"];
    public string LangSearchPlaceholder => _lang["search_placeholder"];

    public string ThemeTooltip => IsDarkTheme ? _lang["light_theme"] : _lang["dark_theme"];

    public ICommand ToggleThemeCommand { get; }
    public ICommand OpenSettingsCommand { get; }
    public ICommand OpenAboutCommand { get; }
    public ICommand OpenFileLocationCommand { get; }
    public ICommand RebuildIndexCommand { get; }

    public event Action? OpenSettingsRequested;

    public MainViewModel(LanguageService lang)
    {
        _lang = lang;
        ToggleThemeCommand = new RelayCommand(_ => IsDarkTheme = !IsDarkTheme);
        OpenSettingsCommand = new RelayCommand(_ => OpenSettingsRequested?.Invoke());
        OpenAboutCommand = new RelayCommand(_ => ShowAbout());
        OpenFileLocationCommand = new RelayCommand<FileSearchResult>(OpenFileLocation);
        RebuildIndexCommand = new RelayCommand(async _ => await StartIndexingAsync());

        StatusText = _lang["status_init"];
        InitializeEngine();
    }

    private void InitializeEngine()
    {
        var indexPath = AppConfig.GetIndexPath();
        if (File.Exists(indexPath))
        {
            if (_search.LoadIndex(indexPath))
            {
                IndexSize = _search.GetIndexSize();
                StatusText = _lang.Format("status_ready", ("count", IndexSize.ToString()));
            }
            else
            {
                StatusText = _lang.Format("status_load_error", ("error", "Failed to load index"));
                _ = StartIndexingAsync();
            }
        }
        else
        {
            _ = StartIndexingAsync();
        }
    }

    private async Task SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            Results.Clear();
            StatusText = _lang.Format("status_ready", ("count", IndexSize.ToString()));
            return;
        }

        if (IndexSize == 0)
        {
            StatusText = _lang["status_building"];
            return;
        }

        var results = await Task.Run(() => _search.Search(query));
        Results.Clear();
        foreach (var r in results)
            Results.Add(r);

        StatusText = results.Count == 0
            ? _lang["status_no_results"]
            : _lang.Format("status_results", ("count", results.Count.ToString()));
    }

    public async Task StartIndexingAsync()
    {
        if (IsIndexing) return;
        IsIndexing = true;
        _indexCts = new CancellationTokenSource();

        StatusText = _lang["status_indexing"];
        var service = new IndexService();
        var progress = new Progress<int>(count =>
            StatusText = _lang.Format("status_indexing_progress", ("count", count.ToString())));

        try
        {
            var success = await service.StartIndexingAsync(progress, _indexCts.Token);
            if (success)
            {
                StatusText = _lang["status_loading_index"];
                await Task.Delay(300);
                InitializeEngine();
            }
            else
            {
                StatusText = _lang["status_failed"];
            }
        }
        catch (OperationCanceledException)
        {
            StatusText = _lang["status_timeout"];
        }
        finally
        {
            IsIndexing = false;
        }
    }

    private static void OpenFileLocation(FileSearchResult? result)
    {
        if (result == null) return;
        var dir = Path.GetDirectoryName(result.FullPath);
        if (dir != null)
        {
            try
            {
                Process.Start("explorer.exe", $"\"{dir}\"");
            }
            catch { }
        }
    }

    private static void ApplyTheme(bool dark)
    {
        ApplicationThemeManager.Apply(dark ? ApplicationTheme.Dark : ApplicationTheme.Light);
    }

    private void ShowAbout()
    {
        var wpfUiAssembly = typeof(Wpf.Ui.Controls.TitleBar).Assembly;
        var version = FileVersionInfo.GetVersionInfo(wpfUiAssembly.Location).ProductVersion ?? "4.3.0";

        MessageBox.Show(
            _lang.Format("about_comments") +
            $"\n\nWPF-UI: {version}\n.NET: {Environment.Version}",
            _lang["about"],
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
