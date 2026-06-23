using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Anything_UI_WPF.Services;

namespace Anything_UI_WPF.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly LanguageService _lang;
    private string _newDirPath = string.Empty;
    private string _selectedLanguage;

    public LanguageService Lang => _lang;

    public ObservableCollection<string> ExcludedDirectories { get; } = new();

    public string NewDirPath
    {
        get => _newDirPath;
        set => SetProperty(ref _newDirPath, value);
    }

    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (SetProperty(ref _selectedLanguage, value))
            {
                _lang.SetLanguage(value);
                AppConfig.SaveLanguage(value);
                OnPropertyChanged(nameof(WindowTitle));
                LanguageChanged?.Invoke();
            }
        }
    }

    public string WindowTitle => _lang["settings_title"];
    public string LangGeneral => _lang["general"];
    public string LangLanguage => _lang["language"];
    public string LangExcludedDirs => _lang["excluded_dirs"];
    public string LangDirPlaceholder => _lang["dir_placeholder"];
    public string LangAddDir => _lang["add_dir"];
    public string LangRemove => _lang["remove"];
    public string LangRebuildIndex => _lang["rebuild_index"];

    public ICommand AddDirCommand { get; }
    public ICommand RemoveDirCommand { get; }
    public ICommand RebuildIndexCommand { get; }
    public ICommand CloseCommand { get; }

    public event Action? LanguageChanged;
    public event Action? RebuildIndexRequested;
    public event Action? CloseRequested;

    public SettingsViewModel(LanguageService lang)
    {
        _lang = lang;
        _selectedLanguage = lang.CurrentCode;

        AddDirCommand = new RelayCommand(_ => AddDirectory());
        RemoveDirCommand = new RelayCommand<string>(dir => RemoveDirectory(dir));
        RebuildIndexCommand = new RelayCommand(_ => RebuildIndexRequested?.Invoke());
        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke());

        LoadExcludedDirectories();
    }

    private void LoadExcludedDirectories()
    {
        ExcludedDirectories.Clear();
        foreach (var dir in AppConfig.LoadCustomSkipDirs())
            ExcludedDirectories.Add(dir);
    }

    private void AddDirectory()
    {
        var path = NewDirPath.Trim();
        if (string.IsNullOrEmpty(path)) return;
        if (Directory.Exists(path) && !ExcludedDirectories.Contains(path))
        {
            ExcludedDirectories.Add(path);
            AppConfig.SaveCustomSkipDirs(ExcludedDirectories.ToList());
            NewDirPath = string.Empty;
        }
    }

    private void RemoveDirectory(string? dir)
    {
        if (dir == null) return;
        ExcludedDirectories.Remove(dir);
        AppConfig.SaveCustomSkipDirs(ExcludedDirectories.ToList());
    }
}
