using System.Windows;
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
    private System.Windows.Forms.NotifyIcon? _trayIcon;
    private bool _closing;

    public MainViewModel ViewModel => (MainViewModel)DataContext;

    public MainWindow()
    {
        try
        {
            _lang = new LanguageService();
            var savedLang = AppConfig.LoadLanguage();
            if (savedLang != null)
                _lang.SetLanguage(savedLang);

            var vm = new MainViewModel(_lang);
            var savedTheme = AppConfig.LoadTheme();
            if (savedTheme.HasValue)
                vm.IsDarkTheme = savedTheme.Value;

            DataContext = vm;
            InitializeComponent();

            vm.OpenSettingsRequested += OpenSettings;
            vm.CloseRequested += () => _closing = true;

            SetButtonIcons();
            SetupTrayIcon();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Init error:\n\n{ex}");
            throw;
        }
    }

    private void SetupTrayIcon()
    {
        try
        {
            _trayIcon = new System.Windows.Forms.NotifyIcon();
            var processPath = Environment.ProcessPath;
            if (processPath != null && System.Drawing.Icon.ExtractAssociatedIcon(processPath) is { } icon)
                _trayIcon.Icon = icon;
            _trayIcon.Text = "Anything";
            _trayIcon.Visible = true;

            var menu = new System.Windows.Forms.ContextMenuStrip();
            menu.Items.Add(_lang["tray_show"], null, (_, _) => RestoreFromTray());
            menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            menu.Items.Add(_lang["tray_quit"], null, (_, _) =>
            {
                _closing = true;
                CleanupTray();
                System.Windows.Application.Current.Shutdown();
            });
            _trayIcon.ContextMenuStrip = menu;
            _trayIcon.DoubleClick += (_, _) => RestoreFromTray();
        }
        catch { }
    }

    private void RestoreFromTray()
    {
        Show();
        WindowState = System.Windows.WindowState.Normal;
        Activate();
    }

    private void CleanupTray()
    {
        if (_trayIcon != null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _trayIcon = null;
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
            var vm = (MainViewModel)DataContext;
            _settingsWindow = new SettingsWindow(_lang, () => _ = vm.StartIndexingAsync());
            _settingsWindow.Owner = this;
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
            _settingsWindow.Show();
        }
        else
        {
            _settingsWindow.Activate();
        }
    }

    private void SearchHistory_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && sender is System.Windows.Controls.ListBox lb)
        {
            var vm = (MainViewModel)DataContext;
            vm.SelectHistoryItemCommand.Execute(e.AddedItems[0]);
            lb.SelectedItem = null;
        }
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        if (WindowState == System.Windows.WindowState.Minimized)
        {
            Hide();
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (!_closing)
        {
            e.Cancel = true;
            WindowState = System.Windows.WindowState.Minimized;
            Hide();
        }
        else
        {
            CleanupTray();
        }
        base.OnClosing(e);
    }
}
