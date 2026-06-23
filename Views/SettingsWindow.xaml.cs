using Anything_UI_WPF.Services;
using Anything_UI_WPF.ViewModels;
using Wpf.Ui.Controls;

namespace Anything_UI_WPF.Views;

public partial class SettingsWindow : FluentWindow
{
    public SettingsViewModel ViewModel => (SettingsViewModel)DataContext;

    public SettingsWindow(LanguageService lang)
    {
        DataContext = new SettingsViewModel(lang);
        InitializeComponent();
    }
}
