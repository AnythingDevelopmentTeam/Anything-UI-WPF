using System.Windows.Input;
using Anything_UI_WPF.Models;
using Anything_UI_WPF.Native;
using Anything_UI_WPF.Services;
using Anything_UI_WPF.ViewModels;
using Anything_UI_WPF.Views;
using Wpf.Ui.Controls;

namespace Anything_UI_WPF;

public partial class MainWindow : FluentWindow
{
    private readonly LanguageService _lang;
    private SettingsWindow? _settingsWindow;

    public MainViewModel ViewModel => (MainViewModel)DataContext;

    public MainWindow()
    {
        try
        {
            _lang = new LanguageService();
            var savedLang = AppConfig.LoadLanguage();
            if (savedLang != null)
                _lang.SetLanguage(savedLang);

            DataContext = new MainViewModel(_lang);
            InitializeComponent();

            ((MainViewModel)DataContext).OpenSettingsRequested += OpenSettings;

            SetButtonIcons();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Init error:\n\n{ex}");
            throw;
        }
    }

    private void SetButtonIcons()
    {
        SettingsBtn.Icon = AppIcons.CreateIcon(AppIcons.Settings);
        ThemeBtn.Icon = AppIcons.CreateIcon(AppIcons.Moon);
        AboutBtn.Icon = AppIcons.CreateIcon(AppIcons.Info);

        ((MainViewModel)DataContext).PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.IsDarkTheme))
                UpdateThemeIcon();
        };
    }

    private void UpdateThemeIcon()
    {
        var vm = (MainViewModel)DataContext;
        ThemeBtn.Icon = AppIcons.CreateIcon(vm.IsDarkTheme ? AppIcons.Sun : AppIcons.Moon);
    }

    private void ResultsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ResultsListView.SelectedItem is FileSearchResult result)
        {
            var dir = System.IO.Path.GetDirectoryName(result.FullPath);
            if (dir != null)
            {
                try
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"\"{dir}\"");
                }
                catch { }
            }
        }
    }

    private void OpenSettings()
    {
        if (_settingsWindow == null || !_settingsWindow.IsVisible)
        {
            _settingsWindow = new SettingsWindow(_lang);
            _settingsWindow.Owner = this;
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
            _settingsWindow.Show();
        }
        else
        {
            _settingsWindow.Activate();
        }
    }
}
